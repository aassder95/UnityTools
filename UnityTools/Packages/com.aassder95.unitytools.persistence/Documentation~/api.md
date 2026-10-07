# Persistence 1.0 API 계약

## VersionedSaveStore<T>

`T`는 class입니다. 경로·모델 버전·codec·검증·migration을 주입하며 전역 상태·coroutine·구독이 없습니다. 파일 stream은 호출 안에서 정리하므로 별도 Init/Release/Dispose는 없습니다. 동일 경로에는 단일 writer를 사용합니다.

| API | 성공 | 예상 실패 |
| --- | --- | --- |
| `TryCreate(path, curVersion, codec, validate, migrations, out store)` | 절대 경로 고정과 store 생성 | 잘못된 경로/버전, null codec/validate, 잘못된 migration이면 false와 null store |
| `TryLoad(out data, out wasRecovered, out wasMigrated)` | 검증된 데이터와 복구/변환 플래그 | 파일 없음, 복구 불가, 미래 본문, 변환/codec/검증 거절이면 false, null data와 false flags |
| `TrySave(data)` | 검증·직렬화·파일 작업 완료 | null/무효 데이터, codec 거절, 미래 본문 보호, 처리 가능한 파일 오류이면 false |

기존 API에 더해 `TrySave(data, out ESaveFailure failure)`와 `TryLoad(out data, out wasRecovered, out wasMigrated, out ESaveFailure failure)`를 제공합니다. 기존 API도 동일한 구현 경로를 사용합니다. 저장 format과 복구·미래 버전 보호 정책은 변경하지 않습니다.

### 상세 실패 원인

| ESaveFailure | 의미 |
| --- | --- |
| `None` | 작업 성공. 백업 복구 성공도 포함 |
| `InvalidArgument` | null 저장 데이터 또는 저수준 쓰기의 null content |
| `FileNotFound` | 파일 또는 상위 디렉터리 없음 |
| `IoError` | 파일 잠금 등 처리 가능한 IOException |
| `AccessDenied` | UnauthorizedAccessException |
| `InvalidPath` | 빈 경로 또는 ArgumentException |
| `UnsupportedOperation` | NotSupportedException |
| `CorruptData` | envelope 해석 실패, 잘못된 버전·빈 payload·hash 불일치 |
| `FutureVersion` | 읽은 파일이 미래 버전이거나 해당 store가 저장 보호 상태 |
| `MigrationMissing` | 필요한 변환 단계 없음 |
| `MigrationFailed` | 변환 거절 또는 빈 변환 payload |
| `SerializationFailed` | codec 직렬화 거절 또는 빈 payload |
| `DeserializationFailed` | codec 역직렬화 거절 또는 null 데이터 |
| `ValidationFailed` | 게임 데이터 검증 함수의 거절 |

로드 성공 시 failure는 None이며 복구·변환 여부는 기존 플래그로 확인합니다. 본문과 백업이 모두 실패하면 본문 원인을 반환하되, 본문이 FileNotFound일 때는 백업 원인을 반환합니다. 따라서 VersionedSaveStore의 FileNotFound는 본문과 백업이 모두 없음을 뜻합니다. 미래 버전 본문은 백업 시도 없이 FutureVersion을 반환하며 저장 보호를 활성화합니다. 미래 버전 백업의 거절은 저장 보호를 활성화하지 않습니다. 실패 시 data는 null, 두 플래그는 false입니다.

오류는 실제 실패한 처리 단계와 OS 예외를 기준으로 분류합니다. 경로가 디렉터리이거나 파일 시스템이 지원하지 않는 동작의 분류는 OS에 따라 달라질 수 있습니다. codec 거절은 손상과 codec 구현 오류를 더 세분하지 않습니다. callback이 직접 발생시킨 예외를 enum으로 변환하지 않습니다.

저장 전 기존 본문을 읽지 못했을 때의 동작은 기존 정책을 유지하며 최종 쓰기 실패 원인을 반환합니다. 상세 결과는 파일 존재 사전 검사나 모든 저장 실패에 대한 보호 보장을 추가하지 않습니다.

TryCreate는 디렉터리나 파일을 만들지 않으며 쓰기 가능 여부를 보장하지 않습니다. 첫 저장 때 디렉터리를 생성합니다. null migrations는 빈 배열입니다. 원소는 null이 아니고 1 ≤ FromVersion < curVersion, ToVersion = FromVersion + 1, ToVersion ≤ curVersion이며 FromVersion은 중복할 수 없습니다. 단계 누락은 실제 이전 데이터를 로드할 때 확인합니다.

