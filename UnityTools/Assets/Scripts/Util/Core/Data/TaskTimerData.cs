namespace UnityTools.Util
{
    public class TaskTimerData
    {
        //============================================================
        // Properties
        //============================================================
        public string Id { get; }
        public int RemainingSec { get; }
        public int DurationSec { get; }
        public float Progress { get; }

        //============================================================
        // Constructors
        //============================================================
        public TaskTimerData(string id, int remainingSec, int durationSec, float progress)
        {
            Id = id;
            RemainingSec = remainingSec;
            DurationSec = durationSec;
            Progress = progress;
        }
    }
}
