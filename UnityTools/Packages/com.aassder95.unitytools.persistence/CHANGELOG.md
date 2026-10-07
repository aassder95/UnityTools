# Changelog

## Unreleased

- 기존 bool API를 유지하는 저장·로드·파일 API의 상세 실패 원인 overload와 ESaveFailure 추가
- 파일 없음·손상·미래 버전·변환·codec·검증·파일 입출력 실패 분류와 원인 우선순위 문서화
- 상세 결과와 백업·본문 보존 회귀 테스트 추가; 저장 envelope format 변경 없음

## [1.0.0] - 2026-10-01

- Save Recovery Lab UPM 샘플 장면과 여섯 가지 저장·복구 실험 추가
- 파일·데이터 보존 및 장면 버튼 구독 수명 검증 추가

- 공개 API와 실패 출력, 동기 실행 및 단일 writer 계약 문서화
- 기존 저장 format 및 저장 거절/실패/미래 버전 보호 회귀 테스트 추가
- migration 시작 버전의 int overflow로 음수 다음 버전을 허용하던 생성 검증 오류 수정
- UPM 폴더에 MIT 라이선스 원문 포함
- 0.1.0과 API 및 envelope format 동일; 패키지 업그레이드 migration 불필요
- 공개 tag unitytools-persistence/v1.0.0 발행; 이후 상세 실패 API는 Unreleased에 기록

## 0.1.0

- 버전형 로컬 저장, 순차 migration, 검증, 무결성 검사, 백업 복구 추가
