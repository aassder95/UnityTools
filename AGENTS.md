# AGENTS.md

이 문서는 Unity 클라이언트 프로젝트에서 Codex가 따라야 할 공용 작업 지침이다.

## Tool Execution Policy

repo 파일 읽기 / 검색 중 `windows sandbox: helper_unknown_error: apply deny-read ACLs`가 한 번 발생하면, 같은 샌드박스 명령을 반복 재시도하지 않는다.

AGENTS.md 확인, repo 파일 검색, 코드 읽기처럼 작업에 필요한 확인은 즉시 `require_escalated`로 전환해 진행한다.

같은 ACL 오류가 발생한 명령을 다른 표현의 동일한 샌드박스 명령으로 다시 실행하지 않는다.

메모리 파일 접근에서 같은 ACL 오류가 발생하면 재시도하지 않고, 현재 repo의 실제 파일과 사용자 제공 맥락을 기준으로 진행한다.

ACL 반복 실패는 중간 보고로 길게 나열하지 않는다.

필요한 경우 최종 응답에서만 AGENTS 확인 여부와 메모리 사용 여부를 짧게 보고한다.
## Git Operation Policy

git add / commit / push / stash / reset 등 Git 상태를 바꾸는 작업은 사용자의 현재 요청이 명확할 때만 수행한다.

이전 대화에서 커밋 / 스테이지 / 푸시 요청이 있었더라도, 이후 사용자가 다른 작업 질문이나 새 요청으로 넘어간 경우 자동으로 이어서 실행하지 않는다.

Git 작업 직전에는 최신 사용자 요청이 여전히 Git 작업 실행인지 확인한다.

코드 설명, 검토, 다음 작업 상담, 컨벤션 논의에 답변하는 중에는 임의로 stage / commit / push 하지 않는다.

커밋 또는 푸시가 필요한 경우에도 변경 범위와 포함 파일을 확인한 뒤 수행한다.

pull / rebase / push 준비 과정에서 임시 stash를 남기지 않는다.

stash가 꼭 필요한 경우 작업 전 stash list를 확인하고, Codex가 만든 stash임을 알 수 있는 메시지를 사용한다.

Codex가 만든 임시 stash는 같은 작업 안에서 pop / apply 후 drop까지 완료한다.

stash pop / apply 중 충돌이 나거나 복구가 끝나지 않으면 stash를 삭제하지 말고, 남은 stash 이름과 이유를 최종 응답에 명확히 보고한다.

사용자가 만든 기존 stash는 명시 요청 없이 drop / clear 하지 않는다.

작업 종료 전 stash list를 다시 확인해 Codex 임시 stash가 남아 있지 않은지 확인한다.

커밋 메시지는 회사 프로젝트 기록으로 남는다고 보고 의미 있는 문장으로 작성한다.

커밋 메시지의 type과 scope는 conventional commit 형식에 맞춰 영어를 사용할 수 있다.

커밋 메시지 설명문은 기본적으로 한글로 작성한다.

예시:

```text
feat(express): 오브젝트 변경 타입 활성화
fix(conveyor): 수신 예약 해제 누락 수정
refactor(rider): 라이더 대기열 갱신 로직 정리
```

영어 설명문은 외부 라이브러리명, API 이름, 프로토콜, 파일명처럼 한글로 바꾸면 의미가 흐려지는 기술 식별자에 한해 사용한다.

금지 예시:

```text
feat(express): enable object change type
feat(express): ????? ?? ?? ?? ??
```

커밋 메시지에 ???, 깨진 한글, mojibake, placeholder 텍스트가 들어간 상태로 커밋하거나 push 하지 않는다.

커밋 생성 후 push 전에 git log -1 --format=%B로 커밋 메시지가 정상 표시되는지 확인한다.

PowerShell 인코딩 문제로 한글 커밋 메시지가 깨질 수 있는 환경에서는 UTF-8 안전한 방식으로 커밋 메시지를 전달한다.

커밋 메시지가 깨졌다면 push 전에 반드시 amend로 수정하고 다시 확인한다.

깨진 커밋 메시지가 이미 원격에 올라간 경우, 원격 변경을 덮어쓰는 force push를 임의로 하지 않고 사용자에게 상황과 선택지를 보고한다.

## Bug Fix Policy

