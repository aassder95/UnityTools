using System;
using System.Globalization;
using System.IO;
using NUnit.Framework;

namespace UnityTools.Benchmark.Samples.Tests
{
    public class UiLabSeriesTests
    {
        //============================================================
        // Logic
        //============================================================
        [Test]
        public void SummaryCalculatesPairedRatiosAndDeviation()
        {
            double[] baseline = { 10.0d, 100.0d };
            double[] virtualized = { 5.0d, 100.0d };
            Assert.That(UiLabMetricSummary.TryCreate(baseline, virtualized, out UiLabMetricSummary summary), Is.True);
            Assert.That(summary.BaselineMedian, Is.EqualTo(55.0d));
            Assert.That(summary.VirtualizedMedian, Is.EqualTo(52.5d));
            Assert.That(summary.BaselineDeviation, Is.EqualTo(45.0d));
            Assert.That(summary.VirtualizedDeviation, Is.EqualTo(47.5d));
            Assert.That(summary.ImprovementMedian, Is.EqualTo(25.0d));
            Assert.That(summary.ImprovementDeviation, Is.EqualTo(25.0d));
            Assert.That(summary.ImprovementCnt, Is.EqualTo(2));
        }

        [Test]
        public void MedianPreservesInputAndExcludesZeroRatios()
        {
            double[] baseline = { 100.0d, 0.0d, 10.0d };
            double[] virtualized = { 50.0d, 1.0d, 5.0d };
            Assert.That(UiLabMetricSummary.TryCreate(baseline, virtualized, out UiLabMetricSummary summary), Is.True);
            Assert.That(summary.BaselineMedian, Is.EqualTo(10.0d));
            Assert.That(summary.ImprovementMedian, Is.EqualTo(50.0d));
            Assert.That(summary.ImprovementCnt, Is.EqualTo(2));
            Assert.That(baseline, Is.EqualTo(new[] { 100.0d, 0.0d, 10.0d }));
            Assert.That(virtualized, Is.EqualTo(new[] { 50.0d, 1.0d, 5.0d }));
            Assert.That(UiLabMetricSummary.TryCreate(new[] { 0.0d }, new[] { 0.0d }, out summary), Is.True);
            Assert.That(summary.HasImprovement, Is.False);
            Assert.That(summary.BaselineDeviation, Is.Zero);
            Assert.That(UiLabMetricSummary.TryCreate(new[] { 10.0d }, new[] { 20.0d }, out summary), Is.True);
            Assert.That(summary.ImprovementMedian, Is.EqualTo(-100.0d));
        }

        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        [TestCase(-1.0d)]
        public void InvalidMetricsAreRejected(double value)
        {
            Assert.That(UiLabMetricSummary.TryCreate(new[] { value }, new[] { 1.0d }, out _), Is.False);
            Assert.That(UiLabMetricSummary.TryCreate(new[] { 1.0d }, new[] { value }, out _), Is.False);
        }

        [Test]
        public void SeriesRejectsUnpairedOrMismatchedRuns()
        {
            Assert.That(UiLabSeries.TryCreate(0, true, Environment(), out _), Is.False);
            Assert.That(UiLabSeries.TryCreate(21, true, Environment(), out _), Is.False);
            Assert.That(UiLabSeries.TryCreate(4, true, Environment(), out UiLabSeries series), Is.True);
            Assert.That(series.TrySummarize(out _), Is.False);
            Assert.That(series.TryAdd(null), Is.False);
            Assert.That(UiLabSeries.TryCreate(4, true, new UiLabEnvironment(1280, 720, 0, null, 0, -1), out _), Is.False);
            Assert.That(series.TryAdd(new UiLabComparison(Report("Sweep.Baseline", 42, double.NaN), Report("Sweep.Virtualized", 42, 1.0d), 1.0d, 1.0d, true, 710.0f, 480.0f)), Is.False);
            UiLabComparison first = Pair(true);
            Assert.That(series.TryAdd(first), Is.True);
            Assert.That(series.TryAdd(first), Is.False);
            Assert.That(series.TryAdd(Pair(true)), Is.False);
            Assert.That(series.TryAdd(Pair(false, 43)), Is.False);
            Assert.That(series.TryAdd(Pair(false, 42, 800.0f)), Is.False);
            Assert.That(series.ComparisonCnt, Is.EqualTo(1));
            Assert.That(series.TryAdd(Pair(false)), Is.True);
            Assert.That(series.TrySummarize(out UiLabSeriesSummary summary), Is.True);
            Assert.That(summary.FrameP95.ImprovementMedian, Is.EqualTo(50.0d));
            Assert.That(series.TryGetComparison(-1, out _), Is.False);
            Assert.That(series.TryGetComparison(2, out _), Is.False);
        }

