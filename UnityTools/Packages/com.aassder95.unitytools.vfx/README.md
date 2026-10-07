# UnityTools VFX Browser

Unity 2022.3 이상에서 사용하는 Editor 전용 독립 UPM 패키지입니다. UI·Timer·Persistence·외부 tween 패키지에 의존하지 않으며 Player assembly를 포함하지 않습니다. 현재 0.1.0 개발 버전으로 release tag는 없습니다.

## 설치와 사용

Package Manager의 `Add package from disk...`에서 이 폴더의 `package.json`을 선택합니다. 이 저장소의 데모 프로젝트에서는 embedded package로 사용합니다. `Tools > UnityTools > VFX Browser`를 엽니다.

- 상단에 Project 폴더를 지정하고 **Refresh**로 파티클을 포함한 prefab을 찾습니다. 최초 기본 폴더는 `Assets`입니다.
- 이름과 경로를 함께 검색합니다. 공백으로 나눈 모든 검색어가 일치해야 하며 영문 대소문자를 구분하지 않습니다.
- **Looping / OneShot**, **Favorites**, 별 버튼으로 목록을 좁힙니다. 즐겨찾기는 프로젝트별 EditorPrefs에 GUID로 저장해 prefab 이동·이름 변경 후에도 유지됩니다.
- Looping은 비활성 자식을 포함한 하위 ParticleSystem 중 하나라도 loop 설정이 켜져 있는 prefab입니다. 목록에는 시스템 개수를 표시하고, 미리보기 아래에는 현재 살아 있는 파티클 수를 표시합니다.
- 목록은 페이지당 12개이며 포커스가 있는 창에서 항목별 대표 프레임을 순차 생성합니다. **Frame (sec)**으로 0~10초 중 캡처 시점을 고릅니다. 96×96 이미지 캐시는 최대 24개이며, 이미지가 제거되어도 분석 색상은 유지됩니다.
- **Color**로 대표 색상·Unanalyzed(미분석)·Invisible(검출된 색상 없음)을 필터링합니다. **Analyze colors**는 현재 폴더 전체를 순차 분석하며 **Stop analysis**로 중단합니다. 색상 필터를 먼저 선택하면 목록이 비어 있을 수 있으므로 전체 분석을 실행하세요.
- 프레임 변경, Refresh, 에셋 변경, Play Mode 전환, 창 닫기와 스크립트 리로드 시 메모리 썸네일과 분석 결과를 정리합니다. Refresh/프레임 변경 시 유효한 디스크 캐시를 복원합니다.
- 목록에서 선택한 하나의 prefab을 미리보기합니다. **Play**, **Restart**, 시간 slider, **Replay**, **Speed**로 재생을 조절합니다.
- 드래그로 카메라를 회전하고 휠로 확대합니다. **Fit**은 현재 파티클의 bounds에 화면을 맞춥니다. **Ping prefab**으로 원본을 찾습니다.
- 원본 에셋 변경 감지 시 현재 미리보기를 종료하고 Refresh를 안내합니다. 재생은 창에 포커스가 있을 때만 진행합니다. Play Mode 전환·창 닫기·스크립트 리로드 시 미리보기 리소스를 정리합니다.

## 지원 범위

미리보기는 별도의 Preview scene에서 Transform, ParticleSystem/ParticleSystemRenderer, MeshFilter/MeshRenderer, SpriteRenderer만 복제합니다. 프로젝트 MonoBehaviour·Animator·AudioSource·Collider는 복제하지 않습니다. 원본 prefab을 인스턴스화하거나 머티리얼을 수정하지 않습니다.

prefab 내부 sub-emitter, custom simulation space, shape의 MeshRenderer/SpriteRenderer 참조를 복제본으로 연결합니다. 비활성 root는 미리보기에서 활성화하고 자식의 활성 상태는 유지합니다. sub-emitter는 부모 시뮬레이션에서 실행하며 직접 중복 실행하지 않습니다. seed는 고정하고 시간 탐색은 처음부터 다시 시뮬레이션합니다. 탐색 시간 상한은 30초입니다.

VFX Graph, SkinnedMeshRenderer, Animator/스크립트 기반 연출, prefab 외부 오브젝트 참조, 충돌 대상, 오디오, 원본의 동적 위치 이동은 지원 범위에 포함하지 않습니다. Canvas·CanvasRenderer·Image·Mask·RectMask2D도 복제하지 않으며 Canvas 마스크의 stencil·clip rect를 재현하지 않습니다. prefab 외부 sub-emitter 연결은 복제하지 않습니다. shader는 현재 프로젝트의 render pipeline과 material에 의존하며 모든 커스텀 shader의 재생을 보장하지 않습니다. shader의 글로벌 시간은 별도로 덮어쓰지 않습니다.

