# UnityTools Benchmark

Unity 2022.3 이상에서 같은 게임 시나리오를 반복 측정하는 독립 UPM 패키지입니다. UI와 Timer 패키지에 의존하지 않습니다.

## 측정값

- 프레임 간격: 평균, 95백분위, 최대 (ms), 평균 간격에서 계산한 FPS
- Main Thread 마커 시간: 평균, 95백분위 (ms)
- GC Allocated In Frame: 프레임당 평균과 합계 (bytes)
- System Used Memory: 측정 구간 최댓값 (bytes)
- 선택한 게임 `ProfilerMarker`: 평균과 95백분위 (ms)

`ProfilerRecorder.LastValue`는 완료된 이전 프레임 값을 제공합니다. 워밍업은 측정에서 제외합니다. 카운터가 제공되지 않으면 `TryStart`가 실패합니다. 특히 `GC Allocated In Frame`은 Release Player에서 제공되지 않으므로 Android Development Build에서 측정하세요. Editor 수치는 기기 수치와 직접 비교하지 마세요.

## 사용

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
    _benchmark.TryStart("AirportBaseline", agentCnt, seed, 120, 600, "Passenger.Tick");
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