버그 수정 시 단순 방어 코드나 예외 회피용 가드를 먼저 추가하지 않는다.

수정 전 반드시 다음을 확인한다:

- 증상이 발생하는 실제 코드 경로
- 잘못된 상태가 만들어지는 최초 원인
- 기존 설계상 보장되어야 하는 invariant
- 같은 원인으로 재발할 수 있는 인접 경로
- 수정 후 검증 방법

가드 코드는 다음 경우에만 추가한다:

- 외부 입력, 저장 데이터, 네트워크 응답처럼 실제로 불신해야 하는 경계값일 때
- 원인 제거와 별도로 사용자 데이터 보호가 필요한 때
- 왜 가드가 필요한지 코드 또는 설명으로 근거가 명확할 때

원인을 100% 확정하지 못한 상태에서는 동작 변경 수정을 먼저 하지 않는다.

원인이 불확실하면 먼저 로그를 더 디테일하게 추가해 실제 상태를 관측한다.

로그는 입력값, 분기 조건, 상태 전이, 외부 의존 결과, invariant 깨짐 지점처럼 원인 가설을 판별할 수 있는 위치에 심는다.

로그 추가는 가능한 한 동작을 바꾸지 않는 관측 코드로 제한한다.

더 이상 의미 있게 로그를 심을 지점이 없고 원인이 여전히 확정되지 않은 경우에만, 하나의 최소 가설 수정을 시도한다.

가설 수정 후 새 로그 / 재현 결과로 원인이 아니라고 확인되면 해당 수정은 되돌린다.

가설 수정이 실패했는데 그 위에 다른 수정을 덧대지 않는다.

필요한 경우 로그는 유지한 채 실패한 동작 변경만 되돌리고 다음 원인 가설을 검증한다.

각 진단 단계에서는 다음을 구분한다:

- 현재 원인 가설
- 추가한 로그와 확인하려는 신호
- 수정하지 않고 로그를 먼저 선택한 이유
- 가설 수정이 필요하다면 그 최소 범위
- 결과가 가설과 맞지 않을 때 되돌릴 코드

수정 후 문제가 해결되지 않으면, 실패한 수정 위에 계속 덧대지 않는다.

해결되지 않은 수정은 원칙적으로 되돌리고, 새 원인 가설을 세운 뒤 다시 수정한다.

단, 실패한 수정 중 별도 가치가 확인된 정리나 안전 개선은 유지할 수 있지만, 그 이유를 명확히 설명해야 한다.

연속 수정이 필요한 경우 매 단계마다 다음을 구분한다:

- 이전 수정이 해결하지 못한 이유
- 되돌릴 코드
- 유지할 코드와 유지 사유
- 새로 확인한 원인
- 다음 수정 범위

원인 검증 없이 방어 코드, 우회 코드, 임시 예외 처리를 누적하지 않는다.

최종 응답에는 원인, 수정 내용, 검증 결과를 구분해서 보고한다.

## Pre-Commit Review Policy

작업을 마무리하고 커밋하기 전, 변경 범위에 대해 자체 검수를 수행한다.

검수 항목:

- AGENTS.md 코딩 컨벤션 준수 여부
- 중복 코드 / 중복 데이터 / 중복 serialized field 여부
- 공통화하거나 상위 클래스로 승격할 만한 책임이 있는지
- 상속, helper, data class 분리가 실제 책임 기준으로 적절한지
- 사용되지 않는 필드, 메서드, 클래스, using, serialized YAML 잔재 여부
- 임시 로그, 임시 가드, 디버그 코드, 테스트용 하드코딩 잔재 여부 확인
- 실패한 수정이 누적되어 남아 있지 않은지
- prefab / scene / asset의 serialized field 이름이 코드 변경과 일치하는지

검수 중 발견한 문제는 현재 작업과 직접 관련된 범위에서 정리한다.

현재 작업과 무관한 큰 리팩터링은 임의로 진행하지 않고 별도 이슈로 보고한다.

상위 클래스로 승격하거나 공통화할 때는 단순 코드 줄 수가 아니라 책임과 변경 이유가 같은지 기준으로 판단한다.

최종 응답에는 검수 결과와 남은 리스크를 간단히 보고한다.

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

static / readonly / static readonly 필드도 _camelCase를 사용한다.

s_ 접두사는 사용하지 않는다.

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

