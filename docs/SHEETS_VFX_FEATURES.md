# Sheets / VFX 추가 기능 검증

## 생성 설정 프리셋

Unity 2022.3.62f3: Editor 46/46, Play Mode 56/56, 생성 코드 컴파일/읽기, Windows Mono Development build/Player 실행 통과. 프리셋 에셋 round-trip, Enum 배열, 한글 출력 경로, CSV rename 참조 유지, 헤더 불일치 시 원자적 거부를 검사했다. 실제 저장 대화상자와 수동 domain reload UI는 별도 확인 대상이다.

## VFX 태그와 컬렉션

Unity 2022.3.62f3: Editor 25/25, skip 0, Windows Mono Development build 통과, Player에 Editor DLL 미포함. 라벨 정규화/중복 제거, 저장 후 재로드, prefab rename 유지, 여러 필터 조합, 원본 prefab 바이트 보존 검증. 수동 GUI 입력/대화상자는 별도 확인 대상.

## CSV 오류 목록과 데이터 참조

Unity 2022.3.62f3: Editor 50/50, Play Mode 56/56, 생성 코드 컴파일/읽기, Windows Mono Development build/Player 통과. 여러 열/행의 타입 오류, quoted multiline 이후 물리적 행 번호, 빈/중복 키, 배열 참조의 모든 누락 ID, 빈 테이블의 잘못된 규칙, 프리셋 규칙 직렬화 검사. UI 저장은 모든 검증 통과 후만 제공한다.

## VFX 디스크 분석 캐시

Unity 2022.3.62f3: Editor 26/26, skip 0, Windows Mono Development build 통과, Editor DLL 미포함. 재로드 후 PNG/색상 복원, 파일 갱신 시각/바이트가 그대로 유지되는 캐시 hit, 프레임 변경, material 저장 후 실제 파일 변경과 캐시 miss, 손상 metadata 거부, 기존 자원 해제/메모리 상한 회귀 검사. 변경 이벤트에서는 기존 결과를 비우고 Refresh에서 복원한다. 의존성 키에는 import hash와 실제 의존 파일의 SHA256을 포함한다.
