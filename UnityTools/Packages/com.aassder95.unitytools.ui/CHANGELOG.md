# Changelog

## [Unreleased]

### Changed

- `IStorage`와 `Persistence`의 저장 API를 실패 여부가 드러나는 `TrySave`, `TryLoad`, `TryHasKey`, `TryDelete` 계약으로 변경
- Timer 생성·초기화·강제 전환·저장 상태 조회 API를 `TryCreate`, `TryInit`, `TryForceOpen`, `TryForceClosed`, `TryGetClaimed` 계약으로 통일
- Timer 생성 시 저장소와 UTC 시계 함수를 주입할 수 있게 하고 저장 성공 후에만 상태를 전이하도록 변경
- Task Timer 시계 역행 보정 시 trusted update 시각을 함께 저장해 같은 역행 구간의 중복 가산을 방지
- Period Timer 삭제도 영속 데이터 삭제 성공 후에만 런타임 handle을 해제하도록 순서를 보장
- `Deque`의 제거·조회 API를 빈 컬렉션에서도 예외가 발생하지 않는 `TryDequeue`, `TryDequeueBack`, `TryPeek`, `TryPeekBack` 계약으로 변경
- `TaskTimer`와 `PeriodTimer`의 상태 구현과 `Fsm` 노출을 제거하고 상태 전이를 Timer 내부 책임으로 제한
- 샘플 전용 UIManager, SampleLauncher, SampleModule 타입을 Runtime 어셈블리에서 Samples 전용 영역으로 이동
- `MonoSingleton`이 누락 인스턴스를 씬 검색으로 자동 복구하지 않고 `Awake` 등록만 사용하도록 변경
- FileStorage 키를 단일 파일명으로 제한하고 루트 경로 이탈을 차단
- `ObjectPool.TryReturn` 소유권 검증과 Spawner 활성 객체 상한·반환 수명주기를 추가
- Rank 행 배경 Image의 serialized field 이름을 `_imgBackground`로 명확히 하고 관련 prefab 연결을 함께 갱신
- OSA 의존 Rank 예제를 독립 `OSA Rank Sample`과 전용 어셈블리로 분리해 기본 UI 샘플이 OSA 없이 컴파일되도록 변경

## [1.0.0] - 2026-07-22

- MVP 기반 UI 수명주기 패키지화
- 가상화 DynamicScrollView와 전용 Inspector 포함
- Inventory, Rank, Rank OSA, Timer 샘플 제공
- 모바일 해상도 대응 패널 레이아웃 및 텍스트 대비 개선 반영
