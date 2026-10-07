# Timer Simulation Lab

Package Manager에서 Timer의 **Timer Simulation Lab**을 Import하고 `TimerSimulationLab.unity`를 열어 Play합니다. uGUI가 필요하며 Active Input Handling은 Input Manager 또는 Both로 설정합니다. Timer 코어는 UI package와 uGUI에 의존하지 않습니다. 장면 생성 메뉴는 `Tools > UnityTools > Create Timer Simulation Lab`입니다.

| 실험 | 입력·동작 | 확인하는 invariant |
| --- | --- | --- |
| FORWARD TIME | 60초 Timer, 가상 UTC +90초, 같은 handle 해제·초기화 | 남은 시간 0, 초기화 시 자연 만료 판정 |
| OFFLINE RESTORE | service 해제, UTC +30초, 새 service 생성; 다시 +90초 | 30초 남은 Processing 복원 후 Completed 복원 |
| CLOCK ROLLBACK | 시작 시각보다 UTC -30초, 초기화 두 번 | duration 60→90초 보정, 같은 시각에서 추가 보정 없음 |
| SAVE FAILURE | 시작·정지·재개·취소·강제 완료·수령 각각 저장 거절 | 실패한 단계의 상태·snapshot 보존 |
| DUPLICATE CLAIM | 강제 완료·수령 후 service 재생성 | claimed 복원, None 상태에서 재수령 거절 |
| PAUSE / RESUME | 20초 경과 후 정지, 하루 뒤 복원·재개, 40초 경과 | 정지 중 남은 40초와 snapshot 유지, 재개 후 자연 완료 |
| CANCEL / RESTART | 정지한 작업 취소, 하루 뒤 복원, 새 30초 작업 시작 | 수령 없이 None 복원, 중복 취소 거절, 등록 유지 |
| UNREGISTER / RESTORE | 개별 등록 해제, 30초 뒤 재등록; 다시 해제·만료 후 복원 | 저장 유지·등록 수 0→1, UTC는 계속 경과해 10초 남은 작업과 완료 상태 복원 |

각 실행은 고정 가상 UTC와 새 메모리 저장소로 시작합니다. 입력·출력 패널에 UTC, 현재 상태, 남은 시간, duration, 실제 저장 key/value를 표시합니다. 결과 패널에는 실행 순서와 실제 bool 반환값·보존 검사를 표시하며, 예상 실패를 포함한 invariant를 만족하면 PASS입니다. 실행 구성 실패는 별도로 표시합니다. OS 시간과 PlayerPrefs, 실제 게임 저장 파일에는 접근하지 않습니다.

시간 앞으로 이동 실험은 `TryComplete`로 강제 완료하지 않고 초기화의 만료 판정을 사용합니다. 샘플은 실험 종료 시 service를 Release하여 coroutine과 구독을 정리합니다. 따라서 실시간으로 흘러가는 타이머 UI가 아니라 재현 가능한 상태·저장 실험입니다.

오프라인/재시작은 **같은 메모리 저장소를 유지한 service 재생성**입니다. 앱 프로세스 종료·디스크 내구성 검증은 아닙니다. 역행 보정은 현재 Timer 정책을 그대로 보여주며 서버 검증이나 부정행위 방지를 제공하지 않습니다. 수령 실험은 Timer의 claimed 상태만 검증하며, 외부 재화 지급과 저장의 원자적 transaction까지 보장하지 않습니다. 새 Timer 작업을 명시적으로 시작하면 새 수령 주기가 시작될 수 있습니다.

테스트는 여덟 실험의 상태·저장 invariant, 반복 재현, 여섯 저장 실패, 장면 버튼 연결과 모든 버튼의 비활성화 구독 해제를 검사합니다. 정지·취소 실험은 중간 snapshot도 결과 패널에 표시합니다. 버튼은 두 줄로 배치됩니다.

```powershell
powershell -ExecutionPolicy Bypass -File tools/test-upm-compatibility.ps1 -Source Local -Scenarios timer-lab
```

## 검증 기록

2026-10-07, Timer 1.2.0 미게시 후보의 빈 프로젝트 설치에서 Unity `2022.3.62f3`·`6000.3.20f1` 모두 Play Mode **46/46**(코어 35 + 실험실 11), skip 0, Windows Mono Development Build를 통과했습니다. 기존 장면 오브젝트 ID 94개와 .meta GUID를 유지하고 버튼 오브젝트를 추가한 최종 장면으로 다시 검사했습니다. 여덟 버튼의 실행·비활성화 해제와 결과 텍스트 높이 검사를 포함합니다. Unity 2022.3의 1440×900 렌더 이미지에서 새 세 실험과 저장 실패 결과의 배치도 확인했습니다. OS 마우스·실제 터치 조작 검증과는 별개입니다.

2026-10-01, 빈 프로젝트에 로컬 Timer를 설치하고 Package Manager Sample API로 Import했습니다. Unity `2022.3.62f3`과 `6000.3.20f1` 모두 Play Mode **25/25**(코어 17 + 실험실 8), Windows Development Build(Mono)가 통과했습니다. 장면의 누락 스크립트 검사와 버튼·구독 테스트를 포함합니다. 직접 화면 조작·배치 확인, Android/iOS, IL2CPP는 검증하지 않았습니다.

## Delete / Restore

DELETE / RESTORE는 저장 실패 시 진행 상태와 snapshot 보존, 삭제 재시도 후 등록 해제·미수령 상태, 하루 뒤 재등록 시 None 상태와 새 30초 작업 시작을 확인합니다. 기존 Unregister / Restore는 작업 기록을 유지하며 Delete / Restore는 기록을 초기화합니다. 기존 enum 숫자는 유지하고 DeleteRestore를 마지막에 추가했습니다.