        [Test]
        public void CsvLinksRunsAndSummaryWithoutChangingLegacy()
        {
            string path = Path.Combine(Path.GetTempPath(), "series-" + Guid.NewGuid().ToString("N") + ".csv");
            string legacyPath = path + ".legacy";
            CultureInfo original = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
                Assert.That(UiLabSeries.TryCreate(4, true, Environment(), out UiLabSeries series), Is.True);
                UiLabComparison first = Pair(true);
                Assert.That(series.TryAdd(first), Is.True);
                Assert.That(series.TryAdd(Pair(false)), Is.True);
                Assert.That(series.TryExport(path), Is.True);
                string[] lines = File.ReadAllLines(path);
                Assert.That(lines.Length, Is.EqualTo(10));
                Assert.That(lines[0], Is.EqualTo(UiLabSeries.CsvHeader));
                for (int idx = 1; idx < lines.Length; ++idx)
                {
                    Assert.That(lines[idx].Split(',').Length, Is.EqualTo(52));
                    Assert.That(lines[idx], Does.StartWith(series.SeriesId + ","));
                }

                string[] gcSummary = lines[8].Split(',');
                Assert.That(gcSummary[38], Is.EqualTo("gc_mean_bytes"));
                Assert.That(gcSummary[43], Is.Empty);
                Assert.That(gcSummary[44], Is.Empty);
                Assert.That(gcSummary[45], Is.EqualTo("0"));
                Assert.That(series.TryExport(path), Is.True);
                Assert.That(File.ReadAllLines(path).Length, Is.EqualTo(19));
                Assert.That(first.TryExport(legacyPath), Is.True);
                Assert.That(File.ReadAllLines(legacyPath)[0].Split(',').Length, Is.EqualTo(33));
                File.WriteAllText(path, "incompatible header\n");
                Assert.That(series.TryExport(path), Is.False);
                Assert.That(File.ReadAllText(path), Is.EqualTo("incompatible header\n"));
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
                File.Delete(path);
                File.Delete(legacyPath);
            }
        }

        //============================================================
        // Utilities
        //============================================================
        private static UiLabEnvironment Environment()
        {
            return new UiLabEnvironment(1280, 720, 0, "High", 0, -1);
        }

        private static UiLabComparison Pair(bool isBaselineFirst, int seed = 42, float width = 710.0f)
        {
            return new UiLabComparison(Report("Sweep.Baseline", seed, 10.0d), Report("Sweep.Virtualized", seed, 5.0d), 2.0d, 1.0d, isBaselineFirst, width, 480.0f);
        }

        private static UiLabReport Report(string scenarioId, int seed, double frameMs)
        {
            BenchmarkResult result = new BenchmarkResult(scenarioId, 100, seed, 1, 3, "UiLab.Tick", "Unity", "Device", "Windows", DateTime.UtcNow, frameMs, frameMs, frameMs, frameMs, frameMs, 0.0d, 0, 100, 0.0d, 0.0d);
            return new UiLabReport(result, 100, 100, 10, 3, 10, 30);
        }
    }
}
