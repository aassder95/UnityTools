# AGENTS.md

Unity 클라이언트 프로젝트에서 Codex가 따라야 할 공용 작업 지침이다.

## 1. 작업 정책

### 도구 실행

- repo 읽기/검색 중 `windows sandbox: helper_unknown_error: apply deny-read ACLs`가 한 번 발생하면 같은 샌드박스 명령을 반복하거나 표현만 바꿔 재시도하지 않는다.
- `AGENTS.md` 확인, repo 검색, 코드 읽기처럼 필요한 작업은 즉시 `require_escalated`로 전환한다.
- 메모리 파일에서 같은 ACL 오류가 발생하면 재시도하지 않고 실제 repo와 사용자 제공 맥락을 기준으로 진행한다.
- ACL 실패 과정은 중간 보고에 반복 나열하지 않는다. 필요하면 최종 응답에 AGENTS 확인 여부와 메모리 사용 여부만 짧게 적는다.

### Git 작업

- `add`, `commit`, `push`, `stash`, `reset` 등 Git 상태 변경은 사용자의 **현재 요청**이 명확할 때만 수행한다.
- 과거 요청은 이어서 실행하지 않는다. Git 작업 직전에 최신 요청이 여전히 Git 실행인지 확인한다.
- 코드 설명, 검토, 상담, 컨벤션 논의 중에는 임의로 stage/commit/push하지 않는다.
- commit/push 전 변경 범위와 포함 파일을 확인한다.
- pull/rebase/push를 위해 임시 stash를 남기지 않는다.
- stash가 꼭 필요하면 작업 전 목록을 확인하고 Codex가 만든 항목임을 식별할 수 있는 메시지를 사용한다.
- Codex가 만든 stash는 같은 작업 안에서 pop/apply 후 drop한다. 충돌이나 미복구 상태면 삭제하지 말고 이름과 이유를 보고한다.
- 사용자 기존 stash는 명시 요청 없이 drop/clear하지 않는다.
- 작업 종료 전 stash 목록을 다시 확인한다.

#### 커밋 메시지

- 회사 프로젝트 기록에 맞는 의미 있는 문장으로 작성한다.
- Conventional Commit의 type/scope는 영어, 설명문은 기본적으로 한글로 작성한다.
- 예: `feat(express): 오브젝트 변경 타입 활성화`, `fix(conveyor): 수신 예약 해제 누락 수정`
- 외부 라이브러리, API, 프로토콜, 파일명 같은 기술 식별자만 필요한 범위에서 영어로 유지한다.
- 영어 설명문, `???`, 깨진 한글, mojibake, placeholder가 든 메시지로 commit/push하지 않는다.
- commit 후 push 전에 `git log -1 --format=%B`로 메시지를 확인한다.
- PowerShell에서는 UTF-8 안전한 방식으로 메시지를 전달한다. 깨졌다면 push 전 amend한다.
- 깨진 메시지가 이미 원격에 올라갔다면 임의로 force push하지 말고 상황과 선택지를 보고한다.

### 버그 수정

- 방어 코드나 예외 회피용 가드보다 실제 코드 경로의 첫 번째 잘못된 상태와 원인을 먼저 찾는다.
- 수정 전 다음을 확인한다.
  - 증상이 발생하는 실제 코드 경로
  - 잘못된 상태가 만들어지는 최초 원인
  - 기존 설계상 보장되어야 하는 invariant
  - 같은 원인으로 재발할 수 있는 인접 경로
  - 수정 후 검증 방법
