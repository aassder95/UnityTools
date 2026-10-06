# Changelog

## Unreleased

- unscaled 대기·가속 반복·release click 중복 방지·다중 포인터 소유권을 제공하는 UiRepeatButton 추가
- 포인터 눌림 스케일·빠른 재입력·비활성화/포커스 손실 복원을 제공하는 UiButtonPressScale 추가
- Button Input Sample과 전용 Inspector·입력/수명주기 회귀 테스트 추가

## [2.1.0] - 2026-10-06

- 명시적인 좌표계·Motion asset·아이콘 재사용·도착/완료/취소 알림을 제공하는 UiRewardFlyer 추가
- Reward Flyer Sample 장면·prefab·설정 asset과 좌표 변환·수명주기 회귀 테스트 추가

- 가변 높이·전환 결과·특정 Popup 닫기를 시연하는 UiFeatureDemo 장면과 Editor 생성기 추가

- 지정한 Popup만 닫는 TryClosePopup overload와 하위 Popup 제거 시 활성 포커스 보존

- Canvas fade 요청별 완료 대기 Task와 Completed/Cancelled 결과 추가
- 요청 교체·명시적 취소·비활성화·파괴 시 대기 종료와 입력 상태 정리

- 세로 단일 열 DynamicScroll의 가변 높이 초기화·변경·삽입 API 추가
- 누적 높이 기반 표시 범위 검색과 높이·목록 변경 시 anchor 보존
- 기존 항목 재사용과 고정 크기 목록 복귀 회귀 테스트 추가

## [2.0.0] - 2026-07-24

### Added

- Screen, Popup, Overlay 레이어와 Back 순서, 모달 입력 차단, 포커스 복원을 관리하는 `UiNavigator`
- `CanvasGroup` transition과 전환 중 입력 차단을 제공하는 `UiCanvasTransition`, `TransitionView<TModel>`
- 선택적 `UnityTools.Ui.InputSystem` assembly의 `UiBackInput`
- 화면 회전과 해상도 변경을 반영하는 `UiSafeAreaFitter`
- DynamicScroll 보이는 항목·범위 증분 갱신, anchor 보존 삽입·제거, Start/Center/End 이동

### Changed

- runtime assembly와 공개 namespace를 `UnityTools.Ui`로 변경
- `UiNavigator`의 실패 가능한 상태 변경 API를 `TryXxx` 반환 계약으로 통일
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
