# AGENTS.md

이 문서는 Unity 클라이언트 프로젝트에서 Codex가 따라야 할 공용 작업 지침이다.

## Coding Convention

============================================================
1. Section Rules
============================================================

코드는 목적 기반 섹션으로 분리한다.

필요한 섹션만 사용하며, 빈 섹션은 만들지 않는다.

------------------------------------------------------------
Section Header
------------------------------------------------------------

상위 섹션은 아래 형식을 사용한다.

```csharp
//============================================================
// SectionName
//============================================================
```

------------------------------------------------------------
Section Order
------------------------------------------------------------

섹션이 필요한 경우 아래 순서를 기본으로 사용한다.

Type Declarations
Constants
Readonly
Inspector Fields
Fields
Events
Properties
Constructors
Unity Methods
Init/Register
Persistence
Logic
Coroutines
Callbacks
Utilities
Nested Types

파일 성격상 필요 없는 섹션은 생략한다.

------------------------------------------------------------
Sub Section
------------------------------------------------------------

상위 섹션 내부에서 목적 구분이 필요한 경우에만 서브 섹션을 사용한다.

예시:

```csharp
//------------------------------------------------------------
// Push
//------------------------------------------------------------
```

과도한 분리는 금지한다.

------------------------------------------------------------
Type Declaration Placement
------------------------------------------------------------

public / protected enum과 delegate는 가능한 한 top-level 타입으로 분리한다.

top-level public enum / delegate에는 Type Declarations 섹션 헤더를 만들지 않는다.

private enum은 해당 클래스 내부 상태 / 옵션 정의일 때만 Type Declarations 섹션에 둔다.

외부 여러 클래스에서 참조하는 enum을 nested public 타입으로 만들지 않는다.

예시:

```csharp
//============================================================
// Type Declarations
//============================================================
private enum EParabolaState
{
    None = 0,
    Moving = 1,
}
```

------------------------------------------------------------
Nested Type Placement
------------------------------------------------------------

private nested class / struct는 파일 맨 아래 Nested Types 섹션에 둔다.

상위 클래스의 주요 흐름을 먼저 읽을 수 있게 보조 상태 클래스와 helper type은 아래로 내린다.

예시:

```csharp
//============================================================
// Nested Types
//============================================================
private class ParabolaState
{
}
```

============================================================
2. Naming Rules
============================================================

------------------------------------------------------------
General Naming
------------------------------------------------------------

클래스 / 함수 / 프로퍼티는 PascalCase를 사용한다.

상수는 UPPER_SNAKE_CASE를 사용한다.

enum 타입명은 E + PascalCase를 사용한다.

enum 값에는 별도 접두사 규칙을 두지 않는다.

struct 멤버는 public PascalCase, private _camelCase를 사용한다.

------------------------------------------------------------
Member Naming
------------------------------------------------------------

멤버 변수는 _camelCase를 사용한다.

Unity 객체/컴포넌트 참조 필드는 의미에 맞는 접두사를 사용한다.

Unity Prefix:

rt
tr
go
img
txt
cg
btn
ani
ps
col
col2d
spr

Unity Prefix가 타입 의미를 포함하므로 변수명 뒤에 같은 타입명을 반복하지 않는다.

예시:

```csharp
[SerializeField] private BoxCollider[] _colVisualBounds;
```

금지 예시:

```csharp
[SerializeField] private BoxCollider[] _colVisualBoundsColliders;
```

------------------------------------------------------------
Boolean Naming
------------------------------------------------------------

bool 변수는 아래 접두사를 사용한다.

Is
Has
Can
Should

예시:

_isOpen
_hasReward
_canMove
_shouldRefresh

------------------------------------------------------------
Collection Naming
------------------------------------------------------------

리스트 / 컬렉션은 복수형을 사용한다.

------------------------------------------------------------
Time Naming
------------------------------------------------------------

시간 변수는 단위를 반드시 명시한다.

단위 생략을 금지한다.

