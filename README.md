# UnityTools

Unity 2022.3 이상에서 UI, Timer, Benchmark, Persistence 기능을 각각 독립 설치할 수 있는 UPM 패키지입니다. 데모 프로젝트는 Unity 6.3 LTS를 사용하고 패키지의 최소 지원 버전은 2022.3을 유지합니다. 저장소는 하나지만 package assembly와 dependency는 분리되어 있습니다. Benchmark와 Persistence는 아직 개발 버전이며 release tag가 없습니다.

## 패키지

| 패키지 | 버전 | Assembly | 주요 책임 |
| --- | --- | --- | --- |
| `com.aassder95.unitytools.ui` | `2.0.0` | `UnityTools.Ui` | MVP, navigation, transition, focus, safe area, DynamicScroll |
| `com.aassder95.unitytools.timer` | `1.0.0` | `UnityTools.Timer` | Task/Period Timer, service/handle, UTC, persistence |
| `com.aassder95.unitytools.benchmark` | `0.1.0` | `UnityTools.Benchmark` | Main Thread, GC, memory, custom marker 측정 및 CSV 출력 |
| `com.aassder95.unitytools.persistence` | `0.1.0` | `UnityTools.Persistence` | 버전형 저장, migration, 검증, 백업 복구 |

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

Benchmark 개발 버전은 Package Manager의 `Add package from disk...`에서 현재 checkout의 `UnityTools/Packages/com.aassder95.unitytools.benchmark/package.json`을 선택합니다. 사용법은 [Benchmark README](UnityTools/Packages/com.aassder95.unitytools.benchmark/README.md)를 참고하세요.

Persistence 개발 버전도 `Add package from disk...`에서 `UnityTools/Packages/com.aassder95.unitytools.persistence/package.json`을 선택합니다. 사용법은 [Persistence README](UnityTools/Packages/com.aassder95.unitytools.persistence/README.md)를 참고하세요.

UI 기본 패키지는 Timer와 Input System에 의존하지 않습니다. Input System Back 입력이 필요하면 프로젝트에 `com.unity.inputsystem`을 추가하면 `UnityTools.Ui.InputSystem` 선택 assembly가 활성화됩니다.

## Breaking migration

이번 UI 2.0은 호환 shim을 제공하지 않는 breaking release입니다. `UnityTools.Util.*`, 전역 Manager/Singleton, 범용 pooling/persistence API를 사용하던 프로젝트는 [MIGRATION.md](MIGRATION.md)를 따라 명시적인 package API로 이전해야 합니다.

## 검증

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\verify-release.ps1 -Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\test-upm-packages.ps1 -Source Remote
```

UPM 검증 스크립트는 Timer 단독, UI 단독(Input System 없음), UI+Input System 프로젝트를 각각 생성하고 패키지 PlayMode 테스트를 실행합니다.

Unity 6.3에서 Git 설치, 샘플 Import, PlayMode 테스트, Windows Development Build까지 확인하려면 다음 명령을 실행합니다.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\test-upm-compatibility.ps1
```

기본 Editor는 `6000.3.20f1`이며, 검증 대상 원격 커밋은 스크립트의 `UiRef`, `TimerRef`, `BenchmarkRef`, `PersistenceRef`에 고정합니다. 개발 브랜치의 검증이며 release tag 검증과 구분합니다. `-Scenarios timer`처럼 한 시나리오만 선택할 수 있고, 로컬 변경은 `-Source Local`로 검증합니다. 실행 결과와 로그, packages-lock, 테스트 XML, 빌드는 출력된 임시 프로젝트 경로에 보존합니다. Editor 설치 경로가 다르면 `-UnityPath`를 지정하세요.

기본 시나리오는 Timer, UI, UI+Input System, Benchmark, Persistence, UI Performance Lab입니다. UI와 Benchmark의 단독 프로젝트는 샘플 의존성이 없는 상태를 검증하고, UI 샘플은 Input System 조합에서, 성능 실험실은 Benchmark+UI 조합에서 Import합니다. 장면의 누락 스크립트도 검사합니다. Player 빌드 성공은 실제 실행, 화면 배치, 터치 또는 대상 기기 성능 검증을 의미하지 않습니다.

2026-09-30, Unity `6000.3.20f1`에서 빈 프로젝트별 Git URL 설치를 검증했습니다. UI·Timer·Benchmark는 `18665ab98b9bc97c9f664a6a0407ba1353f59145`, Persistence는 `472a08ef24418e81045dcc66109c1a9b7c86606b` 기준입니다.

| 시나리오 | 샘플 Import | PlayMode 테스트 | Windows Development Build |
| --- | --- | --- | --- |
| Timer 단독 | Timer Sample Scene | 17/17 | 성공 |
| UI 단독 | 미사용 | 24/24 | 성공 |
| UI + Input System | UI Sample Scene | 25/25 | 성공 |
| Benchmark 단독 | 미사용 | 4/4 | 성공 |
| Persistence 단독 | 등록된 샘플 없음 | 6/6 | 성공 |
| Benchmark + UI | UI Performance Lab | 9/9 | 성공 |

Unity 6.3의 lock 파일에서 uGUI `2.0.0`, TMP `5.0.0`, Test Framework `1.6.0` 내장 패키지 해석을 확인했습니다. Windows 빌드는 Mono backend로 검증했고 Android/iOS, IL2CPP, 직접 화면 조작과 실제 기기 성능은 이 결과에 포함하지 않습니다.

## 라이선스

UnityTools 자체 코드는 [MIT License](LICENSE)로 배포됩니다. 프로젝트 개발용 `Assets/Plugins`의 DOTween Standard는 UPM 패키지에 포함되지 않으며 원 저작권자의 라이선스를 따릅니다. 출처와 버전은 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)를 확인하세요.

보안 정리로 Git 이력이 재작성됐으므로 정리 이전 clone은 재사용하지 말고 새로 clone해야 합니다. 자세한 기록은 [SECURITY_CLEANUP.md](SECURITY_CLEANUP.md)에 있습니다.
