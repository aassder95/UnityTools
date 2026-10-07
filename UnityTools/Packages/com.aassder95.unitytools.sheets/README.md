# UnityTools Sheets

Unity 2022.3 이상을 지원하는 0.1.0 개발 버전입니다. CSV 파싱, C# 데이터 타입 생성과 키 기반 조회를 제공하며 release tag는 아직 없습니다. UI·Timer·Persistence·UniTask·네트워크·Addressables 의존성이 없습니다. Runtime은 UnityEngine 참조가 없는 일반 C# assembly이며 생성 도구는 Editor 전용 assembly입니다.

## 설치와 생성

Package Manager의 `Add package from disk...`에서 이 폴더의 `package.json`을 선택합니다. `Tools > UnityTools > CSV Generator`를 열고 Project의 CSV TextAsset을 연결합니다.

1. **Read CSV**로 헤더와 행을 읽습니다.
2. Namespace와 Class를 정하고 각 헤더에 Property 이름과 타입을 지정합니다. 초기 Property 이름은 Column1, Column2 등이며 기본 타입은 String입니다. 자동 타입 추론은 하지 않습니다.
   Enum 타입을 선택하면 기존 enum의 전체 타입명(예: Game.Data.EGrade)을 입력합니다. 타입은 먼저 컴파일되어 있어야 합니다.
   Array를 켜면 해당 타입의 읽기 전용 목록을 생성합니다. 셀의 원소 구분자는 `|`입니다.
3. **Validate & Preview**로 전체 행의 타입을 검증하고 코드를 확인합니다.
4. **Save C#**로 Assets 안에 클래스 이름과 같은 파일명으로 저장합니다. 기존 경로를 선택하면 해당 파일을 덮어씁니다. 생성 후 Unity 컴파일이 완료되어야 타입을 사용할 수 있습니다.

Property와 class 이름은 영문 대문자로 시작하는 ASCII C# 식별자, namespace는 점으로 나눈 ASCII 식별자를 사용합니다. 원본 CSV 헤더는 한글·공백·따옴표·개행을 포함할 수 있고 별도의 Property 이름으로 연결합니다. bool Property는 Is·Has·Can·Should 접두사를 사용합니다. 예약어, 중복 이름과 생성 API 이름 충돌은 거부합니다. 모든 헤더를 한 번씩 매핑해야 합니다.

생성 타입은 private readonly field, 읽기 전용 Property, private 생성자, static `TryRead(CsvRow, out T, out string)`를 갖습니다. Reflection으로 런타임 값을 매핑하지 않습니다. 저장한 프리셋을 선택하면 Editor 창의 생성 설정을 복원할 수 있습니다. 저장하지 않은 변경은 창 재생성이나 스크립트 리로드 후 유지되지 않습니다. 생성 코드를 수동으로 고치면 다음 생성에서 덮어쓸 수 있습니다.

## Enum 열

`CsvEnum<T>.TryParse(text, out T value)`는 선언된 enum 이름과 정확히 일치하는 문자열만 허용합니다. 대소문자·공백을 정규화하지 않으며 숫자, 미정의 이름, 쉼표 조합은 거부합니다. Flags enum도 선언된 멤버 이름만 허용합니다. 별칭 멤버는 각 이름으로 읽을 수 있습니다. 실패한 out 값은 사용하지 않습니다. 타입별 이름·값 조회표는 최초 사용 시 한 번 구성하고 이후 변환에서 재사용합니다.

생성 API는 `new CsvColumn("Grade", "Grade", ECsvColumnType.Enum, typeof(EGrade))`로 연결합니다. 기존 3인자 생성자는 유지합니다. enum은 public top-level 타입이며 namespace와 이름은 ASCII C# 식별자여야 합니다. nested enum·Editor 전용 타입·미해석 타입은 빈 테이블에서도 거부합니다. 생성기는 프로젝트 런타임 assembly와 core library의 enum을 지원하며 별도 외부 DLL 타입은 이번 범위에서 제외합니다. 생성 파일이 들어가는 assembly에서 대상 enum을 참조할 수 있도록 asmdef 참조를 설정해야 합니다.

