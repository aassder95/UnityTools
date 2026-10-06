using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityTools.Benchmark.Tests
{
    public class BenchmarkTests
    {
        //============================================================
        // Readonly
        //============================================================
        private static readonly ProfilerMarker _sampleMarker = new ProfilerMarker(ProfilerCategory.Scripts, "Benchmark.Tests.Sample");

        //============================================================
        // Logic
        //============================================================
        [Test]
        public void SessionExcludesWarmupAndCalculatesPercentile()
        {
            Assert.That(BenchmarkSession.TryCreate("Airport", 300, 42, 1, 4, "Passenger.Tick", "2022.3", "Device", "Android", out BenchmarkSession session), Is.True);
            Assert.That(session.AddFrame(100.0d, 100000000, 999, 9999, 9000000), Is.True);
            Assert.That(session.AddFrame(10.0d, 8000000, 10, 100, 1000000), Is.True);
            Assert.That(session.AddFrame(20.0d, 16000000, 20, 200, 2000000), Is.True);
            Assert.That(session.AddFrame(30.0d, 24000000, 30, 300, 3000000), Is.True);
            Assert.That(session.TryGetResult(DateTime.UtcNow, out _), Is.False);
            Assert.That(session.AddFrame(40.0d, 32000000, 40, 400, 4000000), Is.True);
            Assert.That(session.AddFrame(50.0d, 40000000, 50, 500, 5000000), Is.False);
            Assert.That(session.TryGetResult(DateTime.UtcNow, out BenchmarkResult result), Is.True);
            Assert.That(result.MeanFrameMs, Is.EqualTo(25.0d));
            Assert.That(result.P95FrameMs, Is.EqualTo(40.0d));
            Assert.That(result.MaxFrameMs, Is.EqualTo(40.0d));
            Assert.That(result.FpsFromMeanFrame, Is.EqualTo(40.0d));
            Assert.That(result.MeanMainThreadMs, Is.EqualTo(20.0d));
            Assert.That(result.P95MainThreadMs, Is.EqualTo(32.0d));
            Assert.That(result.TotalGcBytes, Is.EqualTo(100));
            Assert.That(result.PeakMemoryBytes, Is.EqualTo(400));
            Assert.That(result.MeanMarkerMs, Is.EqualTo(2.5d));
            Assert.That(result.P95MarkerMs, Is.EqualTo(4.0d));
        }

        [Test]
        public void SessionRejectsInvalidInput()
        {
            Assert.That(BenchmarkSession.TryCreate("", 100, 1, 0, 10, null, "2022.3", "Device", "Android", out _), Is.False);
            Assert.That(BenchmarkSession.TryCreate("Airport", 100, 1, -1, 10, null, "2022.3", "Device", "Android", out _), Is.False);
            Assert.That(BenchmarkSession.TryCreate("Airport", 100, 1, 0, 0, null, "2022.3", "Device", "Android", out _), Is.False);
        }

        [TestCase(1, 1)]
        [TestCase(20, 19)]
        [TestCase(100, 95)]
        public void PercentileUsesNearestRank(int sampleCnt, int expectedP95)
        {
            Assert.That(BenchmarkSession.TryCreate("Percentile", 1, 1, 0, sampleCnt, "Marker", "Unity", "Device", "Platform", out BenchmarkSession session), Is.True);
            for (int idx = sampleCnt; idx > 0; --idx)
            {
                Assert.That(session.AddFrame(idx, idx * 1000000L, idx, idx, idx * 1000000L), Is.True);
            }

            Assert.That(session.TryGetResult(DateTime.UtcNow, out BenchmarkResult result), Is.True);
            Assert.That(result.P95FrameMs, Is.EqualTo(expectedP95));
            Assert.That(result.P95MainThreadMs, Is.EqualTo(expectedP95));
            Assert.That(result.P95MarkerMs, Is.EqualTo(expectedP95));
            Assert.That(session.TryGetResult(DateTime.UtcNow, out BenchmarkResult repeated), Is.True);
            Assert.That(repeated.P95FrameMs, Is.EqualTo(result.P95FrameMs));
            Assert.That(repeated.MeanFrameMs, Is.EqualTo(result.MeanFrameMs));
        }

        [Test]
        public void InvalidFramesPreserveProgress()
        {
            Assert.That(BenchmarkSession.TryCreate("InvalidFrames", 1, 1, 1, 1, null, "Unity", "Device", "Platform", out BenchmarkSession session), Is.True);
            Assert.That(session.AddFrame(double.NaN, 1000000, 0, 0, 0), Is.False);
            Assert.That(session.AddFrame(double.PositiveInfinity, 1000000, 0, 0, 0), Is.False);
            Assert.That(session.AddFrame(0.0d, 1000000, 0, 0, 0), Is.False);
            Assert.That(session.AddFrame(1.0d, 0, 0, 0, 0), Is.False);
            Assert.That(session.AddFrame(1.0d, 1000000, -1, 0, 0), Is.False);
            Assert.That(session.AddFrame(100.0d, 1000000, 100, 100, 0), Is.True);
            Assert.That(session.SampleCnt, Is.EqualTo(0));
            Assert.That(session.AddFrame(1.0d, 1000000, 0, -1, 0), Is.False);
            Assert.That(session.AddFrame(1.0d, 1000000, 0, 0, -1), Is.False);
            Assert.That(session.AddFrame(2.0d, 1000000, 2, 2, 0), Is.True);
            Assert.That(session.TryGetResult(DateTime.UtcNow, out BenchmarkResult result), Is.True);
            Assert.That(result.MeanFrameMs, Is.EqualTo(2.0d));
            Assert.That(result.TotalGcBytes, Is.EqualTo(2));
        }

        [Test]
        public void AllocationCounterDetectsManagedAllocation()
        {
            using (ProfilerRecorder recorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC.Alloc", 512, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.CollectOnlyOnCurrentThread))
            {
                Assert.That(recorder.Valid, Is.True);
                recorder.Reset();
                recorder.Start();
                byte[] allocation = new byte[4096];
                recorder.Stop();
                GC.KeepAlive(allocation);
                Assert.That(recorder.Count, Is.GreaterThan(0));
            }
        }

        [Test]
        public void IncompleteCollectionDoesNotAllocate()
        {
            Assert.That(BenchmarkSession.TryCreate("Allocation", 1, 1, 1, 256, null, "Unity", "Device", "Platform", out BenchmarkSession session), Is.True);
            session.AddFrame(1.0d, 1000000, 0, 0, 0);
            session.TryGetResult(DateTime.UtcNow, out _);
            bool hasSucceeded = true;
            using (ProfilerRecorder recorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC.Alloc", 512, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.CollectOnlyOnCurrentThread))
            {
                Assert.That(recorder.Valid, Is.True);
                recorder.Reset();
                recorder.Start();
                for (int idx = 0; idx < 128; ++idx)
                {
                    hasSucceeded &= session.AddFrame(1.0d, 1000000, 0, 0, 0);
                    hasSucceeded &= !session.TryGetResult(DateTime.UtcNow, out _);
                }

                recorder.Stop();
                Assert.That(hasSucceeded, Is.True);
                Assert.That(recorder.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void ResultAllocatesSingleObject()
        {
            Assert.That(BenchmarkSession.TryCreate("Small", 1, 1, 0, 1, "Marker", "Unity", "Device", "Platform", out BenchmarkSession small), Is.True);
            Assert.That(BenchmarkSession.TryCreate("Large", 1, 1, 0, 256, "Marker", "Unity", "Device", "Platform", out BenchmarkSession large), Is.True);
            small.AddFrame(1.0d, 1000000, 0, 0, 0);
            for (int idx = 0; idx < 256; ++idx)
            {
                large.AddFrame(idx + 1.0d, 1000000, 0, 0, 0);
            }

            Assert.That(small.TryGetResult(DateTime.UtcNow, out _), Is.True);
            Assert.That(large.TryGetResult(DateTime.UtcNow, out _), Is.True);
            using (ProfilerRecorder recorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC.Alloc", 512, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.CollectOnlyOnCurrentThread))
            {
                Assert.That(recorder.Valid, Is.True);
                recorder.Reset();
                recorder.Start();
                bool hasSmall = small.TryGetResult(DateTime.UtcNow, out _);
                recorder.Stop();
                int smallAllocCnt = recorder.Count;
                recorder.Reset();
                recorder.Start();
                bool hasLarge = large.TryGetResult(DateTime.UtcNow, out _);
                recorder.Stop();
                int largeAllocCnt = recorder.Count;
                Assert.That(hasSmall && hasLarge, Is.True);
                Assert.That(smallAllocCnt, Is.EqualTo(1));
                Assert.That(largeAllocCnt, Is.EqualTo(1));
            }
        }

        [Test]
        public void CsvAppendsHeaderOnceAndEscapesScenario()
        {
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
            BenchmarkResult result = new BenchmarkResult("Airport, \"Rush\"", 100, 1, 0, 1, null,
                "2022.3", "Device", "Android", new DateTime(2026, 9, 29, 0, 0, 0, DateTimeKind.Utc),
                20.0d, 20.0d, 20.0d, 15.0d, 15.0d, 5.0d, 5, 100, 0.0d, 0.0d);
            try
            {
                Assert.That(BenchmarkCsv.TryAppend(path, result), Is.True);
                Assert.That(BenchmarkCsv.TryAppend(path, result), Is.True);
                string[] lines = File.ReadAllLines(path);
                Assert.That(lines.Length, Is.EqualTo(3));
                Assert.That(lines[0], Is.EqualTo(BenchmarkCsv.HEADER));
                Assert.That(lines[1], Does.StartWith("\"Airport, \"\"Rush\"\"\",100,1"));
                Assert.That(lines[2], Is.EqualTo(lines[1]));
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        [UnityTest]
        public IEnumerator SamplerCollectsWithoutAllocation()
        {
            BenchmarkSampler sampler = new BenchmarkSampler();
            Assert.That(sampler.TryStart("SamplerAllocation", 1, 1, 1, 100), Is.True);
            try
            {
                yield return null;
                bool hasCompleted = sampler.TryCaptureFrame(out _);
                using (ProfilerRecorder recorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC.Alloc", 512, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.CollectOnlyOnCurrentThread))
                {
                    Assert.That(recorder.Valid, Is.True);
                    recorder.Stop();
                    int allocCnt = 0;
                    for (int idx = 0; idx < 8; ++idx)
                    {
                        yield return null;
                        recorder.Reset();
                        recorder.Start();
                        hasCompleted |= sampler.TryCaptureFrame(out _);
                        recorder.Stop();
                        allocCnt += recorder.Count;
                    }

                    Assert.That(hasCompleted, Is.False);
                    Assert.That(allocCnt, Is.EqualTo(0));
                }
            }
            finally
            {
                sampler.Release();
            }
        }

        [UnityTest]
        public IEnumerator SamplerRestartsAfterCancellation()
        {
            BenchmarkSampler sampler = new BenchmarkSampler();
            Assert.That(sampler.TryStart("Cancelled", 1, 1, 0, 100), Is.True);
            Assert.That(sampler.TryStart("Duplicate", 1, 1, 0, 1), Is.False);
            sampler.Release();
            sampler.Release();
            Assert.That(sampler.IsRunning, Is.False);
            Assert.That(sampler.TryCaptureFrame(out BenchmarkResult stopped), Is.False);
            Assert.That(stopped, Is.Null);
            Assert.That(sampler.TryStart("Restarted", 1, 1, 0, 2), Is.True);
            Assert.That(sampler.TryCaptureFrame(out _), Is.False);
            BenchmarkResult result = null;
            try
            {
                for (int idx = 0; idx < 20 && result == null; ++idx)
                {
                    yield return null;
                    if (sampler.TryCaptureFrame(out BenchmarkResult captured))
                        result = captured;

                    Assert.That(sampler.TryCaptureFrame(out BenchmarkResult duplicate), Is.False);
                    Assert.That(duplicate, Is.Null);
                }

                Assert.That(result, Is.Not.Null);
                Assert.That(result.ScenarioId, Is.EqualTo("Restarted"));
                Assert.That(result.SampleFrames, Is.EqualTo(2));
                Assert.That(sampler.IsRunning, Is.False);
            }
            finally
            {
                sampler.Release();
            }
        }

        [UnityTest]
        public IEnumerator SamplerCompletesWithRegisteredInactiveMarker()
        {
            BenchmarkSampler sampler = new BenchmarkSampler();
            _ = new ProfilerMarker(ProfilerCategory.Scripts, "Benchmark.Tests.Inactive");
            Assert.That(sampler.TryStart("Inactive", 1, 1, 0, 2, "Benchmark.Tests.Inactive"), Is.True);
            BenchmarkResult result = null;
            try
            {
                for (int idx = 0; idx < 20 && result == null; ++idx)
                {
                    yield return null;
                    if (sampler.TryCaptureFrame(out BenchmarkResult captured))
                        result = captured;
                }

                Assert.That(result, Is.Not.Null);
                Assert.That(result.MeanMarkerMs, Is.EqualTo(0.0d));
                Assert.That(result.P95MarkerMs, Is.EqualTo(0.0d));
            }
            finally
            {
                sampler.Release();
            }
        }

        [UnityTest]
        public IEnumerator SamplerCollectsProfilerFrames()
        {
            BenchmarkSampler sampler = new BenchmarkSampler();
            Assert.That(sampler.TryStart("Sampler", 100, 1, 1, 2, "Benchmark.Tests.Sample"), Is.True);
            BenchmarkResult result = null;
            try
            {
                for (int idx = 0; idx < 12 && result == null; ++idx)
                {
                    using (_sampleMarker.Auto())
                    {
                        _ = idx * idx;
                    }

                    yield return null;
                    sampler.TryCaptureFrame(out result);
                }

                Assert.That(result, Is.Not.Null);
                Assert.That(result.SampleFrames, Is.EqualTo(2));
                Assert.That(result.MeanFrameMs, Is.GreaterThan(0.0d));
                Assert.That(sampler.IsRunning, Is.False);
            }
            finally
            {
                sampler.Release();
            }
        }
    }
}
