# UI Performance Lab

DynamicScroll의 항목 재사용과 프레임 비용을 확인하는 실행 가능한 샘플입니다. Benchmark와 UI 패키지를 설치한 뒤 Benchmark의 `UI Performance Lab` 샘플을 Import합니다. `UiPerformanceLab.unity`를 열고 Play를 누르세요. 장면을 다시 만들려면 `Tools > UnityTools > Create UI Performance Lab`을 사용합니다.

기존에 Import한 샘플은 다시 Import하거나 생성기로 새 장면을 만드세요. 새 필수 참조 `_inputPairCnt`는 반복 횟수 InputField입니다. 기존 필드 rename은 없으며, 패키지/미러 장면과 생성기에 참조를 연결했습니다. 직접 만든 장면에서는 이 참조를 Inspector에서 연결해야 합니다.

## 실행

항목 수(기본 1,000)와 시드(기본 42)를 입력하고 시나리오를 선택한 뒤 RUN을 누릅니다. 기본 설정은 워밍업 120프레임, 측정 600프레임입니다. Controller Inspector에서 이동 간격, 삽입 개수, 변경 주기, 제한 시간을 조정할 수 있습니다.

## 일반 스크롤 / 가상화 비교

COMPARE A/B는 같은 데이터·시드·viewport·시나리오를 두 방식으로 순차 실행합니다. Pairs 입력은 1~20쌍(기본 4)이며, 선택한 첫 순서에서 시작해 매 쌍마다 A→B/B→A를 교대합니다. 1쌍을 선택하면 단일 비교입니다. 비교는 10,000개 이하(Inspector에서 더 낮은 제한 설정 가능), RUN의 가상화 단독 실행은 100,000개 이하입니다.

- Baseline: 모든 항목의 UI 객체를 생성하는 고정 높이 ScrollRect입니다. 레이아웃 그룹은 사용하지 않으며, 삽입/삭제 때 전체 위치와 바인딩을 갱신합니다.
- Virtualized: DynamicScroll이 보이는 항목을 생성·재사용하며 삽입/삭제를 증분 처리합니다.
- 각 실행 전 객체를 해제하고 Destroy 완료 프레임을 기다린 후 측정 밖에서 GC.Collect를 실행합니다. 공유 템플릿·폰트와 Unity 내부 캐시는 유지되므로 완전히 새 프로세스의 cold start와는 다릅니다.
- Init + layout은 UI 객체 생성·최초 바인딩·Canvas.ForceUpdateCanvases의 동기 경과 시간입니다. 데이터 문자열 생성·정리·GC·GPU 렌더링은 포함하지 않습니다. 프레임 통계는 이후 워밍업을 제외한 구간입니다.
- Process peak는 프로세스 전체 메모리입니다. Unity allocator가 해제한 메모리를 유지할 수 있으므로 두 값의 차이를 UI 메모리 절감량으로 해석하지 않습니다. 두 실행 순서를 반복 측정하세요.
- Created objects는 각 실행 중 생성 객체 수(일반 방식은 삽입 시 추가 생성 포함), Peak active는 생성된 활성 UI 수입니다. 일반 방식은 viewport 밖에도 활성 객체를 유지합니다. 바인딩 수는 초기화·워밍업을 포함합니다.

화면에는 완료된 비교 쌍의 초기화·Frame P95·Main Thread P95·GC 평균·프로세스 최대 메모리의 중앙값과 모집단 표준편차를 표시합니다. 각 쌍의 개선율은 `(1 - Virtualized / Baseline) × 100`입니다. 양수는 감소, 음수는 증가를 뜻합니다. 각 쌍의 개선율을 계산한 뒤 중앙값·표준편차를 구하므로 두 방식의 중앙값 비율과는 다릅니다. Baseline이 0인 지표는 개선율에서 제외하고 유효 쌍 수를 표시합니다. 모든 기준값이 0이면 N/A입니다. 한 쌍의 표준편차는 0입니다.

