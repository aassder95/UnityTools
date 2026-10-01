using System;

namespace UnityTools.Benchmark.Samples
{
    public class UiLabMetricSummary
    {
        //============================================================
        // Readonly
        //============================================================
        private static readonly Comparison<double> _valueOrder = (left, right) => left.CompareTo(right);

        //============================================================
        // Properties
        //============================================================
        public double BaselineMedian { get; }
        public double VirtualizedMedian { get; }
        public double BaselineDeviation { get; }
        public double VirtualizedDeviation { get; }
        public double ImprovementMedian { get; }
        public double ImprovementDeviation { get; }
        public int ImprovementCnt { get; }
        public bool HasImprovement => ImprovementCnt > 0;

        //============================================================
        // Constructors
        //============================================================
        private UiLabMetricSummary(double baselineMedian, double virtualizedMedian, double baselineDeviation, double virtualizedDeviation, double improvementMedian, double improvementDeviation, int improvementCnt)
        {
            BaselineMedian = baselineMedian;
            VirtualizedMedian = virtualizedMedian;
            BaselineDeviation = baselineDeviation;
            VirtualizedDeviation = virtualizedDeviation;
            ImprovementMedian = improvementMedian;
            ImprovementDeviation = improvementDeviation;
            ImprovementCnt = improvementCnt;
        }

        //============================================================
        // Logic
        //============================================================
        public static bool TryCreate(double[] baselineValues, double[] virtualizedValues, out UiLabMetricSummary summary)
        {
            summary = null;
            if (baselineValues == null || virtualizedValues == null || baselineValues.Length == 0 || baselineValues.Length != virtualizedValues.Length)
                return false;

            double[] improvements = new double[baselineValues.Length];
            int improvementCnt = 0;
            for (int idx = 0; idx < baselineValues.Length; ++idx)
            {
                double baseline = baselineValues[idx];
                double virtualized = virtualizedValues[idx];
                if (baseline < 0.0d || virtualized < 0.0d || double.IsNaN(baseline) || double.IsNaN(virtualized) || double.IsInfinity(baseline) || double.IsInfinity(virtualized))
                    return false;

                if (baseline > 0.0d)
                {
                    double improvement = (1.0d - virtualized / baseline) * 100.0d;
                    if (double.IsInfinity(improvement))
                        return false;

                    improvements[improvementCnt++] = improvement;
                }
            }

            double[] baselineSorted = (double[])baselineValues.Clone();
            double[] virtualizedSorted = (double[])virtualizedValues.Clone();
            Array.Sort(baselineSorted, _valueOrder);
            Array.Sort(virtualizedSorted, _valueOrder);
            Array.Resize(ref improvements, improvementCnt);
            Array.Sort(improvements, _valueOrder);
            summary = new UiLabMetricSummary(Median(baselineSorted, baselineSorted.Length), Median(virtualizedSorted, virtualizedSorted.Length), Deviation(baselineValues, baselineValues.Length), Deviation(virtualizedValues, virtualizedValues.Length), improvementCnt > 0 ? Median(improvements, improvementCnt) : 0.0d, improvementCnt > 0 ? Deviation(improvements, improvementCnt) : 0.0d, improvementCnt);
            return true;
        }

        //============================================================
        // Utilities
        //============================================================
        private static double Median(double[] sortedValues, int cnt)
        {
            int middleIdx = cnt / 2;
            return cnt % 2 == 0 ? sortedValues[middleIdx - 1] * 0.5d + sortedValues[middleIdx] * 0.5d : sortedValues[middleIdx];
        }

        private static double Deviation(double[] values, int cnt)
        {
            double mean = 0.0d;
            double squares = 0.0d;
            for (int idx = 0; idx < cnt; ++idx)
            {
                double delta = values[idx] - mean;
                mean += delta / (idx + 1);
                squares += delta * (values[idx] - mean);
            }

            return Math.Sqrt(Math.Max(0.0d, squares / cnt));
        }
    }
}
