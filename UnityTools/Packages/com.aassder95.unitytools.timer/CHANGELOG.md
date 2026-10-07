# Changelog

## [1.2.0] - 2026-10-07

- Timer Simulation Lab에 정지·재개, 취소·재시작, 개별 해제·복원 실험과 버튼 추가
- 여섯 저장 실패의 상태 보존 및 모든 실험 버튼 구독 해제 검증 추가

- 진행·일시정지 작업을 수령 없이 취소하는 TryCancel과 선택적 ICancellableTaskTimer 추가
- 취소 저장 실패 시 상태 유지와 취소 후 복원·재시작 검증 추가

- 작업·주기 서비스에 저장 데이터를 유지하는 개별 등록 해제 TryUnregister 추가
- 등록 해제 시 이벤트·coroutine 정리와 재등록 복원 검증 추가

## [1.1.0] - 2026-10-06

- 작업 타이머의 일시정지·재개와 오프라인 정지 상태 복원 추가
- 기존 상태 숫자·ITaskTimer 계약·v1 저장 호환 유지, 정지 snapshot v2와 저장 실패 회귀 검증 추가

- 작업·주기 서비스의 등록 개수와 전체 데이터 사본 조회 추가
- 등록·교체·삭제·해제 및 시간·상태·수령 변경을 알리는 OnTimersChanged 추가
- 저장 실패 시 변경 알림과 기존 상태·사본 보존 회귀 테스트 추가

- Timer Simulation Lab 샘플에 가상 UTC·오프라인 복원·역행·저장 실패·수령 상태 실험 추가
- 실험 상태·저장 snapshot과 장면 버튼 구독 수명 검증 추가

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