Frame P95의 요약은 **각 실행 P95의 중앙값**이며 모든 프레임을 합친 P95가 아닙니다. 표준편차는 완료된 실행 간 분산을 설명하며 신뢰구간·통계적 유의성·성능 우위를 보장하지 않습니다. 짝수 쌍을 선택해야 두 순서의 실행 수가 같습니다.

STOP·비활성화·시간 초과·카운터 실패가 발생하면 진행 중인 불완전 쌍은 제외하고 이미 완료된 쌍은 조회·내보내기할 수 있습니다. 화면과 CSV의 요청/완료 쌍 수를 함께 확인하세요. 화면 해상도·품질 단계·VSync·프레임 제한은 시작 시 기록하고 각 phase 시작 및 쌍 완료 시 비교합니다. 변경이 확인되면 해당 쌍을 제외하고 중단합니다. 워크로드·프레임 설정·viewport가 다른 쌍도 같은 series에 추가하지 않습니다.

## 반복 비교 CSV

EXPORT CSV는 `Application.persistentDataPath/ui-performance-series-v1.csv`에 52열로 저장합니다. `series_id`가 반복 실험을, `pair_id`가 완성된 A/B 쌍을 식별합니다.

- `record_type=run`: 각 쌍의 Baseline·Virtualized 두 행. 기존 Benchmark·비교 33열과 `pair_idx`·요청/완료 쌍 수를 보존합니다. 항목 수·시드·워밍업/측정 프레임·시나리오·Unity/기기/플랫폼·UTC 시각·viewport·워크로드를 재분석할 수 있습니다.
- `record_type=summary`: 초기화, Frame P95, Main Thread P95, GC 평균, 최대 프로세스 메모리의 다섯 행. 중앙값·모집단 표준편차·쌍별 개선율의 중앙값/표준편차·유효 개선율 쌍 수를 저장합니다. 개선율 N/A는 빈 CSV 칸입니다.
- 모든 행에 화면 폭/높이·품질 index/이름·VSync·프레임 제한을 기록합니다. 요약의 원래 실행 조건은 같은 series_id의 run 행에서 읽습니다.
- 파일에 append하므로 같은 결과를 여러 번 내보내면 동일한 ID의 행이 반복됩니다. 분석 시 run은 series_id/pair_id/mode, summary는 series_id/metric/completed_pairs로 중복을 구분하세요. 기존 파일의 헤더가 다르면 추가하지 않습니다. 파일 쓰기의 전원 손실 원자성은 보장하지 않습니다.

기존 `UiLabComparison.TryExport`의 단일 쌍 33열 CSV와 `UiLabReport`의 RUN CSV는 유지합니다. UI의 COMPARE 내보내기는 별도 series 파일을 사용합니다. 소스 revision·빌드 설정·기기 온도 등은 별도로 기록하세요.

- Sweep: 일정한 항목 간격으로 앞뒤 이동합니다.
- Random Jump: 같은 시드로 재현되는 임의 위치로 이동합니다.
- Insert & Remove: 중간 항목을 보고 있는 상태에서 앞쪽에 항목을 삽입·제거합니다. 기존 항목을 보는 화면 anchor가 유지되는지 확인합니다.

STOP은 결과를 만들지 않고 취소합니다. 완료 후 EXPORT CSV를 누르면 `Application.persistentDataPath/ui-performance-lab.csv`에 결과를 추가하고 경로를 화면에 표시합니다. 입력 오류, 카운터 미지원, 제한 시간 초과, 저장 실패도 화면에 표시합니다.

## 결과 해석

Frame 평균/P95/최대, Main Thread P95, `UiLab.Tick` P95, 평균 GC, 최대 메모리와 생성 객체 수·최대 활성 항목 수·바인딩 횟수를 표시합니다. CSV에는 기기, 플랫폼, Unity 버전, UTC, 시드, 워밍업/측정 프레임, 시나리오 설정도 기록합니다. `agent_cnt` 열은 이 샘플에서 초기 데이터 항목 수입니다.

