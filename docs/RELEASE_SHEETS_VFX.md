# Sheets / VFX 0.1.0 배포 준비

현재 두 패키지는 0.1.0 개발 버전이며 공개 release tag는 없습니다. 아래 주소는 배포 준비를 위해 고정한 개발 커밋의 Git 설치 주소입니다. 정식 릴리스 발행을 의미하지 않습니다.

| 패키지 | 버전 | 예정 tag | 의존성 |
| --- | --- | --- | --- |
| com.aassder95.unitytools.sheets | 0.1.0 | unitytools-sheets/v0.1.0 | 없음 |
| com.aassder95.unitytools.vfx | 0.1.0 | unitytools-vfx/v0.1.0 | 없음, Editor 전용 |

검증 소스는 `f9c1cf1b48086b04572ec0399bc284a0909625a2`입니다. Sheets 그룹 조회·enum·배열 지원은 `6439032`, 실제 VFX와 Editor 창 검증 도구는 `f9c1cf1`에 포함됐습니다. 버전은 유지하며 tag는 생성하거나 push하지 않았습니다. Timer·Persistence·데모 scene의 별도 작업은 이번 배포 범위에 포함하지 않습니다.

## 고정 개발 커밋 설치

Package Manager의 Add package from git URL에 필요한 패키지 주소를 입력합니다.

```text
https://github.com/aassder95/UnityTools.git?path=/UnityTools/Packages/com.aassder95.unitytools.sheets#f9c1cf1b48086b04572ec0399bc284a0909625a2
https://github.com/aassder95/UnityTools.git?path=/UnityTools/Packages/com.aassder95.unitytools.vfx#f9c1cf1b48086b04572ec0399bc284a0909625a2
```

두 패키지 모두 Unity 2022.3 이상을 지원하며 패키지 안에 MIT 라이선스가 있습니다. Sheets Runtime은 UnityEngine 참조가 없는 일반 C# assembly이고 생성기는 Editor 전용입니다. VFX에는 Player용 assembly가 없습니다. 검증에 사용한 Pizza-Idle의 CSV·에셋 사본과 캡처 이미지는 임시 프로젝트에만 보존하며 패키지에 넣지 않습니다.

## 재현 방법

저장소 루트에서 다음 명령을 실행합니다. UnityVersion을 `6000.3.20f1`로 바꿔 Unity 6도 검사합니다. 설치된 Editor와 Windows build support, Git, 실제 렌더링을 위한 그래픽 장치가 필요합니다.

```powershell
$sourceRef = 'f9c1cf1b48086b04572ec0399bc284a0909625a2'
& tools/test-sheets-package.ps1 -UnityVersion 2022.3.62f3 -SourceRef $sourceRef
& tools/test-vfx-package.ps1 -UnityVersion 2022.3.62f3 -SourceRef $sourceRef
```

SourceRef를 생략하면 로컬 경로 패키지를 검사합니다. Git 검증과 구분하세요. `confirm-upm-source.ps1`은 manifest의 정확한 URL, lock의 source=git·hash, 설치 cache의 package 이름·0.1.0 버전·빈 dependency를 검사합니다. 프로젝트마다 `git-source-result.json`, `summary.json`, `editor-version.txt`, XML·로그·빌드를 보존합니다.

Sheets의 `-ProjectCsvPaths`에는 SauceSeq_Palette.csv와 SauceSeq_Stages.csv 경로를 전달할 수 있습니다. ID 단일 조회, randomWeight/reward1Key 그룹, 숫자·bool·answer 정수 배열의 검증과 생성 코드 컴파일을 검사합니다. 게임 프로젝트 로드 경로는 변경하지 않습니다.

VFX의 `-SourceAssets`와 `-VisualPaths`는 실제 prefab·머티리얼의 상대 경로를 받습니다. 현재 실제 VFX fixture는 Pizza-Idle의 분수 prefab 두 개와 UIAdditive.mat입니다. Python이 GUID 의존 파일을 별도 프로젝트에 복사하며 원본·사본 SHA-256 보존을 검사합니다. 자세한 대상과 한계는 VFX README에 있습니다.

