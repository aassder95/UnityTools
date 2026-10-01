using System;
using Unity.Profiling;
using UnityEngine;

namespace UnityTools.Benchmark
{
    public class BenchmarkSampler
    {
        //============================================================
        // Fields
        //============================================================
        private BenchmarkSession _session;
        private ProfilerRecorder _frameRecorder;
        private ProfilerRecorder _gcRecorder;
        private ProfilerRecorder _memoryRecorder;
        private ProfilerRecorder _markerRecorder;
        private int _lastFrameIdx;
        private bool _hasMarker;

        //============================================================
        // Properties
        //============================================================
        public bool IsRunning => _session != null;

        //============================================================
        // Logic
        //============================================================
        public bool TryStart(string scenarioId, int agentCnt, int seed, int warmupFrames, int sampleFrames, string markerName = null)
        {
            if (IsRunning || !BenchmarkSession.TryCreate(scenarioId, agentCnt, seed, warmupFrames, sampleFrames,
                    markerName, Application.unityVersion, SystemInfo.deviceModel, Application.platform.ToString(), out BenchmarkSession session))
                return false;

            _frameRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 1);
            _gcRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 1);
            _memoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory", 1);
            _hasMarker = !string.IsNullOrWhiteSpace(markerName);
            if (_hasMarker)
                _markerRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Scripts, markerName, 1);

            if (!_frameRecorder.Valid || !_gcRecorder.Valid || !_memoryRecorder.Valid || (_hasMarker && !_markerRecorder.Valid))
            {
                ReleaseRecorders();
                return false;
            }

            _session = session;
            _lastFrameIdx = Time.frameCount;
            return true;
        }

        public bool TryCaptureFrame(out BenchmarkResult result)
        {
            result = null;
            if (!IsRunning || Time.frameCount == _lastFrameIdx)
                return false;

            _lastFrameIdx = Time.frameCount;
            if (_frameRecorder.Count == 0 || _gcRecorder.Count == 0 || _memoryRecorder.Count == 0 || (_hasMarker && _markerRecorder.Count == 0))
                return false;

            long markerNs = _hasMarker ? _markerRecorder.LastValue : 0;
            if (!_session.AddFrame(Time.unscaledDeltaTime * 1000.0d, _frameRecorder.LastValue,
                    _gcRecorder.LastValue, _memoryRecorder.LastValue, markerNs))
                return false;

            if (!_session.IsComplete || !_session.TryGetResult(DateTime.UtcNow, out result))
                return false;

            Release();
            return true;
        }

        public void Release()
        {
            if (!IsRunning)
                return;

            ReleaseRecorders();
            _session = null;
            _hasMarker = false;
        }

        //============================================================
        // Utilities
        //============================================================
        private void ReleaseRecorders()
        {
            _frameRecorder.Dispose();
            _gcRecorder.Dispose();
            _memoryRecorder.Dispose();
            if (_hasMarker)
                _markerRecorder.Dispose();
        }
    }
}
