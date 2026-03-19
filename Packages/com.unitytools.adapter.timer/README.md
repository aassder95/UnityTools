# UnityTools Adapter Timer

`UnityTools.Core`의 타이머 기능을 현재 프로젝트 구조에 연결하는 Adapter 패키지입니다.

## 포함 범위

- Runtime
  - `TimerManager`
  - `Singletons` (TimerManager 등록 전용)
  - `DebugLogBootstrap`
- Editor
  - `TaskTimerTestWindow`
  - `PeriodTimerTestWindow`
  - `StorageDebugValueReader`

## 역할

- Core 패키지는 순수 재사용 로직을 제공
- Adapter 패키지는 프로젝트 실행/디버그 흐름에 맞는 통합 코드를 제공
