# UnityTools VFX Browser

Unity 2022.3 이상에서 사용하는 Editor 전용 독립 UPM 패키지입니다. UI·Timer·Persistence·외부 tween 패키지에 의존하지 않으며 Player assembly를 포함하지 않습니다. 현재 0.1.0 개발 버전으로 release tag는 없습니다.

## 설치와 사용

Package Manager의 `Add package from disk...`에서 이 폴더의 `package.json`을 선택합니다. 이 저장소의 데모 프로젝트에서는 embedded package로 사용합니다. `Tools > UnityTools > VFX Browser`를 엽니다.

- 상단에 Project 폴더를 지정하고 **Refresh**로 파티클을 포함한 prefab을 찾습니다. 최초 기본 폴더는 `Assets`입니다.
- 이름과 경로를 함께 검색합니다. 공백으로 나눈 모든 검색어가 일치해야 하며 영문 대소문자를 구분하지 않습니다.
- **Looping / OneShot**, **Favorites**, 별 버튼으로 목록을 좁힙니다. 즐겨찾기는 프로젝트별 EditorPrefs에 GUID로 저장해 prefab 이동·이름 변경 후에도 유지됩니다.
- Looping은 비활성 자식을 포함한 하위 ParticleSystem 중 하나라도 loop 설정이 켜져 있는 prefab입니다. 목록에는 시스템 개수를 표시하고, 미리보기 아래에는 현재 살아 있는 파티클 수를 표시합니다.
- 목록에서 선택한 하나의 prefab을 미리보기합니다. **Play**, **Restart**, 시간 slider, **Replay**, **Speed**로 재생을 조절합니다.
- 드래그로 카메라를 회전하고 휠로 확대합니다. **Fit**은 현재 파티클의 bounds에 화면을 맞춥니다. **Ping prefab**으로 원본을 찾습니다.
- 원본 에셋 변경 감지 시 현재 미리보기를 종료하고 Refresh를 안내합니다. 재생은 창에 포커스가 있을 때만 진행합니다. Play Mode 전환·창 닫기·스크립트 리로드 시 미리보기 리소스를 정리합니다.

## 지원 범위

미리보기는 별도의 Preview scene에서 Transform, ParticleSystem/ParticleSystemRenderer, MeshFilter/MeshRenderer, SpriteRenderer만 복제합니다. 프로젝트 MonoBehaviour·Animator·AudioSource·Collider는 복제하지 않습니다. 원본 prefab을 인스턴스화하거나 머티리얼을 수정하지 않습니다.

prefab 내부 sub-emitter, custom simulation space, shape의 MeshRenderer/SpriteRenderer 참조를 복제본으로 연결합니다. 비활성 root는 미리보기에서 활성화하고 자식의 활성 상태는 유지합니다. sub-emitter는 부모 시뮬레이션에서 실행하며 직접 중복 실행하지 않습니다. seed는 고정하고 시간 탐색은 처음부터 다시 시뮬레이션합니다. 탐색 시간 상한은 30초입니다.

VFX Graph, SkinnedMeshRenderer, Animator/스크립트 기반 연출, prefab 외부 오브젝트 참조, 충돌 대상, 오디오, 원본의 동적 위치 이동은 지원 범위에 포함하지 않습니다. prefab 외부 sub-emitter 연결은 복제하지 않습니다. shader는 현재 프로젝트의 render pipeline과 material에 의존하며 모든 커스텀 shader의 재생을 보장하지 않습니다. shader의 글로벌 시간은 별도로 덮어쓰지 않습니다.

현재 버전은 목록과 선택한 항목 하나의 미리보기입니다. 다중 실시간 썸네일, 색상 분석, 자연어 의미 검색 인덱스는 포함하지 않습니다. 검색 인덱스는 Refresh 시 동기적으로 구성하므로 큰 프로젝트에서는 탐색 폴더를 좁히세요.

## 검증

`Tests/Editor`는 파티클 prefab 색인, 다중 검색어·한글·반복 필터, GUID 기반 즐겨찾기와 창 재개방 복원, 원본 prefab/material 보존, 복제본 참조, 비활성 root, 시간 탐색과 창 닫기/세션 정리를 검사합니다. 그래픽 장치가 있는 Editor 실행에서는 실제 렌더링 결과의 픽셀도 검사합니다. `-nographics` 실행은 렌더링 테스트를 skip하며 성공한 렌더링으로 간주하지 않습니다.

개별 패키지 검증은 `tools/test-vfx-package.ps1`을 사용합니다. 결과 XML·로그·검증 이미지를 보존합니다. 실제 라이브러리의 커스텀 shader·대상 기기 런타임 동작은 별도 확인이 필요합니다.

### 2026-10-06 실행 결과

| Unity | Edit Mode 테스트 | 실제 미리보기 렌더링 | Windows Development 빌드 |
| --- | --- | --- | --- |
| 2022.3.62f3 | 12/12 통과, skip 0 | 픽셀 검사 통과 | 성공, VFX Editor assembly 제외 |
| 6000.3.20f1 | 12/12 통과, skip 0 | 픽셀 검사 통과 | 성공, VFX Editor assembly 제외 |

각 버전의 새 프로젝트에 이 로컬 패키지만 설치해 검증했습니다. 렌더링 검사는 테스트에서 생성한 prefab과 Built-in 파티클 shader를 사용합니다. 패키지 정적 검사 5개 패키지와 검증 도구 회귀 테스트 19개도 통과했습니다. 실제 프로젝트 prefab과 커스텀 render pipeline에서의 수동 GUI 조작은 아직 검증하지 않았습니다.
