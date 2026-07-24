# Changelog

## [1.0.0] - 2026-07-24

### Added

- 독립 `UnityTools.Timer` assembly
- `TaskTimerService`, `PeriodTimerService`와 handle/data/interface
- 생성자에서 주입하는 coroutine runner, `IStorage`, UTC provider
- 기본 `PlayerPrefsStorage`
- static `Instance`가 없는 선택적 `TimerHost`
- UI dependency가 없는 독립 Timer Sample Scene

### Changed

- Task/Period Timer 상태 전이를 각 Timer 내부 책임으로 제한
- 저장 성공 후에만 runtime 상태를 변경
- Task Timer clock rollback 보정의 중복 가산 방지
- 같은 ID를 등록할 때 이전 handle의 event와 coroutine 해제

### Removed

- singleton Timer Manager
- 범용 `CoroutineHelper`, StateMachine, Utility
- 범용 serializer, file storage, persistence facade