- 가드는 불신해야 하는 외부 입력/저장 데이터/네트워크 경계, 원인 제거와 별개인 사용자 데이터 보호, 필요 근거가 명확한 경우에만 추가한다.
- 원인을 100% 확정하지 못했으면 동작을 먼저 바꾸지 않고 입력값, 분기 조건, 상태 전이, 외부 의존 결과, invariant 파손 지점에 진단 로그를 추가한다.
- 진단 로그는 가능한 한 동작을 바꾸지 않아야 한다.
- 더 심을 로그가 없는데도 원인이 불확실할 때만 하나의 최소 가설 수정을 시도한다.
- 가설이 틀렸으면 실패한 동작 변경을 되돌리고 로그를 유지한 채 다음 가설을 검증한다. 실패한 수정 위에 다른 수정을 덧대지 않는다.
- 실패한 수정 중 독립적인 가치가 확인된 정리/안전 개선만 이유를 명시하고 유지할 수 있다.
- 각 진단 단계에서 원인 가설, 관측 로그와 신호, 로그를 우선한 이유, 최소 수정 범위, 실패 시 되돌릴 코드를 구분한다.
- 연속 수정 시 이전 수정의 실패 이유, 되돌릴 코드, 유지할 코드와 사유, 새 원인, 다음 범위를 구분한다.
- 원인 검증 없이 방어 코드, 우회 코드, 임시 예외를 누적하지 않는다.
- 최종 응답은 **원인 / 수정 내용 / 검증 결과**를 구분한다.

### 작업 완료 검수

commit 전 변경 범위를 다음 기준으로 자체 검수한다.

- 이 문서의 컨벤션 준수 여부
- 중복 코드, 데이터, serialized field
- 같은 책임과 변경 이유를 가진 공통화/상위 클래스 승격 후보
- 상속, helper, data class 분리가 실제 책임에 맞는지
- 미사용 필드, 메서드, 클래스, using, serialized YAML 잔재
- 임시 로그/가드/디버그 코드/테스트 하드코딩
- 실패한 수정의 잔재
- 코드와 prefab/scene/asset의 serialized field 이름 일치

현재 작업과 직접 관련된 문제만 정리한다. 무관한 큰 리팩터링은 임의로 진행하지 않고 별도 보고한다. 최종 응답에는 검수 결과와 남은 리스크를 짧게 포함한다.

## 2. 코드 구조

### 섹션

- 코드는 목적 기반 섹션으로 나누되 필요한 섹션만 만들고 빈 섹션은 금지한다.
- 순서: `Type Declarations` → `Constants` → `Readonly` → `Inspector Fields` → `Fields` → `Events` → `Properties` → `Constructors` → `Unity Methods` → `Init/Register` → `Persistence` → `Logic` → `Coroutines` → `Callbacks` → `Utilities` → `Nested Types`
- 상위 섹션 형식:

```csharp
//============================================================
// SectionName
//============================================================
```

- 상위 섹션 안에서 실제 목적 구분이 필요할 때만 아래 서브 섹션을 사용한다. 과도하게 나누지 않는다.

```csharp
//------------------------------------------------------------
// Push
//------------------------------------------------------------
```

### 타입 배치

- `public`/`protected` enum과 delegate는 가능한 top-level 타입으로 분리하며, top-level public enum/delegate에는 섹션 헤더를 만들지 않는다.
- `private enum`은 해당 클래스 내부 상태/옵션일 때만 `Type Declarations`에 둔다.
- 여러 클래스가 참조하는 enum을 nested public 타입으로 만들지 않는다.
- private nested class/struct와 보조 상태/helper 타입은 파일 맨 아래 `Nested Types`에 둔다.
- 한 `.cs` 파일에는 주요 top-level class/struct 하나만 둔다. MonoBehaviour, abstract base, concrete class, top-level helper는 각각 별도 파일로 분리한다.

### 타입 설계

- 과도한 클래스 분리를 금지한다. 함수 수가 아니라 책임과 변경 이유가 여러 개인지로 분리를 판단한다.
- struct는 값 타입이 적합한 단순 데이터 묶음에만 사용한다. 복잡한 상태 변경이나 참조 공유는 class로 작성한다.
- 이름 구분만 위한 빈 class/MonoBehaviour를 만들지 않는다. 동작, 상태, Inspector 설정, override 중 실제 책임이 있어야 한다.
- 빈 파생 클래스로 prefab이나 런타임 분기를 구분하지 않는다.
- `sealed class`를 사용하지 않는다.
- 직접 작성하는 로직 클래스에는 `partial class`를 사용하지 않는다. 생성 코드나 Unity/도구 요구만 예외다.
- 인터페이스는 역할 기반으로 작성하고 `I + PascalCase`를 사용한다. 데이터나 편의 기능 묶음용 인터페이스는 만들지 않는다.

### 주석

