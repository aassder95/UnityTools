# UnityTools Sheets

Unity 2022.3 이상을 지원하는 0.1.0 개발 버전입니다. CSV 파싱과 C# 데이터 타입 생성을 제공하며 release tag는 아직 없습니다. UI·Timer·Persistence·UniTask·네트워크·Addressables 의존성이 없습니다. Runtime은 UnityEngine 참조가 없는 일반 C# assembly이며 생성 도구는 Editor 전용 assembly입니다.

## 설치와 생성

Package Manager의 `Add package from disk...`에서 이 폴더의 `package.json`을 선택합니다. `Tools > UnityTools > CSV Generator`를 열고 Project의 CSV TextAsset을 연결합니다.

1. **Read CSV**로 헤더와 행을 읽습니다.
2. Namespace와 Class를 정하고 각 헤더에 Property 이름과 타입을 지정합니다. 초기 Property 이름은 Column1, Column2 등이며 기본 타입은 String입니다. 자동 타입 추론은 하지 않습니다.
3. **Validate & Preview**로 전체 행의 타입을 검증하고 코드를 확인합니다.
4. **Save C#**로 Assets 안에 클래스 이름과 같은 파일명으로 저장합니다. 기존 경로를 선택하면 해당 파일을 덮어씁니다. 생성 후 Unity 컴파일이 완료되어야 타입을 사용할 수 있습니다.

Property와 class 이름은 영문 대문자로 시작하는 ASCII C# 식별자, namespace는 점으로 나눈 ASCII 식별자를 사용합니다. 원본 CSV 헤더는 한글·공백·따옴표·개행을 포함할 수 있고 별도의 Property 이름으로 연결합니다. bool Property는 Is·Has·Can·Should 접두사를 사용합니다. 예약어, 중복 이름과 생성 API 이름 충돌은 거부합니다. 모든 헤더를 한 번씩 매핑해야 합니다.

생성 타입은 private readonly field, 읽기 전용 Property, private 생성자, static `TryRead(CsvRow, out T, out string)`를 갖습니다. Reflection으로 런타임 값을 매핑하지 않습니다. Editor 창의 설정은 일시적이며 창 재생성이나 스크립트 리로드 후 다시 읽어 지정합니다. 생성 코드를 수동으로 고치면 다음 생성에서 덮어쓸 수 있습니다.

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

## CSV 계약

- 구분자는 쉼표이고 첫 번째 비어 있지 않은 레코드는 헤더입니다. 헤더만 있는 CSV도 허용합니다.
- LF·CRLF·CR 줄바꿈과 시작 위치의 UTF-8 BOM 문자를 지원합니다.
- 따옴표 안의 쉼표·개행과 `""` 이스케이프를 보존합니다. 셀의 공백·개행을 trim하거나 정규화하지 않습니다.
- 내용이 없는 물리적 빈 줄만 건너뜁니다. 쉼표만 있는 행, `""`, 공백이 있는 행은 데이터입니다. `#`는 일반 문자열이며 주석행으로 건너뛰지 않습니다.
- 헤더는 대소문자를 구분합니다. 비어 있거나 공백만 있는 헤더, 중복 헤더, 행의 열 개수 불일치, 닫히지 않은 따옴표, 따옴표 뒤의 공백/추가 문자와 unquoted 셀의 따옴표는 오류입니다.
- 실패하면 table은 null이고 부분 결과를 반환하지 않습니다. 행 번호는 여러 줄 셀을 반영한 물리적 시작 행입니다.
- int·long은 정수, float·double은 invariant culture의 소수점과 지수 표기만 허용합니다. 천 단위 쉼표, NaN, Infinity, 범위 초과는 거부합니다. bool은 true/false(대소문자 무관)와 1/0을 지원합니다. 빈 숫자/bool 셀에 기본값을 넣지 않습니다.

문자열 전체를 메모리에서 파싱하므로 초기 데이터 로드에 사용하세요. frame loop용 streaming parser가 아닙니다. 파일 읽기·TextAsset 전달·실패 처리·저장소·키 중복 검사·목록 유지 책임은 호출자에게 있습니다. enum, 배열, Vector, 날짜, 암호화, 다운로드, 코드 hot reload는 이번 버전에 포함하지 않습니다.

## 검증

`tools/test-sheets-package.ps1`은 새 Unity 프로젝트에서 Editor 생성기 테스트, Runtime Play Mode 테스트, 생성 코드의 재컴파일과 읽기 검증, Windows Development build와 실행을 검사합니다. XML·로그·생성 코드와 runtime 결과 파일을 보존합니다. 창 열기 테스트를 위해 Editor에는 그래픽 장치가 필요하며 Player 검증은 headless로 실행합니다. 실제 프로젝트 데이터와 수동 GUI 레이아웃·파일 저장 대화상자, IL2CPP/mobile 검증은 별도입니다.

### 2026-10-06 실행 결과

| Unity | Editor | Runtime Play Mode | 생성 코드 | Windows Development |
| --- | --- | --- | --- | --- |
| 2022.3.62f3 | 22/22 통과 | 25/25 통과 | 컴파일·읽기 통과 | 빌드·Player 실행 통과 |
| 6000.3.20f1 | 22/22 통과 | 25/25 통과 | 컴파일·읽기 통과 | 빌드·Player 실행 통과 |

각 테스트의 skip은 0이며 Player에는 Sheets Editor assembly가 포함되지 않았습니다. 생성 코드에서 모든 지원 타입·한글·쉼표·따옴표·여러 줄 헤더/셀을 읽고, 잘못된 타입과 누락된 열의 실패 반환을 확인했습니다. 테스트의 문화권은 fr-FR로 바꿔 소수점 파싱을 검증했습니다. 창 열기·CSV 읽기·열 매핑·창 닫기 테스트도 포함합니다. 정적 검사는 6개 패키지, 검증 도구 회귀 테스트는 21개 통과했습니다. 자동 테스트의 창 열기는 그래픽 장치가 필요하여 최종 실행은 그래픽 모드로 진행했습니다.
