# UnityTools UI Framework

현재 버전은 2.2.0입니다. 아래 공개 tag로 고정 설치할 수 있습니다. 포함 범위와 검증 기록은 [릴리스 안내](../../../docs/RELEASE_UI_TIMER_NEXT.md)를 참고하세요.

Unity 2022.3 이상에서 사용하는 uGUI 기반 UI 패키지입니다.

## 포함 기능

- Model / View / Presenter 수명주기와 숨은 화면 갱신 병합
- Screen / Popup / Overlay navigation, Back 처리, 모달 입력 차단, 포커스 복원
- `CanvasGroup` 기반 transition과 전환 중 입력 잠금
- pool을 내부 구현으로 사용하는 DynamicScroll 증분 갱신
- 세로 / 가로, Start / Center / End 정렬 이동
- 해상도와 회전 변경을 추적하는 Safe Area
- 짧은 클릭·길게 누르기·가속 반복을 제공하는 `UiRepeatButton`과 unscaled 눌림 연출 `UiButtonPressScale`
- DynamicScrollView 전용 Inspector
- 보상 아이콘의 분산·HUD 이동·내부 재사용을 제공하는 `UiRewardFlyer`

보상 연출의 Inspector 연결과 API 계약은 [Reward Flyer 안내](Documentation~/reward-flyer.md)를 참고하세요. Package Manager의 **Reward Flyer Sample**을 Import하거나, 데모 프로젝트의 `Assets/RewardFlyerSample/RewardFlyerSample.unity`를 열어 실행합니다. 이 기능은 현재 버전에만 있으며 기존 `unitytools-ui/v2.0.0` tag에는 포함되지 않습니다.

Timer, 범용 singleton, logging, persistence, pooling API는 포함하지 않습니다.

## 설치

Unity Package Manager의 `Add package from git URL...`에서 다음 주소를 사용합니다.

```text
https://github.com/aassder95/UnityTools.git?path=/UnityTools/Packages/com.aassder95.unitytools.ui#unitytools-ui/v2.2.0
```

런타임 assembly는 `UnityTools.Ui`, Editor assembly는 `UnityTools.Ui.Editor`입니다.

## UI 탐색

`UiNavigator`는 전역 Singleton이 아닌 일반 C# 객체입니다. 조합 루트에서 생성한 뒤 Presenter와 선택적인 입력·포커스 제어 객체를 `UiNavigationEntry`로 묶습니다.

```csharp
UiNavigator navigator = new();

UiNavigationEntry home = new(homePresenter, homeTransition, homeFocus);
UiNavigationEntry settings = new(settingsPresenter, settingsTransition, settingsFocus);
UiNavigationEntry confirm = new(confirmPresenter, confirmTransition, confirmFocus, true);

bool isHomeShown = navigator.TryPushScreen(home);
bool isSettingsShown = isHomeShown && navigator.TryPushScreen(settings);
bool isConfirmOpened = isSettingsShown && navigator.TryOpenPopup(confirm);
bool isBackHandled = isConfirmOpened && navigator.TryHandleBack();
```

현재 버전에서는 `TryClosePopup(entry)`로 등록했던 `UiNavigationEntry`의 팝업만 닫을 수 있습니다. 위에 열린 다른 팝업은 유지하며, 남은 모달 팝업에 따라 화면 입력을 다시 계산합니다. 아래 팝업을 닫을 때 활성 팝업·오버레이의 포커스를 다시 설정하지 않습니다. 맨 위 팝업을 닫으면 기존 방식으로 포커스를 복원합니다. null·미등록·이미 닫은 entry는 상태 변경이나 알림 없이 false를 반환하고, 성공하면 `OnChanged`를 한 번 호출합니다.

기존 `TryClosePopup()`과 Back은 맨 위 팝업을 닫습니다. 동일 Presenter로 만든 새 entry가 아니라 등록에 사용한 entry를 전달하세요. 이 overload는 기존 `unitytools-ui/v2.0.0` tag에 포함되지 않습니다.

2026-10-06, Local UPM 설치로 Unity `2022.3.62f3`과 `6000.3.20f1` 각각 Play Mode 43/43 및 Windows Development Build(Mono)를 통과했습니다. 특정 팝업 닫기의 회귀 검증 5건이 포함됩니다. 직접 화면 조작·모바일·IL2CPP 검증은 포함하지 않습니다.

## 전환 완료 대기와 취소

