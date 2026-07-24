# UnityTools UI Framework

Unity 2022.3 이상에서 사용하는 uGUI 기반 UI 패키지입니다.

## 포함 기능

- Model / View / Presenter 수명주기와 숨은 화면 갱신 병합
- Screen / Popup / Overlay navigation, Back 처리, 모달 입력 차단, 포커스 복원
- `CanvasGroup` 기반 transition과 전환 중 입력 잠금
- pool을 내부 구현으로 사용하는 DynamicScroll 증분 갱신
- 세로 / 가로, Start / Center / End 정렬 이동
- 해상도와 회전 변경을 추적하는 Safe Area
- DynamicScrollView 전용 Inspector

Timer, 범용 singleton, logging, persistence, pooling API는 포함하지 않습니다.

## 설치

Unity Package Manager의 `Add package from git URL...`에서 다음 주소를 사용합니다.

```text
https://github.com/aassder95/UnityTools.git?path=/UnityTools/Packages/com.aassder95.unitytools.ui#unitytools-ui/v2.0.0
```

런타임 assembly는 `UnityTools.Ui`, Editor assembly는 `UnityTools.Ui.Editor`입니다.

## UI 탐색

`UiNavigator`는 전역 Singleton이 아닌 일반 C# 객체입니다. 조합 루트에서 생성한 뒤 Presenter와 선택적인 입력·포커스 제어 객체를 `UiNavigationEntry`로 묶습니다.

```csharp
UiNavigator navigator = new();

UiNavigationEntry home = new(homePresenter, homeTransition, homeFocus);
UiNavigationEntry settings = new(settingsPresenter, settingsTransition, settingsFocus);
UiNavigationEntry confirm = new(confirmPresenter, confirmTransition, confirmFocus, true);

navigator.PushScreen(home);
navigator.PushScreen(settings);
navigator.OpenPopup(confirm);
navigator.HandleBack();
```

## 선택적 Input System

기본 UI package는 Input System을 요구하지 않습니다. Back action이 필요하면 프로젝트에 `com.unity.inputsystem`을 추가하고 `UnityTools.Ui.InputSystem` assembly를 참조합니다. `UiBackInput.Init(navigator)`로 Back action을 같은 navigation 흐름에 연결할 수 있습니다.

## DynamicScroll

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

## 샘플

Package Manager에서 `UI Sample Scene`을 Import하면 Inventory와 Rank navigation 예제가 복사됩니다. Timer sample은 Timer package에 별도로 포함됩니다.

## Migration과 라이선스

`UnityTools.Util.*` 호환 shim은 제공하지 않습니다. [migration 문서](https://github.com/aassder95/UnityTools/blob/unitytools-ui/v2.0.0/MIGRATION.md)를 따라 assembly와 namespace를 변경하세요.

이 package의 자체 코드는 [MIT License](https://github.com/aassder95/UnityTools/blob/unitytools-ui/v2.0.0/LICENSE)로 배포됩니다.