## 검증 결과

2026-10-07 공개 Git 주소의 고정 소스 `f9c1cf1b48086b04572ec0399bc284a0909625a2`를 별도 프로젝트에 설치했습니다. 네 프로젝트 모두 manifest·lock·cache의 패키지 이름, 버전 0.1.0, 의존성 및 source hash를 확인했습니다. 실행한 Editor 버전도 각 프로젝트의 `editor-version.txt`로 확인했습니다.

| Unity | 패키지 | Editor 테스트 | PlayMode 테스트 | 추가 검증 |
| --- | --- | --- | --- | --- |
| 2022.3.62f3 | Sheets | 43/43 | 56/56 | 생성 코드 컴파일·읽기, Windows build·Player 통과 |
| 6000.3.20f1 | Sheets | 43/43 | 56/56 | 생성 코드 컴파일·읽기, Windows build·Player 통과 |
| 2022.3.62f3 | VFX | 24/24 | 대상 없음 | 실제 VFX 3건·Windows build 통과 |
| 6000.3.20f1 | VFX | 24/24 | 대상 없음 | 실제 VFX 3건·Windows build 통과 |

모든 테스트의 실패·skip은 0입니다. 두 Unity 버전에서 실제 CSV의 Palette 7행·Stages 10행, ID 단일 조회와 그룹 조회, enum 및 배열 생성 코드를 검사했습니다. VFX는 분수 prefab 2개와 Molip/UI_Additive 초록색 probe를 검사했고 원본·복사본 12개 파일의 hash를 확인했습니다. 네 Windows build 모두 Editor assembly가 제외됐습니다. 검증 도구 회귀 테스트는 36/36 통과했습니다.

검증 프로젝트와 결과는 아래 임시 경로에 보존했습니다. 각 폴더의 `summary.json`, `git-source-result.json`, `results-*.xml`과 단계별 로그를 확인할 수 있습니다. 임시 폴더는 운영체제 정리 대상이 될 수 있습니다.

```text
C:\Users\search\AppData\Local\Temp\UnityTools-Sheets-2022.3.62f3-857cc275ee8b422587eb7f4717c46e4a
C:\Users\search\AppData\Local\Temp\UnityTools-Sheets-6000.3.20f1-e377dcda91bc42b29ad270d6fa5a7144
C:\Users\search\AppData\Local\Temp\UnityTools-Vfx-2022.3.62f3-5ac43e4750664f3d97700315c08f87b0
C:\Users\search\AppData\Local\Temp\UnityTools-Vfx-6000.3.20f1-6fadc999820b48efa119b64b57e05e8d
```

## IL2CPP 추가 검증

`tools/test-sheets-il2cpp.ps1`은 앞선 Git 설치 검증이 완료된 Sheets 임시 프로젝트를 받아 IL2CPP·High managed stripping으로 추가 빌드합니다. 같은 프로젝트에서 동시에 실행하지 마세요. 이 도구는 임시 프로젝트의 빌드 대상·backend·stripping 설정을 변경하며 기존 Mono 결과와 별도 `Il2Cpp-<id>` 폴더에 결과를 기록합니다. 제품 프로젝트에서는 실행하지 않습니다.

```powershell
& tools/test-sheets-il2cpp.ps1 -Project '<Sheets 검증 프로젝트 절대 경로>' -Target Windows
& tools/test-sheets-il2cpp.ps1 -Project '<Sheets 검증 프로젝트 절대 경로>' -Target Android
```

Windows는 GameAssembly.dll과 managed Sheets assembly 제외 여부를 검사하고 생성 코드의 Player 검증을 실행합니다. Android는 ARM64 APK의 `lib/arm64-v8a/libil2cpp.so`와 managed Sheets assembly 제외 여부를 검사합니다. Android 기기에 설치하거나 실행하지 않으며 summary의 Player는 `Not run (Android build only)`입니다. 생성 코드 검증은 한글·문화권, 일곱 가지 배열 타입, enum, 단일 키·그룹 조회, 잘못된 입력의 부분 결과 미공개를 포함합니다.

