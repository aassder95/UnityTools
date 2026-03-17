# UnityTools Core

`UnityTools` 프로젝트에서 공통으로 재사용 가능한 유틸리티 코드를 추출한 UPM 패키지입니다.

## 포함 범위

- Runtime
  - Timer 유틸리티 (`TaskTimer`, `PeriodTimer`, `Handle`, `StateMachine`)
  - 공통 코어 (`ObjectPool`, `Deque`, `Singleton`, `EventDispatcher`)
  - UI 유틸 (`DynamicScrollView` 계열)
  - 저장소 유틸 (`IStorage`, `PlayerPrefsStorage`, `FileStorage`)
  - 확장/헬퍼 (`CollectionExtensions`, `CoroutineHelper`, `DateTimeUtils` 등)
- Editor
  - CSV -> JSON 변환 메뉴
  - 데이터 정리 메뉴 (`PlayerPrefs`, `PersistentData`)

## 설치

### 1. Git URL 방식 (권장)

`Package Manager > Add package from git URL...` 에 아래 형태로 추가합니다.

```text
https://<git-repo-url>.git?path=/Packages/com.unitytools.core#v0.1.0
```

### 2. 로컬 파일 경로 방식

개발 레포를 서브모듈/동일 워크스페이스로 둘 경우:

```json
"com.unitytools.core": "file:../../Packages/com.unitytools.core"
```

## 마이그레이션 가이드

- 공통 코드: 패키지에서 제공
- 프로젝트 전용 코드: 기존 프로젝트 `Assets` 레이어에 유지
  - 예: 매니저 결합 코드, 게임 도메인별 테스트 윈도우

현재 레포에서는 아래 파일을 프로젝트 전용(Adapter)으로 유지합니다.

- `Assets/Scripts/Util/Core/Singletons.cs`
- `Assets/Scripts/Util/Editor/TaskTimerTestWindow.cs`
- `Assets/Scripts/Util/Editor/PeriodTimerTestWindow.cs`

## 샘플

`Samples~/QuickStart` 폴더에 최소 실행 예제가 포함되어 있습니다.

## 릴리스

아래 스크립트로 `package.json` 버전과 태그를 일치시켜 릴리스할 수 있습니다.

```powershell
pwsh ./Tools/Package/release-unitytools-core.ps1 -Version 0.1.0
```

원격 태그 푸시까지 한 번에 수행하려면:

```powershell
pwsh ./Tools/Package/release-unitytools-core.ps1 -Version 0.1.0 -Push
```