썸네일은 정지 이미지이고 선택한 항목 하나만 실시간 재생합니다. 색상은 렌더링된 단일 프레임에서 밝기와 알파를 가중한 HSV 분류입니다. 좌측 아래 픽셀을 배경으로 추정하므로 화면을 가득 채우는 효과, 어두운 효과, 여러 색이 섞인 효과는 예상과 다르게 분류될 수 있습니다. 낮은 채도의 회색도 White로 분류합니다. 지연 시작·짧은 효과는 Frame을 조정하세요. 자연어 의미 검색은 포함하지 않습니다.

검색 인덱스는 Refresh 시 동기적으로 구성합니다. 썸네일은 최소 0.1초 간격으로 하나씩 생성하지만 복잡한 prefab의 시뮬레이션·렌더링은 Editor를 잠시 멈출 수 있습니다. 큰 프로젝트에서는 탐색 폴더를 좁히세요.

## 검증

`Tests/Editor`는 파티클 prefab 색인, 다중 검색어·한글·반복 필터, GUID 기반 즐겨찾기와 창 재개방 복원, 원본 prefab/material 보존, 복제본 참조, 비활성 root, 시간 탐색과 창 닫기/세션 정리를 검사합니다. 추가로 색상 분류, 실제 썸네일 색상·크기, 재캡처·캐시 상한·삭제, 제거된 이미지의 색상 유지, 색상 필터와 에셋 변경 시 정리를 검사합니다. `-nographics` 실행은 렌더링 테스트를 skip하며 성공한 렌더링으로 간주하지 않습니다.

개별 패키지 검증은 `tools/test-vfx-package.ps1`을 사용합니다. 결과 XML·로그·검증 이미지를 보존합니다. 실제 라이브러리의 커스텀 shader·대상 기기 런타임 동작은 별도 확인이 필요합니다.

### 2026-10-06 실행 결과

| Unity | Edit Mode 테스트 | 실제 미리보기 렌더링 | Windows Development 빌드 |
| --- | --- | --- | --- |
| 2022.3.62f3 | 23/23 통과, skip 0 | 미리보기·썸네일 색상 검사 통과 | 성공, VFX Editor assembly 제외 |
| 6000.3.20f1 | 23/23 통과, skip 0 | 미리보기·썸네일 색상 검사 통과 | 성공, VFX Editor assembly 제외 |

각 버전의 새 프로젝트에 이 로컬 패키지만 설치해 검증했습니다. 위 렌더링 회귀 검사는 테스트에서 생성한 prefab과 Built-in 파티클 shader를 사용합니다. 6개 패키지 정적 검사도 통과했습니다.

### 실제 프로젝트 VFX 검증

`tools/test-vfx-package.ps1`의 `-SourceAssets`와 `-VisualPaths`로 원본 Assets 경로와 그 하위의 상대 경로를 전달할 수 있습니다. `tools/copy-vfx-fixture.py`가 prefab·머티리얼·shader·텍스처와 GUID 의존 파일을 임시 프로젝트의 Assets/ProjectVfx로 복사합니다. 원본 코드를 실행하지 않도록 C# 의존성은 거부합니다. 해석할 수 없는 GUID·경로형 shader include도 실패로 처리합니다. 복사 대상은 별도 임시 프로젝트로 제한하고 원본 또는 그 하위 경로를 목적지로 사용할 수 없습니다. Python 실행 환경이 필요하며 `-NoGraphics`와 함께 사용할 수 없습니다.

검증용 fixture는 Pizza-Idle의 `Asset/PolygonShops/Prefabs/FX/FX_Fountain_Spray_01.prefab`, `FX_Fountain_Spray_02.prefab`, `3.Resources/Materials/UIAdditive.mat`입니다. 커스텀 shader 검사는 `Molip/UI_Additive` 머티리얼을 임시 녹색 파티클에 연결합니다. 실제 게임의 UI 효과 동작을 재현하는 검사는 아닙니다. 임의의 프로젝트 shader를 이 fixture의 성공 기준으로 자동 검증하지 않습니다.

실제 prefab의 1초 썸네일에서 표시되는 색상, 머티리얼 shader 지원 여부·컴파일 오류, 커스텀 shader의 녹색 렌더링과 원본/사본 SHA-256 보존을 검사합니다. 결과는 `project-vfx-source.json`, `project-vfx-result.txt`, `ProjectVfxImages`, Editor 로그에 남습니다. 에셋 사본과 캡처 이미지는 임시 프로젝트에만 보존하며 패키지에 배포하지 않습니다.

