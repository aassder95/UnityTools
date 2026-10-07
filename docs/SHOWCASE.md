# UnityTools Showcase

세 실험실을 한 진입 화면에서 실행하고, 관측 결과와 설계 선택을 함께 설명하는 데모입니다. Showcase는 저장소 개발 프로젝트의 `Assets/Showcase`에 있으며 독립 UPM 패키지에 포함되지 않습니다. UI·Timer·Benchmark·Persistence의 기본 의존성 계약을 유지합니다.

![Showcase 선택 화면](images/showcase.png)

Unity 2022.3에서 1440×900 Canvas를 실제 렌더한 미리보기입니다. 기존 가로 화면 미리보기이며, 개발 브랜치에서는 세로 카드 배치·Safe Area·스크롤과 회전을 추가했습니다.

## 실행

1. `UnityTools` 프로젝트를 Unity 6.3으로 엽니다.
2. `Tools > UnityTools > Configure Showcase Build Scenes`를 실행합니다. Showcase를 시작 장면으로 두고 세 실험실을 등록하며 기존 다른 장면도 보존합니다. Unity 6에서는 해당 Build Profile의 Scene List가 이 목록을 사용하도록 확인합니다.
3. `Assets/Showcase/Showcase.unity`를 열고 Play합니다. Active Input Handling은 Input Manager 또는 Both를 사용합니다.
4. 카드의 OPEN LAB으로 실험실에 들어가고 오른쪽 위 `< SHOWCASE` 버튼으로 돌아옵니다.

현재 Build Settings는 자동 변경하지 않습니다. 장면이 등록되지 않은 경우 선택 화면에서 실행 불가를 표시합니다. 장면을 다시 만들려면 `Tools > UnityTools > Create Showcase`를 사용합니다. 생성 메뉴는 세 개발용 실험실의 고정 경로를 사용하며 새 장면의 Inspector 참조를 명시적으로 연결합니다.

## 5분 시연 순서

### 1. UI Performance — 비용과 규모

- 1,000개, 같은 seed·시나리오 설정으로 COMPARE A/B를 실행합니다.
- Baseline → Virtualized와 반대 순서를 모두 실행합니다. 초기화 시간, 생성 객체 수와 steady frame 비용을 구분해 봅니다.
- RUN의 가상화 단독 실행에서는 100,000개까지 확인할 수 있습니다. A/B 비교의 상한은 10,000개입니다.
- EXPORT CSV로 같은 pair_id의 두 결과를 함께 남깁니다. 실행 중 Showcase로 돌아오면 장면이 해제되고 다음 방문은 새 인스턴스로 시작합니다.

**설명할 선택:** 표시 범위의 항목 재사용, deque 양끝 처리, seed를 공유하는 비교와 워밍업 분리.

**해석 범위:** 성능 수치는 실행 기기·빌드·해상도·작업량에 따라 달라집니다. Main Thread와 프로세스 메모리는 앱 전체의 영향도 받습니다. 고정된 개선율을 제시하지 않으며 실험실의 성능 측정은 대상 기기 Development Build에서 반복해야 합니다.

### 2. Save Recovery — 데이터 보호

- V1 → V2에서 이전 level 보존과 추가 필드·재변환 방지를 확인합니다.
- BACKUP RECOVERY에서 실제 손상 입력, 유효한 백업과 복구 후 파일을 비교합니다.
- FUTURE VERSION에서 이전 앱의 로드·저장 거절과 파일 보존을 확인합니다.

**설명할 선택:** codec·migration·파일 교체 책임 분리, 유효성 검증, 손상된 본문이 정상 백업을 덮어쓰지 않는 정책.

**해석 범위:** PASS는 예상 실패와 보호 조건을 만족한 경우도 포함합니다. SHA-256은 우발적인 손상 검출용이며 암호화·변조 방지가 아닙니다. 플랫폼별 파일 교체와 강제 종료 내구성은 별도 검증이 필요합니다. 전용 임시 fixture 경로만 사용합니다.

### 3. Timer Simulation — 시간과 상태

- OFFLINE RESTORE에서 같은 메모리 저장소를 유지한 service 재생성과 남은 시간 복원을 확인합니다.
- CLOCK ROLLBACK에서 역행 보정과 같은 UTC로 재초기화할 때 추가 보정이 없는지 확인합니다.
- SAVE FAILURE와 DUPLICATE CLAIM에서 상태·snapshot 보존과 재수령 거절을 확인합니다.

**설명할 선택:** UTC·저장소 주입, 재현 가능한 fixture, 저장 성공을 기준으로 하는 상태 변경과 service 수명 정리.

