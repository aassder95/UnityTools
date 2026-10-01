using System;

namespace UnityTools.Benchmark
{
    public class BenchmarkSession
    {
        //============================================================
        // Readonly
        //============================================================
        private static readonly Comparison<double> _sampleOrder = (left, right) => left.CompareTo(right);
        private readonly string _scenarioId;
        private readonly int _agentCnt;
        private readonly int _seed;
        private readonly int _warmupFrames;
        private readonly int _sampleFrames;
        private readonly string _markerName;
        private readonly string _unityVersion;
        private readonly string _deviceModel;
        private readonly string _platform;
        private readonly double[] _frameTimesMs;
        private readonly double[] _mainThreadTimesMs;
        private readonly double[] _markerTimesMs;

        //============================================================
        // Fields
        //============================================================
        private int _warmupCnt;
        private int _sampleCnt;
        private double _frameTotalMs;
        private double _mainThreadTotalMs;
        private double _markerTotalMs;
        private long _gcTotalBytes;
        private double _maxFrameMs;
        private long _peakMemoryBytes;

        //============================================================
        // Properties
        //============================================================
        public bool IsComplete => _sampleCnt == _sampleFrames;
        public int SampleCnt => _sampleCnt;

        //============================================================
        // Constructors
        //============================================================
        private BenchmarkSession(string scenarioId, int agentCnt, int seed, int warmupFrames, int sampleFrames, string markerName,
            string unityVersion, string deviceModel, string platform)
        {
            _scenarioId = scenarioId;
            _agentCnt = agentCnt;
            _seed = seed;
            _warmupFrames = warmupFrames;
            _sampleFrames = sampleFrames;
            _markerName = markerName;
            _unityVersion = unityVersion;
            _deviceModel = deviceModel;
            _platform = platform;
            _frameTimesMs = new double[sampleFrames];
            _mainThreadTimesMs = new double[sampleFrames];
            _markerTimesMs = markerName == null ? null : new double[sampleFrames];
        }

        //============================================================
        // Logic
        //============================================================
        public static bool TryCreate(string scenarioId, int agentCnt, int seed, int warmupFrames, int sampleFrames,
            string markerName, string unityVersion, string deviceModel, string platform, out BenchmarkSession session)
        {
            session = null;
            if (string.IsNullOrWhiteSpace(scenarioId) || agentCnt < 0 || warmupFrames < 0 || sampleFrames <= 0)
                return false;

            if (string.IsNullOrWhiteSpace(markerName))
                markerName = null;

            session = new BenchmarkSession(scenarioId, agentCnt, seed, warmupFrames, sampleFrames, markerName,
                unityVersion, deviceModel, platform);
            return true;
        }

        public bool AddFrame(double frameIntervalMs, long mainThreadNs, long gcBytes, long memoryBytes, long markerNs)
        {
            if (IsComplete || frameIntervalMs <= 0.0d || double.IsNaN(frameIntervalMs) || double.IsInfinity(frameIntervalMs)
                || mainThreadNs <= 0 || gcBytes < 0 || memoryBytes < 0 || markerNs < 0)
                return false;

            if (_warmupCnt < _warmupFrames)
            {
                ++_warmupCnt;
                return true;
            }

            double mainThreadMs = mainThreadNs / 1000000.0d;
            _frameTimesMs[_sampleCnt] = frameIntervalMs;
            _mainThreadTimesMs[_sampleCnt] = mainThreadMs;
            _frameTotalMs += frameIntervalMs;
            _mainThreadTotalMs += mainThreadMs;
            _gcTotalBytes += gcBytes;
            if (frameIntervalMs > _maxFrameMs)
                _maxFrameMs = frameIntervalMs;

            if (memoryBytes > _peakMemoryBytes)
                _peakMemoryBytes = memoryBytes;

            if (_markerTimesMs != null)
            {
                double markerMs = markerNs / 1000000.0d;
                _markerTimesMs[_sampleCnt] = markerMs;
                _markerTotalMs += markerMs;
            }

            ++_sampleCnt;
            return true;
        }

        public bool TryGetResult(DateTime recordedUtc, out BenchmarkResult result)
        {
            result = null;
            if (!IsComplete)
                return false;

            Array.Sort(_frameTimesMs, _sampleOrder);
            Array.Sort(_mainThreadTimesMs, _sampleOrder);
            int p95Idx = (int)Math.Ceiling(_sampleFrames * 0.95d) - 1;
            double p95MarkerMs = 0.0d;
            if (_markerTimesMs != null)
            {
                Array.Sort(_markerTimesMs, _sampleOrder);
                p95MarkerMs = _markerTimesMs[p95Idx];
            }

            result = new BenchmarkResult(_scenarioId, _agentCnt, _seed, _warmupFrames, _sampleFrames, _markerName,
                _unityVersion, _deviceModel, _platform, recordedUtc, _frameTotalMs / _sampleFrames,
                _frameTimesMs[p95Idx], _maxFrameMs, _mainThreadTotalMs / _sampleFrames, _mainThreadTimesMs[p95Idx],
                _gcTotalBytes / (double)_sampleFrames, _gcTotalBytes,
                _peakMemoryBytes, _markerTotalMs / _sampleFrames, p95MarkerMs);
            return true;
        }
    }
}