생성 객체 수는 viewport content의 실제 자식 수이며 비활성 pool 객체도 포함합니다. 바인딩 횟수와 최대 활성 항목 수는 워밍업을 포함한 실행 구간 값입니다. 프레임 통계는 워밍업을 제외합니다. 항목 표시 문자열과 삽입 데이터는 실행 전에 만들고, 측정 중 결과 텍스트를 갱신하지 않습니다. Text의 mesh 갱신, DynamicScroll 및 삽입/삭제 자체의 비용은 측정 대상입니다. 이 샘플은 GC 0을 보장하지 않습니다.

측정 마커는 데이터 변경·스크롤·바인딩의 동기 실행 범위를 포함합니다. Canvas 렌더링까지 포함한 전체 비용은 프레임 및 Main Thread 통계로 확인하세요. 반복 실행은 이미 생성된 객체를 재사용하므로 최초 초기화 비용 측정과 구분해야 합니다.

## 포트폴리오 시연

1. 같은 viewport에서 1,000 / 10,000 / 100,000 항목을 각각 실행합니다.
2. 데이터 수에 비해 생성 UI 객체 수가 작게 유지되는 모습을 보여줍니다.
3. 같은 기기·빌드·해상도·품질·시드·프레임 설정으로 각 조건을 여러 번 실행합니다.
4. CSV와 화면 녹화에 측정 조건을 함께 남깁니다. 최적화 전후 비교에는 각각의 코드 revision도 별도로 기록하세요.

Editor 결과는 기능 확인용입니다. Android 등 대상 기기의 Development Build에서 최종 측정하고 Editor 수치와 직접 비교하지 마세요. GC 카운터가 없는 Release Build에서는 실행이 거부될 수 있습니다. 기본 장면의 StandaloneInputModule을 사용하려면 Active Input Handling을 Input Manager 또는 Both로 설정합니다.

샘플의 UI 의존성은 샘플 assembly에만 있습니다. Benchmark runtime 패키지는 UI 패키지에 의존하지 않습니다. Import 후 Test Runner에서 `UnityTools.Benchmark.Samples.Tests` Play Mode 테스트를 실행할 수 있습니다. 장면을 Build Settings에 추가해야 장면 로드 테스트가 실행됩니다.

## 비교 기능 검증 기록

2026-10-01, 빈 프로젝트에 로컬 패키지를 설치하고 Package Manager Sample API로 Import했습니다. Unity `2022.3.62f3`과 `6000.3.20f1` 모두 Play Mode **30/30**(코어 14 + 샘플 16), Windows Development Build(Mono)가 통과했습니다. 기존 샘플 장면 및 생성기로 새로 만든 장면을 각각 검증했습니다. 검증에는 기존 세 시나리오·두 시작 순서·가상화 단독 1,000/10,000/100,000개, 네 쌍 순서 교대, 완료된 쌍을 보존하는 취소·비활성화·설정 변경 중단, 쌍별 통계·0 기준 N/A·CSV 연결·기존 33열 형식 유지가 포함됩니다.

기능·빌드 검증 결과이며 성능 개선율의 증거는 아닙니다. 직접 화면 조작·배치, Android/iOS, IL2CPP, 실제 기기에서의 비교 측정은 별도 검증이 필요합니다.


최신 생성 장면 검증 증거는 다음 경로의 `ui-lab/results-final.xml`, `builder-final.log`, `build-final.log`, `build-result.txt`에 보존했습니다. 최초 샘플 Import 검증은 같은 프로젝트의 `results.xml`과 `summary.json`에서 확인합니다.

- `C:/Users/search/AppData/Local/Temp/UnityTools-Compatibility-2022.3.62f3-4ba703f9604841c7a978b6b4b7f9d133`
- `C:/Users/search/AppData/Local/Temp/UnityTools-Compatibility-6000.3.20f1-7c832508ba734c748a46c50b8b686f58`
