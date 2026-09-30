using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace UnityTools.Benchmark.Samples
{
    public class UiLabReport
    {
        //============================================================
        // Properties
        //============================================================
        public BenchmarkResult Measurement { get; }
        public int CreatedItemCnt { get; }
        public int PeakLiveItemCnt { get; }
        public long BindCnt { get; }
        public int ItemStep { get; }
        public int MutationCnt { get; }
        public int MutationIntervalFrames { get; }

        //============================================================
        // Constructors
        //============================================================
        public UiLabReport(BenchmarkResult measurement, int createdItemCnt, int peakLiveItemCnt, long bindCnt, int itemStep, int mutationCnt, int mutationIntervalFrames)
        {
            Measurement = measurement;
            CreatedItemCnt = createdItemCnt;
            PeakLiveItemCnt = peakLiveItemCnt;
            BindCnt = bindCnt;
            ItemStep = itemStep;
            MutationCnt = mutationCnt;
            MutationIntervalFrames = mutationIntervalFrames;
        }

        //============================================================
        // Persistence
        //============================================================
        public bool TryExport(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || Measurement == null)
                return false;

            try
            {
                string header = BenchmarkCsv.HEADER + ",created_item_cnt,peak_live_item_cnt,bind_cnt,item_step,mutation_cnt,mutation_interval_frames";
                string row = BenchmarkCsv.ToRow(Measurement) + "," + CreatedItemCnt.ToString(CultureInfo.InvariantCulture) + "," + PeakLiveItemCnt.ToString(CultureInfo.InvariantCulture) + "," + BindCnt.ToString(CultureInfo.InvariantCulture) + "," + ItemStep.ToString(CultureInfo.InvariantCulture) + "," + MutationCnt.ToString(CultureInfo.InvariantCulture) + "," + MutationIntervalFrames.ToString(CultureInfo.InvariantCulture);
                bool needsHeader = !File.Exists(path) || new FileInfo(path).Length == 0;
                File.AppendAllText(path, (needsHeader ? header + Environment.NewLine : string.Empty) + row + Environment.NewLine, new UTF8Encoding(false));
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
    }
}
