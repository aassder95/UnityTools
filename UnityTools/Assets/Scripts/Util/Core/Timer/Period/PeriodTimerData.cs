namespace UnityTools.Util.Core.Timer.Period
{
    public class PeriodTimerData
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly string _id;
        private readonly EPeriodTimerType _curType;

        //============================================================
        //Properties
        //============================================================
        public string Id => _id;
        public EPeriodTimerType CurType => _curType;

        //============================================================
        //Constructors
        //============================================================
        public PeriodTimerData(string id, EPeriodTimerType curType)
        {
            _id = id;
            _curType = curType;
        }
    }
}
