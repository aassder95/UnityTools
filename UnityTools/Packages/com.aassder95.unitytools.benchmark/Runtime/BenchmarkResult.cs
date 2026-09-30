using System;

namespace UnityTools.Benchmark
{
    public class BenchmarkResult
    {
        //============================================================
        // Properties
        //============================================================
        public string ScenarioId { get; }
        public int AgentCnt { get; }
        public int Seed { get; }
        public int WarmupFrames { get; }
        public int SampleFrames { get; }
        public string MarkerName { get; }
        public string UnityVersion { get; }
        public string DeviceModel { get; }
        public string Platform { get; }
        public DateTime RecordedUtc { get; }
        public double MeanFrameMs { get; }
        public double P95FrameMs { get; }
        public double MaxFrameMs { get; }
        public double FpsFromMeanFrame { get; }
        public double MeanMainThreadMs { get; }
        public double P95MainThreadMs { get; }
        public double MeanGcBytes { get; }
        public long TotalGcBytes { get; }
        public long PeakMemoryBytes { get; }
        public double MeanMarkerMs { get; }
        public double P95MarkerMs { get; }

        //============================================================
        // Constructors
        //============================================================
        public BenchmarkResult(string scenarioId, int agentCnt, int seed, int warmupFrames, int sampleFrames, string markerName,
            string unityVersion, string deviceModel, string platform, DateTime recordedUtc, double meanFrameMs,
            double p95FrameMs, double maxFrameMs, double meanMainThreadMs, double p95MainThreadMs,
            double meanGcBytes, long totalGcBytes, long peakMemoryBytes, double meanMarkerMs, double p95MarkerMs)
        {
            ScenarioId = scenarioId;
            AgentCnt = agentCnt;
            Seed = seed;
            WarmupFrames = warmupFrames;
            SampleFrames = sampleFrames;
            MarkerName = markerName;
            UnityVersion = unityVersion;
            DeviceModel = deviceModel;
            Platform = platform;
            RecordedUtc = recordedUtc;
            MeanFrameMs = meanFrameMs;
            P95FrameMs = p95FrameMs;
            MaxFrameMs = maxFrameMs;
            FpsFromMeanFrame = meanFrameMs > 0.0d ? 1000.0d / meanFrameMs : 0.0d;
            MeanMainThreadMs = meanMainThreadMs;
            P95MainThreadMs = p95MainThreadMs;
            MeanGcBytes = meanGcBytes;
            TotalGcBytes = totalGcBytes;
            PeakMemoryBytes = peakMemoryBytes;
            MeanMarkerMs = meanMarkerMs;
            P95MarkerMs = p95MarkerMs;
        }
    }
}
