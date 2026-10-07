# Persistence 1.0.0 릴리스 준비

현재는 **미게시 릴리스 후보**입니다. 후보 package version은 1.0.0이고 예정 tag는 `unitytools-persistence/v1.0.0`입니다. 기존 공개 tag를 가리키는 설치 주소로 안내하지 않습니다. Benchmark는 이번 준비 범위에 포함하지 않습니다.

## 확정한 계약

- 공개 타입·method·signature를 변경하지 않습니다. `VersionedSaveStore<T>`, `SaveFileStore`, `ISaveCodec<T>`, `ISaveMigration`, `UnityJsonSaveCodec<T>`를 유지합니다.
- `_version`/`_payload`/`_hash` envelope와 Base64 SHA-256 format을 유지합니다. package version은 payload schema version과 별개이며 0.1.0에서 업그레이드할 때 새 migration이 필요하지 않습니다.
- 버전별 migration, 최종 검증, 백업 복구와 미래 버전 **본문** 보호를 제공합니다. 외부 보상 transaction·암호화·다중 writer를 제공하지 않습니다.
- 파일 입출력은 동기식이며 단일 writer를 지원합니다. 고정 API 명칭을 이유로 공개 범위를 줄이거나 새 실패 enum을 도입하지 않습니다.
- UPM 폴더에 MIT LICENSE.md를 포함합니다. 저장 format field·기존 meta GUID·prefab·scene은 변경하지 않습니다.

런타임 변경은 TryCreate의 잘못된 migration 버전 검증 한 곳입니다. int.MaxValue의 +1 overflow로 음수 다음 버전을 받아들이던 경로를 시작 버전의 상한 검사로 제거했습니다. 정상 설정의 동작·공개 signature·저장 format은 유지합니다. API 계약은 [패키지 문서](../UnityTools/Packages/com.aassder95.unitytools.persistence/Documentation~/api.md), 최소 사용 예제는 [README](../UnityTools/Packages/com.aassder95.unitytools.persistence/README.md)에 있습니다.

## 후보 Git 설치 검증

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/test-persistence-release.ps1
```

현재 Persistence 폴더만 임시 Git 저장소로 복사하고 단일 commit을 고정합니다. 원본 working tree·index·branch·tag·origin은 변경하지 않습니다. `git+file:///.../candidate.git?path=/UnityTools/Packages/com.aassder95.unitytools.persistence#<commit>`으로 빈 Unity 프로젝트에 설치합니다. 이는 직접 `file:` 폴더 참조와 구분하는 실제 Git clone 경로입니다. [Unity의 Git FILE URL 안내](https://docs.unity3d.com/2022.3/Documentation/Manual/upm-git.html)를 따릅니다.

Unity 2022.3.62f3과 6000.3.20f1에서 다음 두 조건을 별도로 실행합니다.

| 시나리오 | 조건 |
| --- | --- |
| persistence | 코어 단독, uGUI와 실험실 Import 없이 테스트·Windows Mono build |
| save-lab | 같은 Git commit 설치, Package Manager Sample API Import·장면 검사·테스트·Windows Mono build |

이후 package cache의 실제 manifest 버전·이름·빈 dependency와 lock의 `source=git`, 정확한 commit hash를 확인합니다. Candidate 모드는 설치 파일 전체의 개수·경로·내용도 고정 소스와 대조합니다. Windows Git의 LF/CRLF 변환은 허용하며, `package.json`은 JSON 서식과 Unity가 추가한 `_fingerprint`만 비교에서 제외합니다. 나머지 manifest 값의 변경은 실패합니다. `release-validation.json`, `source-files.json`, 원본 Git snapshot, 프로젝트별 XML·Editor 로그·packages-lock·build를 출력 경로에 보존합니다.

집계 중단 후에는 `-ResumeFrom <출력 경로>`로 같은 고정 후보를 재개할 수 있습니다. 후보 Git 소스가 변경됐다면 중단하며, 기존 성공 XML·빌드·lock·manifest를 재확인한 후 완료한 버전은 재실행하지 않습니다. 수정한 working tree로 새 후보를 확인할 때는 ResumeFrom을 사용하지 않습니다. Windows 경로 제한을 피하도록 출력 폴더에는 짧은 이름을 사용합니다.

후보 검증은 공개 GitHub tag 설치나 HTTPS 접근 성공을 의미하지 않습니다. 공개 발행 후 해당 ref의 재검증이 필요합니다. 임시 Git 저장소의 commit은 검증 fixture이며 배포 브랜치 commit과 별개입니다.

## 검증 결과

2026-10-01에 고정 후보 `87b22f6fcbb97c3989d53958e898567b95418e0a`를 Git clone으로 설치해 검증했습니다. 두 버전 모두 실제 package cache에서 1.0.0·빈 dependency와 lock의 정확한 commit hash를 확인했습니다.

| Unity | 코어 Play Mode | Save Recovery Lab Play Mode | Windows Mono build |
| --- | --- | --- | --- |
| 2022.3.62f3 | 18/18 통과 | 26/26 통과 | 코어·샘플 모두 성공 |
| 6000.3.20f1 | 18/18 통과 | 26/26 통과 | 코어·샘플 모두 성공 |