사용 가능한 단위 예시:

Sec
Ms
Min
Hour
Day

프로젝트에서 사용하는 시간 단위는 의미가 명확한 이름으로 작성한다.

지역 변수 / 매개 변수 예시:

cooldownSec
requestTimeoutMs
retryIntervalMin
dailyResetHour
retentionDay

멤버 변수 예시:

_cooldownSec
_requestTimeoutMs

------------------------------------------------------------
Parameter Rules
------------------------------------------------------------

지역 변수 / 매개 변수는 언더바를 사용하지 않는다.

------------------------------------------------------------
Naming Length
------------------------------------------------------------

함수명 / 변수명은 직관적이면서 가능한 짧게 작성한다.

짧게 줄이는 것보다 의미가 명확한 이름을 우선한다.

의미가 유지되는 범위에서 단어 축약을 허용한다.

축약어는 프로젝트에서 공통으로 사용하는 표현을 우선한다.

============================================================
3. Event Rules
============================================================

이벤트는 상태 변경 알림 용도로만 사용한다.

이벤트를 흐름 제어에 사용하지 않는다.
이벤트 구독자의 처리 결과에 따라 호출자 로직이 분기되면 안 된다.

------------------------------------------------------------
Event Naming
------------------------------------------------------------

내부 이벤트 필드는 아래 형식을 사용한다.

_onXxx

외부 공개 이벤트는 아래 형식을 사용한다.

OnXxx

------------------------------------------------------------
Event Exposure
------------------------------------------------------------

외부에는 add/remove accessor만 노출한다.

예시:

```csharp
private event Action<Type> _onXxx;
public event Action<Type> OnXxx { add => _onXxx += value; remove => _onXxx -= value; }
```

------------------------------------------------------------
Event Invoke
------------------------------------------------------------

이벤트 호출은 클래스 내부에서만 수행한다.

예시:

```csharp
_onXxx?.Invoke(value);
```

외부에서는 구독 / 해제만 가능하다.

Event Lifetime
------------------------------------------------------------

이벤트 구독은 반드시 해제한다.

구독과 해제는 생명주기 짝을 맞춘다.

Init/Register에서 구독한 이벤트는 Release/OnDestroy에서 해제한다.

OnEnable에서 구독한 이벤트는 OnDisable에서 해제한다.

해제가 필요한 이벤트 구독에는 익명 람다를 사용하지 않는다.

이벤트 해제 누락은 메모리 릭 및 중복 호출 버그로 간주한다.

============================================================
4. Property Rules
============================================================

Property는 기본적으로 외부 조회용으로만 사용한다.

읽기 전용 Property는 자유롭게 사용할 수 있다.

예시:

```csharp
public string Name => _name;
```

------------------------------------------------------------
Setter Rules
------------------------------------------------------------

상태 변경용 setter는 일반 로직 클래스에서 금지한다.

금지 예시:

```csharp
public int Count { get; private set; }
```

상태 변경은 의미가 드러나는 명시적 함수로 처리한다.

------------------------------------------------------------
Private Setter Exception
------------------------------------------------------------

아래 데이터 전용 타입에 한해 private setter를 허용한다.

- Model
- DTO
- Settings
- 직렬화 대상 데이터 클래스

해당 클래스는 Feature / Service 로직을 포함하지 않아야 한다.

허용 예시:

```csharp
public int Count { get; private set; }
```

MonoBehaviour, Manager, Controller 등 로직을 포함하는 클래스는
직렬화 필드를 가지더라도 private setter 예외 대상이 아니다.

============================================================
5. Inspector Rules
============================================================

Inspector 노출은 필요한 경우에만 사용한다.

Inspector 노출 필드는 [SerializeField] private를 사용한다.

------------------------------------------------------------
Expose Order
------------------------------------------------------------

Inspector 노출 순서는 아래 기준을 기본으로 한다.

핵심
보조 UI
옵션

필요한 경우 [Header]를 사용해 그룹을 구분한다.

