namespace UnityTools.Persistence.Samples
{
    public class SaveLabReport
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly bool _isPassed;
        private readonly string _before;
        private readonly string _after;
        private readonly string _result;

        //============================================================
        // Properties
        //============================================================
        public bool IsPassed => _isPassed;
        public string Before => _before;
        public string After => _after;
        public string Result => _result;

        //============================================================
        // Constructors
        //============================================================
        public SaveLabReport(bool isPassed, string before, string after, string result)
        {
            _isPassed = isPassed;
            _before = before;
            _after = after;
            _result = result;
        }
    }
}
