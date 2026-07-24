# UnityTools 2.0 Migration

UI 2.0은 이전 `UnityTools.Util` 단일 assembly와 호환 계층을 제공하지 않습니다.

## Assembly와 namespace

| 이전 | 변경 후 |
| --- | --- |
| `UnityTools.Util` | `UnityTools.Ui` |
| `UnityTools.Util.Editor` | `UnityTools.Ui.Editor` |
| `UnityTools.Util.Tests` | `UnityTools.Ui.Tests` |
| `UnityTools.Util.UiFramework.*` | `UnityTools.Ui.*` |
| Timer가 포함된 UI package | `com.aassder95.unitytools.timer` 별도 설치 |

asmdef reference와 `using`을 함께 변경해야 합니다. serialized field 이름을 바꾼 변경은 없으며, package 내부 이동 파일은 기존 `.meta` GUID를 유지했습니다.

## Timer

- `TaskTimerManager` → 조합 루트에서 생성하는 `TaskTimerService`
- `PeriodTimerManager` → 조합 루트에서 생성하는 `PeriodTimerService`
- coroutine runner, `IStorage`, UTC provider는 생성자로 명시적으로 전달합니다.
- MonoBehaviour 소유가 필요하면 선택적인 `TimerHost.Init/Release`를 사용합니다. static `Instance`는 없습니다.
- 기본 저장소는 `PlayerPrefsStorage`입니다.

```csharp
TaskTimerService timers = new TaskTimerService(runner, new PlayerPrefsStorage(), () => DateTime.UtcNow);
timers.TryCreate("BUILD", out TaskTimerHandle handle);
timers.TryInit(handle);
timers.TryStart("BUILD", 60.0d);
```

## DynamicScroll

범용 `IPoolable`, `ObjectPool`, `Spawner`는 제거됐습니다. DynamicScroll item은 `IDynamicScrollItem`의 수명주기를 직접 구현합니다.

```csharp
public void OnGet()
{
    gameObject.SetActive(true);
}

public void OnReturn()
{
    gameObject.SetActive(false);
}
```

pool과 deque/index 보조 구현은 `UnityTools.Ui` 내부 책임이며 공개 API가 아닙니다.

## Input System

`UiBackInput`은 `UnityTools.Ui.InputSystem` 선택 assembly로 이동했습니다. 기본 UI package만 설치하면 Input System 없이 컴파일됩니다. Back action이 필요할 때만 `com.unity.inputsystem`을 설치하고 해당 assembly를 참조하세요.

## 제거된 범용 API

- `MonoSingleton`
- `EventDispatcher`
- 전역 logging 계층
- `CoroutineHelper`
- 범용 State/Utility
- 범용 serializer, file storage, persistence facade

프로젝트별로 이미 소유한 공통 인프라를 사용하고, UI/Timer package에는 각 package 책임만 연결하세요.