------------------------------------------------------------
Exposure Target
------------------------------------------------------------

Inspector 노출은 MonoBehaviour, ScriptableObject 등 Unity 직렬화 대상 타입에서만 허용한다.

일반 로직 클래스에는 Inspector 노출 필드를 만들지 않는다.

------------------------------------------------------------
Unity Reference Naming
------------------------------------------------------------

Unity 객체/컴포넌트 참조 필드는 Naming Rules의 Unity Prefix를 사용한다.

예시:

```csharp
[SerializeField] private RectTransform _rtRoot;
[SerializeField] private TextMeshProUGUI _txtTitle;
[SerializeField] private Button _btnClose;
```

------------------------------------------------------------
Runtime Data
------------------------------------------------------------

런타임에 변하는 상태값은 Inspector 저장 필드로 노출하지 않는다.

디버그 확인이 필요한 경우에도 상시 SerializeField로 남기지 않는다.

------------------------------------------------------------
NonSerialized Usage
------------------------------------------------------------

private 필드에는 [NonSerialized]를 붙이지 않는다.

Unity에서 private 필드는 [SerializeField]가 없으면 직렬화되지 않는다.

런타임 전용 필드는 attribute 없이 private 필드로 작성한다.

[NonSerialized]는 legacy public field나 외부 serializer 요구처럼 명확한 이유가 있는 경우에만 예외로 허용한다.

------------------------------------------------------------
Serialized Field Rename
------------------------------------------------------------

[FormerlySerializedAs] 사용을 금지한다.

직렬화 필드 이름을 변경해야 하는 경우 해당 컴포넌트를 들고 있는 prefab / scene / asset을 검색해 serialized field 이름을 함께 교체한다.

변경 후 기존 필드명이 prefab / scene / asset YAML에 남아 있지 않은지 검색으로 확인한다.

============================================================
6. Coding Rules
============================================================

------------------------------------------------------------
Constant Usage
------------------------------------------------------------

한 곳에서만 쓰이는 숫자값을 무분별하게 상수로 분리하지 않는다.

상수는 재사용, 중복 제거, 도메인 의미 표현, 외부 계약 고정이 필요한 경우에만 사용한다.

Constants 섹션은 유효한 상수가 있을 때만 만든다.

한 메서드나 한 호출 흐름에서만 쓰이는 튜닝값 묶음을 Constants 섹션으로 올리지 않는다.

값을 조정해야 하는 데이터라면 serialized field / data asset으로 옮기고, 그렇지 않은 단일 사용 값은 inline literal로 둔다.

메서드 호출의 의미가 인자명이나 호출 맥락으로 충분히 드러나는 단일 사용 값은 inline literal을 허용한다.

optional parameter 기본값도 단일 사용 값이면 상수로 분리하지 않고 inline literal로 작성한다.

밸런스 값 / 설정 값 / 콘텐츠 데이터는 상수화하지 말고 Data / Config Rules를 따른다.

예시:

```csharp
packageItem.Parabola(targetPos, 0.2f, 1.5f, PackageParabolaComplete, 360.0f, 0.65f);
```

금지 예시:

```csharp
private const float PACKAGE_BOX_OUTPUT_DURATION_SEC = 0.2f;
private const float PACKAGE_BOX_OUTPUT_ARC_HEIGHT = 1.5f;
private const float PACKAGE_BOX_OUTPUT_ROTATE_Y_DEG = 360.0f;
private const float PACKAGE_BOX_OUTPUT_MIN_SCALE = 0.65f;
private const float DEFAULT_PARABOLA_PEAK_ROTATE_Y_DEG = 360.0f;
private const float DEFAULT_PARABOLA_PEAK_MIN_SCALE = 0.35f;
private const float DEFAULT_PARABOLA_PEAK_MOTION_DURATION_SEC = 0.4f;
```

------------------------------------------------------------
Class Split
------------------------------------------------------------

과도한 클래스 분리를 금지한다.

