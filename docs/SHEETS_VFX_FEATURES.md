# Sheets / VFX 추가 기능 검증

## 생성 설정 프리셋

Unity 2022.3.62f3: Editor 46/46, Play Mode 56/56, 생성 코드 컴파일/읽기, Windows Mono Development build/Player 실행 통과. 프리셋 에셋 round-trip, Enum 배열, 한글 출력 경로, CSV rename 참조 유지, 헤더 불일치 시 원자적 거부를 검사했다. 실제 저장 대화상자와 수동 domain reload UI는 별도 확인 대상이다.

## VFX 태그와 컬렉션

Unity 2022.3.62f3: Editor 25/25, skip 0, Windows Mono Development build 통과, Player에 Editor DLL 미포함. 라벨 정규화/중복 제거, 저장 후 재로드, prefab rename 유지, 여러 필터 조합, 원본 prefab 바이트 보존 검증. 수동 GUI 입력/대화상자는 별도 확인 대상.

## CSV 오류 목록과 데이터 참조

Unity 2022.3.62f3: Editor 50/50, Play Mode 56/56, 생성 코드 컴파일/읽기, Windows Mono Development build/Player 통과. 여러 열/행의 타입 오류, quoted multiline 이후 물리적 행 번호, 빈/중복 키, 배열 참조의 모든 누락 ID, 빈 테이블의 잘못된 규칙, 프리셋 규칙 직렬화 검사. UI 저장은 모든 검증 통과 후만 제공한다.

## VFX 디스크 분석 캐시

Unity 2022.3.62f3: Editor 26/26, skip 0, Windows Mono Development build 통과, Editor DLL 미포함. 재로드 후 PNG/색상 복원, 파일 갱신 시각/바이트가 그대로 유지되는 캐시 hit, 프레임 변경, material 저장 후 실제 파일 변경과 캐시 miss, 손상 metadata 거부, 기존 자원 해제/메모리 상한 회귀 검사. 변경 이벤트에서는 기존 결과를 비우고 Refresh에서 복원한다. 의존성 키에는 import hash와 실제 의존 파일의 SHA256을 포함한다.

## VFX 비교 보기

Unity 2022.3.62f3: Editor 28/28, skip 0, Windows Mono Development build 통과, Editor DLL 미포함. 서로 다른 크기의 두 prefab을 같은 시간/카메라/화각으로 비교 렌더링, 잘못된 seek와 prefab 입력의 기존 상태 보존, 원본 prefab 바이트 보존, 별도 scene과 창의 양쪽 렌더링, 닫기 시 양쪽 자원 해제를 검사했다. 실제 마우스 드래그/휠/재생 버튼 조작은 수동 확인 대상이다.

## 최종 검증

검증한 코드: `8a9e7ac` (기능 5개를 개별 커밋/푸시). 브랜치: `codex/sheets-vfx-features`. 기존 로컬 develop의 다른 변경 사항은 포함하지 않는다. 패키지 버전과 공개 release tag는 변경하지 않는다.

| Unity | Sheets Editor | Sheets Play Mode | VFX Editor | Windows Mono Development |
| --- | --- | --- | --- | --- |
| 2022.3.62f3 | 50/50 | 56/56 | 28/28 | 두 패키지 build 통과, Sheets Player 실행 통과 |
| 6000.3.20f1 | 50/50 | 56/56 | 28/28 | 두 패키지 build 통과, Sheets Player 실행 통과 |

모든 테스트 failed/skip 0. Editor assembly의 Player 제외 확인. VFX Player build는 Editor 패키지 제외를 검사하며 게임 내 효과 실행 증거가 아니다. Built-in pipeline의 local UPM 소스를 검사했다. 이번 기능의 URP/IL2CPP/mobile runtime 검증은 실행하지 않았다.

추가 확인: `python tools/verify-upm.py` 6개 패키지 통과; `python -m unittest discover -s tools/tests -q` 36/36 통과; `git diff --check` 통과. 변경된 Editor 코드에 `[TEST]`, 임시 디버그 로그/필드, 직접 throw, 내부 접근 제한자, serialized rename은 없다. 신규 `.meta` GUID는 중복 없이 추가하며 기존 GUID/scene/prefab/runtime API를 변경하지 않는다.

### 재현 명령

```powershell
powershell -ExecutionPolicy Bypass -File tools/test-sheets-package.ps1 -UnityVersion 2022.3.62f3
powershell -ExecutionPolicy Bypass -File tools/test-sheets-package.ps1 -UnityVersion 6000.3.20f1
powershell -ExecutionPolicy Bypass -File tools/test-vfx-package.ps1 -UnityVersion 2022.3.62f3
powershell -ExecutionPolicy Bypass -File tools/test-vfx-package.ps1 -UnityVersion 6000.3.20f1
```

### 검증 결과 경로

- Sheets 2022: `C:/Users/search/AppData/Local/Temp/UnityTools-Sheets-2022.3.62f3-c35d856df3d947d5bc89a7383de43110/summary.json`
- Sheets 6: `C:/Users/search/AppData/Local/Temp/UnityTools-Sheets-6000.3.20f1-267f97eb21534923bb8206cfe1f03118/summary.json`
- VFX 2022: `C:/Users/search/AppData/Local/Temp/UnityTools-Vfx-2022.3.62f3-42f9462344d04628b02177e169941038/summary.json`
- VFX 6: `C:/Users/search/AppData/Local/Temp/UnityTools-Vfx-6000.3.20f1-e842fcc52b684561b59fc1813684ec2e/summary.json`

### 수동 GUI 확인 항목

- CSV: 타입/Enum/Array와 key/reference 설정 후 Editor 폴더에 Save preset. 창 재개방과 script reload 후 프리셋 선택/설정 복원, CSV 헤더 변경 경고 확인.
- CSV: 여러 타입 오류와 중복/누락 ID를 만들고 Validate & Preview. 행/열 목록, Open 동작, Save C# 숨김 확인. 수정 후 생성/한글 경로 저장과 기존 파일 덮어쓰기 확인.
- VFX: Editor 폴더에 Library 생성, Apply labels 후 재개방/공유 에셋 복원. Tag/Collection과 Favorites/Loop/Color의 결합 필터 확인.
- VFX: Analyze colors 후 창 재개방/Refresh. 캐시 복원과 material/프레임 변경 후 재분석, 캐시 권한 실패 경고 확인.
- VFX Compare: 서로 다른 두 효과로 Play/Restart/Seek/Speed/Replay, 드래그/휠, 선택 시점 Fit together 확인. Play Mode/에셋 변경/창 닫기 후 원본 scene 유지 확인.
