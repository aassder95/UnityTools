# 미통합 브랜치 정리 — 2026-10-07

`codex/sheets-vfx-features`의 남은 기능과 `codex/package-releases`의 배포·포트폴리오 자료를 develop에 통합했습니다. 기준 develop은 `b2ea517`이며 기능별 커밋을 순서대로 푸시했습니다.

## 가져온 기능

- Sheets: 생성 프리셋, 오류 목록·참조 검증, 프리셋 일괄 검증·생성, 키 기준 데이터 비교, 필수 값·숫자 범위·문자열 길이 규칙
- VFX: 태그·컬렉션, 썸네일·색상 분석의 디스크 캐시, 두 효과 동기화 비교, 구조·파티클 비용 요약, 비교 PNG 출력
- 배포: Benchmark 1.0.0 manifest·계약·라이선스, 실제 공개 태그 설치 안내, 공개 태그 검증 스크립트, 재현 가능한 TGZ 생성 도구
- 포트폴리오: 2026-10-01 Windows 원본 CSV·환경·화면 자료와 독립 임시 프로젝트용 시연 도구

## 유지한 최신 변경

이미 develop에 있던 migration 버전 overflow 수정과 Benchmark 수집 최적화는 중복 적용하지 않았습니다. Persistence의 `ESaveFailure`·상세 실패 overload, UI Performance Lab의 큰 유한값 통계 보완과 추가 회귀 테스트도 유지했습니다. 해당 런타임·샘플 코드는 통합 기준 develop과 같습니다.

기존 공개 태그 두 개는 `8b5c4f48c32efc9973a0fa8978f1879a7235102a`를 계속 가리킵니다. 태그를 이동하거나 새 Release를 발행하지 않았습니다. 현재 checkout의 추가 API와 공개 1.0.0의 과거 검증을 설치 안내에서 구분합니다.

## 검증 범위

현재 통합 코드의 Unity 2022.3.62f3·6000.3.20f1 검증 결과는 아래에 기록합니다. Windows 빌드는 Development / Mono입니다. 테스트 실행 수는 버전·시나리오 간 중복을 포함합니다.

| 대상 | 2022.3.62f3 | 6000.3.20f1 | Windows 빌드 |
| --- | --- | --- | --- |
| Sheets Editor / PlayMode | 63/63 + 56/56 | 63/63 + 56/56 | 두 버전 성공, Player 실행 통과 |
| VFX Editor | 30/30 | 30/30 | 두 버전 성공 |
| Benchmark 코어 | 14/14 | 14/14 | 두 버전 성공 |
| Persistence 코어 | 25/25 | 25/25 | 두 버전 성공 |
| UI Performance Lab | 32/32 | 32/32 | 두 버전 성공 |
| Save Recovery Lab | 33/33 | 33/33 | 두 버전 성공 |

현재 코드에서 총 506개 테스트 실행과 12개 Windows 빌드가 통과했습니다. 별도로 공개 Benchmark 1.0.0·UI 2.0.0을 새 임시 Unity 6 프로젝트에 Git 설치해 UI Performance Lab 30/30과 Windows 빌드를 확인했습니다.

Sheets 생성 코드 컴파일·읽기와 별도 Windows Player 실행을 확인했습니다. VFX는 그래픽을 활성화한 Editor 테스트와 런타임 빌드에서 Editor assembly 제외를 확인했습니다.

포트폴리오 도구는 새 공개 태그 설치 프로젝트에서 추가 Windows 빌드와 `-CaptureOnly` Player 실행을 통과했습니다. PNG 53장, 1280×720·2fps·26.5초 MP4, 정상 종료 파일을 확인하고 완료 화면을 검토했습니다. 캡처 실행에는 CSV가 생성되지 않았습니다. 과거 임시 프로젝트의 누락된 package cache 때문에 첫 재빌드는 실패했으며, 새 프로젝트에서 같은 도구로 재검증했습니다. 기존 4쌍 성능 측정을 다시 실행한 결과로 해석하지 않습니다.

정적 배포 검사 6개 패키지, 검사기 회귀 테스트 30개, 6종 샘플 사본 검사와 Release 보안 검사를 통과했습니다. TGZ를 두 새 폴더에 재생성한 SHA-256이 서로 같으며 공개 GitHub Release 파일의 해시와도 같습니다. 2026-10-01 원본 CSV의 4쌍·5개 지표 통계를 다시 계산해 저장된 요약과 대조했습니다.

원래 프로젝트의 추적되지 않은 `UnityTools/UnityTools.slnx`는 별도로 보존합니다. 기존 prefab·scene·meta GUID와 저장 envelope format을 변경하지 않았습니다. 포트폴리오 도구의 기존 `[TEST]` 로그 두 호출 위치는 보존했습니다.

## 남은 직접 확인

새 Editor 창의 파일 대화상자·드래그·휠 등 직접 조작, 신규 기능의 URP 조합 전체, Android/iOS·IL2CPP 및 실제 모바일 성능은 이번 검증에 포함하지 않습니다. 포트폴리오의 기존 수치는 2026-10-01 공개 태그의 Windows 측정이며 최신 develop이나 모바일의 성능 수치가 아닙니다.