현재 버전의 `UiCanvasTransition.ShowAsync()`와 `HideAsync()`는 `Task<EUiTransitionResult>`를 반환합니다. 기존 `Show()`·`Hide()`와 동일한 fade 경로를 사용하고, 완료 시 `Completed`, 요청 교체·명시적 취소·비활성화·파괴 시 `Cancelled`를 반환합니다. 기존 `unitytools-ui/v2.0.0` tag에는 포함되지 않습니다.

```csharp
EUiTransitionResult result = await transition.ShowAsync();
if (result == EUiTransitionResult.Cancelled)
    return;

// 전환 후 로딩 화면이나 다음 동작을 이어갑니다.
```

- `IsTransitioning`으로 진행 여부를 확인하고 `CancelTransition()`으로 현재 fade를 중단합니다. 명시적 취소는 직전 요청의 목표 화면 상태로 즉시 맞춥니다. Show 취소는 alpha=1, Hide 취소는 alpha=0과 비활성화입니다. 결과는 Cancelled이며 이전 화면 상태로 되돌리지 않습니다.
- 새 Show/Hide 요청은 기존 대기를 Cancelled로 끝내고 현재 alpha에서 새 전환을 시작합니다. 요청별 Task가 있어 이전 요청을 기다리던 코드가 새 요청의 완료를 성공으로 오인하지 않습니다.
- component·GameObject·부모 비활성화와 파괴는 진행 중 대기를 끝내고 화면 입력을 해제합니다. 비활성화 중인 component 또는 부모 아래 Show 요청은 Cancelled, 이미 비활성인 화면의 Hide 요청은 Completed입니다. 부모는 자동 활성화하지 않습니다.
- duration이 0이면 즉시 완료합니다. fade는 unscaled time을 사용하며 진행 중에는 입력을 차단하고 완료 후 기존 `SetInteractionEnabled` 정책을 적용합니다.
- Unity 메인 스레드에서 호출하고 await합니다. `.Wait()`·미완료 Task의 `.Result`로 Unity 스레드를 막지 않습니다. 취소 결과는 예외가 아니며 CancellationToken은 받지 않습니다.
- 이 API는 Canvas fade의 완료만 나타냅니다. `UiNavigator`와 Presenter의 화면 stack·모델 수명은 기존 동기 API가 소유하므로 취소가 navigation을 되돌리거나 새로 등록하지 않습니다.

## 선택적 Input System

기본 UI package는 Input System을 요구하지 않습니다. Back action이 필요하면 프로젝트에 `com.unity.inputsystem`을 추가하고 `UnityTools.Ui.InputSystem` assembly를 참조합니다. `UiBackInput.Init(navigator)`로 Back action을 같은 navigation 흐름에 연결할 수 있습니다.

## DynamicScroll

현재 버전는 **가변 높이의 세로 단일 열 목록**을 지원합니다. 아래 API는 기존 `unitytools-ui/v2.0.0` tag에 포함되지 않습니다. 항목의 높이를 데이터에서 계산해 전달하며 기존 `InitView(int)`는 고정 크기 목록을 유지합니다.

```csharp
float[] heights = { 80.0f, 160.0f, 100.0f, 240.0f };
if (!scrollView.TryInitView(heights))
    return;

bool isResized = scrollView.TrySetItemHeight(1, 200.0f);
bool isInserted = scrollView.TryInsertItems(0, new float[] { 120.0f, 180.0f });
scrollView.RemoveItems(0, 2);
```

- `TryInitView(IReadOnlyList<float>)`는 높이를 복사해 초기화합니다. 가로 목록, null, 0 이하·NaN·무한 높이, 음수 세로 간격과 표현 가능한 범위를 넘는 전체 높이는 거절합니다. 실패 시 기존 높이와 항목 수를 유지합니다.
- `TrySetItemHeight`와 높이 목록을 받는 `TryInsertItems`는 기본적으로 첫 표시 항목의 화면 위치를 보존합니다. 제거된 anchor는 다음 항목으로 이동하며 마지막 항목 제거·목록 끝에서는 스크롤 범위로 제한합니다. 위치 보존을 끄려면 `shouldPreserveAnchor: false`를 전달합니다.
- 기존 개수 기반 `InsertItems`와 `UpdateItemCnt`로 추가되는 항목에는 prefab의 기본 높이를 사용합니다. 빈 목록, `InitView(int)`로의 고정 크기 복귀와 `ReleaseView` 후 재초기화를 지원합니다.
- 항목 위치는 누적 높이로 계산하고 표시 범위는 이진 검색으로 찾습니다. 스크롤에서는 기존 pool/deque를 재사용합니다. 표시 항목 수가 pool 용량을 넘으면 추가 객체가 생성될 수 있습니다.
- 높이·개수 변경은 배열 복사와 누적 높이 재계산으로 O(n)이며 매 프레임 호출에 적합하지 않습니다. 텍스트 높이 자동 측정과 가변 높이 다중 열·가로 목록은 제공하지 않습니다. 텍스트·화면 폭 변경 후 호출자가 높이를 다시 전달합니다.
- `ScrollRect` content의 LayoutGroup/ContentSizeFitter가 이 배치와 크기를 덮어쓰지 않도록 구성합니다. 항목 크기와 위치는 이 스크롤이 소유합니다.