컬렉션 이름은 원소의 의미를 기준으로 작성한다.

실제 Dictionary / lookup 구조가 아닌 배열이나 리스트에 Map / Maps 이름을 붙이지 않는다.

mapping 원소 배열은 Entries / Items처럼 원소 의미가 드러나는 이름을 사용한다.

금지 예시:

```csharp
private static readonly ExpressRiderDataMap[] s_riderDataMaps;
```

권장 예시:

```csharp
private static readonly ExpressRiderDataEntry[] _riderDataEntries;
```

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

모든 identifier는 의미 단어 수를 제한한다.

변수 / 필드명은 의미 단어 4개 이하를 기본으로 한다.

bool 변수명은 Is / Has / Can / Should 접두사를 포함해 의미 단어 4개 이하로 작성한다.

메서드명은 의미 단어 5개 이하를 기본으로 하며, 복잡한 도메인 동작에서만 6개까지 허용한다.

클래스 / struct / enum / interface 이름은 의미 단어 5개 이하를 기본으로 한다.

이름에 On / For / With / From / To 같은 연결어가 2개 이상 필요하면 이름을 다시 검토한다.

조건 전체를 이름에 설명하지 말고 결과 상태나 의도를 이름으로 표현한다.

호출자가 몰라도 되는 구현 세부사항을 이름에 넣지 않는다.

Queue, List, Dictionary, Map, Cache, Pool, Buffer 같은 저장 방식 / 알고리즘 이름은 외부 계약이 아닐 때 사용하지 않는다.

자료구조가 바뀌어도 호출 의미가 같다면 자료구조 이름을 제거한다.

금지 예시:

```csharp
bool canSpawnOnWaitingPathForPickupQueue;
bool TryGetQueueFrontCar(out Car car);
```

권장 예시:

```csharp
bool canEnterPickupQueue;
bool canSpawnWaitingRider;
bool TryGetFrontCar(out Car car);
```

자료구조 자체를 조작하는 API처럼 호출자가 자료구조를 알아야 하는 경우에만 Queue / List / Cache 같은 이름을 허용한다.

------------------------------------------------------------
Common Abbreviations
------------------------------------------------------------

프로젝트 공통 축약어는 의미가 유지되는 범위에서 우선 사용한다.

Position은 Pos로 축약한다.

Index는 Idx로 축약한다.

Rotation은 Rot으로 축약한다.

Direction은 Dir로 축약한다.

Distance는 Dist로 축약한다.

Velocity는 Vel로 축약한다.

Count는 Cnt로 축약한다.

Current는 Cur로 축약한다.

Previous는 Prev로 축약한다.

Temporary는 Tmp로 축약한다.

Minimum은 Min으로 축약한다.

Maximum은 Max로 축약한다.

Number는 Num으로 축약한다.

Parameter는 Param으로 축약한다.

Reference는 Ref로 축약한다.

Original은 Origin으로 축약한다.

Destination은 Dest로 축약한다.

Unity / .NET API의 Count 프로퍼티명은 그대로 사용한다.

Scale, Target, Duration은 축약하지 않는다.

예시:

```csharp
private Vector3 _startPos;
private Vector3 _targetPos;
private int _selectedIdx;
int roadStartIdx;
Quaternion startRot;
Vector3 moveDir;
float moveDist;
float moveVel;
int itemCnt;
int curStage;
Vector3 prevPos;
float tmpHeight;
float minSpeed;
float maxSpeed;
int itemNum;
float moveParam;
Transform targetRef;
Vector3 originPos;
Vector3 destPos;
```

금지 예시:

```csharp
private Vector3 _startPosition;
private Vector3 _targetPosition;
private int _selectedIndex;
int roadStartIndex;
Quaternion startRotation;
Vector3 moveDirection;
float moveDistance;
float moveVelocity;
int itemCount;
int currentStage;
Vector3 previousPosition;
float temporaryHeight;
float minimumSpeed;
float maximumSpeed;
int itemNumber;
float moveParameter;
Transform targetReference;
Vector3 originalPosition;
Vector3 destinationPosition;
```

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

단순 읽기 전용 Property는 expression-bodied 형식으로 작성한다.

예시:

```csharp
public Vector3 LocalRot => transform.localRotation.eulerAngles;
```

금지 예시:

```csharp
public Vector3 LocalRot
{
    get
    {
        return transform.localRotation.eulerAngles;
    }
}
```