- `Exception`, `Convention exception`, `kept as` 같은 메타 주석으로 구현 선택이나 규칙 예외를 변명하지 않는다.
- 예외가 필요하면 기술적 이유와 범위를 먼저 보고하고 사용자 승인을 받는다.
- 승인된 예외의 주석은 예외라는 사실이 아니라 코드만으로 알 수 없는 제약이나 invariant를 설명한다.
- 코드에서 바로 드러나는 타입 성격/함수 동작/구현 형태를 반복 설명하지 않는다.
- 파일과 타입 위에 같은 설명을 중복하지 않는다.

## 3. 네이밍

### 기본 규칙

- class, method, property: `PascalCase`
- constant: `UPPER_SNAKE_CASE`
- enum 타입: `E + PascalCase`; enum 값에는 별도 접두사를 붙이지 않는다.
- struct 멤버: public은 `PascalCase`, private은 `_camelCase`
- 모든 멤버 필드: `_camelCase`; static/readonly/static readonly도 동일하며 `s_`는 금지한다.
- 지역 변수와 매개 변수에는 앞 언더바를 사용하지 않는다.
- bool은 `Is`, `Has`, `Can`, `Should` 접두사를 사용한다.
- collection은 원소 의미를 나타내는 복수형을 사용한다.
- 실제 Dictionary/lookup이 아닌 배열/list에 `Map/Maps`를 붙이지 않는다. mapping 원소 배열은 `Entries/Items`처럼 짓는다.

### Unity 참조 필드

- Unity 객체/컴포넌트 필드는 의미에 맞는 접두사를 사용한다: `rt`, `tr`, `go`, `img`, `txt`, `cg`, `btn`, `ani`, `ps`, `col`, `col2d`, `spr`
- 접두사가 타입 의미를 포함하므로 같은 타입명을 뒤에 반복하지 않는다.
- `BoxCollider[]`는 `_colVisualBounds`로 작성하며 `_colVisualBoundsColliders`는 금지한다.

### 시간 이름

- 시간 변수에는 `Sec`, `Ms`, `Min`, `Hour`, `Day` 등 단위를 반드시 표시한다.
- 예: `cooldownSec`, `requestTimeoutMs`, `_retryIntervalMin`, `dailyResetHour`

### 길이와 추상화 수준

- 짧지만 의미가 명확한 이름을 사용하고 프로젝트 공통 축약어를 우선한다.
- 변수/필드와 bool은 접두사를 포함해 의미 단어 4개 이하를 기본으로 한다.
- method는 5개 이하를 기본으로 하며 복잡한 도메인 동작만 6개까지 허용한다.
- class/struct/enum/interface는 5개 이하를 기본으로 한다.
- `On`, `For`, `With`, `From`, `To` 같은 연결어가 2개 이상 필요하면 이름을 다시 검토한다.
- 조건 전체가 아니라 결과 상태/의도를 이름으로 표현한다.
- 호출자가 몰라도 되는 저장 방식이나 알고리즘을 이름에 넣지 않는다.
- `Queue`, `List`, `Dictionary`, `Map`, `Cache`, `Pool`, `Buffer`는 자료구조 자체가 외부 계약이거나 호출자가 직접 조작할 때만 사용한다.
- 예: `TryGetQueueFrontCar` 대신 `TryGetFrontCar`

### 공통 축약어

| 원문 | 축약 |
| --- | --- |
| Position / Index / Rotation | `Pos` / `Idx` / `Rot` |
| Direction / Distance / Velocity | `Dir` / `Dist` / `Vel` |
| Count / Current / Previous | `Cnt` / `Cur` / `Prev` |
| Temporary / Minimum / Maximum | `Tmp` / `Min` / `Max` |
| Number / Parameter / Reference | `Num` / `Param` / `Ref` |
| Original / Destination | `Origin` / `Dest` |
| Background / UI / HP | `Bg` / `Ui` / `Hp` |

