# UI 2.2.0 / Timer 1.2.0 플랫폼 검증

공개 tag의 IL2CPP 빌드와 Player 검증을 반복 실행하기 위한 도구입니다. 프로젝트 설정을 변경하므로 실제 개발 프로젝트가 아닌 `test-upm-compatibility.ps1`이 생성한 임시 `timer-lab` 프로젝트를 사용합니다. UI·Timer runtime, 기존 prefab·scene·meta에는 변경이 없습니다.

## 실행

먼저 공개 tag를 빈 프로젝트에 설치하여 Timer Lab 테스트 46개를 통과시킵니다.

```powershell
& tools/test-upm-compatibility.ps1 -Source Remote -UiRef unitytools-ui/v2.2.0 -TimerRef unitytools-timer/v1.2.0 -UnityVersion 6000.3.20f1 -Scenarios timer-lab
& tools/test-ui-timer-il2cpp.ps1 -Project '<결과 경로>/timer-lab' -Target Android
& tools/test-ui-timer-il2cpp.ps1 -Project '<결과 경로>/timer-lab' -Target Windows
& tools/test-ui-timer-il2cpp.ps1 -Project '<결과 경로>/timer-lab' -Target Windows -Backend Mono
```

Unity 2022.3.62f3도 같은 방식으로 실행합니다. 같은 프로젝트에 두 빌드를 동시에 실행하지 않습니다. Android Build Support와 SDK·NDK·JDK, Windows는 Visual Studio C++ 컴파일 도구와 Windows SDK가 필요합니다.

Windows 실행 전 `vswhere`로 C++ 컴파일 도구 설치를 확인합니다. 없으면 manifest·검증 소스·빌드 출력 폴더를 변경하기 전에 원인을 알려주고 종료합니다. 도구 설치가 확인되어도 Unity 버전별 toolchain 호환성과 Windows SDK의 실제 사용 가능 여부는 native 빌드로 확인해야 합니다.

`-Backend Mono`는 Windows Player 검증 코드를 재현하는 별도 경로입니다. stripping은 Disabled이며 summary에 Backend=Mono, NativeAssemblyVerified=false로 기록합니다. IL2CPP native 실행 성공과 구분합니다. 기본 backend는 IL2CPP이며 Android에서는 Mono를 허용하지 않습니다.

도구는 UI 2.2.0 태그를 추가 설치하고 UI·Timer lock의 source=git 및 `cf83ef4377611d7c80a78544da34be3f2f60b33d`를 확인합니다. 검증 전용 장면에서 Inspector 참조를 명시적으로 연결합니다. 기존 import한 샘플 장면도 빌드에 포함합니다. 빌드마다 별도 결과 폴더에 로그·산출물·summary를 저장합니다.

Windows는 native `GameAssembly.dll` 확인 후 Player를 실행해 결과 파일과 종료 코드를 검사합니다. Android는 APK의 `lib/arm64-v8a/libil2cpp.so`와 managed 패키지 DLL 잔재를 검사하며 기기 실행은 수행하지 않습니다.

## Player 검증 범위

- Timer Simulation Lab의 8개 가상 UTC·저장 상태 실험
- timeScale=0에서 반복 버튼 실행
- 다른 pointerId의 release가 현재 입력을 해제하지 않는 동작
- 반복 후 release click 중복 방지
- 눌림 스케일과 release·비활성화 시 복원

버튼 콜백을 직접 호출하는 검증입니다. 실제 OS 터치 전달, 화면 배치, Safe Area, 회전, 모바일 lifecycle 이벤트는 별도 확인해야 합니다.

## 2026-10-07 결과

| 항목 | Unity 2022.3.62f3 | Unity 6000.3.20f1 |
| --- | --- | --- |
| Windows IL2CPP High stripping | C++ toolchain 부재로 중단 | C++ toolchain 부재로 중단 |
| Android ARM64 IL2CPP High stripping | 빌드·APK 검사 통과 | 빌드·APK 검사 통과 |
| Windows Mono Player probe | Passed, 종료 코드 0 | Passed, 종료 코드 0 |

Windows 두 버전은 C# 컴파일과 stripping 단계를 진행했으나 native C++ toolchain을 찾지 못했습니다. 기존 Visual Studio 설치에 C++ 구성 요소가 없다는 로그를 확인했습니다. IL2CPP 성공으로 간주하지 않습니다. 검증 코드 자체는 Unity 2022.3의 임시 프로젝트에서 Mono 빌드로 실행했고 Player log에 예외 없이 통과했습니다. 이 실행은 IL2CPP runtime 검증을 대체하지 않습니다.

후속 검증에서는 `-Backend Mono`로 두 Unity 버전의 빌드·Player를 다시 통과했습니다. 두 summary 모두 Stripping=Disabled, NativeAssemblyVerified=false, Player=Passed이며 종료 코드는 0입니다. Windows IL2CPP 도구 누락 시 manifest와 출력 폴더를 보존하고 사전 중단하는 경로, Android+Mono를 사전 거부하는 경로, PowerShell 구문 검사를 확인했습니다.

연결된 Android emulator 두 대는 `adb devices`에서 offline 상태였으며 APK를 설치하거나 실행하지 않았습니다. iOS, 실제 기기 터치·Safe Area·회전, Windows IL2CPP Player 실행은 미검증입니다. 새 `[TEST]` 로그는 없습니다.

결과 경로:

- Windows 2022.3 실패: `C:/Users/search/AppData/Local/Temp/UnityTools-Compatibility-2022.3.62f3-2a0d56bde1f3465d99f38ef884e10f3e/timer-lab/UiTimerIl2Cpp-Windows-a5b4a8a90e604185ab9c72be95675ec5`
- Windows 6.3 실패: `C:/Users/search/AppData/Local/Temp/UnityTools-Compatibility-6000.3.20f1-c8f12f64a15a4a818e094a05ecbecc95/timer-lab/UiTimerIl2Cpp-Windows-e384cbfe00b04c52aeb2c70bcfcb782b`
- Mono Player: `C:/Users/search/AppData/Local/Temp/UnityTools-Compatibility-2022.3.62f3-2a0d56bde1f3465d99f38ef884e10f3e/timer-lab/UiTimerMonoProbe`
- Mono 2022.3 재검증: `C:/Users/search/AppData/Local/Temp/UnityTools-Compatibility-2022.3.62f3-2a0d56bde1f3465d99f38ef884e10f3e/timer-lab/UiTimer-Mono-Windows-82114674a17d431980e73c4fc8999a04`
- Mono 6.3 재검증: `C:/Users/search/AppData/Local/Temp/UnityTools-Compatibility-6000.3.20f1-c8f12f64a15a4a818e094a05ecbecc95/timer-lab/UiTimer-Mono-Windows-fe94424137df48f2bf728cca9ed033c7`
- Android 2022.3: `C:/Users/search/AppData/Local/Temp/UnityTools-Compatibility-2022.3.62f3-2a0d56bde1f3465d99f38ef884e10f3e/timer-lab/UiTimerIl2Cpp-Android-5229905913ef49349c1cfa403719f7fa`
- Android 6.3: `C:/Users/search/AppData/Local/Temp/UnityTools-Compatibility-6000.3.20f1-c8f12f64a15a4a818e094a05ecbecc95/timer-lab/UiTimerIl2Cpp-Android-c9ac3d287caf420f99a24a3109b6be76`