------------------------------------------------------------
Simple Getter
------------------------------------------------------------

상태 변경이나 부작용 없이 값만 조회하는 단순 GetXxx 메서드는 만들지 않는다.

필드, Property, 다른 단순 getter를 그대로 반환하는 조회는 읽기 전용 Property로 작성한다.

금지 예시:

```csharp
public float GetCameraSize()
{
    return _cameraSize;
}

public float GetSize()
{
    return _myCamera.GetCameraSize();
}
```

권장 예시:

```csharp
public float CameraSize => _cameraSize;
public float Size => _myCamera.CameraSize;
```

계산 비용이 크거나, 실패 가능성이 있거나, 호출 시 동작이 발생하는 경우에는 메서드를 사용한다.

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

Inspector 필드가 많아지면 역할 기준 [Header]로 그룹을 나눈다.

같은 클래스 안에서도 Inspector 필드가 5개를 초과하거나 역할이 2개 이상으로 갈라지면 [Header]를 사용한다.

Header는 필드 목록이 길게 이어져 보이지 않도록 기능 단위로 나눈다.

------------------------------------------------------------
Inherited Inspector Fields
------------------------------------------------------------

부모 클래스에 Inspector 노출 필드가 있고 자식 클래스에서 Inspector 필드를 추가하는 경우, 자식 클래스 필드는 반드시 [Header]로 구분한다.

Header 이름은 자식 클래스의 역할을 기준으로 작성한다.

상속 구조에서 Inspector 필드가 이어 붙어 보이지 않도록 소유 경계를 명확히 한다.

Header는 너무 일반적인 "Settings", "Options" 단독 사용을 피한다.

예시:

```csharp
[SerializeField] private Transform _trBaseRoot;

[Header("Receiver Belt")]
[SerializeField] private ConveyorItemReceiver _targetReceiver;
[SerializeField] private float _moveSpeedPerSec = 1.2f;
```

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
Explicit Child References
------------------------------------------------------------

자식 오브젝트 / 하위 컴포넌트 참조는 자동 수집하지 않는다.

필요한 자식 참조는 `[SerializeField] private` 필드로 만들고 Inspector에서 명시적으로 연결한다.

아래 방식으로 하위 참조를 자동 구성하지 않는다.

- GetComponentsInChildren / GetComponentInChildren / GetComponentsInParent
- Transform.Find
- 하위 Transform 순회
- 이름 / 태그 / 계층 구조 기반 검색
- OnValidate / Awake / Init에서 누락 참조를 자동 복구하는 fallback

자기 자신에 붙은 컴포넌트를 GetComponent / TryGetComponent로 캐싱하는 것은 허용한다.

단, 자기 자신이 아닌 자식 / 부모 / 외부 오브젝트 참조를 자동 검색으로 채우지 않는다.

Inspector 연결이 누락된 경우 자동 fallback으로 숨기지 말고 설정 누락이 드러나게 처리한다.

필수 Inspector 참조가 null인 상태는 정상 런타임 분기가 아니라 설정 오류로 본다.

필수 Inspector 참조 null을 프로퍼티 / 메서드 / 조건식에서 false, null, 기본값으로 조용히 처리하지 않는다.

누락된 필수 참조는 Awake / Init / 에디터 검증 등 명확한 검증 지점에서 에러로 드러나게 한다.

금지 예시:

```csharp
[SerializeField] private RoadLane _pickupLane;
public bool ShouldUseWaitingLine => _pickupLane != null && _pickupLane.Merge != null;
```

권장 예시:

```csharp
[SerializeField] private RoadLane _pickupLane;
public bool ShouldUseWaitingLine => _pickupLane.Merge != null;
```

위 예시에서 `_pickupLane` 누락은 ShouldUseWaitingLine의 정상 false 조건이 아니라 Inspector 설정 오류다.

UI 버튼, 이미지, 텍스트, 슬롯, 포인트, 이펙트, 자식 컴포넌트 배열은 명시 참조를 기본으로 한다.

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
Comment Usage
------------------------------------------------------------

코드가 특정 컨벤션의 예외임을 스스로 주장하거나 구현 선택을 변명하는 메타 주석을 작성하지 않는다.

`Exception`, `Convention exception`, `kept as` 같은 표현으로 코드 구조, 접근 제한자, static 사용 등을 정당화하지 않는다.