- 사용자 작성 식별자의 약어와 initialism은 일반 단어처럼 취급한다. PascalCase에서는 `Bg/Ui/Hp`, camelCase에서는 `bg/ui/hp`로 작성한다.
- 예: `InventoryUi`, `BgColor`, `MaxHp`, `_curHp`
- 전체 대문자는 `MAX_HP`, `UI_FADE_DURATION_SEC` 같은 `UPPER_SNAKE_CASE` 상수에서만 사용한다.
- Unity/.NET/외부 라이브러리의 `UIDocument`, `UIElement` 같은 공식 API 이름은 원래 표기를 유지한다.
- Unity/.NET API의 `Count` property는 그대로 사용한다.
- `Scale`, `Target`, `Duration`은 축약하지 않는다.

## 4. 외부 노출과 상태

### 접근 제한자

- 기본 접근 제한자는 `private`이며 public field는 금지한다.
- 외부 공개는 Property, Event, Method만 허용한다.
- `internal`은 사용하지 않는다.
- 컨벤션을 이유로 기존 public Property/Event/Method를 internal로 변경하지 않는다.
- 접근 범위 축소는 모든 호출부와 상속 관계를 확인하고 사용자가 요청한 경우에만 수행한다.

### Property

- Property는 기본적으로 외부 조회용이며 단순 읽기 전용은 expression-bodied로 작성한다: `public Vector3 LocalRot => transform.localRotation.eulerAngles;`
- 상태 변경/부작용 없이 값만 반환하는 `GetXxx()`는 만들지 말고 읽기 전용 Property를 사용한다.
- 계산 비용이 크거나 실패 가능성/호출 동작이 있으면 method를 사용한다.
- 일반 로직 클래스의 상태 변경용 setter와 `private set`은 금지하고 의미가 드러나는 method로 변경한다.
- Model, DTO, Settings, 직렬화 대상 데이터처럼 Feature/Service 로직이 없는 데이터 전용 타입만 `private set`을 허용한다.
- MonoBehaviour, Manager, Controller는 직렬화 필드가 있어도 예외가 아니다.

### Event

- Event는 상태 변경 알림에만 사용하며 흐름 제어나 구독 결과에 따른 분기에 사용하지 않는다.
- 내부 event field는 `_onXxx`, 외부 event는 `OnXxx`로 작성한다.
- 외부에는 add/remove만 노출하고 invoke는 소유 클래스 내부에서만 수행한다.

```csharp
private event Action<Type> _onXxx;
public event Action<Type> OnXxx { add => _onXxx += value; remove => _onXxx -= value; }
```

- 모든 구독은 해제한다: `Init/Register ↔ Release/OnDestroy`, `OnEnable ↔ OnDisable`
- 해제가 필요한 구독에 익명 lambda를 사용하지 않는다. 누락은 메모리 릭/중복 호출 버그로 본다.

## 5. Inspector와 직렬화

### Inspector 필드

- 필요한 경우에만 Unity 직렬화 대상 MonoBehaviour/ScriptableObject에서 `[SerializeField] private`를 사용한다.
- 일반 로직 클래스에는 Inspector 필드를 만들지 않는다.
- 노출 순서는 핵심 → 보조 UI → 옵션을 기본으로 한다.
- 필드가 5개를 초과하거나 역할이 2개 이상이면 기능/역할 기반 `[Header]`로 나눈다.
- 부모에 Inspector 필드가 있고 자식이 추가하면 자식 역할을 나타내는 `[Header]`를 반드시 사용한다. `Settings`, `Options` 같은 일반명만 쓰지 않는다.
- 런타임 상태나 디버그 값을 상시 serialized field로 노출하지 않는다.

### 명시적 참조

- 자식/부모/외부 오브젝트 참조는 `[SerializeField] private`로 선언하고 Inspector에서 명시적으로 연결한다.
- Inspector에서 연결할 수 있는 고정 참조에 대해 다음 자동 수집과 fallback을 금지한다.
  - `GetComponent(s)InChildren`, `GetComponent(s)InParent`
  - `Transform.Find`, 하위 Transform 순회
  - 이름/태그/계층 기반 검색
  - `OnValidate`, `Awake`, `Init`에서 누락 참조 자동 복구
- 자기 GameObject의 `GetComponent/TryGetComponent` 캐싱만 허용한다.
- UI 버튼/이미지/텍스트/슬롯/포인트/이펙트/자식 컴포넌트 배열도 명시 참조한다.

### 필수 참조 invariant