하나의 클래스가 여러 책임을 가지기 시작하면 분리 여부를 검토한다.

기준은 함수 수가 아니라 책임과 변경 이유의 개수다.

------------------------------------------------------------
Struct Usage
------------------------------------------------------------

struct는 값 타입이 적합한 단순 데이터 묶음에만 사용한다.

복잡한 상태 변경이나 참조 공유가 필요한 경우 class를 사용한다.

------------------------------------------------------------
Wrapper Method
------------------------------------------------------------

내부 로직 없이 다른 함수만 호출하는 wrapper 함수는 만들지 않는다.

이벤트 구독 / 해제를 감싸는 wrapper 함수는 금지한다.

금지 예시:

AddOnXxx
RemoveOnXxx
SetOnXxx

인터페이스 구현, Unity 콜백 연결, 외부 API 호환처럼 형식 맞춤이 필요한 경우는 예외로 허용한다.

------------------------------------------------------------
Trivial Helper
------------------------------------------------------------

한 곳에서만 쓰이는 단순 계산 helper 메서드는 만들지 않는다.

필드 접근, 단순 산술, 단순 조건 반환처럼 호출부에서 바로 이해되는 표현식은 inline으로 작성한다.

재사용, 중복 제거, 복잡한 도메인 의미 표현이 필요한 경우에만 helper 메서드로 분리한다.

금지 예시:

```csharp
private float GetRequiredDistance(float itemPathSize)
{
    return itemPathSize + _itemSpacing;
}
```

권장 예시:

```csharp
float requiredDistance = itemPathSize + _itemSpacing;
```

------------------------------------------------------------
Inheritance
------------------------------------------------------------

sealed class는 사용하지 않는다.

직접 작성하는 로직 클래스에는 partial class를 사용하지 않는다.

생성 코드, Unity/도구 요구사항처럼 partial이 필요한 경우는 예외로 허용한다.

------------------------------------------------------------
Empty Type
------------------------------------------------------------

이름 구분만을 위한 빈 클래스 / 빈 MonoBehaviour를 만들지 않는다.

동작, 상태, Inspector 설정, override 중 하나라도 실제 책임이 있을 때만 별도 타입으로 분리한다.

빈 파생 클래스로 프리팹이나 런타임 분기를 구분하지 않는다.

금지 예시:

```csharp
public class ConveyorMoveOnlyBelt : ConveyorFixedSpeedBeltBase
{
}
```

------------------------------------------------------------
Top-Level Type Per File
------------------------------------------------------------

하나의 .cs 파일에는 하나의 주요 top-level class / struct만 둔다.

MonoBehaviour, abstract base class, concrete class를 같은 파일에 함께 선언하지 않는다.

base class, derived class, helper class가 모두 top-level 타입이면 각각 별도 파일로 분리한다.

private nested class / struct는 Nested Types 규칙에 따라 예외로 허용한다.

금지 예시:

```csharp
public class ConveyorReceiverBelt : ConveyorReceiverBeltBase
{
}

public abstract class ConveyorReceiverBeltBase : ConveyorBeltBase
{
}
```

------------------------------------------------------------
Interface
------------------------------------------------------------

인터페이스는 역할 기반으로 작성한다.

인터페이스 이름은 I + PascalCase를 사용한다.

데이터 묶음이나 단순 편의 묶음을 위한 인터페이스는 만들지 않는다.

============================================================
7. Coroutine Rules
============================================================

Coroutine 핸들 필드는 아래 형식을 사용한다.

_coXxx

Coroutine 메서드는 IEnumerator를 반환하며 아래 형식을 사용한다.

CoXxx

Coroutine 메서드는 내부 전용으로 작성한다.

외부에서 Coroutine 메서드를 직접 호출하지 않는다.
외부 호출이 필요한 경우 일반 메서드를 통해 시작 / 중단한다.

------------------------------------------------------------
Execution Rules
------------------------------------------------------------

