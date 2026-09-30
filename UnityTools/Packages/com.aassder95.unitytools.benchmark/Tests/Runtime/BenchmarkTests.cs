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
