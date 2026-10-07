# UnityTools Benchmark

Unity 2022.3 이상에서 같은 게임 시나리오를 반복 측정하는 독립 UPM 패키지입니다. UI와 Timer 패키지에 의존하지 않습니다.

## 설치

```text
https://github.com/aassder95/UnityTools.git?path=/UnityTools/Packages/com.aassder95.unitytools.benchmark#unitytools-benchmark/v1.0.0
```

1.0.0의 공개 API와 코어 CSV 21열은 [계약 문서](Documentation~/api.md)에 있습니다. [변경 기록](CHANGELOG.md)과 [MIT 라이선스](LICENSE)를 함께 배포합니다.

현재 checkout에는 공개 태그 이후의 통계 안정성 보완도 포함됩니다. 태그는 기존 배포 commit을 유지합니다.

## 측정값

- 프레임 간격: 평균, 95백분위, 최대 (ms), 평균 간격에서 계산한 FPS
- Main Thread 마커 시간: 평균, 95백분위 (ms)
- GC Allocated In Frame: 프레임당 평균과 합계 (bytes)
- System Used Memory: 측정 구간 최댓값 (bytes)
- 선택한 게임 `ProfilerMarker`: 평균과 95백분위 (ms)

`ProfilerRecorder.LastValue`는 완료된 이전 프레임 값을 제공합니다. 워밍업은 측정에서 제외합니다. 카운터가 제공되지 않으면 `TryStart`가 실패합니다. 특히 `GC Allocated In Frame`은 Release Player에서 제공되지 않으므로 Android Development Build에서 측정하세요. Editor 수치는 기기 수치와 직접 비교하지 마세요.

## 측정 계약과 비용

- `TryStart`는 이미 실행 중이면 실패합니다. `Release`는 진행 중인 측정을 취소하며 이후 새 측정을 시작할 수 있습니다.
- 시작한 프레임은 수집하지 않으며 같은 프레임의 중복 호출도 수집하지 않습니다. 매 프레임 한 번 호출해야 연속 구간이 됩니다. 호출을 건너뛰면 중간 프레임을 소급해서 수집하지 않습니다.
- 워밍업은 유효하게 수집한 프레임 수로 셉니다. 유효하지 않은 입력은 워밍업과 샘플 수를 늘리지 않습니다.
- P95는 오름차순 표본의 `ceil(sampleFrames × 0.95) - 1` 위치를 사용합니다. 예를 들어 1~20 표본의 P95는 19입니다. 보간한 백분위가 아닙니다.
- 선택한 마커는 Scripts category에 미리 등록해야 합니다. 같은 프레임의 호출 시간은 합산하며, 실행되지 않은 프레임도 0으로 포함합니다.
- 시작 시 배열과 recorder를 확보합니다. 완료 전 수집 경로에는 managed 할당이 없습니다. 완료 시에는 기존 배열을 정렬하고 결과 객체를 생성합니다. CSV 변환·파일 쓰기도 할당과 동기 I/O를 수행하므로 별도 비용입니다.
- UTC 시각은 측정 완료 시점에 한 번 조회합니다. `BenchmarkSession.TryGetResult`를 직접 쓸 때는 UTC 시각을 전달해야 합니다.

검수 근거와 검증 범위는 [측정 검수 문서](Documentation~/measurement.md)에 있습니다. 측정 도구가 수집하는 프레임에는 애플리케이션·Editor·Profiler의 비용도 들어 있으므로 기기 성능이나 특정 로직만의 비용으로 단정하지 않습니다.

## 사용

UI 성능 시연은 Package Manager에서 `UI Performance Lab` 샘플을 Import하세요. 별도로 UnityTools UI 패키지가 필요합니다. `UiPerformanceLab.unity`에서 항목 수·시드와 세 가지 스크롤 시나리오를 선택해 측정하고 CSV로 저장할 수 있습니다. 자세한 조건과 통계 해석은 샘플 README를 참고하세요.

COMPARE A/B는 전체 항목을 생성하는 일반 ScrollRect와 DynamicScroll 가상화를 같은 조건으로 비교합니다. 초기화 시간과 실행 중 프레임·GC·프로세스 메모리·객체 수를 분리하고, 실행 순서를 바꿀 수 있습니다. 비교는 최대 10,000개이며 가상화 단독 RUN은 최대 100,000개입니다. Pairs 1~20쌍을 반복하며 매 쌍마다 실행 순서를 교대합니다. 완료된 쌍의 중앙값·모집단 표준편차·쌍별 개선율을 표시하고, 취소 시 완료된 쌍을 보존합니다. 별도 series CSV에 개별 실행과 요약을 같은 series_id로 연결하며 화면·품질·VSync·프레임 제한도 기록합니다. 기존 단일 쌍 CSV API는 유지합니다.

게임의 승객 갱신 경로에 `ProfilerMarker`를 붙이고, 동일한 시드로 시나리오를 준비합니다. 마커는 측정 시작 전에 생성되어 있어야 합니다.

```csharp
using System.IO;
using Unity.Profiling;
using UnityEngine;
using UnityTools.Benchmark;

private static readonly ProfilerMarker _passengerTickMarker = new ProfilerMarker(ProfilerCategory.Scripts, "Passenger.Tick");
private readonly BenchmarkSampler _benchmark = new BenchmarkSampler();

private void UpdatePassengers()
{
    using (_passengerTickMarker.Auto())
    {
        // 게임의 실제 승객 갱신
    }
}

private void StartBenchmark(int agentCnt, int seed)
{
    // 게임에서 동일한 승객 수, 시드, 시설 상태로 시나리오를 먼저 준비
    if (!_benchmark.TryStart("AirportBaseline", agentCnt, seed, 120, 600, "Passenger.Tick"))
        Debug.LogWarning("벤치마크를 시작할 수 없습니다. 실행 상태와 Profiler 카운터를 확인하세요.");
}

private void Update()
{
    if (_benchmark.TryCaptureFrame(out BenchmarkResult result))
        BenchmarkCsv.TryAppend(Path.Combine(Application.persistentDataPath, "airport-benchmark.csv"), result);
}

private void OnDestroy()
{
    _benchmark.Release();
}
```

`TryCaptureFrame`은 프레임당 한 번 호출하고 측정이 끝난 프레임에만 `true`를 반환합니다. CSV에는 시나리오 ID, 승객 수, 시드, 워밍업·측정 프레임 수, Unity 버전, 기기, 플랫폼, UTC 시각도 기록합니다. 100/300/500명 결과는 같은 기기·빌드·품질 설정·시드·카메라·시설 상태에서 각각 별도 행으로 기록하세요. 측정 도구는 승객 생성이나 게임 시뮬레이션을 대신하지 않습니다.
