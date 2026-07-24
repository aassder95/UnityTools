# UnityTools Timer

Unity 2022.3 이상에서 사용하는 UTC 기반 Task / Period Timer package입니다.

## 설치

Unity Package Manager의 `Add package from git URL...`에서 다음 주소를 사용합니다.

```text
https://github.com/aassder95/UnityTools.git?path=/UnityTools/Packages/com.aassder95.unitytools.timer#unitytools-timer/v1.0.0
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

## 선택적 TimerHost

MonoBehaviour 수명주기가 필요한 프로젝트는 `TimerHost`를 사용할 수 있습니다.

```csharp
timerHost.Init(storage, () => DateTime.UtcNow);
TaskTimerService taskTimers = timerHost.TaskTimers;
timerHost.Release();
```

`TimerHost`는 static `Instance`를 제공하지 않습니다.

## Persistence

- 외부 저장소는 `IStorage`로 주입합니다.
- 기본 구현은 `PlayerPrefsStorage`입니다.
- 절대 시각은 UTC ticks로 저장합니다.
- 저장 성공 후에만 runtime 상태를 전이합니다.
- Task Timer는 clock rollback 보정을 한 번만 반영합니다.

## 샘플과 라이선스

Package Manager에서 `Timer Sample Scene`을 Import하면 UI package 없이 `TaskTimerService`의 시작·완료·수령과 persistence를 확인할 수 있습니다.

이 package의 자체 코드는 [MIT License](https://github.com/aassder95/UnityTools/blob/unitytools-timer/v1.0.0/LICENSE)로 배포됩니다.
