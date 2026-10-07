# Module / Pizza-Idle 추가 기능 이식

여섯 기능을 `codex/sheets-vfx-features` 브랜치에 각각 커밋하고 푸시했다. 원본 프로젝트는 읽기만 했으며 develop 병합과 release tag 생성은 포함하지 않았다.

| 기능 | 커밋 | 사용 안내 |
| --- | --- | --- |
| 키 기반 Prefab·Sprite·Atlas 카탈로그 | `6af79e8` | [Asset Catalog](../UnityTools/Packages/com.aassder95.unitytools.ui/Documentation~/asset-catalog.md) |
| 호출자 버퍼 기반 TMP 정수 출력 | `39a50f6` | [Integer Text](../UnityTools/Packages/com.aassder95.unitytools.ui/Documentation~/integer-text.md) |
| 보상형 광고 QA 시뮬레이터·Editor 실험실 | `7b63725` | [QA](../UnityTools/Packages/com.aassder95.unitytools.qa/README.md) |
| PointerEventData 기반 TMP 링크 이벤트 | `ef12714` | [TMP Links](../UnityTools/Packages/com.aassder95.unitytools.ui/Documentation~/tmp-links.md) |
| Scene·추가 Define·출력 경로 빌드 프리셋 | `df6c0a4` | [Build](../UnityTools/Packages/com.aassder95.unitytools.build/README.md) |
| VAT 베이커·재생 컴포넌트·Unlit Shader | `ed524fb` | [VAT](../UnityTools/Packages/com.aassder95.unitytools.vat/README.md) |

UI 기능은 기존 UI 패키지에 포함된다. TMP 두 기능은 기존 Localization 선택 assembly를 사용한다. QA·Build·VAT는 독립 패키지이며 Editor assembly를 런타임과 분리한다. 게임의 광고 SDK, 전역 Context/Singleton, 회사별 서명·출시 설정은 가져오지 않았다.

## 검증 결과

| Unity | Built-in EditMode | 카탈로그 직렬화 추가 검사 | PlayMode | Windows Mono 빌드·실행 | URP VAT 렌더링 |
| --- | --- | --- | --- | --- | --- |
| 2022.3.62f3 | 39/39 | 1/1 | 19/19 | Passed | URP 14.0.12, 1/1 |
| 6000.3.20f1 | 39/39 | 1/1 | 19/19 | Passed | URP 17.3.0, 1/1 |

모든 Unity 테스트는 실패와 skip이 0이다. 카탈로그 추가 검사는 실제 prefab 참조와 키를 SerializedObject로 저장하고 재로드해 확인했다. PlayMode는 정수 극값·버퍼 부족·접두/접미, 실제 TMP 링크 내부/외부/오른쪽 클릭, 광고 성공·실패·취소·중복 보상 방지, VAT seek·종료·공유 Material 보존을 검사했다.

VAT Editor 검사는 실제 bone animation을 베이킹하고 원본 보존·중복 저장 거부·Preview Scene 정리·Shader 오류 없음과 두 시점의 렌더링 위치 이동을 확인했다. 같은 렌더링 검사를 Built-in과 URP에서 각각 실행했다. URP Player 빌드는 이번 범위에 포함되지 않는다.

Windows Player는 BuildPresetRunner를 통해 실제 빌드했다. 추가 Define 적용, 기존 전역 Define 및 Android bundle 옵션 보존, Editor assembly 제외를 확인했다. 생성된 Player를 실행해 광고 완료 결과와 VAT one-shot 종료를 확인했다. 화면의 육안 평가나 모바일 성능 측정은 아니다.

결과와 로그는 다음 임시 프로젝트에 보존했다. `summary.json`, `catalog-serialization.xml`, `vat-urp-summary.json`, 테스트 XML 및 Player 로그가 포함된다.

- `C:/Users/search/AppData/Local/Temp/UnityTools-Ports-2022.3.62f3-c28b05336fca40ada0b6aa879f1cb73c`
- `C:/Users/search/AppData/Local/Temp/UnityTools-Ports-6000.3.20f1-f8c94c2b00934875a26cb58a16c0af45`

재실행은 `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools/test-imported-features.ps1 -UnityVersion 2022.3.62f3` 또는 `6000.3.20f1`로 한다. 카탈로그 검사가 추가된 현재 runner는 EditMode 40개를 실행한다. 완료된 새 임시 프로젝트 경로를 `tools/test-vat-urp.ps1 -Project <경로>`에 전달하면 URP를 설치하고 실제 pipeline을 설정해 VAT 렌더링을 검사한다. 소비 프로젝트에 실행하는 스크립트가 아니다.

UPM 정적 검사 대상은 9개 패키지이며 검사기 회귀 테스트 37개가 통과했다. 배포 계약·GUID 중복·누락 metadata·assembly 경계를 확인한다. 정적 검사와 Unity 실행 결과는 별개다.

## 적용 범위와 남은 확인

- 기존 prefab/scene/asset, serialized field 이름과 기존 GUID를 변경하지 않았다. 새 Inspector 참조는 소비 프로젝트에서 명시적으로 연결해야 한다. 카탈로그는 fallback 검색을 하지 않으며 TMP 링크는 URL을 자동으로 열지 않는다.
- 광고 시뮬레이터는 결과와 시간을 명시적으로 지정한다. 실제 광고 SDK 콜백, 네트워크 또는 보상 서버 연동은 게임에서 확인한다. 취소와 Dispose는 소유자 수명주기에 연결한다.
- 빌드 프리셋은 수동으로 활성 플랫폼을 전환한 뒤 실행한다. Android/iOS toolchain·서명·IL2CPP·기기 실행은 검증하지 않았다. 소비 프로젝트의 PlayerSettings를 프리셋으로 자동 덮어쓰지 않는다.
- VAT는 단일 renderer/clip 베이킹·보간·loop/one-shot 재생을 제공한다. multi-renderer 결합, virtual bone, cross-fade, animation event, root motion 및 lighting/shadow는 포함하지 않는다. Module 기존 VAT 에셋은 자동 migration하지 않는다.
- 신규 Editor 창의 마우스 조작과 실제 소비 모델·Canvas 배치는 별도 확인이 필요하다. 이번 변경에 임시 `[TEST]` 로그를 추가하지 않았다.
