# Changelog

## [2.1.0] - 2026-07-24

### Added

- Screen, Popup, Overlay 레이어를 분리하고 Back 순서, 모달 입력 차단, 포커스 복원을 관리하는 `UiNavigator`
- `CanvasGroup` 기반 전환과 전환 중 입력 차단을 제공하는 `UiCanvasTransition`, `TransitionView<TModel>`
- Input System의 Back 액션을 `UiNavigator`에 연결하는 `UiBackInput`과 선택 포커스를 저장·복원하는 `UiFocusScope`
- 화면 회전과 해상도 변경에 맞춰 지정한 축의 앵커만 갱신하는 `UiSafeAreaFitter`
- DynamicScroll의 보이는 항목·범위 증분 갱신, 항목 삽입·제거 시 앵커 보존, Start/Center/End 정렬 이동
- MVP 수명주기, 내비게이션, 전환, DynamicScroll, Safe Area를 검증하는 PlayMode 테스트

### Changed

- 숨겨진 View는 모델 갱신마다 렌더링하지 않고 다음 `Show`에서 최신 모델을 한 번만 반영
- 기본 UI Sample Scene의 Lobby와 모듈 이동을 `UiNavigator` 화면 스택 예제로 변경

## [2.0.0] - 2026-07-24

### Changed

- `IStorage`와 `Persistence`의 저장 API를 실패 여부가 드러나는 `TrySave`, `TryLoad`, `TryHasKey`, `TryDelete` 계약으로 변경
- Timer 생성·초기화·강제 전환·저장 상태 조회 API를 `TryCreate`, `TryInit`, `TryForceOpen`, `TryForceClosed`, `TryGetClaimed` 계약으로 통일
- Timer 생성 시 저장소와 UTC 시계 함수를 주입할 수 있게 하고 저장 성공 후에만 상태를 전이하도록 변경
- Task Timer 시계 역행 보정 시 trusted update 시각을 함께 저장해 같은 역행 구간의 중복 가산을 방지
- Period Timer 삭제도 영속 데이터 삭제 성공 후에만 런타임 handle을 해제하도록 순서를 보장
- `Deque`의 제거·조회 API를 빈 컬렉션에서도 예외가 발생하지 않는 `TryDequeue`, `TryDequeueBack`, `TryPeek`, `TryPeekBack` 계약으로 변경
- `TaskTimer`와 `PeriodTimer`의 상태 구현과 `Fsm` 노출을 제거하고 상태 전이를 Timer 내부 책임으로 제한
- `UnityTools.Util.UiFramework` namespace를 `UnityTools.Util.UiFramework`로 변경
- 샘플 전용 `UiManager`, `SampleLauncher`, `SampleModule` 타입을 Runtime 어셈블리에서 Samples 전용 영역으로 이동
- `MonoSingleton`이 누락 인스턴스를 씬 검색으로 자동 복구하지 않고 `Awake` 등록만 사용하도록 변경
- FileStorage 키를 단일 파일명으로 제한하고 루트 경로 이탈을 차단
- `ObjectPool.TryReturn` 소유권 검증과 Spawner 활성 객체 상한·반환 수명주기를 추가
- Rank 행 배경 Image의 serialized field 이름을 `_imgBackground`로 명확히 하고 관련 prefab 연결을 함께 갱신
- UPM 샘플 Import 시 Unity meta parser 오류가 발생하지 않도록 `.meta` 파일의 EOF 개행 형식을 정규화

## [1.0.0] - 2026-07-22

- MVP 기반 UI 수명주기 패키지화
- 가상화 DynamicScrollView와 전용 Inspector 포함
- Inventory, Rank, Timer 샘플 제공
- 모바일 해상도 대응 패널 레이아웃 및 텍스트 대비 개선 반영
