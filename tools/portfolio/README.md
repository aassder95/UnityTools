# Windows Player 측정 및 시연 재현

패키지 코드와 분리된 자동 시연 도구입니다. 기본 샘플이나 UPM runtime에는 포함되지 않습니다. 호환성 검증 스크립트가 만든 임시 `ui-lab` 프로젝트에만 캡처 컴포넌트와 별도 장면을 생성합니다.

1. `tools/test-package-releases.ps1`로 공개 태그 설치, 테스트 및 빌드를 먼저 완료합니다.
2. 출력된 `release-validation.json`의 Unity 6 `ui-lab` Project 경로를 사용합니다. Unity Editor와 다른 테스트·빌드·GPU 작업을 종료합니다.
3. 새 출력 폴더를 지정해 실행합니다. FFmpeg가 PATH에 있어야 합니다.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/portfolio/capture-portfolio.ps1 -ProjectPath "<검증된 ui-lab 프로젝트>" -OutputPath "<새 출력 폴더>"
```

1280×720 Windowed, VSync 0, target FPS 60, seed 42, Sweep, 120 프레임 워밍업 및 600 프레임 표본으로 1,000개·10,000개 각각 4쌍을 측정합니다. 매 쌍 실행 순서를 교대합니다. 환경·품질·Unity 버전은 environment.txt와 CSV에 기록합니다. 파일 쓰기와 스크린샷은 각 4쌍 측정이 끝난 뒤 수행합니다.

마지막에는 1,000개 1쌍을 별도로 실행하여 0.5초 간격 PNG와 MP4를 만듭니다. 영상은 2 fps 화면 기록이며 캡처 할당이 발생하므로 이 실행은 성능 CSV로 export하지 않습니다. 실시간 60 fps 영상으로 소개하지 않습니다. 일반 데모 플레이어는 Release의 별도 Windows zip을 사용하세요.

시연 전용 카메라는 완료 화면 및 영상 캡처 중에만 활성화하며 측정 구간에는 비활성화합니다. `-CaptureOnly`는 새로운 출력 폴더에서 영상만 다시 만듭니다. 이 모드의 environment.txt에는 기본 설정을 쓰지만 실제 4쌍 CSV를 만들지 않습니다. 기존 측정 폴더의 환경 기록과 구분하세요. 카메라 상태와 캡처 요청의 `[TEST]` 진단 로그를 보존했습니다.

출력:

- items-1000.csv / items-10000.csv: 각각 8개 개별 실행 + 5개 통계 행
- environment.txt: 실행 조건
- items-1000.png / items-10000.png: 완료 화면
- frames/*.png / ui-performance-demo.mp4: 별도 캡처 실행
- player.log / capture-complete.txt: 종료 증거

60 fps 제한에서는 프레임 P95 차이가 작을 수 있습니다. 초기화 시간·Main Thread·객체 수를 함께 읽고 프레임 제한을 풀었을 때의 결과로 바꿔 해석하지 않습니다. 4쌍의 모집단 표준편차는 신뢰구간이 아니며 이 Windows PC의 결과는 모바일 개선율을 증명하지 않습니다. Peak memory는 프로세스 전체 메모리이고 UI 객체만의 점유량이 아닙니다. 상세 통계 계약은 샘플 README를 따릅니다.

`-SkipBuild`는 같은 검증 프로젝트에 이미 생성된 PortfolioBuild를 다시 실행할 때만 사용합니다. 출력 폴더를 재사용하지 않으므로 이전 CSV에 중복 append하지 않습니다. 일반 개발 프로젝트에서 실행하지 마세요.

## 화면 기록 방식

숨겨진 Windows Player에서 ScreenCapture가 실패해 요청을 제거했습니다. 영상 실행에서는 기록할 프레임에만 Canvas를 ScreenSpaceCamera로 전환하고 전용 카메라로 RenderTexture를 렌더링한 뒤 원래 Overlay로 복구합니다. OS 화면 녹화가 아닌 동일 UI의 별도 렌더링입니다. RenderTexture·ReadPixels·PNG 인코딩 비용과 Canvas 갱신이 발생하여 이 실행의 수치를 성능 증거로 쓰지 않습니다. 활성 카메라만 추가했던 실패 가설도 직접 화면 캡처에 사용하지 않습니다.
