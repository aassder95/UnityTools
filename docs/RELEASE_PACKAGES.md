# Benchmark · Persistence 1.0.0

- Benchmark: `unitytools-benchmark/v1.0.0`, 독립 코어와 UI Performance Lab
- Persistence: `unitytools-persistence/v1.0.0`, 독립 코어와 Save Recovery Lab
- 최소 지원 Unity 2022.3, 검증 Editor 2022.3.62f3 / 6000.3.20f1
- Windows Development Build, Mono backend
- 코어의 외부 package dependency 없음; 샘플에서 uGUI 및 UI 조합 별도 검증
- 공개 태그 검증: `tools/test-package-releases.ps1`

각 태그를 실제 Git commit으로 해석한 뒤 빈 프로젝트에서 설치합니다. lock hash, 실제 package cache의 이름·1.0.0·dependency, PlayMode XML, Windows build 결과를 함께 확인합니다. 결과는 release-validation.json과 원본 XML·로그·lock·빌드로 보존합니다.

현재 checkout은 태그 이후의 Sheets·VFX 기능, 통계 안정성 보완 및 Persistence 상세 실패 API도 포함합니다. 아래 2026-10-01 결과는 불변 공개 태그의 과거 검증이며 현재 checkout 전체의 테스트 결과가 아닙니다.

## 공개 Git 설치 검증 결과 — 2026-10-01

두 annotated 태그의 실제 commit은 `8b5c4f48c32efc9973a0fa8978f1879a7235102a`입니다. 공개 HTTPS Git에서 태그를 조회하고 이 commit을 고정해 설치했습니다. 패키지 cache의 1.0.0·빈 dependency와 packages-lock의 정확한 hash를 모두 확인했습니다. UI 샘플은 기존 `unitytools-ui/v2.0.0`을 함께 설치했습니다.

| Unity | Benchmark 코어 | Persistence 코어 | UI Performance Lab | Save Recovery Lab | Windows Mono 빌드 |
| --- | --- | --- | --- | --- | --- |
| 2022.3.62f3 | 14/14 | 18/18 | 30/30 | 26/26 | 네 시나리오 모두 성공 |
| 6000.3.20f1 | 14/14 | 18/18 | 30/30 | 26/26 | 네 시나리오 모두 성공 |

총 176개 테스트 실행과 8개 빌드입니다. 코어 테스트는 샘플 테스트에도 포함되므로 176개 모두 서로 다른 테스트라는 의미는 아닙니다. 원본 XML·로그·lock·manifest는 GitHub Release의 `release-validation-evidence.zip`에 있습니다. 로컬 원본은 `C:/Users/search/AppData/Local/Temp/UTR-5bed58584b1d/release-validation.json`에 보존했습니다.

별도 깨끗한 HTTPS origin 클론에서 `verify-release.ps1 -Release` 보안 검사도 통과했습니다. 정적 검사 4개 패키지·검사기 회귀 17개·샘플 사본 검사가 통과했고 [발행 commit CI](https://github.com/aassder95/UnityTools/actions/runs/36825500440)가 성공했습니다.

## 다운로드

- [Benchmark 1.0.0](https://github.com/aassder95/UnityTools/releases/tag/unitytools-benchmark/v1.0.0)
- [Persistence 1.0.0](https://github.com/aassder95/UnityTools/releases/tag/unitytools-persistence/v1.0.0)

각 패키지의 .tgz는 태그의 파일만 Git archive로 추출하며 manifest·dependency·MIT 라이선스를 검사합니다. SHA-256과 원본 commit은 `package-assets.json`에 있습니다. Unity Package Manager의 Add package from tarball로 설치할 수도 있습니다. 재생성 명령은 `python tools/package-release-assets.py <출력 폴더>`입니다.

[실제 Windows 측정 결과](PORTFOLIO_RESULTS.md)와 원본 CSV를 공개합니다.

포트폴리오 측정 재현 도구는 [tools/portfolio](../tools/portfolio/README.md)에 있습니다. Android/iOS, IL2CPP, WebGL 및 실제 기기 전원 손실 검증은 포함하지 않습니다.