- 필수 Inspector 참조의 null은 정상 분기가 아니라 prefab/scene 설정 오류다.
- null을 `false`, `null`, 기본값으로 조용히 처리하지 않는다.
- 누락 검사만 위한 `Awake/Init`, `HasRequiredReferences` 같은 helper를 만들지 않는다.
- null guard, Error 로그, 조기 return, component/GameObject 비활성화 fallback을 추가하지 않는다.
- 런타임 코드는 필수 참조가 연결됐다는 invariant를 전제로 하며 누락은 실제 사용 경로에서 드러나게 한다.
- 필수 참조 추가/변경 시 해당 component를 쓰는 prefab/scene YAML의 연결 상태를 확인한다.

```csharp
// 금지: 누락을 정상 false로 숨김
public bool ShouldUseWaitingLine => _pickupLane != null && _pickupLane.Merge != null;

// 허용: _pickupLane 연결을 invariant로 취급
public bool ShouldUseWaitingLine => _pickupLane.Merge != null;
```

### Attribute와 필드 rename

- private field에는 `[NonSerialized]`를 붙이지 않는다. `[SerializeField]`가 없는 private field는 이미 직렬화되지 않는다.
- `[NonSerialized]`는 legacy public field나 외부 serializer 요구가 명확할 때만 허용한다.
- `[FormerlySerializedAs]`는 금지한다.
- serialized field를 rename하면 해당 component를 가진 prefab/scene/asset YAML의 field 이름도 함께 변경하고 이전 이름이 남지 않았는지 검색한다.

## 6. 일반 코딩

### 상수와 데이터 값

- 한 곳에서만 쓰이는 숫자를 무분별하게 상수로 분리하지 않는다.
- 상수는 재사용, 중복 제거, 도메인 의미, 외부 계약 고정이 필요할 때만 사용한다.
- 한 method/호출 흐름에서만 쓰이는 튜닝값 묶음과 optional parameter 기본값은 inline literal로 작성한다.
- 조정 가능한 값은 serialized field/data asset으로 옮긴다.
- 밸런스/설정/콘텐츠 값은 코드 상수로 만들지 않는다.
- 유효한 상수가 없으면 `Constants` 섹션도 만들지 않는다.

### Wrapper와 helper

- 내부 로직 없이 다른 method만 호출하는 wrapper를 만들지 않는다.
- `AddOnXxx`, `RemoveOnXxx`, `SetOnXxx` 같은 event wrapper를 금지한다.
- interface 구현, Unity callback 연결, 외부 API 호환처럼 형식이 강제될 때만 예외다.
- 한 곳에서만 쓰이는 필드 접근, 단순 산술, 단순 조건 helper는 호출부에 inline한다.
- 재사용, 중복 제거, 복잡한 도메인 의미가 있을 때만 helper로 분리한다.
- 예: `GetRequiredDistance(itemPathSize)`가 `itemPathSize + _itemSpacing`만 반환하면 helper를 제거한다.

### 넓은 Object 타입

- 일반 로직의 field/parameter/return/array/list에 `System.Object`, `object`, `UnityEngine.Object`를 사용하지 않는다.
- 구체 class/struct/interface 또는 역할 기반 interface/generic constraint를 사용한다.
- 단순 null 검사 공용화를 위해 `Object[]`를 만들거나 Object로 받은 뒤 cast/type 분기로 복구하지 않는다.
- `UnityEngine.Object`는 Editor 인프라, 직렬화 도구, 실제 이종 Unity 객체 처리만 허용한다.
- `object`는 외부 API, override/interface 계약, reflection, serializer 시그니처가 강제할 때만 허용한다.
- 예외는 더 구체적인 타입을 쓸 수 없는 근거를 먼저 확인한다.

### Namespace

- 직접 작성하는 일반 로직에 `global::`를 사용하지 않는다.
- namespace 충돌은 namespace 정리, using alias, 타입명 변경으로 원인을 해결한다.
- generated/source generator/designer/외부 도구 생성 코드만 예외다.
- 직접 작성 코드에서 불가피하면 충돌 원인과 필요성을 먼저 보고한다.

## 7. Unity 생명주기와 의존성

### MonoBehaviour

