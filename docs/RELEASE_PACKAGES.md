# Benchmark · Persistence 1.0.0

- Benchmark: `unitytools-benchmark/v1.0.0`, 독립 코어와 UI Performance Lab
- Persistence: `unitytools-persistence/v1.0.0`, 독립 코어와 Save Recovery Lab
- 최소 지원 Unity 2022.3, 검증 Editor 2022.3.62f3 / 6000.3.20f1
- Windows Development Build, Mono backend
- 코어의 외부 package dependency 없음; 샘플에서 uGUI 및 UI 조합 별도 검증
- 공개 태그 검증: `tools/test-package-releases.ps1`

각 태그를 실제 Git commit으로 해석한 뒤 빈 프로젝트에서 설치합니다. lock hash, 실제 package cache의 이름·1.0.0·dependency, PlayMode XML, Windows build 결과를 함께 확인합니다. 결과는 release-validation.json과 원본 XML·로그·lock·빌드로 보존합니다.

공개 발행 후 검증 결과와 시연 자료를 이 문서에 추가합니다. Android/iOS, IL2CPP, WebGL 및 실제 기기 전원 손실 검증은 포함하지 않습니다.