검증 기록은 `C:/Users/search/AppData/Local/Temp/UTP-6b7467da94bf/release-validation.json`에 보존했습니다. 같은 폴더에 고정 소스 manifest와 버전별 테스트 XML·로그·lock·빌드가 있습니다. 하위 결과의 Source=Remote는 Git 설치 시나리오 이름이며 최상위 Source=Candidate가 실제 소스의 성격입니다. 공개 GitHub tag 검증 결과가 아닙니다.

README의 두 C# 예제 블록은 별도 임시 Assets에 넣어 Unity 2022.3.62f3에서 컴파일 성공을 확인했습니다. 예제 동작을 직접 실행한 결과는 아닙니다. 정적 패키지 검사 4개와 정적 검사기 회귀 테스트 17개도 통과했습니다.

새 코어 회귀 검사는 파일 없음의 출력, 잘못된 생성 인자, migration 버전 overflow 거절, 저장 거절·임시 파일 쓰기 실패의 본문/백업 보존, 로드 전 미래 버전 저장 거절, 호환 로드 후 보호 해제, migration 배열 복사, 개발 버전 envelope 호환성을 확인합니다. 샘플 테스트는 여섯 fixture와 버튼·구독 수명을 확인합니다.

Windows Editor/Mono 검증 범위입니다. Android/iOS·IL2CPP·WebGL·동시 writer·전원 손실·실제 기기 강제 종료 복구는 검증하지 않았습니다. 기존 `verify-release.ps1 -Release`는 UI/Timer 중심 보안 검사이며 SSH origin을 HTTPS 전용으로 제한하므로 별도 gate로 취급합니다. 이를 Persistence 공개 설치 검증 결과로 대신하지 않습니다.

### 2026-10-06 후보 재검수

- 현재 Persistence 파일 57개의 SHA-256이 10월 1일 고정 소스 기록과 모두 일치합니다.
- `-ResumeFrom`으로 두 Unity 버전의 코어·샘플 테스트 XML, 빌드 성공 기록·실행 파일, lock과 manifest를 재확인했습니다. Play Mode와 빌드를 새로 실행한 결과는 아닙니다.
- 네 프로젝트 각각의 설치 파일 57개를 고정 소스와 대조했습니다. 개행과 Unity manifest 처리 차이를 제외한 내용이 모두 일치합니다.
- 설치된 `Runtime/VersionedSaveStore.cs`와 `package.json`을 각각 임시 변경해 검증 실패를 확인했습니다. 원본 바이트와 파일 속성을 복원한 후 전체 재검증이 통과했습니다.
- 정적 UPM 검사 4개 패키지와 검사기 회귀 테스트 17개를 새로 실행해 통과했습니다.
- 이번 보완은 검증 스크립트와 이 문서에 한정합니다. 런타임 API·저장 format·serialized field·GUID 변경은 없습니다. 공개 tag 설치와 모바일·IL2CPP 검증은 여전히 남아 있습니다.

## 발행 순서

1. 변경 범위와 runtime API·저장 format 유지 여부를 검수합니다.
2. 후보 변경을 배포 브랜치에 commit/push하고 정적 CI를 확인합니다.
3. 실제 발행 시 package README·CHANGELOG·루트 README의 후보 표기를 정식 릴리스로 갱신합니다.
4. 확정 commit에 `unitytools-persistence/v1.0.0` tag와 Persistence 전용 Release를 만듭니다. UI·Timer tag를 이동하거나 전체 develop의 무관한 변경을 포함하지 않습니다.
5. 공개 tag를 대상으로 다음 검증을 실행합니다.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/test-persistence-release.ps1 -Source Remote
```

Remote 모드는 tag의 실제 commit을 먼저 조회하고, 고정 HTTPS Git URL로 두 버전의 코어·샘플 검증을 반복합니다. 공개 tag가 없으면 검증을 중단하며 다른 ref로 자동 대체하지 않습니다. 이후 README 설치 링크·공개 검증 기록을 확정합니다.

## Release 본문 초안

### UnityTools Persistence 1.0.0

- Unity 2022.3 이상용 독립 로컬 저장 UPM 패키지
- 순차 schema migration, 최종 데이터 검증, SHA-256 손상 검출
- 백업 복구와 미래 버전 본문의 구버전 덮어쓰기 방지
- Save Recovery Lab: 입력/출력 파일과 여섯 보호 조건 실험
- API·동기 실행·단일 writer·지원 범위 문서 및 MIT 라이선스
- 0.1.0 API/envelope와 호환; 패키지 업그레이드 자체의 migration 불필요

설치 주소는 tag 발행 후 패키지 README의 고정 Git URL을 사용합니다. Windows Mono 외 플랫폼, 암호화·보상 transaction·동시 writer는 지원 검증에 포함하지 않습니다. 실제 공개 tag 검증 결과는 발행 후 추가합니다.
