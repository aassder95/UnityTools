using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace UnityTools.Benchmark
{
    public static class BenchmarkCsv
    {
        //============================================================
        // Constants
        //============================================================
        public const string HEADER = "scenario_id,agent_cnt,seed,warmup_frames,sample_frames,marker_name,unity_version,device_model,platform,recorded_utc,mean_frame_ms,p95_frame_ms,max_frame_ms,fps_from_mean_frame,mean_main_thread_ms,p95_main_thread_ms,mean_gc_bytes,total_gc_bytes,peak_memory_bytes,mean_marker_ms,p95_marker_ms";

        //============================================================
        // Logic
        //============================================================
        public static bool TryAppend(string path, BenchmarkResult result)
        {
            if (string.IsNullOrWhiteSpace(path) || result == null)
                return false;

            try
            {
                bool needsHeader = !File.Exists(path) || new FileInfo(path).Length == 0;
                StringBuilder csv = new StringBuilder();
                if (needsHeader)
                    csv.AppendLine(HEADER);

                csv.AppendLine(ToRow(result));
                File.AppendAllText(path, csv.ToString(), new UTF8Encoding(false));
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

        public static string ToRow(BenchmarkResult result)
        {
            if (result == null)
                return string.Empty;

            return string.Join(",", Escape(result.ScenarioId), result.AgentCnt.ToString(CultureInfo.InvariantCulture),
                result.Seed.ToString(CultureInfo.InvariantCulture), result.WarmupFrames.ToString(CultureInfo.InvariantCulture),
                result.SampleFrames.ToString(CultureInfo.InvariantCulture), Escape(result.MarkerName),
                Escape(result.UnityVersion), Escape(result.DeviceModel), Escape(result.Platform),
                result.RecordedUtc.ToString("O", CultureInfo.InvariantCulture),
                result.MeanFrameMs.ToString("R", CultureInfo.InvariantCulture),
                result.P95FrameMs.ToString("R", CultureInfo.InvariantCulture),
                result.MaxFrameMs.ToString("R", CultureInfo.InvariantCulture),
                result.FpsFromMeanFrame.ToString("R", CultureInfo.InvariantCulture),
                result.MeanMainThreadMs.ToString("R", CultureInfo.InvariantCulture),
                result.P95MainThreadMs.ToString("R", CultureInfo.InvariantCulture),
                result.MeanGcBytes.ToString("R", CultureInfo.InvariantCulture),
                result.TotalGcBytes.ToString(CultureInfo.InvariantCulture),
                result.PeakMemoryBytes.ToString(CultureInfo.InvariantCulture),
                result.MeanMarkerMs.ToString("R", CultureInfo.InvariantCulture),
                result.P95MarkerMs.ToString("R", CultureInfo.InvariantCulture));
        }

        //============================================================
        // Utilities
        //============================================================
        private static string Escape(string value)
        {
            if (value == null)
                return string.Empty;

            if (!value.Contains(",") && !value.Contains("\"") && !value.Contains("\n") && !value.Contains("\r"))
                return value;

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
