# Timer Dashboard

개발 프로젝트 전용 관리 데모입니다. UI와 Timer 패키지 간 필수 의존성은 추가하지 않습니다.

- 장면: `UnityTools/Assets/TimerDashboard/TimerDashboard.unity`
- 재생성: `Tools > UnityTools > Create Timer Dashboard`
- Task ID를 입력하고 Register → Start → Pause / Resume → Complete → Claim을 실행합니다.
- Cancel은 진행을 취소합니다. Unregister는 저장 상태를 보존하며, Delete는 저장 상태를 지워 다음 등록을 초기 상태로 시작합니다.
- 여러 ID를 등록하면 ID 순서로 작업 상태, 남은 초, 진행률을 표시합니다. SHOP 주기 타이머는 상태, 남은 초, 준비 여부를 표시하며 OpenPeriod / ClosePeriod로 전환할 수 있습니다.
- 결과는 UiToastQueue로 순서대로 표시합니다. 중복/대기열 초과 알림은 상태 텍스트에 표시합니다.
- 저장소는 세션 내 메모리입니다. GameObject 재활성화 후 같은 ID를 다시 등록하면 상태를 복원하지만 플레이 종료나 장면 재로드 뒤에는 유지하지 않습니다.
- 서비스 변경 이벤트를 모아 다음 프레임에 목록을 갱신합니다. 매 프레임 목록 생성/검색은 하지 않습니다.

## 검증

```powershell
powershell -ExecutionPolicy Bypass -File tools/test-upm-compatibility.ps1 -Source Local -UnityVersion 2022.3.62f3 -Scenarios timer-dashboard
powershell -ExecutionPolicy Bypass -File tools/test-upm-compatibility.ps1 -Source Local -UnityVersion 6000.3.20f1 -Scenarios timer-dashboard
```

버튼 생명주기, 빈/중복 ID, 저장 실패 시 삭제 보존, 재등록/재활성화 복원을 Play Mode에서 검증합니다. Windows Development Build 결과와 실제 모바일 입력 검증을 구분합니다.

## 화면 대응

세로 2열/가로 4열 버튼과 Safe Area 내부 스크롤을 사용합니다. 목록 갱신은 스크롤 위치를 유지하며 여러 ID의 긴 목록 전체를 스크롤할 수 있습니다. 토스트는 Safe Area 하단에 고정합니다. 합성 viewport의 노치 여백/회전/30개 ID를 검증하며 실제 기기 키보드/터치 검증과 구분합니다.

[세로 화면 미리보기](images/timer-dashboard-portrait.png): Unity 2022.3 실제 렌더이며, 배치 확인용 고정 표시 데이터를 사용했습니다. 버튼/서비스 동작은 별도의 Play Mode 테스트로 검증합니다.
