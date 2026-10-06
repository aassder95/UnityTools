# UI 2.1.0 / Timer 1.1.0 릴리스 후보

현재는 미게시 후보입니다. 예정 tag는 `unitytools-ui/v2.1.0`, `unitytools-timer/v1.1.0`이며 이 주소를 현재 설치 가능한 공개 tag로 안내하지 않습니다. 기존 UI 2.0.0 / Timer 1.0.0 설치는 유지합니다.

## 포함 범위

- UI: 가변 높이 DynamicScroll, Canvas 전환 완료·취소 결과, 특정 Popup 닫기, UI Feature Demo.
- Timer: 등록 목록 사본·개수·변경 알림과 Timer Simulation Lab.
- 공개 API 추가에 따른 minor 후보입니다. 기존 공개 method와 저장 snapshot 형식은 유지하며 serialized field rename이나 migration은 추가하지 않습니다.
- Benchmark와 Persistence는 별도 릴리스입니다. 커밋되지 않은 보상 연출 등 현재 working tree 변경은 후보에 포함하지 않습니다.

## 재현 가능한 후보 검증

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/test-ui-timer-release.ps1 -SourceRef 7d73c81
```

이번 후보는 `7d73c81`로 고정합니다. 이후 추가된 보상 연출 커밋은 이 후보에 포함되지 않습니다. SourceRef를 생략하면 실행 시점의 HEAD를 사용하므로 포함 범위가 달라질 수 있습니다. 원본 index·branch·tag·manifest는 변경하지 않습니다. 커밋된 UI/Timer 폴더를 임시 Git 저장소로 복사하고 **그 사본에서만** version·문서 URL·미게시 변경 기록을 후보 버전으로 갱신합니다. 원본 파일을 덮어쓰지 않으며 기존 패키지의 dependency·GUID·저장 형식을 유지합니다.

두 Unity 버전에서 후보 commit의 Git FILE URL을 빈 프로젝트에 설치합니다.

| 시나리오 | 검증 |
| --- | --- |
| ui | UI 단독, Input System 없이 코어 테스트와 Windows Mono build |
| ui-input | Input System 조합, UI Sample Scene Import·장면 검사·코어 테스트·build |
| timer | Timer Sample Scene Import·코어 테스트·build |
| timer-lab | Timer Simulation Lab Import·샘플과 코어 테스트·build |

패키지 cache의 실제 version과 lock의 `source=git`·정확한 후보 hash를 검사합니다. 임시 경로의 `release-validation.json`, candidate Git 저장소, 프로젝트별 XML·Editor 로그·lock·build를 보존합니다. 기능 데모 버튼 검증은 샘플 README의 별도 Play Mode 증거를 함께 확인합니다. 후보 검증은 공개 GitHub tag 설치 검증이나 실제 모바일·IL2CPP 검증을 대신하지 않습니다.

## 2026-10-06 후보 검증 결과

소스 `7d73c81`에서 만든 UI 2.1.0 / Timer 1.1.0 후보를 검증했습니다. 두 버전에서 모든 시나리오의 Git 설치·sample 장면 검사·Play Mode·Windows Development Build(Mono)가 통과했습니다.

| 시나리오 | Unity 2022.3.62f3 | Unity 6000.3.20f1 |
| --- | --- | --- |
| ui | 43/43, build 성공 | 43/43, build 성공 |
| ui-input | 44/44, build 성공 | 44/44, build 성공 |
| timer | 20/20, build 성공 | 20/20, build 성공 |
| timer-lab | 28/28, build 성공 | 28/28, build 성공 |

Windows PowerShell의 JSON 배열을 중첩 배열로 집계하던 오류를 제거했습니다. 완료된 Unity 실행을 반복하지 않고 최종 집계 코드로 XML·Player 존재·설치된 이름/버전·Git lock hash·시나리오 누락/중복을 재검증했습니다. 정적 배포 검사와 validator 회귀 테스트 17개도 통과했습니다.

검증 보고서와 고정 후보는 아래 경로에 보존했습니다. 각 `release-validation.json`에는 원본·후보 commit, version, 시나리오별 프로젝트 위치와 미게시 상태가 있습니다.

- `C:/Users/search/AppData/Local/Temp/ut-release-afe4131e` — Unity 2022.3
- `C:/Users/search/AppData/Local/Temp/ut-release-f385b030` — Unity 6

원본 manifest의 버전과 공개 tag는 변경하지 않았습니다. 보상 연출 등 후속 커밋을 포함해 발행하려면 그 소스로 새 후보를 검증해야 합니다.

## 발행 전 순서

1. 후보 source commit과 실제 포함 기능을 확정합니다. 추가 기능이 합쳐졌으면 새 후보로 재검증합니다.
2. 확정한 manifest·changelog를 실제 소스에 반영하고 문서의 개발 소스/기존 tag 구분을 새 릴리스에 맞춥니다. 후보의 `Unreleased`는 실제 발행 날짜로 변경합니다.
3. 원본 repo의 staged 범위·diff check·커밋 메시지와 배포 보안 검사를 확인합니다. 기존 `verify-release.ps1 -Release`는 UI 2.0.0 / Timer 1.0.0 계약이므로 그대로 새 후보 검증에 사용하지 않습니다.
4. 릴리스 commit을 푸시한 후 새 tag를 발행합니다. 이 준비 작업에서는 tag를 만들지 않습니다.
5. 공개 Git URL을 대상으로 빈 프로젝트 설치·테스트·build를 재실행한 뒤 설치 안내를 새 tag로 바꿉니다.
