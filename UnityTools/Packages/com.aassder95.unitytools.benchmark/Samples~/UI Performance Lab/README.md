# UI Performance Lab

DynamicScroll의 항목 재사용과 프레임 비용을 확인하는 실행 가능한 샘플입니다. Benchmark와 UI 패키지를 설치한 뒤 Benchmark의 `UI Performance Lab` 샘플을 Import합니다. `UiPerformanceLab.unity`를 열고 Play를 누르세요. 장면을 다시 만들려면 `Tools > UnityTools > Create UI Performance Lab`을 사용합니다.

## 실행

항목 수(기본 10,000)와 시드(기본 42)를 입력하고 시나리오를 선택한 뒤 RUN을 누릅니다. 기본 설정은 워밍업 120프레임, 측정 600프레임입니다. Controller Inspector에서 이동 간격, 삽입 개수, 변경 주기, 제한 시간을 조정할 수 있습니다.

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