`CsvEnumType`·`CsvEnumReader`로 시작하는 이름은 생성 별칭과 충돌하므로 클래스·namespace·Property 이름에 사용할 수 없습니다. 기존 enum 숫자값과 원본 데이터를 변경하거나 enum 선언 자체를 생성하지 않습니다. 표시명 attribute·한글 별칭·숫자 저장 migration은 지원하지 않습니다. 새 데이터는 이름 문자열을 사용하세요.

## 배열 열

String·Int·Long·Float·Double·Bool·Enum 열에서 Array를 선택할 수 있습니다. API 설정은 `new CsvColumn("answer", "Answers", ECsvColumnType.Int, null, true)`이며, enum 배열은 네 번째 인자에 enum Type을 전달합니다. 기존 생성자와 단일 값 동작은 유지합니다. 생성 타입의 Property는 `IReadOnlyList<T>`이며 원소 순서와 중복을 보존합니다. 내부 배열을 외부에 직접 반환하지 않습니다.

`1|2|3`은 세 원소입니다. 완전히 빈 셀은 빈 목록이고, `1||2`·`|1`·`1|` 같은 빈 원소는 모든 타입에서 실패합니다. 문자열의 공백·쉼표·개행은 원소 안에 보존합니다. 숫자·bool의 공백 처리는 기존 CsvValue 규칙을 따르고 enum은 정확한 이름만 허용합니다. 구분자 escaping과 중첩 배열은 지원하지 않으므로 문자열 원소에 `|`를 넣을 수 없습니다. CSV 따옴표는 바깥 CSV 문법이며 배열 구분자를 escape하지 않습니다.

런타임은 `CsvArray<int>.TryParse(text, CsvValue.TryParse, out IReadOnlyList<int> values)`로 직접 사용할 수 있습니다. enum은 `CsvArray<EGrade>.TryParse(text, CsvEnum<EGrade>.TryParse, out IReadOnlyList<EGrade> values)`를 사용합니다. null 입력·변환 함수 누락·잘못된 원소가 있으면 false와 null을 반환하고 부분 목록을 공개하지 않습니다. 사용자 변환 함수의 부작용·예외 처리는 호출자가 책임집니다. 로드 시 분리·변환하므로 frame loop에서 반복 호출하지 마세요. `CsvArrayType`·`CsvArrayReader`로 시작하는 이름은 생성 별칭으로 예약합니다.

## 런타임 사용

```csharp
using UnityTools.Sheets;

if (!CsvParser.TryParse(csvText, out CsvTable table, out string error))
{
    // 호출부에서 오류를 표시하거나 로드를 중단합니다.
    return;
}

for (int idx = 0; idx < table.Rows.Count; idx++)
{
    if (!Game.Data.ItemData.TryRead(table.Rows[idx], out Game.Data.ItemData item, out error))
        return;

    // item의 읽기 전용 Property를 사용합니다.
}
```

`ItemData`는 사용자 CSV에서 생성한 타입 예시입니다. 생성 전에 이 코드를 붙여 넣으면 컴파일되지 않습니다. `CsvRow.TryGetCell`로 문자열을 직접 읽거나 `CsvValue.TryParse`의 int·long·float·double·bool overload로 변환할 수도 있습니다. 실패한 out 값은 사용하지 않습니다. `CsvTable`·`CsvRow` 생성자는 유효한 non-null collection을 전달하는 데이터 구성 API이며 외부 CSV 입력은 `TryParse`로 검증합니다.

## 키 기반 데이터 조회

생성 클래스의 `TryRead`를 그대로 전달해 모든 행을 변환하고 키로 조회할 수 있습니다.