- Unity lifecycle이 필요한 타입만 MonoBehaviour를 상속한다.
- static MonoBehaviour와 MonoBehaviour static field를 통한 전역 접근을 금지한다. 시스템 레벨 MonoSingleton 예외는 Singleton 규칙을 따른다.
- `Awake/Start`에서는 자기 field 초기화와 자기 component 캐싱만 수행한다.
- 외부 의존 초기화는 `Init`에서 수행하며 `Init`이 있으면 `Release`도 작성한다.
- `OnDestroy`에서 event/coroutine/resource를 정리한다.
- `OnEnable`에서 구독/시작한 작업은 `OnDisable`에서 정리한다.

### OnValidate

- Editor 전용 검증/경고/preview 갱신/serialized 데이터 보정에만 사용한다.
- 런타임 초기화, cache 구성, 상태 변경에 의존하지 않는다.
- 단순 런타임 cache 갱신만 필요하면 사용하지 않는다.
- 무거운 검색, 하위 전체 검색, 오브젝트 생성/삭제, 런타임 상태 변경을 수행하지 않는다.
- 런타임 cache는 `Awake`, `Init`, 실제 사용 경로에서 구성한다.

### 자기 참조

- 자기 `transform/gameObject`를 저장하는 `_thisTransform`, `_thisGameObject` field를 만들지 않고 Unity 기본 property를 직접 사용한다.
- `ThisTransform`, `ThisGameObject` 같은 단순 wrapper Property도 만들지 않는다. 내부 호출부도 기본 property로 교체한다.
- 제거 전 실제 할당과 prefab/scene serialized reference를 확인한다. 다른 오브젝트를 참조한다면 자기 참조로 바꾸지 않는다.

### 의존성과 Scene 참조

- 외부 의존성은 명시적으로 전달한다: 일반 C# class는 생성자, MonoBehaviour는 `Init` 또는 Inspector.
- 전역 접근이나 검색으로 의존성을 우회하지 않는다.
- 자기 GameObject 내부 component 캐싱과 허용된 MonoSingleton 기반 class의 instance 해석만 예외다.
- scene object 직접 참조를 최소화하고 `GameObject.Find`를 금지한다.
- `Transform.Find`는 기본 금지다. Inspector로 연결할 수 없고 동적으로 구성되는 소유 하위를 불가피하게 찾아야 할 때만 `Awake/Init`에서 1회 수행하고 cache한다.
- Update/Tick/coroutine 반복 구간에서는 어떤 scene 검색도 수행하지 않는다.

## 8. Coroutine과 제어 흐름

### Coroutine

- handle field는 `_coXxx`, `IEnumerator` method는 `CoXxx`로 작성한다.
- coroutine method는 내부 전용이며 외부에는 일반 시작/중단 method만 노출한다.
- 중복 요청을 무시할지 기존 실행을 중단하고 재시작할지 명확히 한다.
- handle을 명시적으로 관리하고 종료 시 null로 정리한다.
- `OnDestroy`, `Release`, 상태 변경 시 실행 중인 coroutine을 정리한다.

### Guard

- public method는 외부 입력 경계에서 필요한 인자 유효성 검사를 시작부에 수행한다.
- private 내부 method는 호출자가 유효성을 보장하며 이미 검증한 null을 계층마다 반복 검사하지 않는다.
- null 허용 인자는 API 이름이나 계약에서 의도가 드러나야 한다.
- 실패 조건은 가능한 조기 return한다.
- 가능 여부만 확인하면 `CanXxx`, 실패 가능한 실행을 실제로 시도하면 `TryXxx`를 사용한다.

### Try 사용

- 실패가 정상적으로 예상되고 호출자가 실제 분기/복구할 수 있을 때만 사용한다.
- `TryXxx`는 bool을 반환하고 호출자는 반드시 사용한다. 반환값을 무시한다면 Try를 붙이지 않는다.
- 필요한 결과는 out parameter로 반환한다.
- 필수 Inspector 누락, invariant 위반, 초기화 실패를 Try로 숨기지 않는다.
- 외부 경계에서 검증한 값을 내부 계층마다 Try/null 검사로 감싸지 않는다.
- 실패하지 않는 단순 동작이나 wrapper를 bool/Try로 만들지 않는다.
- throw 제거를 이유로 method를 일괄 Try로 바꾸지 않는다.
- 정상적인 Try 실패는 내부 Error 로그를 남발하지 않는다.

