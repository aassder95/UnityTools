# UI 2.1.0 / Timer 1.1.0 릴리스 후보

현재 원본 manifest에 UI 2.1.0 / Timer 1.1.0 미게시 후보를 반영했습니다. 예정 tag는 `unitytools-ui/v2.1.0`, `unitytools-timer/v1.1.0`이며 이 주소를 현재 설치 가능한 공개 tag로 안내하지 않습니다. 기존 UI 2.0.0 / Timer 1.0.0 설치는 유지합니다.

## 포함 범위

- UI: 가변 높이 DynamicScroll, Canvas 전환 완료·취소 결과, 특정 Popup 닫기, 보상 아이콘 이동 연출, UI Feature Demo와 네 화면 비율의 raycast 검증.
- Timer: 등록 목록 사본·개수·변경 알림, 작업 타이머 일시정지·재개와 Timer Simulation Lab.
- 공개 API 추가에 따른 minor 후보입니다. 기존 공개 method·ITaskTimer 계약·enum 숫자를 유지합니다. Paused=3과 선택적 IPausableTaskTimer를 추가합니다. Inspector field rename은 없습니다.
- 기존 v1·4키 저장 데이터를 읽습니다. 정지 상태만 남은 시간을 포함한 snapshot v2를 저장하고 재개 후 v1로 돌아갑니다. 정지 데이터를 가진 상태의 이전 패키지로의 downgrade는 지원하지 않습니다. 기존 데이터를 일괄 변환하는 migration은 없습니다.
- Benchmark·Persistence·VFX·Sheets는 별도 릴리스입니다. 현재 working tree의 다른 작업은 후보에 포함하지 않습니다.

## 재현 가능한 후보 검증

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/test-ui-timer-release.ps1 -SourceRef 44c7225
```

이번 후보는 `44c7225e7be53251631b32a9ac7cded668fa0b75`로 고정합니다. 보상 연출과 작업 타이머 정지·재개를 포함합니다. SourceRef를 생략하면 실행 시점의 HEAD를 사용하므로 포함 범위가 달라질 수 있습니다. 원본 index·branch·tag·manifest는 변경하지 않습니다. 커밋된 UI/Timer 폴더를 임시 Git 저장소로 복사하고 **그 사본에서만** version·문서 URL·미게시 변경 기록을 후보 버전으로 갱신합니다. 원본 파일을 덮어쓰지 않으며 기존 패키지의 dependency·GUID를 유지합니다.

두 Unity 버전에서 후보 commit의 Git FILE URL을 빈 프로젝트에 설치합니다.

| 시나리오 | 검증 |
| --- | --- |
| ui | UI 단독, Input System 없이 코어 테스트와 Windows Mono build |
| ui-input | Input System 조합, UI Sample Scene Import·장면 검사·코어 테스트·build |
| timer | Timer Sample Scene Import·코어 테스트·build |
| timer-lab | Timer Simulation Lab Import·샘플과 코어 테스트·build |

패키지 cache의 실제 version과 lock의 `source=git`·정확한 후보 hash를 검사합니다. 임시 경로의 `release-validation.json`, candidate Git 저장소, 프로젝트별 XML·Editor 로그·lock·build를 보존합니다. 기능 데모 버튼 검증은 샘플 README의 별도 Play Mode 증거를 함께 확인합니다. 후보 검증은 공개 GitHub tag 설치 검증이나 실제 모바일·IL2CPP 검증을 대신하지 않습니다.

## 2026-10-06 최신 후보 검증 결과

소스 `44c7225`를 UI 2.1.0 / Timer 1.1.0 후보로 검증했습니다. 두 버전에서 Git 설치·sample 장면 검사·Play Mode·Windows Development Build(Mono)가 모두 통과했습니다. 실제 package version과 Git lock hash도 확인했습니다.

| 시나리오 | Unity 2022.3.62f3 | Unity 6000.3.20f1 |
| --- | --- | --- |
| ui | 57/57, build 성공 | 57/57, build 성공 |
| ui-input | 58/58, build 성공 | 58/58, build 성공 |
| timer | 28/28, build 성공 | 28/28, build 성공 |
| timer-lab | 36/36, build 성공 | 36/36, build 성공 |

보고서·고정 후보·XML·로그·설치 lock·build는 아래에 보존했습니다. 두 보고서의 SourceCommit은 동일합니다. 후보 저장소는 실행별로 분리됩니다.

- `C:/Users/search/AppData/Local/Temp/ut-release-6291e8af` — Unity 2022.3
- `C:/Users/search/AppData/Local/Temp/ut-release-aaa64bc8` — Unity 6

고정 소스 사본의 패키지 정적 검사 5개와 validator 회귀 테스트 19개도 통과했습니다. 증거는 `C:/Users/search/AppData/Local/Temp/ut-static-fac5ef78/report.json`과 같은 폴더의 소스 사본입니다. 다른 working tree 변경은 이 사본에 포함하지 않았습니다.

UI Feature Demo는 두 버전에서 Play Mode 2/2·Windows build를 별도로 통과했습니다. 1280×720, 1920×1080, 720×1280, 2560×1080에서 버튼 중심 좌표가 화면 안에 있고 GraphicRaycaster가 해당 버튼을 선택하는지 검사한 뒤 EventSystem pointer down/up/click을 전달했습니다. CanvasScaler Expand를 장면과 생성기에 반영했으며 기존 GUID와 참조를 유지했습니다. Unity 6의 네 렌더 이미지도 확인했습니다. 세로 화면에서는 데모 전체가 축소되며 모바일 전용 배치는 아닙니다.

샘플 README의 검증 프로젝트 경로에 `screen-results.xml`, `screen-tests.log`, `screen-build.log`가 있습니다. Unity 6 프로젝트에는 `screen-capture.xml`, `screen-capture.log`, `screen-{width}x{height}.png`도 보존했습니다. 캡처 코드는 임시 검증 프로젝트에서만 사용했고 배포 소스에 추가하지 않았습니다.

VFX는 별도 패키지로 두 버전에서 Edit Mode 12/12·skip 0, 미리보기 픽셀 검사와 Windows build를 다시 통과했습니다. Player에 Editor assembly가 제외되는 것도 확인했습니다. 결과는 `C:/Users/search/AppData/Local/Temp/UnityTools-Vfx-2022.3.62f3-3216623f65764146a27f89b3e11b81f8`과 `C:/Users/search/AppData/Local/Temp/UnityTools-Vfx-6000.3.20f1-1af5996310404349ae0b4674b881d954`의 summary.json·XML·로그에 보존했습니다.

OS 마우스·기기 터치·Safe Area·모바일·IL2CPP는 미검증입니다. CI workflow와 실행 스크립트는 develop에 푸시했지만 등록된 GitHub runner는 0개이며 실제 job·artifact 업로드를 실행하지 않았습니다. 기본 branch인 main 반영과 격리된 실행 PC·라이선스 준비가 필요합니다. 연결 상태는 [CI 안내](CI.md)를 확인하세요. 공개 tag와 실제 manifest version은 아직 발행하지 않았습니다.

## 2026-10-06 이전 후보 검증 결과

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

이전 후보는 보상 연출과 정지·재개를 포함하지 않습니다. 최신 후보와 별도로 보존합니다. 원본 manifest의 버전과 공개 tag는 변경하지 않았습니다.

## 후보의 실제 소스 반영

manifest의 version과 세 문서 URL, 통합한 후보 changelog는 앞서 검증한 `ut-release-6291e8af/candidate` 사본과 일치합니다. 후보 기록은 `Unreleased`로 유지하고 공개 설치 안내는 기존 tag를 사용합니다. 패키지 README에 이 구분을 명시했습니다. 원본의 runtime·Editor·샘플·테스트·meta는 검증 소스 `44c7225` 이후 변경하지 않았습니다. 이 단계에서는 Unity 테스트를 반복하지 않았습니다.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/verify-release.ps1 -Candidate -UiVersion 2.1.0 -TimerVersion 1.1.0
python -m unittest discover -s tools/tests -p test_verify_release.py
```