```csharp
if (!CsvDataSet<Game.Data.ItemData>.TryRead(table, "Id", Game.Data.ItemData.TryRead, out CsvDataSet<Game.Data.ItemData> items, out string loadError))
    return;

if (items.TryGet("001", out Game.Data.ItemData item))
{
    // item을 사용합니다. items.Items는 원본 CSV 행 순서의 읽기 전용 목록입니다.
}
```

키는 CSV 셀의 원본 문자열입니다. `001`과 `1`, `A`와 `a`, 앞뒤 공백은 서로 다른 키이며 trim·숫자 변환을 하지 않습니다. 키 헤더는 빈 테이블에서도 필요합니다. 빈 키·공백뿐인 키·중복 키, 변환 실패·null 결과는 전체 로드를 실패시키며 dataSet은 null이고 부분 목록을 반환하지 않습니다. 오류에는 CSV의 물리적 행 번호가 포함됩니다. 없는 키나 null 키 조회는 false를 반환합니다.

`CsvRowReader<T>`는 참조 타입을 반환하는 동기 변환 함수입니다. 실패하면 false와 오류를 반환하고 외부 상태를 변경하지 않는 함수를 전달하세요. 이 API는 사용자 변환 함수의 부작용을 되돌리거나 예외를 처리하지 않습니다. 목록과 키 색인은 변경할 수 없지만 사용자 타입 내부 상태를 복제하거나 동결하지는 않습니다. 숫자 키·기존 데이터 재로드는 호출자에게 맡깁니다. 새 데이터가 성공했을 때 기존 참조를 교체하면 이전 정상 데이터가 유지됩니다.

### 그룹 조회

반복 키가 정상인 데이터는 `TryReadGroups`로 로드합니다. 각 그룹과 전체 Items는 원본 행 순서의 읽기 전용 목록입니다.

```csharp
if (!CsvDataSet<Game.Data.ItemData>.TryReadGroups(table, "Grade", Game.Data.ItemData.TryRead, out CsvDataSet<Game.Data.ItemData> grades, out string groupError))
    return;

if (grades.TryGetGroup("Rare", out System.Collections.Generic.IReadOnlyList<Game.Data.ItemData> rareItems))
{
    // rareItems의 모든 항목을 사용합니다.
}
```

빈 키·변환 실패·null 결과는 그룹 로드에서도 전체 실패이며 부분 결과를 반환하지 않습니다. `TryGet`은 해당 키에 항목이 정확히 하나일 때만 성공합니다. 여러 항목 중 첫 번째를 임의 선택하지 않습니다. `TryGetGroup`은 일반 TryRead 결과에서도 사용할 수 있으며, 없는 키는 false와 null을 반환합니다. 기존 TryRead의 중복 키 거부와 공개 signature는 유지합니다.

## CSV 계약

- 구분자는 쉼표이고 첫 번째 비어 있지 않은 레코드는 헤더입니다. 헤더만 있는 CSV도 허용합니다.
- LF·CRLF·CR 줄바꿈과 시작 위치의 UTF-8 BOM 문자를 지원합니다.
- 따옴표 안의 쉼표·개행과 `""` 이스케이프를 보존합니다. 셀의 공백·개행을 trim하거나 정규화하지 않습니다.
- 내용이 없는 물리적 빈 줄만 건너뜁니다. 쉼표만 있는 행, `""`, 공백이 있는 행은 데이터입니다. `#`는 일반 문자열이며 주석행으로 건너뛰지 않습니다.
- 헤더는 대소문자를 구분합니다. 비어 있거나 공백만 있는 헤더, 중복 헤더, 행의 열 개수 불일치, 닫히지 않은 따옴표, 따옴표 뒤의 공백/추가 문자와 unquoted 셀의 따옴표는 오류입니다.
- 실패하면 table은 null이고 부분 결과를 반환하지 않습니다. 행 번호는 여러 줄 셀을 반영한 물리적 시작 행입니다.
- int·long은 정수, float·double은 invariant culture의 소수점과 지수 표기만 허용합니다. 천 단위 쉼표, NaN, Infinity, 범위 초과는 거부합니다. bool은 true/false(대소문자 무관)와 1/0을 지원합니다. 빈 숫자/bool 셀에 기본값을 넣지 않습니다.

