# UnityTools UI Framework

Unity 2022.3 이상에서 사용할 수 있는 uGUI 기반 UI 패키지입니다.

## 포함 기능

- Model / View / Presenter 수명주기와 모델 갱신 이벤트
- 오브젝트 풀을 사용하는 가상화 동적 스크롤 뷰
- 세로 / 가로, 고정 개수 / 자동 맞춤 레이아웃
- Task / Period 타이머와 샘플 UI 매니저
- DynamicScrollView 전용 Inspector

## 설치

Unity Package Manager의 `Add package from git URL...`에서 다음 주소를 사용합니다.

```text
https://github.com/aassder95/UnityTools.git?path=/UnityTools/Packages/com.aassder95.unitytools.ui#develop
```

현재 저장소는 인증 없이 접근할 수 없으므로 Unity를 실행하기 전에 GitHub 저장소 접근 권한을 Git에 구성해야 합니다. 인증이 없는 배치 또는 CI 환경에서는 Git URL 설치가 실패합니다.

로컬 개발 프로젝트에서는 `Packages/com.aassder95.unitytools.ui` 임베디드 패키지로 바로 로드됩니다.

## 샘플

Package Manager에서 `UI Sample Scene`을 Import하면 Inventory, Rank, Timer 샘플과 필요한 프리팹 및 폰트가 `Assets/Samples` 아래에 복사됩니다. 이 기본 샘플은 외부 플러그인 없이 컴파일됩니다.

가상화 Rank 예제는 별도 `OSA Rank Sample`로 제공됩니다. 해당 샘플에는 제3자 플러그인이 포함되지 않으므로, 별도 배포되는 `OSA.Core`를 먼저 설치한 뒤 Import해야 합니다.

두 샘플은 서로 독립적이며 필요한 샘플만 선택해서 Import할 수 있습니다.

## 런타임 어셈블리

- `UnityTools.Util`: UI 프레임워크와 공용 런타임 유틸리티
- `UnityTools.Manager`: 타이머 매니저
- `UnityTools.Util.Editor`: DynamicScrollView와 데이터 도구 Inspector