2026-10-07 Windows IL2CPP는 Unity 2022.3.62f3·6000.3.20f1 모두 `ToolchainNotFoundException`으로 실패했습니다. Unity 로그가 Visual Studio C++ tool components와 Windows SDK 누락을 보고했습니다. IL2CPP 모듈은 설치돼 있으나 네이티브 컴파일·Player 실행은 검증하지 못했습니다. 기존 Windows Mono 성공 결과와 구분합니다.

실패 로그:

```text
C:\Users\search\AppData\Local\Temp\UnityTools-Sheets-2022.3.62f3-857cc275ee8b422587eb7f4717c46e4a\Il2Cpp-6c01d3c581c0490782556cb4a9f3a2ec\build.log
C:\Users\search\AppData\Local\Temp\UnityTools-Sheets-6000.3.20f1-e377dcda91bc42b29ad270d6fa5a7144\Il2Cpp-28f8b400a99b41c98394fd8a890c4946\build.log
```

Android ARM64 IL2CPP는 두 Unity 버전 모두 High stripping으로 APK 빌드에 성공했습니다. APK의 ARM64 `libil2cpp.so`와 managed Sheets assembly 제외 여부를 확인했습니다. 현재 adb에서 두 에뮬레이터가 offline으로 표시돼 기기 실행 증거는 없습니다. 네이티브 코드 생성·컴파일·패키징의 성공이며 enum reflection과 그룹 조회의 Android 실행 성공으로 해석하지 않습니다.

| Unity | Android ARM64 IL2CPP | 기기 실행 |
| --- | --- | --- |
| 2022.3.62f3 | APK 빌드·native library 확인 통과 | 미실행 |
| 6000.3.20f1 | APK 빌드·native library 확인 통과 | 미실행 |

성공 결과의 `summary.json`, `build.log`, `Sheets.apk`:

```text
C:\Users\search\AppData\Local\Temp\UnityTools-Sheets-2022.3.62f3-857cc275ee8b422587eb7f4717c46e4a\Il2Cpp-6f89b15fbaa04a96934c8e4f7fff0c6b
C:\Users\search\AppData\Local\Temp\UnityTools-Sheets-6000.3.20f1-e377dcda91bc42b29ad270d6fa5a7144\Il2Cpp-07a06b6231a5459fb40459e4081eafd3
```

검증 도구 자체는 Windows PowerShell 구문 검사와 미검증 로컬 소스 입력의 변경 전 거절을 확인했습니다. 두 Editor에서 C# 검증 스크립트가 컴파일됐으며 Windows 실패 경로와 Android 성공·APK 검사 경로를 실행했습니다. Windows IL2CPP Player 성공 경로는 환경 제약으로 미검증입니다.

## URP 미리보기 추가 검증

Pizza-Idle 원본은 Built-in pipeline입니다. manifest에 URP가 없고 GraphicsSettings의 `m_CustomRenderPipeline`, 모든 QualitySettings의 `customRenderPipeline` 참조가 0임을 확인했습니다. 이전 문서의 원본 URP 표현을 정정합니다.

`tools/test-vfx-urp.ps1`은 Git 설치 검증이 완료된 VFX 임시 프로젝트에 URP와 검증용 renderer/pipeline asset을 설정합니다. 제품 프로젝트에서는 실행하지 않습니다. 같은 프로젝트에서 동시에 실행하지 마세요. 기존 Built-in 검증 결과는 유지하고 추가 결과를 `Urp-<id>` 폴더에 기록합니다.

```powershell
& tools/test-vfx-urp.ps1 -Project '<VFX 검증 프로젝트 절대 경로>'
```

| Unity | URP | 파티클 미리보기 | 시간 탐색·orbit·zoom 후 캡처 |
| --- | --- | --- | --- |
| 2022.3.62f3 | 14.0.12 | Green 통과 | Green 통과 |
| 6000.3.20f1 | 17.3.0 | Green 통과 | Green 통과 |

