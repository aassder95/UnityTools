# Sheets / VFX 작업 흐름 확장

## 일괄 검증과 생성

Unity 2022.3.62f3: Editor 52/52, PlayMode 56/56, 생성 코드 컴파일/읽기, Windows Mono Development build/Player 통과. 여러 프리셋 오류 수집, 검증 실패 시 기존 파일 보존/새 파일 미생성, 중복 출력/타입 거부, 경로 이탈 거부, 검증만 실행할 때 파일 미생성, 정상 생성 확인. 수동 확인 대화상자는 별도 확인 대상. OS 저장 오류 시 앞서 쓴 파일은 유지하므로 파일 시스템 전체 트랜잭션은 제공하지 않는다.

## VFX 비용 요약

Unity 2022.3.62f3: Editor 29/29, skip 0, Windows Mono Development build 통과/Editor DLL 제외. 구조 수, material 슬롯/고유 수, maxParticles 합, 구간 내 관측 peak, 잘못된 구간/샘플 수 거부, 원본 prefab 바이트 보존 확인. 수치는 고정 간격 샘플 기반으로 GPU 시간/draw call/overdraw 측정이 아니다.

## CSV 변경 비교

Unity 2022.3.62f3: Editor 57/57, PlayMode 56/56, 생성 코드/Windows Mono build/Player 통과. 행/열 순서 무시, 한글 multiline 값, 셀 추가/삭제/변경과 헤더 차이, 빈/중복/없는 키 거부 확인. 비교는 원문 문자열이며 숫자 의미 비교가 아니다.