### 예외와 실패

- 직접 작성하는 Unity runtime 코드에서 `throw/throw new`를 금지한다.
- null, 잘못된 인자, Inspector 누락, 잘못된 runtime 상태를 예외로 처리하지 않는다.
- 복구 가능한 실패는 Try, 명시적 반환값, 필요한 Error 로그, 조기 return으로 호출자가 알게 한다.
- 로그만 남기고 실패를 삼키거나 Error 로그 후 실패 상태로 계속 실행하지 않는다.
- 필수 Inspector 누락에는 throw, Error 로그, null guard, 조기 return, 비활성화를 사용하지 않는다.
- Property getter, Unity lifecycle, event callback, coroutine에서 예외를 직접 발생시키지 않는다.
- Editor 도구, 테스트, 외부 API 계약 때문에 예외가 필수면 이유와 범위를 보고하고 승인 후 사용한다.

## 9. Logging

- 디버깅, 운영 확인, 장애 추적에 필요한 로그만 작성하고 메시지는 한국어를 사용한다.
- Update/LateUpdate/FixedUpdate/Tick/coroutine 반복 구간에 매 frame/짧은 주기 로그를 남기지 않는다.
- 반복 상태는 조건부 1회 로그, sampling, counter, debug UI로 확인한다.
- 같은 실패를 여러 계층에서 중복 로그로 남기지 않는다.

### 임시 테스트 로그

- 임시 로그는 `Debug.LogWarning`과 `[TEST][Owner:Method] Message` 형식을 사용한다.
- 여러 위치에서 필요하면 다음 공용 helper를 사용한다.

```csharp
public static void LogTest(string owner, string method, string msg)
{
    Debug.LogWarning($"[TEST][{owner}:{method}] {msg}");
}
```

- 직접 호출 시에도 `Debug.LogWarning($"[TEST][{nameof(Owner)}:{nameof(Method)}] 메시지");` 형식을 유지한다.
- Codex는 최종 수정이라고 판단해 `[TEST]` 로그를 임의 삭제하지 않는다.
- commit/push 전 존재 여부를 보고하고 사용자 승인 후에만 제거한다.
- 운영/장애 추적용 영구 로그에는 `[TEST]`를 붙이지 않는다.

## 10. 성능과 시간

### Frame 성능

- `Update/LateUpdate/FixedUpdate`에서 GC 할당을 금지한다.
- frame loop와 대량 반복은 `for`를 우선한다.
- frame loop에서는 `GetComponent`, `new`, LINQ, boxing, string concat, 불필요한 검색/할당을 금지하거나 최소화한다.
- component는 `Awake/Init`에서 cache한다.

### Unity 검색/할당 API

다음 API는 반복 구간에서 사용하지 않는다.

- `GameObject.Find`, `FindWithTag`, `FindGameObject(s)WithTag`
- `Transform.Find`
- `FindObject(s)OfType`, `FindAnyObjectByType`, `FindFirstObjectByType`
- `Resources.FindObjectsOfTypeAll`, `Camera.main`
- `GetComponent`, `TryGetComponent`, `GetComponents`
- `GetComponent(s)InChildren`, `GetComponent(s)InParent`
- `Renderer.material`, `Input.touches`
- `Physics.RaycastAll`, allocation을 만드는 `Overlap` 계열

참조는 Inspector, Init 주입, Awake cache로 확보한다. 물리 검색은 NonAlloc API를 우선 검토한다. 초기화/Editor/test처럼 영향이 낮은 구간에서는 가독성을 위한 LINQ를 허용한다.

### 시간

- 절대 시각 저장/비교는 UTC를 사용하고 local time은 표시에만 사용한다.
- 저장 format을 프로젝트 안에서 통일한다.
- duration/interval은 의미에 맞는 단위를 사용하고 절대 시각과 상대 시간을 혼용하지 않는다.
- gameplay 경과 시간이 Unity Time인지 실제 시간인지 이름과 코드에서 드러나야 한다.

## 11. Data와 Architecture

### Data/Config