규칙 예외가 필요하면 주석을 추가해 우회하지 말고, 사용자에게 예외가 필요한 이유와 적용 범위를 먼저 보고하고 승인을 받는다.

승인된 예외에 주석이 필요하더라도 규칙 예외라는 사실만 적지 않고, 코드만으로 알 수 없는 실제 기술적 제약이나 invariant를 설명한다.

코드에서 바로 알 수 있는 타입 성격, 함수 동작, 구현 형태를 반복 설명하지 않는다.

파일 위와 타입 위에 같은 설명 주석을 중복하지 않는다.

금지 예시:

```csharp
// Exception: stateless utility is kept as a static helper.
public static class RandomUtils
{
}
```

권장 예시:

```csharp
public static class RandomUtils
{
}
```

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

------------------------------------------------------------
Global Namespace Alias
------------------------------------------------------------

직접 작성하는 일반 로직 코드에서는 `global::` 사용을 금지한다.

`global::`로 네임스페이스 충돌을 우회하지 않는다.

타입 이름 충돌은 namespace 정리, using alias, 타입명 변경처럼 원인을 드러내는 방식으로 해결한다.

generated code, source generator, designer, 외부 도구 생성 코드에서는 예외로 허용한다.

직접 작성 코드에서 반드시 필요하다고 판단되는 경우, 충돌 원인과 `global::`가 필요한 이유를 코드 리뷰나 응답에 명확히 설명한다.

금지 예시:

```csharp
private global::System.String _name;
```

권장 예시:

```csharp
using SystemString = System.String;

private SystemString _name;
```

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

Update / LateUpdate / FixedUpdate / Tick / Coroutine 반복 구간에서 매 프레임 또는 짧은 주기로 출력되는 로그를 남기지 않는다.

반복 구간에서 상태 확인이 필요한 경우 조건부 1회 로그, 샘플링, 카운터, 디버그 UI 등으로 대체한다.

동일 실패를 여러 계층에서 중복 로그로 남기지 않는다.

------------------------------------------------------------
Temporary Test Log
------------------------------------------------------------

임시 확인용 로그는 필터링하기 쉽게 `Debug.LogWarning`을 사용한다.

임시 확인용 로그 메시지는 반드시 `[TEST][Owner:Method] Message` 형식을 사용한다.

임시 로그가 여러 곳에 필요하면 아래 형태의 공용 helper를 사용한다.

```csharp
public static void LogTest(string owner, string method, string msg)
{
    Debug.LogWarning($"[TEST][{owner}:{method}] {msg}");
}
```

호출 예시:

```csharp
LogTest(nameof(ExpressPickupZone), nameof(TryPickupBoxes), "픽업 수량 확인");
```

직접 로그를 작성해야 하는 경우에도 동일한 형식을 유지한다.

```csharp
Debug.LogWarning($"[TEST][{nameof(ExpressPickupZone)}:{nameof(TryPickupBoxes)}] 픽업 수량 확인");
```

`[TEST]` 로그는 Codex가 임의로 최종 수정이라고 판단해서 삭제하지 않는다.

사용자가 로그 제거를 명시적으로 요청하기 전까지 `[TEST]` 로그를 유지한다.

커밋 / push 전 검수에서는 `[TEST]` 로그 존재 여부를 확인하고 사용자에게 보고한다.

사용자가 제거를 승인한 경우에만 `[TEST]` 로그를 삭제한다.

운영 확인, 장애 추적, 사용자 환경에 남겨야 하는 로그에는 `[TEST]` 접두사를 사용하지 않는다.

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
OnValidate
------------------------------------------------------------

OnValidate는 에디터 전용 검증 / 보정 목적일 때만 사용한다.

런타임에 필요한 초기화, 캐시 구성, 상태 변경을 OnValidate에 의존하지 않는다.

Inspector 변경 즉시 사용자에게 의미 있는 검증, 경고, 프리뷰 갱신, serialized 데이터 보정이 없는 단순 런타임 캐시 갱신 용도라면 OnValidate를 사용하지 않는다.

OnValidate에서 무거운 탐색, 하위 오브젝트 전체 검색, 오브젝트 생성 / 삭제, 런타임 상태 변경을 수행하지 않는다.

