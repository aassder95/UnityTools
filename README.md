# UnityTools

Unity 2022.3용 UI와 Timer 기능을 각각 독립 설치할 수 있는 UPM 패키지입니다. 저장소는 하나지만 패키지 assembly, dependency, sample, release tag는 분리되어 있습니다.

## 패키지

| 패키지 | 버전 | Assembly | 주요 책임 |
| --- | --- | --- | --- |
| `com.aassder95.unitytools.ui` | `2.0.0` | `UnityTools.Ui` | MVP, navigation, transition, focus, safe area, DynamicScroll |
| `com.aassder95.unitytools.timer` | `1.0.0` | `UnityTools.Timer` | Task/Period Timer, service/handle, UTC, persistence |

## 설치

Unity Package Manager의 `Add package from git URL...`에 필요한 패키지 주소를 입력합니다.

UI:

```text
https://github.com/aassder95/UnityTools.git?path=/UnityTools/Packages/com.aassder95.unitytools.ui#unitytools-ui/v2.0.0
```

Timer:

```text
https://github.com/aassder95/UnityTools.git?path=/UnityTools/Packages/com.aassder95.unitytools.timer#unitytools-timer/v1.0.0
```

UI 기본 패키지는 Timer와 Input System에 의존하지 않습니다. Input System Back 입력이 필요하면 프로젝트에 `com.unity.inputsystem`을 추가하면 `UnityTools.Ui.InputSystem` 선택 assembly가 활성화됩니다.

## Breaking migration

이번 UI 2.0은 호환 shim을 제공하지 않는 breaking release입니다. `UnityTools.Util.*`, 전역 Manager/Singleton, 범용 pooling/persistence API를 사용하던 프로젝트는 [MIGRATION.md](MIGRATION.md)를 따라 명시적인 package API로 이전해야 합니다.

## 검증

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\verify-release.ps1 -Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\test-upm-packages.ps1 -Source Remote
```

UPM 검증 스크립트는 Timer 단독, UI 단독(Input System 없음), UI+Input System 프로젝트를 각각 생성하고 패키지 PlayMode 테스트를 실행합니다.

## 라이선스

UnityTools 자체 코드는 [MIT License](LICENSE)로 배포됩니다. 프로젝트 개발용 `Assets/Plugins`의 DOTween Standard는 UPM 패키지에 포함되지 않으며 원 저작권자의 라이선스를 따릅니다. 출처와 버전은 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)를 확인하세요.

보안 정리로 Git 이력이 재작성됐으므로 정리 이전 clone은 재사용하지 말고 새로 clone해야 합니다. 자세한 기록은 [SECURITY_CLEANUP.md](SECURITY_CLEANUP.md)에 있습니다.
