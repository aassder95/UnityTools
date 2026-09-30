# Save Recovery Lab

Package Manager에서 Persistence의 **Save Recovery Lab**을 Import하고 `SaveRecoveryLab.unity`를 열어 Play합니다. uGUI가 필요하며 Active Input Handling은 Input Manager 또는 Both로 설정합니다. 장면을 다시 생성하려면 `Tools > UnityTools > Create Save Recovery Lab`을 사용합니다.

| 버튼 | 구성한 입력 | 확인하는 invariant |
| --- | --- | --- |
| V1 -> V2 | level=3인 V1 | level 유지, branchCnt=4 추가, V2 저장 후 재변환 없음 |
| BACKUP RECOVERY | 손상된 본문 + level=3 백업 | 백업 복구, 본문 재저장, 유효 백업 보존 |
| FUTURE VERSION | V2 본문 + V1 백업을 V1 앱으로 접근 | 백업으로 되돌리지 않음, 이전 앱의 저장 거절, 두 파일 보존 |
| MISSING FILE | 본문과 백업 모두 없음 | 로드 실패, 파일 생성 없음 |
| CORRUPT FILE | 잘못된 envelope, 백업 없음 | 로드 실패, 원본 보존 |
| REJECT MIGRATION | 유효한 V1, false를 반환하는 migration | 로드 실패, 원본 보존 |

입력/출력 패널은 실제 파일 내용의 스냅샷입니다. 결과에는 API 반환값, 복구·변환 플래그와 데이터 보존 검증을 표시합니다. **PASS는 예상한 실패와 보호 동작까지 만족했다는 뜻**입니다. 실패 원인은 실험에서 구성한 조건으로 구분하며, bool API를 범용 실패 진단 API처럼 해석하지 않습니다. 실험 구성 또는 파일 입출력 자체가 실패하면 PASS/FAIL 대신 실행 불가를 표시합니다.

매 실행은 `Application.temporaryCachePath/UnityTools-SaveLab/<GUID>/fixture.json`과 `.bak`, `.tmp`만 사용합니다. 완료 시 해당 파일과 빈 실행 폴더를 정리하고 스냅샷은 화면에 유지합니다. 정리 중 권한 오류 또는 앱 강제 종료 시 전용 임시 폴더에 파일이 남을 수 있습니다. 실제 게임 저장 경로에는 접근하지 않습니다.

SHA-256은 우발적인 손상 검출용이며 암호화·변조 방지가 아닙니다. File.Replace의 플랫폼 지원은 별도 확인이 필요합니다. 이 샘플은 프레임 성능 측정 도구가 아닙니다.

Play Mode 테스트는 여섯 실험의 파일·데이터 invariant와 장면 버튼 연결, 비활성화 후 구독 해제를 검사합니다. 루트의 호환성 스크립트로 소스 작업 중 검증할 수 있습니다:

```powershell
powershell -ExecutionPolicy Bypass -File tools/test-upm-compatibility.ps1 -Source Local -Scenarios save-lab
```

## 검증 기록

2026-09-30, 빈 프로젝트에 로컬 Persistence 패키지 설치 후 Package Manager Sample API로 Import했습니다.

| Editor | Play Mode | Windows Development Build |
| --- | --- | --- |
| 2022.3.62f3 | 14/14 (코어 6 + 샘플 8) | 성공, Mono |
| 6000.3.20f1 | 14/14 (코어 6 + 샘플 8) | 성공, Mono |

장면의 누락 스크립트 검사와 버튼 실행·구독 해제 테스트를 포함합니다. 직접 화면 조작·배치 확인, Android/iOS, IL2CPP 검증은 포함하지 않습니다.
