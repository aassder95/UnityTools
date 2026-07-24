namespace UnityTools.Timer.Task
{
    public class TaskTimerData
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly string _id;
        private readonly ETaskTimerType _curType;
        private readonly int _remainingSec;
        private readonly int _durationSec;
        private readonly float _progress;

        //============================================================
        // Properties
        //============================================================
        public string Id => _id;
        public ETaskTimerType CurType => _curType;
        public int RemainingSec => _remainingSec;
        public int DurationSec => _durationSec;
        public float Progress => _progress;

        //============================================================
        // Constructors
        //============================================================
        public TaskTimerData(string id, ETaskTimerType curType, int remainingSec, int durationSec, float progress)
        {
            _id = id;
            _curType = curType;
            _remainingSec = remainingSec;
            _durationSec = durationSec;
            _progress = progress;
        }
    }
}
