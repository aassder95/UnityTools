using System;

namespace UnityTools.Util.Core.Timer.Task
{
    public class TaskTimerStorageSnapshot
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly DateTime _startTime;
        private readonly double _durationSec;
        private readonly DateTime _updatedTime;
        private readonly int _savedStateType;

        //============================================================
        //Properties
        //============================================================
        public DateTime StartTime => _startTime;
        public double DurationSec => _durationSec;
        public DateTime UpdatedTime => _updatedTime;
        public int SavedStateType => _savedStateType;

        //============================================================
        //Constructors
        //============================================================
        public TaskTimerStorageSnapshot(DateTime startTime, double durationSec, DateTime updatedTime, int savedStateType)
        {
            _startTime = startTime;
            _durationSec = durationSec;
            _updatedTime = updatedTime;
            _savedStateType = savedStateType;
        }
    }
}