실제 소스의 후보 보안 검사와 모든 샘플 사본 검사가 통과했습니다. 회귀 테스트 9개는 SSH/HTTPS, credential 거절과 출력 보호, 버전·문서 URL·변경 기록 불일치, 검사 mode 충돌, 기존 Release 기본 버전 호환, 새 Release 설치 안내 요구를 검사합니다. 격리된 Git fixture에서 실행하며 원본 Git 설정은 변경하지 않습니다.

`-Candidate`는 미게시 변경 기록과 후보 문서 URL을 검사합니다. `-Release`는 설치 안내를 포함한 기존 공개 준비 검사를 유지하며 원하는 버전을 인자로 지정할 수 있습니다. 기본값은 기존 UI 2.0.0 / Timer 1.0.0입니다. 두 mode는 함께 사용할 수 없습니다. 이 정적 검사는 공개 tag 존재나 원격 Git 설치 성공을 보장하지 않습니다.

## 발행 전 순서

1. 후보 source commit과 실제 포함 기능을 확정합니다. 추가 기능이 합쳐졌으면 새 후보로 재검증합니다.
2. manifest·후보 changelog 반영은 완료했습니다. 발행 시 `Unreleased`를 실제 발행 날짜로 변경합니다.
3. 원본 repo의 staged 범위·diff check·커밋 메시지와 배포 보안 검사를 확인합니다. 후보는 위의 Candidate mode를 사용합니다. 공개 설치 안내 갱신 후에는 `verify-release.ps1 -Release -UiVersion 2.1.0 -TimerVersion 1.1.0`으로 검사합니다.
4. 릴리스 commit을 푸시한 후 새 tag를 발행합니다. 이 준비 작업에서는 tag를 만들지 않습니다.
5. 공개 Git URL을 대상으로 빈 프로젝트 설치·테스트·build를 재실행한 뒤 설치 안내를 새 tag로 바꿉니다.
