# UI 2.2.0 / Timer 1.2.0 릴리스

2026-10-07 발행용 기록입니다. UI 2.2.0 / Timer 1.2.0의 설치 주소와 변경 기록을 갱신했습니다. 기존 UI 2.1.0 / Timer 1.1.0 tag는 유지합니다. 아래 후보 검증 기록은 발행 이전 검증을 설명하며 공개 tag 검증 결과는 문서 끝에 별도로 기록합니다.

## 포함 범위

- UI 2.2.0: UiRepeatButton의 가속 반복·입력 취소·다중 포인터 처리, UiButtonPressScale의 unscaled 눌림 연출, Button Input Sample.
- Timer 1.2.0: 작업·주기 서비스의 TryUnregister, 작업의 TryCancel과 선택적 ICancellableTaskTimer, Timer Simulation Lab의 여덟 실험.
- 샘플은 일시정지·하루 뒤 복원·재개, 취소·수령 여부·재시작, 등록 해제·UTC 경과·재등록과 여섯 저장 실패를 비교합니다.
- 기존 공개 method, ITaskTimer, enum 숫자를 유지합니다. 취소는 기존 v1 snapshot을 사용하고 수령 표시를 false로 저장합니다. 정지 snapshot v2의 구버전 downgrade 제한은 동일합니다.
- TimerLabController에 버튼 참조 세 개를 추가하고 장면을 두 줄 버튼 배치로 갱신했습니다. 패키지 샘플·프로젝트 사본을 함께 갱신하며 기존 .meta GUID는 유지합니다.
- Benchmark·Persistence·VFX·Sheets와 데모 프로젝트 설정의 다른 로컬 변경은 이번 후보에 포함하지 않습니다.

## 검증

```powershell
& tools/verify-release.ps1 -Candidate -UiVersion 2.2.0 -TimerVersion 1.2.0
& tools/test-upm-compatibility.ps1 -Source Local -UnityVersion 2022.3.62f3 -Scenarios timer-lab,ui,ui-input
& tools/test-upm-compatibility.ps1 -Source Local -UnityVersion 6000.3.20f1 -Scenarios timer-lab,ui,ui-input
```

로컬 설치 검증은 공개 Git tag 설치 검증과 다릅니다. UI 코어·Input System 조합·Timer 코어와 실험실 테스트, 샘플 장면 참조, Windows Mono Development build를 확인합니다. 버튼 이벤트 호출과 텍스트 높이 검사는 OS 마우스·터치 조작이나 실제 기기 화면 검증을 의미하지 않습니다. 모바일·IL2CPP는 별도 검증 대상입니다.

### 2026-10-07 결과

| 시나리오 | Unity 2022.3.62f3 | Unity 6000.3.20f1 | Windows Mono Development Build |
| --- | --- | --- | --- |
| Timer 코어 + Timer Simulation Lab | 46/46 | 46/46 | 두 버전 성공 |
| UI 단독 | 69/69 | 69/69 | 두 버전 성공 |
| UI + Input System·샘플 Import | 70/70 | 70/70 | 두 버전 성공 |

모든 테스트의 skip은 0입니다. Timer 장면은 기존 오브젝트 ID 94개를 유지한 최종 파일로 테스트·빌드를 다시 통과했습니다. 세 버튼의 Inspector 참조와 모든 버튼의 구독 해제, 결과 패널 텍스트 높이를 검사했습니다. Unity 2022.3에서 1440×900 렌더 이미지 네 장(저장 실패·정지/재개·취소/재시작·등록 해제/복원)을 확인했습니다. 후보 보안 검사, 6개 패키지 UPM 정적 검사, 샘플 사본 검사와 기존 릴리스 검사 회귀 테스트 9개도 통과했습니다.

XML·로그·build·lock·summary는 다음 임시 프로젝트에 보존합니다.

- `C:/Users/search/AppData/Local/Temp/UnityTools-Compatibility-2022.3.62f3-233599b441644f50affe786eb53036cd`
- `C:/Users/search/AppData/Local/Temp/UnityTools-Compatibility-6000.3.20f1-4e8be209fdf44490bb42894c5c76b627`
- 렌더 이미지: `C:/Users/search/AppData/Local/Temp/ut-timer-lab-scene-589d822d12764fb0920464570ef89f07`

