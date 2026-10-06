# Button Input

현재 개발 소스의 기능입니다. 기존 공개 UI tag에 포함되지 않습니다. uGUI 이외의 tween·Timer·Input System 의존성을 추가하지 않습니다.

## 반복 버튼

새 UI GameObject에 `UiRepeatButton`을 추가합니다. 이 타입은 `UnityEngine.UI.Button`을 상속하며 기존 `Button`을 대체합니다. 같은 GameObject에 두 Button 컴포넌트를 함께 붙이지 않습니다. Image를 Target Graphic에 명시적으로 연결하고, 기존 `On Click()`에 작업을 연결합니다. 반복 요청은 한 번씩 호출되므로 재화·네트워크 등의 중복 요청 제어는 호출자가 처리합니다.

- 짧은 입력은 PointerUp 다음 PointerClick에서 한 번 호출됩니다.
- Initial Delay Sec을 넘겨 누르면 첫 반복을 호출합니다. 이후 Interval Sec부터 Acceleration을 곱해 간격을 줄이고 Min Interval Sec에서 멈춥니다.
- 반복을 시작했다면 놓을 때 추가 click을 호출하지 않습니다. `RepeatCnt`는 현재/마지막 입력에서 발생한 반복 횟수이며 짧은 클릭은 포함하지 않습니다.
- 시간은 `Time.unscaledTimeAsDouble`입니다. game time scale이 0이어도 동작합니다. 한 프레임에 최대 한 번 호출하고 느린 프레임의 미실행 횟수를 몰아서 호출하지 않습니다.
- PointerExit, Cancel, Deselect, 비활성화, interactable/CanvasGroup 차단, 앱 포커스 손실과 pause에서 멈춥니다. 포인터 이탈 후 재진입만으로 재시작하지 않습니다. 새 PointerDown이 필요합니다.
- 첫 번째 왼쪽 포인터의 ID를 소유합니다. 다른 포인터와 오른쪽 클릭은 반복을 시작하거나 해제하지 못합니다. EventSystem의 Submit은 기본 Button처럼 한 번 호출합니다.

Inspector의 Repeat Timing에서 기본값 0.6초 / 0.3초 / 0.05초 / 0.85를 조정합니다. Min Interval은 Interval 이하여야 합니다. 런타임 변경은 `TryConfigure(initialDelaySec, intervalSec, minIntervalSec, acceleration)`로 검증합니다. false면 기존 설정과 입력 상태를 유지하고, true면 진행 중인 입력을 취소하고 교체합니다. interval은 최소 0.01초, acceleration은 0.1~1.0입니다. 1.0은 가속 없는 반복입니다. `CancelPress()`는 입력과 다음 click을 취소하며 스케일 컴포넌트의 연출은 별도 API로 정리합니다.

`onClick`은 기본 Button 계약을 사용합니다. 일반 C# 알림 event를 추가하거나 이를 상태 알림으로 바꾸지 않습니다. 동적으로 구독했다면 소유자의 비활성화/Release에서 동일한 delegate로 해제하세요.

## 눌림 스케일

`UiButtonPressScale`을 일반 Button 또는 UiRepeatButton과 같은 GameObject에 추가합니다. 자신의 Button과 RectTransform을 Awake에서 캐싱합니다. 자식·부모·전역 검색이나 DOTween을 사용하지 않습니다.

- 왼쪽 PointerDown에서 현재 스케일을 보관하고 Pressed Scale 배율로 보간합니다. PointerUp/Exit에서 복원합니다.
- 복원 중 다시 누르면 처음 원본 스케일을 유지해 반복 입력의 축소 누적을 방지합니다. 복원이 끝난 이후에는 다음 입력 시 새로운 스케일을 읽습니다.
- 비활성화, Button/CanvasGroup 차단, Cancel/Deselect, 포커스 손실·pause는 즉시 원본으로 복원합니다.
- `TryConfigure(pressedScale, durationSec)`는 배율 0.1~1.0, 0 이상의 유한한 시간을 받습니다. 정상 설정은 기존 연출을 정리합니다. `ResetImmediate()`로 명시적으로 복원할 수 있습니다.
- 입력 처리 순서는 기존 EventSystem을 따릅니다. keyboard/gamepad Submit에는 스케일 연출을 붙이지 않습니다. 다른 Animator/tween/layout 코드가 같은 localScale을 동시에 변경하면 연출 소유권이 충돌합니다. 같은 RectTransform의 스케일은 하나의 시스템에서 관리하세요.

사운드·진동은 이번 버전에 포함하지 않습니다. 플랫폼별 구현은 게임 쪽 Button.onClick에 연결합니다. component 활성 상태가 바뀌면 해당 연출이 정리되고, Inspector의 기존 serialized field·GUID rename은 없습니다.

## 샘플과 검증

Package Manager에서 **Button Input Sample**을 Import합니다. 샘플은 명시적으로 연결된 UI 참조와 StandaloneInputModule을 사용합니다. Input System 전용 프로젝트에서는 프로젝트에 맞는 입력 모듈로 교체합니다. `Tools > UnityTools > Create Button Input Sample`은 Assets/ButtonInputSample 장면을 다시 생성합니다. 변경한 샘플은 다른 경로에 보관하세요.

`tools/test-button-input.ps1`은 새 프로젝트에 로컬 UI 패키지를 설치하고 Button Input Sample을 Import합니다. 연결된 참조·전용 Inspector·모든 UI Play Mode 테스트와 Windows Development build를 검사하고 Player에서 일반 클릭, 반복, release 중복 방지, 스케일 복원, 포인터 이탈을 실행합니다. 결과는 TEMP 프로젝트의 XML·로그·summary.json·player-result.txt에 보존합니다. 모바일 실제 터치와 실물 기기·Input System 모듈의 실제 포인터 입력은 별도 검증입니다.

### 2026-10-06 실행 결과

| Unity | UI Play Mode | Package Manager 샘플 Import | Windows Development |
| --- | --- | --- | --- |
| 2022.3.62f3 | 69/69 통과, skip 0 | 성공, 참조·Inspector 확인 | 빌드·Player 실행 통과 |
| 6000.3.20f1 | 69/69 통과, skip 0 | 성공, 참조·Inspector 확인 | 빌드·Player 실행 통과 |

새 버튼 회귀 테스트 12개를 포함합니다. Player는 Time.timeScale 0 상태에서 일반 클릭·가속 반복·release 중복 방지·스케일 복원·포인터 이탈을 ExecuteEvents로 실행했습니다. Editor assembly는 Player에 포함되지 않았습니다. 실제 마우스/터치 장치로 UI 레이아웃과 raycast를 수동 조작한 검증은 별도이며 자동 EventSystem 테스트와 구분합니다. 정적 배포 검사 6개 패키지와 도구 회귀 테스트 21개가 통과했고 Button Input 샘플 14개 파일의 저장소 사본도 일치합니다.