런타임에 필요한 캐시는 Awake, Init, 실제 사용 시점 등 실행 경로에서 구성한다.

------------------------------------------------------------
Self Reference
------------------------------------------------------------

자기 자신의 transform / gameObject를 보관하기 위한 필드를 만들지 않는다.

Unity 기본 property인 transform / gameObject를 직접 사용한다.

_thisTransform, _thisGameObject처럼 자기 자신을 다시 가리키는 필드는 금지한다.

transform / gameObject를 그대로 반환하는 단순 Property wrapper도 만들지 않는다.

ThisTransform, ThisGameObject처럼 Unity 기본 property를 이름만 바꿔 노출하지 않는다.

외부 호출 호환을 이유로 단순 wrapper Property를 남기지 않는다.
프로젝트 내부 호출부를 함께 검색해 transform / gameObject 직접 사용으로 교체하고 wrapper Property를 제거한다.

단, 해당 필드가 자기 자신이 아니라 다른 오브젝트를 참조하는 경우가 있으므로 삭제하거나 transform / gameObject로 바꾸기 전에 실제 할당과 prefab / scene serialized reference를 확인한다.

자기 자신 참조가 확인된 경우에만 필드를 제거하고 transform / gameObject 직접 사용으로 바꾼다.

금지 예시:

```csharp
private Transform _thisTransform;
private GameObject _thisGameObject;
public Transform ThisTransform => transform;
public GameObject ThisGameObject => gameObject;
```

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

Transform.Find 사용을 기본적으로 금지한다.

불가피하게 소유 하위 오브젝트를 찾아야 하는 경우 Awake / Init에서 1회만 사용하고 결과를 필드에 캐싱한다.

Update / Tick / Coroutine 반복 구간에서는 Transform.Find를 사용하지 않는다.

씬 의존성은 Inspector 참조, Init 주입, 또는 명확한 소유 범위 내 캐싱으로 연결한다.

런타임 중 불필요한 오브젝트 검색을 만들지 않는다.

============================================================
11. Access Rules
============================================================

기본 접근 제한자는 private로 둔다.

public 필드는 금지한다.

internal 접근 제한자는 사용하지 않는다.

기존 public Property / Event / Method를 컨벤션 준수를 이유로 internal로 변경하지 않는다.

접근 제한자는 실제 호출 범위와 외부 API 계약을 기준으로 결정한다.

접근 제한자를 축소하려면 전체 호출부와 상속 관계를 먼저 확인하고, 사용자가 해당 변경을 요청한 경우에만 진행한다.

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

------------------------------------------------------------
Unity Search / Allocation API
------------------------------------------------------------

아래 Unity API는 반복 구간에서 사용하지 않는다.

GameObject.Find, FindWithTag, FindGameObjectWithTag, FindGameObjectsWithTag
Transform.Find
Object.FindObjectOfType, FindObjectsOfType, FindAnyObjectByType, FindFirstObjectByType
Resources.FindObjectsOfTypeAll
Camera.main
GetComponent, TryGetComponent, GetComponents
GetComponentInChildren, GetComponentsInChildren
GetComponentInParent, GetComponentsInParent
Renderer.material
Input.touches
Physics RaycastAll / Overlap 계열 allocation API

필요한 참조는 Inspector, Init 주입, Awake 캐싱으로 확보한다.

물리 검색은 가능한 NonAlloc API를 우선 검토한다.

불가피하게 사용하는 경우 호출 위치와 이유를 명확히 남기고 반복 구간에서는 사용하지 않는다.

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

단순 대입식과 삼항 연산식도 가능한 한 한 줄로 작성한다.

논리 연산자(||, &&)만 나열하기 위한 줄바꿈을 하지 않는다.

삼항 연산자의 ? / : 만 나열하기 위한 줄바꿈을 하지 않는다.

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

예시:

```csharp
bool hasPackageTray = hasPackageItem ? conveyorSet.TryShowEmptyPackageOnboardingTray(out packageTray) : conveyorSet.TryShowPackageOnboardingItem(_spawnedPackageItem, out packageTray);
```

금지 예시:

```csharp
bool hasPackageTray = hasPackageItem
    ? conveyorSet.TryShowEmptyPackageOnboardingTray(out packageTray)
    : conveyorSet.TryShowPackageOnboardingItem(_spawnedPackageItem, out packageTray);
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
