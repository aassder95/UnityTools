namespace UnityTools.Util
{
    public class TaskTimerData
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly string _id;
        private readonly int _remainingSec;
        private readonly int _durationSec;
        private readonly float _progress;

        //============================================================
        //Properties
        //============================================================
        public string Id => _id;
        public int RemainingSec => _remainingSec;
        public int DurationSec => _durationSec;
        public float Progress => _progress;

        //============================================================
        //Constructors
        //============================================================
        public TaskTimerData(string id, int remainingSec, int durationSec, float progress)
        {
            _id = id;
            _remainingSec = remainingSec;
            _durationSec = durationSec;
            _progress = progress;
        }
    }
}
