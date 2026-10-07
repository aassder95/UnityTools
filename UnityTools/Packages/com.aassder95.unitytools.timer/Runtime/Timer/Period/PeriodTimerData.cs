namespace UnityTools.Timer.Period
{
    public class PeriodTimerData
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly string _id;
        private readonly EPeriodTimerType _curType;
        private readonly int _remainingSec;
        private readonly int _remainingMin;
        private readonly bool _isReady;

        //============================================================
        // Properties
        //============================================================
        public string Id => _id;
        public EPeriodTimerType CurType => _curType;
        public int RemainingSec => _remainingSec;
        public int RemainingMin => _remainingMin;
        public bool IsReady => _isReady;

        //============================================================
        // Constructors
        //============================================================
        public PeriodTimerData(string id, EPeriodTimerType curType) : this(id, curType, 0, 0, false)
        {
        }

        public PeriodTimerData(string id, EPeriodTimerType curType, int remainingSec, int remainingMin, bool isReady)
        {
            _id = id;
            _curType = curType;
            _remainingSec = remainingSec;
            _remainingMin = remainingMin;
            _isReady = isReady;
        }
    }
}