Coroutine은 중복 실행을 방지해야 한다.

중복 실행 요청 시 무시할지, 기존 Coroutine을 중단 후 재시작할지 명확히 작성한다.

Coroutine 핸들은 명시적으로 관리한다.

Coroutine이 종료되면 핸들 필드를 null로 정리한다.

------------------------------------------------------------
Cleanup Rules
------------------------------------------------------------

Coroutine 종료 시점은 명확하게 관리한다.

OnDestroy, Release 또는 상태 변경 시 실행 중인 Coroutine을 반드시 정리한다.

============================================================
8. Guard Rules
============================================================

public 메서드는 외부 입력 경계로 보고 필요한 인자 유효성 검사를 시작부에서 수행한다.

이미 검증된 값을 내부 메서드로 전달하는 경우 같은 null 검사를 반복하지 않는다.

실패 조건은 가능한 한 조기 return으로 처리한다.

------------------------------------------------------------
Nullable Input
------------------------------------------------------------

null 허용 인자는 의도가 드러나야 한다.

null을 허용하지 않는 외부 입력은 public 메서드 시작부에서 검사한다.

private 내부 메서드는 호출자가 유효한 값을 전달한다는 전제를 둘 수 있다.

------------------------------------------------------------
Can / Try Naming
------------------------------------------------------------

가능 여부만 확인하는 메서드는 CanXxx 형식을 사용한다.

실패 가능한 실행 메서드는 TryXxx 형식을 사용한다.

TryXxx는 성공 여부를 bool로 반환한다.
필요한 경우 out 매개 변수를 사용한다.

============================================================
9. Error Handling Rules
============================================================

예외를 일반 제어 흐름에 사용하지 않는다.

복구 가능한 실패는 TryXxx 패턴으로 처리한다.

------------------------------------------------------------
Failure Handling
------------------------------------------------------------

실패는 호출자가 알 수 있는 방식으로 반환하거나 처리한다.

로그만 남기고 실패를 삼키지 않는다.

실패를 처리할 수 없는 경우 상위 호출자가 판단할 수 있게 반환한다.

------------------------------------------------------------
Logging
------------------------------------------------------------

로그는 디버깅, 운영 확인, 장애 추적에 필요한 경우에만 작성한다.

불필요한 상시 로그는 금지한다.

로그 메시지는 한국어를 사용한다.

동일 실패를 여러 계층에서 중복 로그로 남기지 않는다.

============================================================
10. MonoBehaviour Rules
============================================================

라이프사이클이 필요한 경우에만 MonoBehaviour를 상속한다.

static MonoBehaviour를 금지한다.
MonoBehaviour를 static 인스턴스 필드로 전역 접근 지점으로 만들지 않는다.

------------------------------------------------------------
Unity Lifecycle
------------------------------------------------------------

Awake / Start에서는 자기 필드 초기화와 자기 컴포넌트 캐싱만 수행한다.

외부 객체 의존 초기화는 Init에서 수행한다.

Init가 있으면 Release도 반드시 작성한다.

------------------------------------------------------------
Cleanup
------------------------------------------------------------

OnDestroy에서 이벤트 / Coroutine / 리소스를 정리한다.

OnEnable에서 구독하거나 시작한 작업은 OnDisable에서 정리한다.

------------------------------------------------------------
Scene Reference
------------------------------------------------------------

씬 오브젝트 직접 참조를 최소화한다.

GameObject.Find 사용을 금지한다.

씬 의존성은 Inspector 참조, Init 주입, 또는 명확한 소유 범위 내 캐싱으로 연결한다.

런타임 중 불필요한 오브젝트 검색을 만들지 않는다.

============================================================
11. Access Rules
============================================================

기본 접근 제한자는 private로 둔다.

public 멤버 변수는 금지한다.

internal 접근 제한자는 기본적으로 사용하지 않는다.
asmdef 경계, 테스트 노출, 패키지 API 제한처럼 명확한 이유가 있는 경우에만 예외로 허용한다.