- 게임 데이터와 코드 로직을 분리한다.
- balance/config/content 값을 코드에 하드코딩하지 않는다.
- ScriptableObject, serialized field, config asset 등 프로젝트 데이터 경로로 관리한다.
- 수학 상수, 알고리즘 고정값, 안전 기본값처럼 코드 의미에 속하는 값만 예외다.
- 임시 하드코딩은 완료 전 데이터화하거나 제거한다.

### Manager

- 시스템 단위 책임이 명확할 때만 Manager를 사용한다.
- 편의 기능 묶음, 전역 접근점, 잡다한 기능의 우회 hub로 만들지 않는다.
- 책임이 여러 방향으로 늘어나면 역할 기준 분리를 검토한다.

### Singleton

- 전역 lifecycle과 상태 소유가 명확한 시스템 레벨 객체에만 사용한다.
- 게임 로직/단순 로직 class와 단순 접근 편의에는 사용하지 않는다.
- 허용된 시스템 Singleton은 하나의 static `Instance`를 가질 수 있다.
- 공용 `MonoSingleton<T>` 기반 class는 `Instance` cache가 비었을 때 `FindFirstObjectByType<T>()`로 1회 지연 검색하고 결과를 cache할 수 있다.
- 이 검색 예외는 공용 기반 class 내부만 허용한다. 파생 class, Manager 구현, 일반 로직에서 직접 검색하지 않는다.
- frame loop, Tick, coroutine, 반복 처리에서 Singleton instance를 검색하지 않는다.

## 12. Enum 안전성

- 첫 값이 0이 아니면 모든 enum 값을 명시한다.
- 저장/통신/balance 데이터에 연결된 enum의 숫자값은 변경하지 않는다.
- 신규 저장 데이터는 enum 문자열 저장을 우선한다.
- 기존 숫자 저장을 문자열로 바꾸면 migration을 함께 검토한다.

## 13. Formatting

### 조건과 block

- 논리적으로 하나인 조건은 하나의 if로 합친다: `if (a || b) return;`
- 단순 if의 실행문이 한 줄이면 braces를 생략한다.
- 실행문이 2줄 이상이거나 가독성이 좋아지는 경우 braces를 사용한다.
- 같은 `if/else if/else` chain에서 한 branch가 braces를 쓰면 모든 branch에 braces를 사용한다.
- `for/foreach/while/do` 반복문은 실행문이 한 줄이어도 항상 braces를 사용한다.

```csharp
if (receiveReservationId == 0)
{
    if (trItem == null || IsReceiveCapacityFull)
        return false;
}
else if (trItem == null || !HasStoredItemCapacity())
{
    return false;
}

for (int i = roadStartIdx; i < roadPointCnt; ++i)
{
    combinedPoints[entryPointCnt + i - roadStartIdx] = roadPoints[i];
}
```

### 빈 줄과 attribute

- 같은 섹션의 연속 field 선언 사이에는 빈 줄을 넣지 않는다. 실제 논리 그룹 사이에만 1줄을 둔다.
- field attribute는 선언과 같은 줄에 작성한다: `[SerializeField] private Transform _trRoot;`

### 한 줄 우선

- 단순 method call은 인자별로 줄바꿈하지 않고 한 줄로 작성한다.
- 단순 method declaration도 parameter별로 줄바꿈하지 않고 한 줄로 작성한다.
- 단순 return/condition/assignment/ternary expression은 한 줄로 작성한다.
- `||`, `&&`, `?`, `:`만 나열하기 위한 줄바꿈을 하지 않는다.
- lambda, LINQ, object/collection initializer, 복잡한 nested call, generic constraint, attribute, delegate signature처럼 구조상 여러 줄이 더 명확할 때만 줄바꿈한다.

```csharp
packageItem.Parabola(targetPos, 0.2f, 1.5f, PackageParabolaComplete, 360.0f, 0.65f);
bool hasTray = hasItem ? conveyorSet.TryShowEmptyTray(out tray) : conveyorSet.TryShowItem(_spawnedItem, out tray);
public void ParabolaWithPeakMotion(Vector3 targetPos, float durationSec, float arcHeight, float minScale = 0.35f, Action<Item> callback = null)
```

### Float literal

- float literal은 `1.0f`, `0.0f`처럼 소수점을 포함한다. `1f`, `0f`는 금지한다.