item은 `IDynamicScrollItem`을 구현하며 pool 입출고 수명주기를 직접 받습니다.

```csharp
public void OnGet()
{
    gameObject.SetActive(true);
}

public void OnReturn()
{
    gameObject.SetActive(false);
}
```

보이는 항목만 갱신하거나 삽입·제거 전 화면 anchor를 유지할 수 있습니다.

```csharp
rankScroll.RefreshItem(changedIdx);
rankScroll.RefreshRange(startIdx, changedCnt);
rankScroll.InsertItems(insertIdx, addedCnt);
rankScroll.RemoveItems(removeIdx, removedCnt);
rankScroll.ScrollTo(targetIdx, alignment: EDynamicScrollAlignment.Center);
```

버튼 입력과 연출의 사용법은 [Button Input 안내](Documentation~/button-input.md)를 참고하세요. **Button Input Sample**을 Import하거나 `Assets/ButtonInputSample/ButtonInputSample.unity`를 실행합니다. 새 버튼 기능은 unitytools-ui/v2.2.0부터 포함되며 기존 2.1.0 이하 tag에는 포함되지 않습니다.

## 샘플

Package Manager에서 `UI Sample Scene`을 Import하면 Inventory와 Rank navigation 예제가 복사됩니다. Timer sample은 Timer package에 별도로 포함됩니다.

현재 버전의 `Scenes/UiFeatureDemo.unity`에서는 가변 높이 목록, 전환 완료·취소 결과, 특정 팝업 닫기를 버튼으로 시연합니다. `Tools > UnityTools > Create UI Feature Demo`로 같은 장면을 새 위치에 생성할 수 있습니다. [실행 순서와 확인 사항](Samples~/UI%20Sample%20Scene/README.md)을 참고하세요.

저장소에서 UI sample C#을 변경할 때는 `Samples~`와 `Assets/Samples`를 함께 갱신하고 `tools/verify-sample-mirrors.ps1`로 일치 여부를 확인합니다. 두 위치의 asmdef는 Timer 참조 때문에 의도적으로 다릅니다.

## Migration과 라이선스

`UnityTools.Util.*` 호환 shim은 제공하지 않습니다. [migration 문서](https://github.com/aassder95/UnityTools/blob/unitytools-ui/v2.0.0/MIGRATION.md)를 따라 assembly와 namespace를 변경하세요.

이 package의 자체 코드는 [MIT License](https://github.com/aassder95/UnityTools/blob/unitytools-ui/v2.0.0/LICENSE)로 배포됩니다.
## 토스트 알림 큐 (개발 버전)

`UiToastQueue`의 CanvasGroup과 Text를 Inspector에 명시적으로 연결합니다. `TryEnqueue(key, message, durationSec)`로 알림을 넣으면 FIFO 순서로 표시하고 unscaled 시간으로 숨깁니다. 현재 표시 중이거나 대기 중인 같은 key는 거절하며, 표시 종료 후에는 key를 재사용할 수 있습니다. 대기 개수 상한에 도달하면 false를 반환합니다. `TryDismiss()`는 현재 메시지를 닫고 다음 메시지를 표시하고, `Clear()`와 비활성화는 전체 대기를 정리합니다.

입력을 차단하지 않으며 `OnShown`·`OnDismissed`로 표시 상태를 알립니다. 이벤트 구독자는 해제 책임을 가집니다. 알림은 보상 지급·저장 성공의 증거가 아니며 호출자는 Try 반환값을 처리해야 합니다. 공개 UI 2.2.0에는 포함되지 않습니다.

### UiSafeArea (개발 브랜치)

Canvas 바로 아래의 Safe Area RectTransform을 `_rtSafeArea`에 명시적으로 연결합니다. `Screen.safeArea`를 정규화한 anchor로 적용하고 화면 크기/여백 변경 시에만 갱신합니다. 자식 UI의 배치나 CanvasScaler 정책은 변경하지 않습니다. `TryApplyViewport`로 합성 viewport를 검증할 수 있으며 잘못된 크기/NaN/화면 밖 영역은 이전 배치를 보존하고 거절합니다.
