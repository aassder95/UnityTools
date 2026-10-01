using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace UnityTools.Benchmark.Samples
{
    public class UiLabComparison
    {
        //============================================================
        // Properties
        //============================================================
        public UiLabReport Baseline { get; }
        public UiLabReport Virtualized { get; }
        public double BaselineInitMs { get; }
        public double VirtualizedInitMs { get; }
        public bool IsBaselineFirst { get; }
        public float ViewportWidth { get; }
        public float ViewportHeight { get; }
        public string PairId { get; }

        //============================================================
        // Constructors
        //============================================================
        public UiLabComparison(UiLabReport baseline, UiLabReport virtualized, double baselineInitMs, double virtualizedInitMs, bool isBaselineFirst, float viewportWidth, float viewportHeight)
        {
            Baseline = baseline;
            Virtualized = virtualized;
            BaselineInitMs = baselineInitMs;
            VirtualizedInitMs = virtualizedInitMs;
            IsBaselineFirst = isBaselineFirst;
            ViewportWidth = viewportWidth;
            ViewportHeight = viewportHeight;
            PairId = Guid.NewGuid().ToString("N");
        }

        //============================================================
        // Persistence
        //============================================================
        public bool TryExport(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                string header = BenchmarkCsv.HEADER + ",pair_id,mode,run_order,init_ms,created_item_cnt,peak_live_item_cnt,bind_cnt,item_step,mutation_cnt,mutation_interval_frames,viewport_width,viewport_height";
                string content = Row(Baseline, "Baseline", IsBaselineFirst ? 1 : 2, BaselineInitMs) + Environment.NewLine + Row(Virtualized, "Virtualized", IsBaselineFirst ? 2 : 1, VirtualizedInitMs) + Environment.NewLine;
                bool needsHeader = !File.Exists(path) || new FileInfo(path).Length == 0;
                File.AppendAllText(path, (needsHeader ? header + Environment.NewLine : string.Empty) + content, new UTF8Encoding(false));
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
        private string Row(UiLabReport report, string mode, int order, double initMs)
        {
            return BenchmarkCsv.ToRow(report.Measurement) + string.Format(CultureInfo.InvariantCulture, ",{0},{1},{2},{3:R},{4},{5},{6},{7},{8},{9},{10:R},{11:R}", PairId, mode, order, initMs, report.CreatedItemCnt, report.PeakLiveItemCnt, report.BindCnt, report.ItemStep, report.MutationCnt, report.MutationIntervalFrames, ViewportWidth, ViewportHeight);
        }
    }
}