원본 프로젝트에 설치하거나 scene·prefab을 저장하지 않습니다. 검증은 Built-in pipeline의 격리된 프로젝트를 사용합니다. Pizza-Idle 원본도 Built-in입니다. 실제 UI Canvas·stencil·마스크, 창의 수동 조작과 다른 커스텀 shader는 별도 확인 대상입니다.

추가로 `tools/test-vfx-urp.ps1`에서 Unity 2022.3.62f3 / URP 14.0.12와 Unity 6000.3.20f1 / URP 17.3.0의 파티클 썸네일·시간 탐색·orbit·zoom 후 캡처를 확인했습니다. 두 환경 모두 URP Particles/Unlit 녹색 probe가 통과했고, 기존 분수와 Molip/UI_Additive도 표시됐습니다. `-FullValidation`에서는 두 환경 각각 전체 테스트 24/24, skip 0과 Windows Mono Development build 성공·VFX Editor DLL 제외를 확인했습니다. 빈 SmokeScene의 빌드 호환성 검사이며 실제 Canvas·stencil·마스크, 다른 shader와 Player 실행을 확인한 결과는 아닙니다. 자세한 결과와 보존 경로는 `docs/RELEASE_SHEETS_VFX.md`에 기록했습니다.

2026-10-07, Unity 2022.3.62f3과 6000.3.20f1에서 위 실제 prefab 두 개의 썸네일(White)과 Molip/UI_Additive의 녹색 파티클 캡처가 통과했습니다. 두 버전 모두 VFX 테스트 23/23, skip 0, Windows Development build 성공과 Editor assembly 제외를 확인했습니다. 원본·사본 12개 파일의 SHA-256이 복사 전과 일치했습니다. fixture 복사 도구의 GUID 의존성·파일 보존, 소스/목적지 중첩 거부, 누락 의존성 실패 테스트도 통과했습니다.

## 태그와 컬렉션

`Create library`로 Editor 폴더에 라이브러리 `.asset`을 만들거나 Library 필드에서 기존 에셋을 선택합니다. 선택한 효과에 쉼표로 태그와 컬렉션을 입력하고 `Apply labels`로 저장합니다. 필터는 대소문자를 무시한 정확한 라벨 이름으로 적용하며, 이름/Loop/Favorites/Color 필터와 함께 사용할 수 있습니다. GUID 기반으로 prefab 이동/rename 이후에도 유지하며 원본 prefab에는 기록하지 않습니다. 라이브러리 에셋은 팀과 공유할 수 있습니다.

## 분석 캐시 저장

썸네일 PNG와 색상은 `Library/UnityTools/VfxThumbnails`에 GUID별로 저장합니다. 창 재열기/Refresh/reload에서 색상을 복원하고 필요한 썸네일만 메모리에 불러옵니다. 프레임 시간, prefab/material/texture 의존성, 활성 렌더 파이프라인, 색 공간, 그래픽 API, Unity 버전이 바뀌면 다시 분석합니다. 에셋당 최근 설정 하나를 저장하며 다른 대표 프레임을 사용하면 이전 캐시를 대체합니다. 메모리 텍스처 제한 24개는 유지합니다. 캐시 저장 실패는 경고로 표시하고 메모리 미리보기는 계속 사용할 수 있습니다. Library 캐시는 프로젝트 공유/Player 빌드 대상이 아닙니다.

## VFX 비교 보기

`Tools > UnityTools > VFX Compare` 또는 Browser의 Compare 버튼으로 비교 창을 엽니다. Left/Right에 파티클 prefab 두 개를 연결합니다. 같은 시간, 배속, Replay 구간으로 재생하며 드래그/휠은 양쪽 카메라에 함께 적용됩니다. Fit together는 두 효과의 bounds를 합쳐 같은 카메라 위치와 스케일로 비교합니다. 선택한 시점에 Fit together를 누르면 현재 파티클 범위로 다시 맞춥니다. 두 별도 PreviewScene만 사용하며 원본/사용자 스크립트/오디오는 실행하지 않습니다. 창 닫기, reload, Play Mode 진입, 에셋 변경 시 양쪽 자원을 해제합니다. 에셋 변경 후 Refresh previews로 다시 연결합니다. Canvas/VFX Graph 등 지원 범위는 기존 Browser와 같습니다.
