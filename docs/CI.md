# UPM 자동 검증

## PR 정적 검사

`.github/workflows/upm-static.yml`은 PR, develop/main/codex 브랜치 push, 수동 실행에서 GitHub 제공 Windows runner를 사용합니다. Unity 설치·라이선스와 GitHub secret이 필요하지 않습니다. checkout과 artifact action은 고정 commit SHA를 사용하며 token 권한은 contents:read입니다.

- 네 패키지의 JSON·이름·버전·최소 Unity 버전 계약
- UI 기본 dependency(uGUI/TMP), 나머지 독립 패키지의 dependency 및 runtime assembly 경계
- Input System 선택 assembly의 constraint
- sample 경로 탈출·필수 README/장면 누락
- 배포 asset의 meta 누락·고아 meta·패키지 내부 GUID 중복
- vendor 파일과 legacy namespace 혼입
- UPM Samples와 개발 Assets 사본 일치
- 고의로 손상시킨 임시 배포 사본에 대한 validator 회귀 테스트

검사 실패 시에도 나머지 정적 검사와 결과 요약을 실행합니다. GitHub 실행의 Summary에는 배포 계약·validator 회귀 테스트·샘플 사본 검사의 성공/실패를 각각 표시합니다. 배포 계약 결과는 `upm-static-report` artifact의 JSON 파일로 14일간 보관합니다. 이 파일은 배포 검사 결과만 담으며 회귀 테스트와 mirror 결과는 Summary 및 각 step 로그에서 확인합니다.

JSON은 `schema_version`, `status`, `packages`, `error_count`, `errors`, `unity_runtime_tested`를 포함합니다. `errors`에는 저장소 상대 경로와 실패 사유가 들어가고, 현재 workflow의 `unity_runtime_tested`는 항상 false입니다. 잘못된 JSON·UTF-8·sample 경로·assembly 배열도 파일별 오류로 보고합니다. 검사 오류 또는 report 저장 실패는 exit code 1을 반환합니다.

Markdown·텍스트와 폴더의 신규 GUID 생성 여부는 정적 검사 대상에서 제외합니다. 기존 UI 샘플은 Import 시 이들의 meta를 생성하는 구조입니다. 스크립트·scene·prefab 등 참조되는 asset의 meta는 검사합니다. 중복 GUID는 패키지 내부에서 검사하며 `Samples~`와 `Assets`의 의도적인 사본은 mirror 검사로 확인합니다.

로컬 실행:

```powershell
python tools/verify-upm.py
python -m unittest discover -s tools/tests -p 'test_verify_upm.py' -v
powershell -ExecutionPolicy Bypass -File tools/verify-sample-mirrors.ps1
```

Python 3.10 이상을 사용하며 별도 Python 패키지는 필요하지 않습니다. JSON 보고서가 필요하면 출력 경로를 지정합니다.

```powershell
python tools/verify-upm.py --report "$env:TEMP/upm-static-report.json"
```

정적 검사는 Unity compile·Play Mode·Player build 성공을 의미하지 않습니다. `verify-release.ps1 -Release`는 별도의 release 보안·Git 이력·원격 URL 검사이며 PR 검사와 구분합니다. 현재 SSH origin은 release 도구의 HTTPS URL 계약과 다르므로 임의로 origin을 변경하지 않습니다.

## Unity 실행 조건과 후속 연결

2026-10-01 저장소 runner API 확인 결과 등록된 self-hosted runner는 0개입니다. 이 작업에서 runner 등록, 계정 연결, 라이선스/secret 저장은 수행하지 않습니다. 라이선스가 활성화된 현재 PC에서는 기존 `test-upm-compatibility.ps1 -Source Local`로 Unity 검증을 실행할 수 있습니다.

GitHub에서 Unity를 실행하려면 먼저 격리된 실행 환경, Editor 설치·Windows Build Support 및 해당 환경에서 사용할 수 있는 활성 라이선스를 마련해야 합니다. Unity Personal에 수동 `.ulf` 활성화를 일괄 적용할 수 있다고 가정하지 않습니다. [Unity 수동 활성화 조건](https://docs.unity3d.com/6000.0/Documentation/Manual/ManualActivationGuide.html)을 확인하세요.

이 저장소는 public입니다. 개인 개발 PC를 외부 PR용 runner로 연결하는 구성을 기본으로 사용하지 않습니다. GitHub는 public 저장소의 self-hosted runner에 대해 외부 PR 코드가 환경을 침해할 수 있다고 설명합니다. [GitHub runner 보안 안내](https://docs.github.com/en/actions/reference/security/secure-use)

Unity CI를 연결할 때는 2022.3/6.3와 패키지별 독립 프로젝트를 구성하고, 테스트 XML·Editor 로그·summary.json 및 필요 시 Player build를 artifact로 보관합니다. 성공한 실행 증거가 생긴 후 README에 **정적 검사**와 **Unity 테스트** 배지를 구분해 추가합니다. 현재 정적 workflow를 Unity 호환성 배지로 표시하지 않습니다.
