# UnityTools Timer

현재 버전은 1.1.0입니다. 아래 공개 tag로 고정 설치할 수 있습니다.

## 등록 목록 조회와 변경 알림

현재 버전의 `TaskTimerService`와 `PeriodTimerService`는 `TimerCnt`, `GetSnapshots()`, `OnTimersChanged`를 제공합니다. 기존 `unitytools-timer/v1.0.0` tag에는 포함되지 않습니다.

- `TimerCnt`는 초기화에 성공해 서비스에 등록된 타이머 수입니다. 생성만 한 handle과 저장소에만 남은 타이머는 포함하지 않습니다.
- `GetSnapshots()`는 등록 목록을 데이터 사본 배열로 반환합니다. 배열과 각 데이터는 호출자가 보관할 수 있으며 이후 시간·목록 변경이 기존 사본을 바꾸지 않습니다. 순서는 보장하지 않으며 `Id`로 항목을 연결합니다.
- 작업 타이머 사본에는 상태·남은 초·전체 초·진행률, 주기 타이머 사본에는 기존 `PeriodTimerData`의 ID와 상태가 포함됩니다. 주기 타이머 남은 시간은 해당 handle에서 조회합니다.
- `OnTimersChanged`는 등록·동일 ID 교체·등록된 주기 타이머 삭제·비어 있지 않은 서비스 해제와 기존 handle의 남은 시간·상태 알림 및 작업 타이머 수령을 전달합니다. 생성만 하거나 저장 실패로 동작이 거절되면 목록 변경 알림을 추가하지 않습니다.
- 하나의 동작이 남은 시간과 상태 알림을 모두 발생시키면 여러 번 호출될 수 있습니다. 알림은 즉시 발생하므로 화면에서는 dirty flag를 설정하고 필요한 시점에 한 번 조회합니다. 이벤트 안에서 서비스나 handle을 변경하지 않습니다.
- 배열과 DTO를 생성하는 조회는 매 프레임 호출에 적합하지 않습니다. 저장소 전체 검색, 일괄 실행·수령·저장 transaction은 제공하지 않습니다. 구독자는 자신의 Release/OnDisable에서 구독을 해제합니다.

목록 화면은 처음 열 때 `GetSnapshots()`로 구성하고, 이후 `OnTimersChanged`에 등록한 명명된 callback에서 갱신을 예약합니다. 기존 개별 타이머 이벤트와 API도 유지됩니다.

Unity 2022.3 이상에서 사용하는 UTC 기반 Task / Period Timer package입니다.

## 설치

Unity Package Manager의 `Add package from git URL...`에서 다음 주소를 사용합니다.

```text
https://github.com/aassder95/UnityTools.git?path=/UnityTools/Packages/com.aassder95.unitytools.timer#unitytools-timer/v1.1.0
```

runtime assembly는 `UnityTools.Timer`이며 UI package에 의존하지 않습니다.

## Service 구성

`TaskTimerService`와 `PeriodTimerService`는 일반 C# 객체입니다. coroutine runner, 저장소, UTC provider를 조합 루트에서 명시적으로 전달합니다.

```csharp
IStorage storage = new PlayerPrefsStorage();
TaskTimerService taskTimers = new TaskTimerService(runner, storage, () => DateTime.UtcNow);

taskTimers.TryCreate("BUILD", out TaskTimerHandle handle);
taskTimers.TryInit(handle);
taskTimers.TryStart("BUILD", 60.0d);
```

같은 ID를 다시 등록하면 service가 이전 handle의 event와 coroutine을 해제하고 새 handle을 소유합니다. `Release`는 service가 소유한 모든 handle을 정리합니다.

## 개별 등록 해제 (개발 버전)

`TaskTimerService.TryUnregister(id)`와 `PeriodTimerService.TryUnregister(id)`는 등록된 타이머 하나의 coroutine과 이벤트 연결을 정리하고 서비스 목록에서 제외합니다. 성공하면 `OnTimersChanged`를 한 번 호출하며, 빈 ID·미등록 ID·중복 해제는 false입니다. 저장소를 읽거나 쓰지 않으므로 저장소 장애 중에도 해제할 수 있습니다.

저장 데이터와 UTC 일정은 유지됩니다. 작업의 시간을 멈추려면 `TryPause`를 사용하고, 주기 저장 데이터를 삭제하려면 기존 `TryDelete`를 사용합니다. 해제한 handle은 `TryInit`으로 다시 등록할 수 있으며 그 사이의 실제 경과 시간을 반영해 복원합니다. 해제한 handle의 외부 구독은 구독자가 직접 정리합니다. 이 API는 공개 `unitytools-timer/v1.1.0` tag에 포함되지 않습니다.

## 선택적 TimerHost

MonoBehaviour 수명주기가 필요한 프로젝트는 `TimerHost`를 사용할 수 있습니다.

```csharp
timerHost.Init(storage, () => DateTime.UtcNow);
TaskTimerService taskTimers = timerHost.TaskTimers;
timerHost.Release();
```

`TimerHost`는 static `Instance`를 제공하지 않습니다.