외부 공개는 아래 형태만 허용한다.

Property
Event
Method

Inspector 노출이 필요한 필드는 Inspector Rules를 따른다.

============================================================
12. Enum Safety Rules
============================================================

첫 값이 0이 아니면 모든 enum 값을 명시한다.

저장, 통신, 밸런스 데이터와 연결된 enum의 숫자값은 변경하지 않는다.

신규 저장 데이터에는 enum 문자열 저장을 우선한다.

기존 숫자 저장 enum을 문자열 저장으로 바꿀 때는 마이그레이션을 함께 고려한다.

============================================================
13. Performance Rules
============================================================

Update / LateUpdate / FixedUpdate에서는 GC 할당을 금지한다.

프레임 루프와 대량 반복 처리에서는 for를 우선 사용한다.

프레임 루프에서는 GetComponent, new, LINQ, boxing, string concat,
불필요한 검색과 할당을 금지 또는 최소화한다.

GetComponent는 Awake / Init에서 캐싱한다.

초기화, 에디터 코드, 테스트 코드처럼 성능 영향이 낮은 구간에서는
가독성을 위해 LINQ 사용을 허용한다.

============================================================
14. Time Rules
============================================================

절대 시각 저장 및 비교는 UTC를 사용한다.

로컬 시간은 표시 용도로만 사용한다.

시간 저장 포맷은 프로젝트 내에서 통일한다.

------------------------------------------------------------
Duration / Interval
------------------------------------------------------------

지속 시간(duration)과 간격(interval)은 의미에 맞는 단위를 사용한다.

절대 시각과 상대 시간을 혼용하지 않는다.

게임플레이 경과 시간은 Unity Time 기준인지 실제 시간 기준인지 의도가 드러나야 한다.

============================================================
15. Formatting Rules
============================================================

불필요한 분기 분할을 금지한다.

논리적으로 하나의 조건으로 표현 가능한 경우 하나의 if 문으로 작성한다.

예시:

```csharp
if (a || b)
    return;
```

아래와 같은 불필요한 분리는 금지한다.

```csharp
if (a)
    return;

if (b)
    return;
```

------------------------------------------------------------
Block Rules
------------------------------------------------------------

한 줄로 표현 가능한 단순 if 분기는 블록({})을 사용하지 않는다.

예시:

```csharp
if (a || b)
    return;
```

아래와 같은 불필요한 블록은 금지한다.

```csharp
if (a || b)
{
    return;
}
```

단, 아래 경우에는 블록을 사용한다.

- 실행문이 2줄 이상인 경우
- 가독성이 더 좋아지는 경우
- 같은 if / else if / else 체인 안에서 다른 분기가 블록을 사용하는 경우

같은 if / else if / else 체인에서 한 분기가 블록을 사용하면 나머지 분기도 블록을 맞춘다.

예시:

```csharp
if (receiveReservationId == 0)
{
    if (trItem == null || IsReceiveCapacityFull)
        return false;
}
else if (trItem == null || !HasStoredItemCapacity() || !_receiveReservations.TryConsume(receiveReservationId))
{
    return false;
}
```

금지 예시:

```csharp
if (receiveReservationId == 0)
{
    if (trItem == null || IsReceiveCapacityFull)
        return false;
}
else if (trItem == null || !HasStoredItemCapacity() || !_receiveReservations.TryConsume(receiveReservationId))
    return false;
```

for / foreach / while / do 반복문은 실행문이 한 줄이어도 항상 블록을 사용한다.

예시:

```csharp
for (int i = roadStartIdx; i < roadPointCount; ++i)
{
    combinedPoints[entryPointCount + i - roadStartIdx] = roadPoints[i];
}
```

금지 예시:

```csharp
for (int i = roadStartIdx; i < roadPointCount; ++i)
    combinedPoints[entryPointCount + i - roadStartIdx] = roadPoints[i];
```

------------------------------------------------------------
Blank Line Rules
------------------------------------------------------------

