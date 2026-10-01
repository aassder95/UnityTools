using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace UnityTools.Benchmark.Samples
{
    public class UiLabSeries
    {
        //============================================================
        // Constants
        //============================================================
        private const int MAX_PAIR_CNT = 20;

        //============================================================
        // Readonly
        //============================================================
        private static readonly string _csvHeader = "series_id,record_type,pair_idx,requested_pairs,completed_pairs," + UiLabComparison.CsvHeader + ",metric,baseline_median,virtualized_median,baseline_stddev,virtualized_stddev,improvement_median_pct,improvement_stddev_pct,improvement_pairs,screen_width,screen_height,quality_idx,quality_name,vsync_cnt,target_frame_rate";
        private readonly List<UiLabComparison> _comparisons;
        private readonly bool _isBaselineFirst;

        //============================================================
        // Properties
        //============================================================
        public static int MaxPairCnt => MAX_PAIR_CNT;
        public static string CsvHeader => _csvHeader;
        public string SeriesId { get; }
        public int RequestedPairCnt { get; }
        public int ComparisonCnt => _comparisons.Count;
        public bool IsComplete => ComparisonCnt == RequestedPairCnt;
        public bool IsNextBaselineFirst => ComparisonCnt % 2 == 0 ? _isBaselineFirst : !_isBaselineFirst;
        public UiLabEnvironment Environment { get; }

        //============================================================
        // Constructors
        //============================================================
        private UiLabSeries(int requestedPairCnt, bool isBaselineFirst, UiLabEnvironment environment)
        {
            RequestedPairCnt = requestedPairCnt;
            _isBaselineFirst = isBaselineFirst;
            Environment = environment;
            SeriesId = Guid.NewGuid().ToString("N");
            _comparisons = new List<UiLabComparison>(requestedPairCnt);
        }

        //============================================================
        // Logic
        //============================================================
        public static bool TryCreate(int requestedPairCnt, bool isBaselineFirst, UiLabEnvironment environment, out UiLabSeries series)
        {
            series = null;
            if (requestedPairCnt < 1 || requestedPairCnt > MAX_PAIR_CNT || environment == null || environment.ScreenWidth < 1 || environment.ScreenHeight < 1 || environment.QualityIdx < 0 || environment.QualityName == null)
                return false;

            series = new UiLabSeries(requestedPairCnt, isBaselineFirst, environment);
            return true;
        }

        public bool TryAdd(UiLabComparison comparison)
        {
            if (IsComplete || comparison == null || comparison.Baseline == null || comparison.Virtualized == null || comparison.IsBaselineFirst != IsNextBaselineFirst || !MatchesWorkload(comparison.Baseline, comparison.Virtualized))
                return false;

            if (double.IsNaN(comparison.BaselineInitMs) || double.IsInfinity(comparison.BaselineInitMs) || comparison.BaselineInitMs < 0.0d || double.IsNaN(comparison.VirtualizedInitMs) || double.IsInfinity(comparison.VirtualizedInitMs) || comparison.VirtualizedInitMs < 0.0d || comparison.ViewportWidth <= 0.0f || comparison.ViewportHeight <= 0.0f || float.IsNaN(comparison.ViewportWidth) || float.IsNaN(comparison.ViewportHeight) || float.IsInfinity(comparison.ViewportWidth) || float.IsInfinity(comparison.ViewportHeight))
                return false;

            if (ComparisonCnt > 0)
            {
                UiLabComparison first = _comparisons[0];
                if (!MatchesWorkload(first.Baseline, comparison.Baseline) || first.Baseline.Measurement.ScenarioId != comparison.Baseline.Measurement.ScenarioId || first.Virtualized.Measurement.ScenarioId != comparison.Virtualized.Measurement.ScenarioId || first.ViewportWidth != comparison.ViewportWidth || first.ViewportHeight != comparison.ViewportHeight)
                    return false;
            }

            for (int idx = 0; idx < ComparisonCnt; ++idx)
            {
                if (_comparisons[idx].PairId == comparison.PairId)
                    return false;
            }

            _comparisons.Add(comparison);
            return true;
        }

        public bool TryGetComparison(int idx, out UiLabComparison comparison)
        {
            comparison = null;
            if (idx < 0 || idx >= ComparisonCnt)
                return false;

            comparison = _comparisons[idx];
            return true;
        }

        public bool TrySummarize(out UiLabSeriesSummary summary)
        {
            summary = null;
            if (ComparisonCnt == 0 || !TrySummarizeMetric(pair => pair.BaselineInitMs, pair => pair.VirtualizedInitMs, out UiLabMetricSummary initialization) || !TrySummarizeMetric(pair => pair.Baseline.Measurement.P95FrameMs, pair => pair.Virtualized.Measurement.P95FrameMs, out UiLabMetricSummary frameP95) || !TrySummarizeMetric(pair => pair.Baseline.Measurement.P95MainThreadMs, pair => pair.Virtualized.Measurement.P95MainThreadMs, out UiLabMetricSummary mainP95) || !TrySummarizeMetric(pair => pair.Baseline.Measurement.MeanGcBytes, pair => pair.Virtualized.Measurement.MeanGcBytes, out UiLabMetricSummary gcMean) || !TrySummarizeMetric(pair => pair.Baseline.Measurement.PeakMemoryBytes, pair => pair.Virtualized.Measurement.PeakMemoryBytes, out UiLabMetricSummary peakMemory))
                return false;

            summary = new UiLabSeriesSummary(initialization, frameP95, mainP95, gcMean, peakMemory);
            return true;
        }

        //============================================================
        // Persistence
        //============================================================
        public bool TryExport(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !TrySummarize(out UiLabSeriesSummary summary))
                return false;

            try
            {
                bool needsHeader = !File.Exists(path) || new FileInfo(path).Length == 0;
                if (!needsHeader)
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        if (reader.ReadLine() != _csvHeader)
                            return false;
                    }
                }

                StringBuilder content = new StringBuilder();
                if (needsHeader)
                    content.AppendLine(_csvHeader);

                for (int idx = 0; idx < ComparisonCnt; ++idx)
                {
                    UiLabComparison pair = _comparisons[idx];
                    string prefix = Prefix("run", idx + 1);
                    content.AppendLine(prefix + pair.ToCsvRow(true) + ",,,,,,,,," + EnvironmentRow());
                    content.AppendLine(prefix + pair.ToCsvRow(false) + ",,,,,,,,," + EnvironmentRow());
                }

                AppendSummary(content, "init_ms", summary.Initialization);
                AppendSummary(content, "frame_p95_ms", summary.FrameP95);
                AppendSummary(content, "main_thread_p95_ms", summary.MainThreadP95);
                AppendSummary(content, "gc_mean_bytes", summary.GcMean);
                AppendSummary(content, "peak_memory_bytes", summary.PeakMemory);
                File.AppendAllText(path, content.ToString(), new UTF8Encoding(false));
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }

        //============================================================
        // Utilities
        //============================================================
        private static bool MatchesWorkload(UiLabReport baseline, UiLabReport virtualized)
        {
            BenchmarkResult first = baseline.Measurement;
            BenchmarkResult second = virtualized.Measurement;
            return first != null && second != null && HasValidMetrics(first) && HasValidMetrics(second) && first.AgentCnt == second.AgentCnt && first.Seed == second.Seed && first.WarmupFrames == second.WarmupFrames && first.SampleFrames == second.SampleFrames && first.MarkerName == second.MarkerName && first.UnityVersion == second.UnityVersion && first.DeviceModel == second.DeviceModel && first.Platform == second.Platform && baseline.ItemStep == virtualized.ItemStep && baseline.MutationCnt == virtualized.MutationCnt && baseline.MutationIntervalFrames == virtualized.MutationIntervalFrames;
        }

        private static bool HasValidMetrics(BenchmarkResult measurement)
        {
            return IsValidMetric(measurement.P95FrameMs) && IsValidMetric(measurement.P95MainThreadMs) && IsValidMetric(measurement.MeanGcBytes) && measurement.PeakMemoryBytes >= 0;
        }

        private static bool IsValidMetric(double value)
        {
            return value >= 0.0d && !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private bool TrySummarizeMetric(Func<UiLabComparison, double> baselineValue, Func<UiLabComparison, double> virtualizedValue, out UiLabMetricSummary summary)
        {
            double[] baselineValues = new double[ComparisonCnt];
            double[] virtualizedValues = new double[ComparisonCnt];
            for (int idx = 0; idx < ComparisonCnt; ++idx)
            {
                baselineValues[idx] = baselineValue(_comparisons[idx]);
                virtualizedValues[idx] = virtualizedValue(_comparisons[idx]);
            }

            return UiLabMetricSummary.TryCreate(baselineValues, virtualizedValues, out summary);
        }

        private string Prefix(string recordType, int pairIdx)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},", SeriesId, recordType, pairIdx > 0 ? pairIdx.ToString(CultureInfo.InvariantCulture) : string.Empty, RequestedPairCnt, ComparisonCnt);
        }

        private string EnvironmentRow()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},\"{3}\",{4},{5}", Environment.ScreenWidth, Environment.ScreenHeight, Environment.QualityIdx, Environment.QualityName.Replace("\"", "\"\""), Environment.VSyncCnt, Environment.TargetFrameRate);
        }

        private void AppendSummary(StringBuilder content, string metric, UiLabMetricSummary summary)
        {
            string improvementMedian = summary.HasImprovement ? summary.ImprovementMedian.ToString("R", CultureInfo.InvariantCulture) : string.Empty;
            string improvementDeviation = summary.HasImprovement ? summary.ImprovementDeviation.ToString("R", CultureInfo.InvariantCulture) : string.Empty;
            content.AppendLine(Prefix("summary", 0) + new string(',', 32) + string.Format(CultureInfo.InvariantCulture, ",{0},{1:R},{2:R},{3:R},{4:R},{5},{6},{7},", metric, summary.BaselineMedian, summary.VirtualizedMedian, summary.BaselineDeviation, summary.VirtualizedDeviation, improvementMedian, improvementDeviation, summary.ImprovementCnt) + EnvironmentRow());
        }
    }
}
