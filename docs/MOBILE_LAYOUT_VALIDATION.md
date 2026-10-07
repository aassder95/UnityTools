# 모바일 배치 검증 (2026-10-07)

## 변경 범위

- UI: Canvas 바로 아래의 명시적인 RectTransform에 UiSafeArea를 연결합니다. 노치·홈 영역의 여백을 정규화 anchor로 적용합니다.
- Timer Simulation Lab: 세로 버튼 2열 / 상태 1열, 가로 버튼 5열 / 상태 2열, 긴 본문 높이 계산과 세로 스크롤을 추가했습니다.
- Timer Dashboard: 세로 2열 / 가로 4열, 긴 타이머 목록 스크롤, 목록 갱신 시 스크롤 위치 유지와 Safe Area 하단 토스트를 적용했습니다.
- Showcase: 세로 1열 / 가로 3열 카드, 스크롤과 Safe Area 안쪽 복귀 버튼을 적용했습니다.
- UI Performance / Save Recovery 내부 화면은 이번 변경 범위에 포함하지 않습니다.

## 검증 방법

합성 viewport는 세로 1080×2400 (아래 100px / 위 120px), 가로 2400×1080 (왼쪽 120px / 오른쪽 60px), 일반 1440×900입니다. 회전 후 버튼 크기·위치, 본문 높이, Safe Area anchor, 스크롤 콘텐츠 범위와 30개 타이머 목록을 Play Mode에서 확인합니다.

CanvasScaler의 배율 변경을 먼저 반영한 뒤 Text.preferredHeight를 측정합니다. 이전 배율로 측정하면 회전 후 폰트의 픽셀 반올림 차이 때문에 본문이 잘릴 수 있습니다. 화면 크기/여백 변경 시에만 재배치하고, Update에는 검색·새 객체·문자열 생성이 없습니다.

| Editor | UI | Timer Lab | Dashboard | Showcase | Windows Development Build |
| --- | --- | --- | --- | --- | --- |
| 2022.3.62f3 | 76/76 | 59/59 | 4/4 | 5/5 | 각 시나리오 성공 |
| 6000.3.20f1 | 76/76 | 59/59 | 4/4 | 5/5 | 각 시나리오 성공 |

모든 테스트는 skipped=0입니다. Dashboard의 최종 저장 장면은 기존 object ID를 보존한 뒤 2022.3에서 다시 4/4를 통과했습니다. 기존 object ID는 Timer Lab 130개, Dashboard 163개, Showcase 140개를 보존했으며 기존 .meta GUID도 변경하지 않았습니다. 신규 필드 참조와 장면 내부 fileID의 유일성/연결을 확인했습니다.

## 재현

```powershell
./tools/test-upm-compatibility.ps1 -Source Local -UnityVersion 2022.3.62f3 -Scenarios @('ui','timer-lab','timer-dashboard','showcase')
./tools/test-upm-compatibility.ps1 -Source Local -UnityVersion 6000.3.20f1 -Scenarios @('ui','timer-lab','timer-dashboard','showcase')
```

두 명령은 PowerShell에서 직접 실행합니다. 결과 XML·로그·Windows Player는 출력된 Temp 경로에 남습니다. UPM 6개 패키지 정적 검사와 샘플 미러 검사와 도구 회귀 테스트 36개도 통과했습니다.

## 렌더 확인 및 한계

Unity 2022.3에서 [Timer Lab 세로](images/timer-lab-portrait.png), [가로](images/timer-lab-landscape.png), [Dashboard 세로](images/timer-dashboard-portrait.png), [Showcase 세로](images/showcase-portrait.png)를 렌더해 배치를 확인했습니다. Dashboard 캡처는 표시용 고정 fixture이며 실제 서비스/버튼은 Play Mode 테스트로 검증합니다.

합성 Safe Area와 Unity 렌더는 실제 휴대폰 검증을 대체하지 않습니다. Android/iOS 실제 터치, 키보드 출현, 기기별 스크롤 관성과 홈 제스처는 이번 검증에 포함하지 않았습니다.
