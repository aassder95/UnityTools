# Module 공용 기능 이식

## 전달 범위

`codex/sheets-vfx-features` 브랜치에 다음 기능을 각각 커밋하고 푸시했다. develop 병합과 release tag 생성은 포함하지 않았다.

| 기능 | 커밋 | 진입점 |
| --- | --- | --- |
| UI 에셋 분석 | `2f719be` | Tools > UnityTools > UI > Asset Report |
| VFX 대표 프레임 자동 선택 | `d433055` | VFX Browser > Select representative frame |
| Shader별 Material 현황 | `2bf33f5` | Tools > UnityTools > VFX > Shader Usage |
| Enum 표시명과 Inspector 선택 | `7054939` | EnumDisplayName / EnumDropdown / EnumDisplay<T> |
| TMP 다국어 문구·폰트 | `7e3d04a` | UiLocalizedText.Init(IUiLocaleSource) |

Module의 기능을 UnityTools API와 기존 패키지 구조에 맞게 작성했다. 게임의 ContextFinder, 전역 언어 서비스, 프로젝트 설정 에셋 자동 생성은 가져오지 않았다. Module/Pizza-Idle 원본은 변경하지 않았다.

## 사용 안내

- [UI 에셋 분석](../UnityTools/Packages/com.aassder95.unitytools.ui/Editor/ASSET_REPORT.md): 프리팹의 Image별 Sprite/Texture/Atlas/Material 조회. 비활성 Image 포함. 런타임 overrideSprite는 저장되지 않으므로 추적하지 않는다. Atlas packable 설정을 확인하며 GPU 배칭, scene 사용 여부 또는 빌드 포함 여부는 판정하지 않는다.
- [대표 프레임](../UnityTools/Packages/com.aassder95.unitytools.vfx/Editor/FRAME_SELECTION.md): 재생 구간의 36개 시점을 같은 bounds로 비교한다. 선택 시 미리보기를 멈추고 전역 썸네일 시점이 바뀐다. 샘플 사이의 짧은 효과와 화면 전체를 채우는 효과는 수동 확인이 필요하다.
- [Shader 현황](../UnityTools/Packages/com.aassder95.unitytools.vfx/Editor/SHADER_USAGE.md): Material 하위 에셋 포함, 폴더 제외와 Shader 필터 지원. 미사용 Material도 포함한다.
- [Enum 표시명](../UnityTools/Packages/com.aassder95.unitytools.ui/Runtime/ENUM_DISPLAY.md): 숫자값 유지, 중복 표시명 파싱 실패, 별칭 처리. 일반 enum용 드롭다운이며 Flags는 지원하지 않는다. IL2CPP/high stripping 사용 시 소비 프로젝트의 reflection 보존 설정을 확인한다.
- [다국어](../UnityTools/Packages/com.aassder95.unitytools.ui/Runtime/Localization/LOCALIZATION.md): TMP 참조를 Inspector로 연결하고 게임의 언어 공급자를 명시적으로 주입한다. 번역 데이터나 언어 선택 UI는 포함하지 않는다. 조회 실패 시 기존 표시를 유지하며 IsResolved=false로 알린다.

## 검증

`tools/test-imported-features.ps1`은 분리된 임시 프로젝트에서 로컬 UPM 패키지를 가져와 그래픽 장치를 사용해 검사한다.

| Unity | EditMode | PlayMode | Windows Player 빌드 | Editor assembly 제외 |
| --- | --- | --- | --- | --- |
| 2022.3.62f3 | 37/37, skip 0 | 4/4, skip 0 | Passed | Passed |
| 6000.3.20f1 | 37/37, skip 0 | 4/4, skip 0 | Passed | Passed |

EditMode는 기존 VFX 회귀 테스트, 실제 대표 프레임 렌더링과 Preview Scene 정리, 원본 prefab 보존, UI Atlas/Sprite 구분, Shader 하위 에셋과 제외 폴더 경계를 검사했다. PlayMode는 enum 값/별칭/중복 이름과 TMP 언어 변경, 폰트·Material 적용, 구독 수명주기, 조회 실패의 부분 변경 방지를 검사했다.

결과 파일:

- `C:/Users/search/AppData/Local/Temp/UnityTools-Ports-2022.3.62f3-5816e585835d43a88bebb2d8776b1bc3/summary.json`
- `C:/Users/search/AppData/Local/Temp/UnityTools-Ports-6000.3.20f1-3afb158c8c9a4df2888d649f1ac22267/summary.json`

검증 중 프리팹에 저장되지 않는 overrideSprite를 테스트에 사용한 문제와 불완전한 테스트용 TMP 폰트를 수정했다. 이를 숨기기 위한 런타임 가드는 추가하지 않았다.

기존 serialized field 이름, prefab/scene/asset, 기존 GUID를 변경하지 않았다. 새 metadata 29개의 GUID 중복/형식을 확인했고 변경 파일에 `[TEST]` 로그는 없다. UPM 정적 검사를 통과했다.

신규 창의 실제 마우스 조작, Inspector 드롭다운의 다중 선택/Undo, 소비 게임의 번역 데이터·Inspector 연결, URP, IL2CPP, 모바일 기기 실행은 이번 검증에 포함하지 않았다. Windows Player 빌드는 compilation과 Editor assembly 제외를 확인하며 새 기능의 Player 실행 검증은 PlayMode와 구분한다.
