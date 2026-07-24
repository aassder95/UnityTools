# UnityTools UI Framework

Unity 2022.3 이상에서 사용할 수 있는 uGUI 기반 UI 패키지입니다.

## 포함 기능

- Model / View / Presenter 수명주기와 숨은 화면 갱신 병합
- Screen / Popup / Overlay 스택, Back 처리, 모달 입력 차단, UI 포커스 복원
- `CanvasGroup` 기반 Show / Hide 전환과 전환 중 입력 잠금
- 오브젝트 풀 기반 가상화 동적 스크롤과 항목 단위 증분 갱신
- 세로 / 가로, 고정 개수 / 자동 맞춤, Start / Center / End 이동 정렬
- 해상도·회전 변경을 추적하는 모바일 Safe Area 적용
- Task / Period 타이머와 샘플 UI 매니저
- DynamicScrollView 전용 Inspector

## 설치

Unity Package Manager의 `Add package from git URL...`에서 다음 주소를 사용합니다.

```text
https://github.com/aassder95/UnityTools.git?path=/UnityTools/Packages/com.aassder95.unitytools.ui#develop
```

현재 저장소는 인증 없이 접근할 수 없으므로 Unity를 실행하기 전에 GitHub 저장소 접근 권한을 Git에 구성해야 합니다. 인증이 없는 배치 또는 CI 환경에서는 Git URL 설치가 실패합니다.

로컬 개발 프로젝트에서는 `Packages/com.aassder95.unitytools.ui` 임베디드 패키지로 바로 로드됩니다.

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
navigator.HandleBack(); // Popup을 먼저 닫고, 그 다음 Screen을 Pop합니다.
```

View에 `UiCanvasTransition`을 붙이고 `TransitionView<TModel>`을 상속하면 Presenter의 `Show` / `Hide`와 Fade 전환이 연결됩니다. `UiBackInput`의 Back Action을 Inspector에서 연결한 뒤 조합 루트에서 `Init(navigator)`를 호출하면 키보드, 게임패드, 모바일 Back 액션을 같은 탐색 흐름으로 전달할 수 있습니다.

## DynamicScroll 증분 갱신

전체 목록을 다시 만들지 않고 현재 보이는 항목만 갱신하거나, 삽입·제거 전의 화면 앵커를 유지할 수 있습니다.

```csharp
rankScroll.RefreshItem(changedIdx);
rankScroll.RefreshRange(startIdx, changedCnt);
rankScroll.InsertItems(insertIdx, addedCnt);
rankScroll.RemoveItems(removeIdx, removedCnt);
rankScroll.UpdateItemCnt(totalCnt);
rankScroll.ScrollTo(targetIdx, alignment: EDynamicScrollAlignment.Center);
```

`InsertItems`와 `RemoveItems`는 기본적으로 현재 첫 보이는 항목의 위치를 보존합니다. 데이터 컬렉션을 먼저 변경한 뒤 동일한 인덱스와 개수를 스크롤에 전달합니다.

## Safe Area와 포커스

- `UiSafeAreaFitter`의 Target에 Safe Area를 적용할 `RectTransform`을 명시적으로 연결합니다.
- 가로 또는 세로 적용을 끄면 해당 축의 기존 Anchor와 Offset을 유지합니다.
- `UiFocusScope`에는 Scene의 `EventSystem`과 화면의 기본 `Selectable`을 연결합니다.
- Popup이나 Screen이 닫히면 `UiNavigator`가 이전 선택을 복원합니다.

## 샘플

Package Manager에서 `UI Sample Scene`을 Import하면 Inventory, Rank, Timer 샘플과 필요한 프리팹 및 폰트가 `Assets/Samples` 아래에 복사됩니다. Sample Lobby와 각 모듈 이동은 `UiNavigator` 화면 스택으로 동작하며, 외부 플러그인 없이 컴파일됩니다.

## 런타임 어셈블리

- `UnityTools.Util`: UI 프레임워크와 공용 런타임 유틸리티
- `UnityTools.Manager`: 타이머 매니저
- `UnityTools.Util.Editor`: DynamicScrollView와 데이터 도구 Inspector