## 작업 타이머 일시정지·재개

현재 버전의 `TaskTimerService.TryPause(id)` / `TryResume(id)` 및 `TaskTimerHandle`의 같은 method로 처리 중인 작업을 정지·재개합니다. `Paused=3`을 추가했고 기존 None/Processing/Completed 숫자는 유지합니다. 정지 중에는 남은 시간·진행률이 고정되고 coroutine이 종료됩니다. 오프라인 시간과 정지 중 시계 역행은 남은 시간을 소비하지 않습니다. 재개 시 현재 UTC를 기준으로 종료 시각을 다시 구성합니다. EndTime은 정지 상태의 실시간 종료 예정 시각으로 사용하지 않습니다.

정지·재개는 저장 성공 후에만 상태와 알림을 변경합니다. 중복 요청·다른 상태·미등록 ID·저장 실패는 false이며 기존 상태를 유지합니다. 정지 상태에서 시작·감소·강제 완료·수령은 거절합니다. 상태 변화는 기존 OnStateTransition과 서비스 OnTimersChanged로 전달합니다.

기존 `ITaskTimer` 구현 계약은 유지합니다. 선택적 `IPausableTaskTimer` 역할을 구현한 타이머만 handle에서 정지·재개할 수 있습니다. UTC 주기 경계에 따른 Period Timer에는 일시정지를 추가하지 않습니다.

정지 snapshot은 `2|startTicks|durationSec|3|updatedTicks|0|remainingSec`입니다. 기존 v1·4키 저장 데이터를 읽으며, 정지하지 않은 상태와 재개 후에는 기존 v1 형식을 씁니다. 정지 snapshot은 구버전 패키지에서 읽을 수 없으므로 저장 상태가 정지인 채로 downgrade하지 않습니다. 읽을 수 없는 snapshot을 덮어쓰지 않습니다. 기존 데이터의 일괄 변환은 필요하지 않습니다.

## 작업 취소 (개발 버전)

`TaskTimerService.TryCancel(id)`와 `TaskTimerHandle.TryCancel()`은 Processing·Paused 작업을 취소해 None 상태로 돌립니다. 서비스 등록은 유지하며 새 작업을 시작할 수 있습니다. 완료·수령 알림은 발생하지 않고 기존 상태 변경 및 서비스 `OnTimersChanged`로 취소를 알립니다. None·Completed 상태, 미등록 ID, 저장 실패는 false이며 기존 상태를 유지합니다. 완료된 보상을 버리는 기능은 포함하지 않습니다.

취소는 수령 표시가 false인 기존 v1 snapshot을 저장한 후 coroutine·시간·진행률을 정리합니다. 취소 후 재접속해도 작업은 되살아나지 않으며 `TryGetClaimed`는 false를 반환합니다. 기존 `ITaskTimer` 계약은 유지하고, 선택적 `ICancellableTaskTimer`를 구현한 타이머만 handle에서 취소할 수 있습니다. Period Timer에는 취소를 추가하지 않습니다. 공개 `unitytools-timer/v1.1.0` tag에는 이 API가 포함되지 않습니다.

## Persistence

- 외부 저장소는 `IStorage`로 주입합니다.
- 기본 구현은 `PlayerPrefsStorage`입니다.
- 절대 시각은 UTC ticks로 저장합니다.
- 저장 성공 후에만 runtime 상태를 전이합니다.
- Task Timer는 clock rollback 보정을 한 번만 반영합니다.
- 새 상태는 타이머별 단일 snapshot 키에 저장합니다. 기존 4키 데이터는 snapshot이 없을 때 읽으며, 이후 상태 변경부터 새 형식으로 저장합니다.
- `PeriodTimerService.TryDelete`는 snapshot에 삭제 상태를 기록합니다. 이전 형식의 키는 호환성을 위해 그대로 두지만, 삭제 상태가 우선되어 타이머가 다시 살아나지 않습니다.

## 샘플과 라이선스

개발 checkout에는 **Timer Simulation Lab** 샘플도 있습니다. 가상 UTC, 오프라인 복원, 시계 역행, 저장 실패와 중복 수령 거절을 실행하고 실제 저장 snapshot과 상태를 비교합니다. 자세한 범위와 실행 방법은 [실험실 안내](Samples~/Timer%20Simulation%20Lab/README.md)를 확인하세요. 기존 `unitytools-timer/v1.0.0` release tag에는 새 샘플이 포함되지 않습니다.

Package Manager에서 `Timer Sample Scene`을 Import하면 UI package 없이 `TaskTimerService`의 시작·완료·수령과 persistence를 확인할 수 있습니다.

이 package의 자체 코드는 [MIT License](https://github.com/aassder95/UnityTools/blob/unitytools-timer/v1.0.0/LICENSE)로 배포됩니다.

2026-10-06: Unity 2022.3.62f3 / 6000.3.20f1에서 Timer Play Mode 28/28과 Windows Mono Development Build를 통과했습니다. 정지 후 오프라인 복원·재개, 시계 역행, 저장 실패 시 상태 보존, 잘못된 정지 snapshot 거절을 검사했습니다.