같은 섹션 안에서 연속된 필드 선언 사이에는 빈 줄을 넣지 않는다.

논리 그룹을 나눌 때만 빈 줄 1줄을 사용한다.

예시:

```csharp
[SerializeField] private AdStageCharacterPlayerOverrideSettings _characterPlayerSettings = new AdStageCharacterPlayerOverrideSettings();
[SerializeField] private AdStageCharacterWorkerOverrideSettings _characterWorkerSettings = new AdStageCharacterWorkerOverrideSettings();
[SerializeField] private AdStageCharacterCarryOverrideSettings _characterCarrySettings = new AdStageCharacterCarryOverrideSettings();
[SerializeField] private AdStageCharacterCarOverrideSettings _characterCarSettings = new AdStageCharacterCarOverrideSettings();
```

금지 예시:

```csharp
[SerializeField] private AdStageCharacterPlayerOverrideSettings _characterPlayerSettings = new AdStageCharacterPlayerOverrideSettings();

[SerializeField] private AdStageCharacterWorkerOverrideSettings _characterWorkerSettings = new AdStageCharacterWorkerOverrideSettings();
```

------------------------------------------------------------
Attribute Line Break
------------------------------------------------------------

필드 attribute는 필드 선언과 같은 줄에 작성한다.

[SerializeField]를 단독 줄로 분리하지 않는다.

예시:

```csharp
[SerializeField] private Transform _trRoot;
[SerializeField] protected Transform _transform;
```

금지 예시:

```csharp
[SerializeField]
private Transform _trRoot;

[SerializeField]
protected Transform _transform;
```

------------------------------------------------------------
Method Call Line Break
------------------------------------------------------------

단순 메서드 호출은 가능한 한 한 줄로 작성한다.

인자만 나열된 호출을 인자별 줄바꿈으로 작성하지 않는다.

예시:

```csharp
packageItem.Parabola(itemsTransform.position + Vector3.up * (tempCount * packageItem.itemSizeY), PACKAGE_BOX_OUTPUT_DURATION_SEC, PACKAGE_BOX_OUTPUT_ARC_HEIGHT, PackageParabolaComplete, PACKAGE_BOX_OUTPUT_ROTATE_Y_DEG, PACKAGE_BOX_OUTPUT_MIN_SCALE);
```

금지 예시:

```csharp
packageItem.Parabola(
    itemsTransform.position + Vector3.up * (tempCount * packageItem.itemSizeY),
    PACKAGE_BOX_OUTPUT_DURATION_SEC,
    PACKAGE_BOX_OUTPUT_ARC_HEIGHT,
    PackageParabolaComplete,
    PACKAGE_BOX_OUTPUT_ROTATE_Y_DEG,
    PACKAGE_BOX_OUTPUT_MIN_SCALE);
```

람다, object initializer, collection initializer, 복잡한 중첩 호출처럼 한 줄이 오히려 읽기 어려운 경우에만 줄바꿈을 허용한다.

------------------------------------------------------------
Expression Line Break
------------------------------------------------------------

단순 return expression과 조건식은 가능한 한 한 줄로 작성한다.

논리 연산자(||, &&)만 나열하기 위한 줄바꿈을 하지 않는다.

예시:

```csharp
return _cachedPoints == null || !ReferenceEquals(_cachedRoad, road) || !ReferenceEquals(_cachedEntryPoints, entryPoints) || !ReferenceEquals(_cachedRoadPoints, roadPoints) || !ReferenceEquals(_cachedStopPoint, _trStopPoint) || _cachedEntryPointCount != entryPointCount || _cachedRoadPointCount != roadPointCount;
```

금지 예시:

```csharp
return _cachedPoints == null
    || !ReferenceEquals(_cachedRoad, road)
    || !ReferenceEquals(_cachedEntryPoints, entryPoints)
    || !ReferenceEquals(_cachedRoadPoints, roadPoints)
    || !ReferenceEquals(_cachedStopPoint, _trStopPoint)
    || _cachedEntryPointCount != entryPointCount
    || _cachedRoadPointCount != roadPointCount;
```