**해석 범위:** OS 시간과 실제 게임 저장 파일은 변경하지 않습니다. service 재생성은 앱 프로세스 재시작·디스크 내구성 검증이 아닙니다. claimed 상태는 외부 보상 지급과 저장의 원자적 transaction을 보장하지 않습니다.

## 장면 수명과 입력

Showcase 장면을 유지하고 선택한 실험실 하나를 additive로 로드합니다. 실험실이 active scene이 되며, 해당 실험실의 EventSystem을 사용하도록 허브 입력 객체를 비활성화합니다. 돌아올 때 실험실을 unload한 후 허브 입력을 복구합니다. 반환 버튼은 별도 Canvas로 표시하며 자체 EventSystem은 만들지 않습니다.

로드·해제 중에는 중복 이동을 무시합니다. 실험실이 열린 동안 다른 실험실로 직접 이동하지 않고 허브를 경유합니다. navigation owner는 허브 화면과 별개 GameObject에 두어 화면 비활성화가 coroutine을 중단시키지 않습니다. 진행 중 Showcase 장면 자체를 외부에서 제거하는 사용법은 이 데모의 지원 흐름에 포함하지 않습니다.

## 재현 검증

빈 프로젝트에 네 패키지를 설치하고 Package Manager Sample API로 필요한 세 실험실만 Import합니다. Showcase를 복사한 뒤 실제 Import 경로로 연결을 다시 생성하고 시작 장면으로 등록합니다.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/test-upm-compatibility.ps1 -Source Local -Scenarios showcase -UnityVersion 2022.3.62f3
powershell -NoProfile -ExecutionPolicy Bypass -File tools/test-upm-compatibility.ps1 -Source Local -Scenarios showcase -UnityVersion 6000.3.20f1
```

테스트 범위는 합성 세로/가로 viewport의 카드 배치·Safe Area와 세 실험실 반복 진입·복귀, 저장/시간 실험 실행, 중복 요청, 성능 실행 중 해제·재진입, EventSystem 중복 방지, 미등록 장면과 구독 해제입니다. 결과 XML·로그·빌드와 `summary.json`은 출력된 임시 프로젝트에 보존합니다. Showcase 시나리오는 현재 `-Source Local`만 지원합니다.

2026-10-01, 빈 프로젝트에서 로컬 패키지 설치·샘플 Import·누락 스크립트 검사 후 검증했습니다.

| Editor | Showcase Play Mode | Windows Development Build |
| --- | --- | --- |
| 2022.3.62f3 | 4/4 | 성공, Mono |
| 6000.3.20f1 | 4/4 | 성공, Mono |

네 테스트는 Showcase 통합 흐름에 대한 결과이며 각 패키지 전체 테스트를 다시 실행한 수가 아닙니다. 실제 포인터 조작, Windows Player 실행, Android/iOS와 IL2CPP는 이번 검증에 포함하지 않습니다. 캡처는 선택 화면의 1440×900 렌더만 확인합니다.

이전 실험실 검증 기록은 각 [UI Performance Lab](../UnityTools/Assets/PerformanceLab/README.md), [Save Recovery Lab](../UnityTools/Assets/SaveRecoveryLab/README.md), [Timer Simulation Lab](../UnityTools/Assets/TimerSimulationLab/README.md)에 있습니다. [정적 CI 실행](https://github.com/aassder95/UnityTools/actions/runs/36803052811)은 배포 계약 검사이며 Unity 테스트 결과와 구분합니다.

설계 선택과 대안은 [DESIGN.md](DESIGN.md)를 참고하세요.

## 모바일 배치 보완 (2026-10-07)

Showcase 선택 화면은 세로 1열/가로 3열 카드와 세로 스크롤을 사용하고, 복귀 버튼은 Safe Area 안쪽에 고정합니다. Timer Simulation Lab과 별도의 Timer Dashboard도 Safe Area와 회전을 지원합니다. UI Performance/Save Recovery 실험실 내부 화면은 이번 배치 변경 대상에 포함하지 않습니다.

1080×2400/2400×1080 합성 viewport를 검증하며 실제 Android/iOS 터치·키보드와 기기별 스크롤 동작은 아직 검증하지 않았습니다.

Timer Lab의 Unity 2022.3 실제 렌더 캡처입니다. 노치 영역은 합성 viewport로 지정했습니다. 가로 화면의 아래 결과는 세로 스크롤로 확인합니다.

[세로 화면](images/timer-lab-portrait.png) · [가로 화면](images/timer-lab-landscape.png)

[Showcase 세로 화면](images/showcase-portrait.png): 하단 카드는 스크롤로 접근합니다.