후보 검증 스크립트는 일반·괄호형 Unreleased 및 버전 명시 후보 헤더를 인식하도록 갱신했고 PowerShell 구문 검사를 통과했습니다. 이 실행의 소스는 로컬 파일 참조이며 공개 Git 설치 결과가 아닙니다.

## 발행 순서

1. 후보 변경 범위 검수와 Unity 결과를 확정하고 해당 변경만 커밋합니다.
2. 고정 후보 commit을 빈 프로젝트에 Git URL로 설치해 cache 버전·lock hash·테스트·빌드를 확인합니다. `tools/test-ui-timer-release.ps1 -SourceRef <후보 commit> -UiVersion 2.2.0 -TimerVersion 1.2.0`을 사용합니다. 이 스크립트는 커밋된 소스만 복사하므로 현재 미커밋 변경은 포함하지 않습니다. 일반 Unreleased와 버전이 명시된 후보 변경 기록을 모두 인식합니다.
3. 변경 기록의 Unreleased를 발행 날짜로 변경하고 안정 설치 주소·버전 표를 갱신합니다.
4. 검증한 commit에 unitytools-ui/v2.2.0, unitytools-timer/v1.2.0 tag를 발행하고 공개 tag 설치를 재검증합니다.
5. GitHub release에 결과와 한계를 기록합니다. 기존 tag는 이동하지 않습니다.

2026-10-07 공개 tag·GitHub release 발행을 완료했습니다. 아래 공개 tag 검증 결과를 참고하세요.

## 고정 Git 후보 검증

2026-10-07: fc6ce8e0d48df4254e639f5b59a552d2de110fdb를 GitHub Git URL로 빈 프로젝트에 설치했습니다. Unity 2022.3.62f3 / 6000.3.20f1 각각 UI 69/69, UI+Input System 70/70, Timer 35/35, Timer Lab 46/46, skip 0 및 네 Windows Mono Development build가 통과했습니다. 여덟 프로젝트의 lock hash·source=git와 cache의 UI 2.2.0 / Timer 1.2.0을 검사했습니다.

결과 경로:
- C:/Users/search/AppData/Local/Temp/UnityTools-Compatibility-2022.3.62f3-3ca2430c15b74a50a37c704467fd81de
- C:/Users/search/AppData/Local/Temp/UnityTools-Compatibility-6000.3.20f1-e35a4293f9754664a8f7878554bafd07

발행 commit은 이 후보와 runtime·Editor·sample·test·manifest·meta가 동일하며 설치 문서와 변경 기록만 갱신합니다.

## 공개 tag 검증 및 발행 결과

2026-10-07: UI `unitytools-ui/v2.2.0`, Timer `unitytools-timer/v1.2.0`은 모두 `cf83ef4377611d7c80a78544da34be3f2f60b33d`를 가리킵니다. 기존 태그는 이동하지 않았습니다.

빈 프로젝트에 공개 tag Git URL로 설치하여 Unity 2022.3.62f3 / 6000.3.20f1 각각 UI 69/69, UI+Input System 70/70, Timer 35/35, Timer Lab 46/46 테스트와 네 Windows Mono Development build를 통과했습니다. 모든 skip은 0이며 여덟 프로젝트의 lock source=git·정확한 commit hash·cache 버전(UI 2.2.0 / Timer 1.2.0)을 확인했습니다. 발행 커밋의 GitHub 정적 CI도 통과했습니다.

- [UI 2.2.0 GitHub release](https://github.com/aassder95/UnityTools/releases/tag/unitytools-ui/v2.2.0)
- [Timer 1.2.0 GitHub release](https://github.com/aassder95/UnityTools/releases/tag/unitytools-timer/v1.2.0)
- 2022.3 결과: `C:/Users/search/AppData/Local/Temp/UnityTools-Compatibility-2022.3.62f3-2a0d56bde1f3465d99f38ef884e10f3e`
- 6.3 결과: `C:/Users/search/AppData/Local/Temp/UnityTools-Compatibility-6000.3.20f1-c8f12f64a15a4a818e094a05ecbecc95`

Runtime·Editor·샘플·테스트·manifest·meta는 검증한 고정 후보와 동일합니다. 새 `[TEST]` 로그는 없습니다. 모바일 기기 조작과 IL2CPP는 이번 검증에 포함하지 않았습니다.
