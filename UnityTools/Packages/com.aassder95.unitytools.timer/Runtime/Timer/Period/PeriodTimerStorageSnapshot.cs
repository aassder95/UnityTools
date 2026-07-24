using System;

namespace UnityTools.Timer.Period
{
    public class PeriodTimerStorageSnapshot
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly DateTime _openEndTime;
        private readonly DateTime _closedEndTime;
        private readonly DateTime _openUpdatedTime;
        private readonly bool _isTamperedFlag;

        //============================================================
        // Properties
        //============================================================
        public DateTime OpenEndTime => _openEndTime;
        public DateTime ClosedEndTime => _closedEndTime;
        public DateTime OpenUpdatedTime => _openUpdatedTime;
        public bool IsTamperedFlag => _isTamperedFlag;

        //============================================================
        // Constructors
        //============================================================
        public PeriodTimerStorageSnapshot(DateTime openEndTime, DateTime closedEndTime, DateTime openUpdatedTime, bool isTamperedFlag)
        {
            _openEndTime = openEndTime;
            _closedEndTime = closedEndTime;
            _openUpdatedTime = openUpdatedTime;
            _isTamperedFlag = isTamperedFlag;
        }
    }
}