배열은 복사하지만 migration 객체는 공유하므로 생성 후 설정을 변경하지 않습니다. codec/migration/validate는 순수하고 예외 없는 호출을 전제로 합니다. TrySave의 기존 본문 검사에서도 이들을 실행할 수 있으므로 파일 쓰기나 보상 지급 같은 부작용을 넣지 않습니다. custom callback 예외를 store가 대신 삼키지 않습니다.

로드는 파일을 다시 쓰지 않습니다. 변환/복구 내용을 저장할 시점은 호출자가 정하고 TrySave 결과를 확인합니다. 낮은 버전의 유효 본문도 migration/검증 거절로 로드가 실패할 수 있으며 명시적인 새 저장은 이를 덮어쓸 수 있습니다. TryLoad=false만으로 새 게임을 저장하지 않습니다.

### 미래 버전 보호

본문이 현재보다 높은 버전을 선언하면 hash 검증 전에도 보수적으로 거절합니다. 구버전 백업으로 대체하지 않으며 store의 저장도 잠급니다. 외부에서 본문을 교체/삭제해도 즉시 해제되지 않고, 같은 store가 호환 가능한 데이터의 로드에 성공하면 해제됩니다.

보호 대상은 미래 버전 **본문**입니다. 백업 전체 이력의 버전 보호나 다중 writer coordination은 제공하지 않습니다. `.bak`은 직전 파일 또는 보존 중인 유효 백업이며 전체 세대 복구 로그가 아닙니다.

### 저장 format

envelope는 `_version`(정수), `_payload`(문자열), `_hash`(Base64 문자열)입니다. hash는 `version.ToString(InvariantCulture) + "\n" + payload`의 UTF-8 bytes에 SHA-256을 적용합니다. package SemVer와 게임 payload schema version은 별개입니다. 0.1.0과 1.0.0에서 같은 format을 사용합니다.

## ISaveCodec<T> / UnityJsonSaveCodec<T>

TrySerialize/TryDeserialize는 성공 여부를 반환하며 성공 payload는 비어 있지 않고 data는 null이 아니어야 합니다. 기본 codec은 JsonUtility를 사용하고 null/빈 입력과 ArgumentException을 false로 처리합니다. Unity 직렬화 DTO field를 사용하며 property·Dictionary·임의 다형성 지원을 가정하지 않습니다. Serialize 성공은 게임 데이터 유효성을 뜻하지 않으므로 validate를 함께 전달합니다.

## ISaveMigration

FromVersion/ToVersion은 한 단계의 schema 전이이며 TryMigrate는 다음 버전 payload를 반환합니다. version으로 단계를 선택하므로 배열 순서에 의존하지 않습니다. 모든 단계 완료 후 최종 codec과 validate를 실행합니다. 변환 실패는 로드 실패이며 원본을 다시 쓰지 않습니다.

## SaveFileStore

저수준 파일 API이며 envelope/hash/미래 버전 보호는 제공하지 않습니다. 일반 저장은 VersionedSaveStore를 사용합니다.

- 생성자 `SaveFileStore(path)`는 쓰기를 수행하거나 경로를 검증하지 않습니다.
- `TryRead(isBackup, out content)`는 본문 또는 `.bak`을 UTF-8로 읽습니다. 처리 가능한 실패는 false와 null content입니다.
- `TryWrite(content, shouldPreserveBackup)`는 `.tmp` 쓰기와 flush 후 File.Replace 또는 신규 File.Move를 수행합니다. true는 해당 작업 완료를 뜻합니다.
- `TryRead(isBackup, out content, out failure)`와 `TryWrite(content, shouldPreserveBackup, out failure)`는 동일 작업의 상세 실패 원인을 반환합니다. 읽기는 File.Exists 사전 검사 대신 실제 읽기의 예외로 파일 없음과 접근·입출력 실패를 구분합니다.
- `shouldPreserveBackup=true`는 기존 백업을 교체 대상으로 사용하지 않습니다. 이전 본문을 백업하는 일반 동작은 false입니다.

임시 파일 정리를 시도하지만 권한 오류나 강제 종료로 `.tmp`가 남을 수 있습니다. 실패 위치와 OS의 교체 동작에 따라 보존 결과가 달라질 수 있으므로 모든 실패/전원 손실에 대한 원자적 transaction으로 해석하지 않습니다. 동일 경로의 외부 변경과 다중 writer는 지원하지 않습니다.
