# Persistence 상세 실패 원인

`TrySave(data, out ESaveFailure failure)` / `TryLoad(out data, out wasRecovered, out wasMigrated, out ESaveFailure failure)`

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