문자열 전체를 메모리에서 파싱하므로 초기 데이터 로드에 사용하세요. frame loop용 streaming parser가 아닙니다. 파일 읽기·TextAsset 전달·실패 처리·데이터 수명은 호출자에게 있습니다. 키 중복 검사와 타입별 목록은 선택적으로 CsvDataSet을 사용합니다. Vector, 날짜, 암호화, 다운로드, 코드 hot reload는 이번 버전에 포함하지 않습니다.

## 검증

`tools/test-sheets-package.ps1`은 새 Unity 프로젝트에서 Editor 생성기 테스트, Runtime Play Mode 테스트, 생성 코드의 재컴파일과 읽기 검증, Windows Development build와 실행을 검사합니다. XML·로그·생성 코드와 runtime 결과 파일을 보존합니다. 창 열기 테스트를 위해 Editor에는 그래픽 장치가 필요하며 Player 검증은 headless로 실행합니다. 실제 프로젝트 데이터와 수동 GUI 레이아웃·파일 저장 대화상자, IL2CPP/mobile 검증은 별도입니다.

`-ProjectCsvPaths`는 Pizza-Idle의 SauceSeq_Palette.csv·SauceSeq_Stages.csv 검증용 선택 인자입니다. 해당 파일을 읽어 ID 단일 키·randomWeight/reward1Key 그룹 조회와 int/float/bool 열을 검증하고, 생성 타입을 임시 프로젝트에서 컴파일합니다. 원본 파일을 수정하지 않습니다. 게임 실행 경로를 바꾸거나 원본 프로젝트에 패키지를 설치하는 검증은 아닙니다. 다른 스키마의 매핑은 이 검증 fixture에서 별도로 지정해야 합니다.

### 2026-10-06 실행 결과

| Unity | Editor | Runtime Play Mode | 생성 코드 | Windows Development |
| --- | --- | --- | --- | --- |
| 2022.3.62f3 | 22/22 통과 | 38/38 통과 | 컴파일·읽기·키/그룹 조회 통과 | 빌드·Player 실행 통과 |
| 6000.3.20f1 | 22/22 통과 | 38/38 통과 | 컴파일·읽기·키/그룹 조회 통과 | 빌드·Player 실행 통과 |

두 버전에서 Pizza-Idle의 SauceSeq_Palette 7행과 SauceSeq_Stages 10행을 읽었습니다. 원본 ID 키와 randomWeight/reward1Key 그룹 조회, int/float/bool 열 검증과 생성 코드 컴파일이 통과했습니다. answer의 파이프 구분 값과 colorHex는 문자열로 보존합니다. 실제 데이터 조회는 Editor에서 검사했고 Player의 그룹 조회는 별도 생성 fixture로 검사했습니다. Module의 TestData.csv는 암호화된 내용이라 직접 CSV 입력 검증 대상에서 제외했습니다. 실제 게임 로드 경로·VFX prefab·커스텀 shader는 이번 검사에 포함하지 않았습니다.

각 테스트의 skip은 0이며 Player에는 Sheets Editor assembly가 포함되지 않았습니다. 생성 코드에서 모든 지원 타입·한글·쉼표·따옴표·여러 줄 헤더/셀을 읽고, 잘못된 타입과 누락된 열의 실패 반환을 확인했습니다. 키 조회·중복 키 거부도 생성 타입으로 Editor와 실제 Player에서 실행했습니다. Runtime 테스트는 원본 키·순서·읽기 전용 목록, 빈 키·변환 실패·null 결과·중복 키의 부분 결과 거부와 물리적 행 번호를 확인합니다. 테스트의 문화권은 fr-FR로 바꿔 소수점 파싱을 검증했습니다. 창 열기·CSV 읽기·열 매핑·창 닫기 테스트도 포함합니다. 정적 검사는 6개 패키지, 검증 도구 회귀 테스트는 30개 통과했습니다. 자동 테스트의 창 열기는 그래픽 장치가 필요하여 최종 실행은 그래픽 모드로 진행했습니다.