복잡한 람다, LINQ, object initializer처럼 구조 자체가 여러 줄인 경우에만 줄바꿈을 허용한다.

------------------------------------------------------------
Method Declaration Line Break
------------------------------------------------------------

단순 메서드 선언은 가능한 한 한 줄로 작성한다.

파라미터만 나열된 선언을 파라미터별 줄바꿈으로 작성하지 않는다.

예시:

```csharp
public void ParabolaWithPeakMotion(Vector3 targetPos, float timer, float height, float peakRotateYDeg = DEFAULT_PARABOLA_PEAK_ROTATE_Y_DEG, float peakMinScale = DEFAULT_PARABOLA_PEAK_MIN_SCALE, float peakMotionDurationSec = DEFAULT_PARABOLA_PEAK_MOTION_DURATION_SEC, Action<Item> callback = null)
```

금지 예시:

```csharp
public void ParabolaWithPeakMotion(
    Vector3 targetPos,
    float timer,
    float height,
    float peakRotateYDeg = DEFAULT_PARABOLA_PEAK_ROTATE_Y_DEG,
    float peakMinScale = DEFAULT_PARABOLA_PEAK_MIN_SCALE,
    float peakMotionDurationSec = DEFAULT_PARABOLA_PEAK_MOTION_DURATION_SEC,
    Action<Item> callback = null)
```

generic constraint, attribute, 복잡한 delegate 시그니처처럼 한 줄이 오히려 읽기 어려운 경우에만 줄바꿈을 허용한다.

------------------------------------------------------------
Float Literal
------------------------------------------------------------

float 리터럴은 소수점 형태로 작성한다.

축약형 float 리터럴은 사용하지 않는다.

예시:

```csharp
float minScale = 1.0f;
float startTimeSec = 0.0f;
```

금지 예시:

```csharp
float minScale = 1f;
float startTimeSec = 0f;
```

============================================================
16. Dependency Rules
============================================================

클래스는 가능한 한 의존성을 최소화한다.

외부 객체 의존성은 명시적으로 전달한다.

일반 C# 클래스의 외부 의존성은 생성자에서 주입한다.

MonoBehaviour의 외부 의존성은 Init 또는 Inspector 참조로 연결한다.

전역 접근이나 검색으로 의존성을 우회하지 않는다.

자기 GameObject 내부 컴포넌트 캐싱은 허용한다.

============================================================
17. Data / Config Rules
============================================================

게임 데이터는 코드 로직과 분리한다.

밸런스 값 / 설정 값 / 콘텐츠 데이터는 코드에 하드코딩하지 않는다.

데이터는 ScriptableObject, serialized field, config asset 등 프로젝트에 맞는 데이터 경로로 관리한다.

수학 상수, 알고리즘 내부 고정값, 안전 기본값처럼 코드 의미에 속하는 값은 예외로 허용한다.

임시 하드코딩 값은 작업 완료 전에 데이터화하거나 제거한다.

============================================================
18. Manager Rules
============================================================

Manager 클래스는 시스템 단위 책임이 명확한 경우에만 사용한다.

Manager 이름은 단순 편의 묶음에 사용하지 않는다.

Manager를 전역 접근 지점으로 만들기 위한 용도로 사용하지 않는다.

잡다한 기능을 모으는 우회용 허브 클래스를 만들지 않는다.

책임이 여러 방향으로 늘어나면 역할 기준으로 분리를 검토한다.

------------------------------------------------------------
Singleton
------------------------------------------------------------

Singleton은 시스템 레벨 객체처럼 전역 생명주기와 상태 소유가 명확한 경우에만 사용한다.

게임 로직 클래스 및 단순 로직 클래스에는 Singleton을 사용하지 않는다.

단순 접근 편의를 위해 Singleton을 만들지 않는다.
