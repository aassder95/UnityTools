namespace UnityTools.Timer.Samples
{
    public class TimerLabReport
    {
        //============================================================
        // Properties
        //============================================================
        public bool IsPassed { get; }
        public string Before { get; }
        public string After { get; }
        public string Result { get; }

        //============================================================
        // Constructors
        //============================================================
        public TimerLabReport(bool isPassed, string before, string after, string result)
        {
            IsPassed = isPassed;
            Before = before;
            After = after;
            Result = result;
        }
    }
}
