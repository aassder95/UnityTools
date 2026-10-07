# Sheets / VFX 작업 흐름 확장

## 일괄 검증과 생성

Unity 2022.3.62f3: Editor 52/52, PlayMode 56/56, 생성 코드 컴파일/읽기, Windows Mono Development build/Player 통과. 여러 프리셋 오류 수집, 검증 실패 시 기존 파일 보존/새 파일 미생성, 중복 출력/타입 거부, 경로 이탈 거부, 검증만 실행할 때 파일 미생성, 정상 생성 확인. 수동 확인 대화상자는 별도 확인 대상. OS 저장 오류 시 앞서 쓴 파일은 유지하므로 파일 시스템 전체 트랜잭션은 제공하지 않는다.

## VFX 비용 요약

Unity 2022.3.62f3: Editor 29/29, skip 0, Windows Mono Development build 통과/Editor DLL 제외. 구조 수, material 슬롯/고유 수, maxParticles 합, 구간 내 관측 peak, 잘못된 구간/샘플 수 거부, 원본 prefab 바이트 보존 확인. 수치는 고정 간격 샘플 기반으로 GPU 시간/draw call/overdraw 측정이 아니다.

## CSV 변경 비교

Unity 2022.3.62f3: Editor 57/57, PlayMode 56/56, 생성 코드/Windows Mono build/Player 통과. 행/열 순서 무시, 한글 multiline 값, 셀 추가/삭제/변경과 헤더 차이, 빈/중복/없는 키 거부 확인. 비교는 원문 문자열이며 숫자 의미 비교가 아니다.

## VFX 비교 이미지 내보내기

Unity 2022.3.62f3: Editor 30/30, skip 0, Windows Mono Development build/Editor DLL 제외 통과. PNG 재로드 후 좌/우 픽셀 배치와 크기, 비교 시간 유지, 원본 보존, 잘못된 크기/확장자 거부와 기존 파일 보존 확인. 수동 저장 대화상자와 overwrite 선택은 별도 확인 대상.

## 값 검증 규칙

필수 값(공백 포함), 숫자 범위(양끝 포함/유한 숫자/invariant culture), 문자열 길이(UTF-16 단위/양끝 포함) 규칙을 프리셋에 저장한다. CSV Generator와 CSV Batch에 함께 적용한다. 기본 목록은 비어 있으며 기존 serialized 필드명/GUID는 변경하지 않는다. Capture API에 optional rules 인자를 추가해 기존 소스 호출은 유지한다. 규칙이 있으면 셀 전체에 적용하며 배열 원소별 검증은 포함하지 않는다.

Unity 2022.3.62f3: Editor 63/63, PlayMode 56/56, 생성 코드 컴파일/읽기, Windows Mono Development build/Player 통과. Required/Range/Length 복수 오류, 양끝 경계, 잘못된 범위와 빈 테이블, 프리셋 직렬화와 Batch 차단 확인.

## 최종 검증

기능 커밋: `2012c93` 일괄 생성, `9f74649` 데이터 비교, `4fd56c3` 값 규칙, `8101c39` 비용 요약, `79cb72f` PNG 내보내기. 브랜치는 `codex/sheets-vfx-features`이며 기존 로컬 develop의 변경을 포함하지 않는다. 패키지 버전과 release tag는 유지한다.

| Unity | Sheets Editor | Sheets PlayMode | VFX Editor | Windows Mono Development |
| --- | --- | --- | --- | --- |
| 2022.3.62f3 | 63/63 | 56/56 | 30/30 | 두 build 통과, Sheets Player 실행 통과 |
| 6000.3.20f1 | 63/63 | 56/56 | 30/30 | 두 build 통과, Sheets Player 실행 통과 |

모든 failed/skip 0. 생성 코드 컴파일/읽기와 Player의 Editor assembly 제외 확인. VFX build는 Editor 패키지 제외를 검사하며 게임 내 효과 실행 검증은 아니다. Local UPM/Built-in pipeline 기준이다. 이번 변경의 URP/IL2CPP/기기 runtime은 검사하지 않았다.

정적 UPM 검사 6개 패키지 통과, 도구 회귀 36/36, git diff --check 통과. Sheets/VFX .meta 79개 GUID 중복 없음. 기존 GUID/serialized field rename/scene/prefab 수정 없음. 새 규칙 목록이 추가되며 Capture에 optional rules 인자가 추가된다. 변경 코드의 임시 로그/필드, 직접 throw, sealed/internal/FormerlySerializedAs, [TEST] 로그 없음.

### 결과 경로

- Sheets 2022: `C:/Users/search/AppData/Local/Temp/UnityTools-Sheets-2022.3.62f3-2aba7121ad894e55b5a1130789277875/summary.json`
- Sheets 6: `C:/Users/search/AppData/Local/Temp/UnityTools-Sheets-6000.3.20f1-b573078f8bba4f738c1f7e27a921a051/summary.json`
- VFX 2022: `C:/Users/search/AppData/Local/Temp/UnityTools-Vfx-2022.3.62f3-2729e96557004d4b8b363db9ea5a49de/summary.json`
- VFX 6: `C:/Users/search/AppData/Local/Temp/UnityTools-Vfx-6000.3.20f1-61e7dde03bc843f78e98ba9d8cdb1f1b/summary.json`

### 재현 명령

```powershell
powershell -ExecutionPolicy Bypass -File tools/test-sheets-package.ps1 -UnityVersion 2022.3.62f3
powershell -ExecutionPolicy Bypass -File tools/test-sheets-package.ps1 -UnityVersion 6000.3.20f1
powershell -ExecutionPolicy Bypass -File tools/test-vfx-package.ps1 -UnityVersion 2022.3.62f3
powershell -ExecutionPolicy Bypass -File tools/test-vfx-package.ps1 -UnityVersion 6000.3.20f1
```

### 수동 확인 대상

- CSV Batch: 프리셋 여러 개 연결, Validate all 오류 목록, Generate all 확인/취소/덮어쓰기, 출력 경로 오류 표시.
- CSV Diff: Before/After 연결, 키 입력, 여러 줄/한글 값과 행 추가/삭제/헤더 변경 표시 확인.
- CSV Generator: Add value rule로 세 종류의 규칙 설정, 오류 목록과 Save C# 차단 확인, Save preset/재개방 후 규칙 복원, CSV Batch에도 동일한 규칙 적용 확인.
- VFX Browser: Replay 구간 조정 후 비용 요약 분석, 결과 표시와 prefab 전환/Refresh 시 초기화 확인.
- VFX Compare: Seek/Orbit/Fit 후 Export PNG, 저장/취소/덮어쓰기, 가로 1024×512의 좌우 화면과 선택 시점 확인.

자동 검증은 데이터·생성·렌더링 API와 기존 창 회귀를 포함하며 위 수동 입력/대화상자 동작을 완료한 것으로 간주하지 않는다.