고정 VFX 소스는 이전과 같은 `f9c1cf1b48086b04572ec0399bc284a0909625a2`입니다. 활성 pipeline, 실행한 Editor/URP 버전과 lock을 확인했습니다. 원본·사본 12개 파일의 SHA-256은 유지됐습니다. 두 버전에서 기존 분수 2개는 White, Molip/UI_Additive probe는 Green으로 관측했습니다. 기존 shader의 이번 미리보기 결과이며 URP용 shader 전반이나 실제 UI Canvas·stencil·마스크의 호환성을 보장하지 않습니다. 이번 추가 검증은 기존 24개 테스트 전체나 URP Player build를 다시 실행한 결과가 아닙니다.

Unity 2022.3 최초 시도는 요청 URP 14.0.11과 Editor가 설치한 실제 14.0.12가 달라 버전 검증에서 거절됐습니다. 도구의 요청을 실제 버전에 맞추고 재실행해 위 결과를 확인했습니다. 미검증 소스 입력의 변경 전 거절과 PowerShell 구문 검사도 통과했습니다. 제품 VFX 코드는 변경하지 않았습니다.

`summary.json`, `result.txt`, 단계별 로그와 PNG 캡처:

```text
C:\Users\search\AppData\Local\Temp\UnityTools-Vfx-2022.3.62f3-5ac43e4750664f3d97700315c08f87b0\Urp-f70855d0278c463c95aa7d48aab2ed2b
C:\Users\search\AppData\Local\Temp\UnityTools-Vfx-6000.3.20f1-6fadc999820b48efa119b64b57e05e8d\Urp-66b98e602e8944e5897aef87c1e6ef28
```

## 사용 흐름과 남은 확인

자동 Editor 창 검사는 CSV 읽기 후 enum 배열·정수 배열 설정과 생성 소스 미리보기를 두 Editor 프레임에 걸쳐 그립니다. VFX는 색상 필터·썸네일·선택한 미리보기를 그린 뒤 창 닫기에서 리소스 정리를 확인합니다. 이 검사는 입력 장치로 버튼·체크박스를 직접 조작하는 수동 테스트를 대신하지 않습니다.

- CSV Generator: enum 타입명 입력·잘못된 타입 오류, Array 선택·해제, Validate & Preview, 파일 저장 대화상자·덮어쓰기·한글 경로를 수동 확인합니다.
- VFX Browser: 폴더·검색·페이지 이동·즐겨찾기·색상 필터·전체 분석/중단·Frame 변경과 카메라 조작을 수동 확인합니다.
- 실제 프로젝트: 원본 Built-in의 실제 UI Canvas·stencil·마스크를 확인합니다. 별도 URP 프로젝트에서는 파티클 캡처를 확인했으며 Canvas 동작·전체 테스트·Player build는 별도 확인 대상입니다.
- 런타임: 모바일·IL2CPP에서 Sheets 데이터 로드와 생성 코드 사용을 확인합니다. Android ARM64 IL2CPP·High stripping APK 빌드는 통과했으며, 실행 증거는 Windows Mono Development build입니다.

## 발행 순서

1. 위 수동·대상 환경 확인 결과와 허용할 제한을 기록합니다.
2. 발행할 소스를 확정합니다. 코드가 달라지면 새 고정 commit으로 Git 설치 검증을 다시 실행합니다.
3. 해당 패키지 README·CHANGELOG의 개발 상태와 버전을 실제 발행 내용에 맞게 갱신합니다. 기존 enum 숫자와 GUID를 유지합니다.
4. staged 파일 목록·stat·diff check·커밋 메시지를 확인해 발행 변경만 commit/push합니다.
5. 확정 소스에 예정 tag를 만들고 push한 뒤, tag 설치 주소로 새 프로젝트 검증을 수행합니다. 현재 고정 commit 검증을 tag 검증으로 보고하지 않습니다.
