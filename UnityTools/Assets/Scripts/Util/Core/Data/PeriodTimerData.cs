namespace UnityTools.Util
{
    public class PeriodTimerData
    {
        //============================================================
        // Properties
        //============================================================
        public string Id { get; }
        public EPeriodTimerType CurType { get; }

        //============================================================
        // Constructors
        //============================================================
        public PeriodTimerData(string id, EPeriodTimerType curType)
        {
            Id = id;
            CurType = curType;
        }
    }
}
