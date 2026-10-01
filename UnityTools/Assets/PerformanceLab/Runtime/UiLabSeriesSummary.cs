namespace UnityTools.Benchmark.Samples
{
    public class UiLabSeriesSummary
    {
        //============================================================
        // Properties
        //============================================================
        public UiLabMetricSummary Initialization { get; }
        public UiLabMetricSummary FrameP95 { get; }
        public UiLabMetricSummary MainThreadP95 { get; }
        public UiLabMetricSummary GcMean { get; }
        public UiLabMetricSummary PeakMemory { get; }

        //============================================================
        // Constructors
        //============================================================
        public UiLabSeriesSummary(UiLabMetricSummary initialization, UiLabMetricSummary frameP95, UiLabMetricSummary mainThreadP95, UiLabMetricSummary gcMean, UiLabMetricSummary peakMemory)
        {
            Initialization = initialization;
            FrameP95 = frameP95;
            MainThreadP95 = mainThreadP95;
            GcMean = gcMean;
            PeakMemory = peakMemory;
        }
    }
}
