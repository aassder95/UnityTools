# UI Sample Scene

## 요구 사항

- Unity 2022.3 이상
- TextMeshPro 3.0.7
- Input System 1.14.0

## 실행

1. Package Manager에서 `UI Sample Scene`을 Import합니다.
2. `Scenes/SampleScene.unity`를 엽니다.
3. Play Mode에 진입합니다.
4. Sample Lobby에서 Inventory, Rank, Timer를 선택합니다.
5. 각 화면의 Refresh, Random, Timer 테스트 버튼을 실행합니다.
6. 화면 아래 Back 버튼으로 Lobby로 돌아옵니다.

## 확인할 프레임워크 흐름

- `UiManager`가 `UiNavigator`를 생성하고 Sample Lobby를 첫 Screen으로 등록합니다.
- Sample 선택 시 모듈 Presenter가 Screen 스택에 Push됩니다.
- Back 버튼은 `UiNavigator.TryHandleBack()`을 호출해 현재 모듈을 닫고 Lobby를 복원합니다.
- `SampleModuleBase`가 `IPresenter` 수명주기와 표시 상태 계약을 구현합니다.