### 2026-10-07 enum 검증

Unity 2022.3.62f3과 6000.3.20f1에서 최종 Editor 30/30, Runtime 48/48, skip 0, 사용자 enum을 참조하는 생성 코드 컴파일·읽기와 Windows Development build·Player 실행이 통과했습니다. enum의 기존 숫자값 10·20은 유지하며 이름으로 읽었습니다. Player에서 숫자·대소문자 차이·공백·쉼표 조합·미정의 이름·빈 값의 실패를 확인했습니다. Player에는 Sheets Editor assembly가 포함되지 않았습니다. 검증 도구 회귀 테스트는 33개 통과했습니다. enum 입력란의 수동 GUI 조작과 IL2CPP/mobile은 별도 확인 대상입니다.

### 2026-10-07 배열 검증

두 Unity 버전에서 Editor 42/42, Runtime 56/56, skip 0, 생성 배열 코드 컴파일·읽기, Windows Development build·Player 실행이 통과했습니다. 실제 Player에서 String·Int·Long·Float·Double·Bool·Enum 배열의 값, 빈 목록과 잘못된 원소의 실패·null 결과를 검사했습니다. 생성 Property는 읽기 전용 목록이며 Runtime 테스트에서 내부 배열을 변경할 수 없는지 확인했습니다. 검증 도구 회귀 테스트는 33개 통과했습니다.

Pizza-Idle의 Palette 7행·Stages 10행 검증도 다시 통과했습니다. 이번에는 Stages의 `answer`를 정수 배열로 설정해 모든 행을 검증하고 생성 코드를 컴파일했습니다. 원본 CSV는 변경하지 않았습니다. 이 실제 CSV 검사는 Editor에서의 타입 검증·코드 컴파일이며 원본 게임 실행 경로를 바꾸지 않습니다. 배열 선택란의 수동 GUI 조작과 모바일·IL2CPP는 별도 확인 대상입니다.

## 생성 설정 프리셋

`Save preset`으로 CSV 참조, 열 타입/Enum/배열, Namespace/Class, 출력 경로를 프로젝트 `.asset`에 저장합니다. Preset 필드에서 선택하면 창 재열기와 스크립트 reload 이후 복원됩니다. CSV 이동/rename은 GUID 참조로 유지됩니다. 헤더 수/이름/순서가 달라지면 자동 적용을 중단하고 경고합니다. 변경된 설정은 Save preset으로 명시적으로 저장하며, C# 저장 성공 시 출력 경로도 저장합니다. 프리셋은 Editor 전용 에셋이므로 Editor 폴더 아래에 저장하세요.

## 검증 오류 목록과 참조 규칙

Validate & Preview는 모든 데이터 타입 오류를 물리적 행 번호와 헤더별로 표시합니다. 스키마 이름/타입 설정 오류는 먼저 수정해야 데이터 타입 검사를 진행합니다. Open은 해당 CSV의 행으로 열기를 요청합니다(연결된 외부 편집기 지원 여부에 따라 달라집니다). Unique key header는 빈 키/중복 키를 검사합니다. Add reference로 원본 열, 대상 CSV/키 열, Pipe array 여부를 지정하면 누락된 ID를 모두 보고합니다. 키와 ID는 원문 문자열을 대소문자 구분하여 비교하며 숫자 정규화는 하지 않습니다. 빈 배열은 허용하고 빈 단일 참조는 오류입니다. 검증 규칙도 프리셋에 저장하며 오류가 남으면 C# 저장을 표시하지 않습니다.
