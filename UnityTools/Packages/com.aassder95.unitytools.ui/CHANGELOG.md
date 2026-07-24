# Changelog

## [2.0.0] - 2026-07-24

### Added

- Screen, Popup, Overlay 레이어와 Back 순서, 모달 입력 차단, 포커스 복원을 관리하는 `UiNavigator`
- `CanvasGroup` transition과 전환 중 입력 차단을 제공하는 `UiCanvasTransition`, `TransitionView<TModel>`
- 선택적 `UnityTools.Ui.InputSystem` assembly의 `UiBackInput`
- 화면 회전과 해상도 변경을 반영하는 `UiSafeAreaFitter`
- DynamicScroll 보이는 항목·범위 증분 갱신, anchor 보존 삽입·제거, Start/Center/End 이동

### Changed

- runtime assembly와 공개 namespace를 `UnityTools.Ui`로 변경
- 숨겨진 View의 모델 갱신을 다음 `Show`까지 병합
- `IDynamicScrollItem`에 `OnGet`, `OnReturn` pool 수명주기 통합
- pool, deque, index 보조 구현을 UI 내부 책임으로 변경
- 기본 UI package에서 Timer와 Input System 필수 dependency 제거
- UI sample을 Inventory와 Rank navigation 예제로 제한

### Removed

- `UnityTools.Util.*` 호환 API
- 범용 `IPoolable`, `ObjectPool`, `Spawner`
- `MonoSingleton`, `EventDispatcher`, 전역 logging, 범용 Utility
- Timer sample과 Timer runtime

## [1.0.0] - 2026-07-22

- MVP 기반 UI 수명주기 package
- 가상화 DynamicScrollView와 전용 Inspector
- Inventory, Rank, Timer sample
