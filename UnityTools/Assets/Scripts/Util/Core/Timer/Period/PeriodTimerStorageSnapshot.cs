using System;
using UnityTools.Util.Constants;
using UnityTools.Util.Core.Collections;
using UnityTools.Util.Core.Events;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.Core.Persistence;
using UnityTools.Util.Core.Pooling;
using UnityTools.Util.Core.Singleton;
using UnityTools.Util.Core.State;
using UnityTools.Util.Core.Timer.Period;
using UnityTools.Util.Core.Timer.Shared;
using UnityTools.Util.Core.Timer.Task;
using UnityTools.Util.Coroutines;
using UnityTools.Util.Extensions;
using UnityTools.Util.UIFramework;
using UnityTools.Util.Utilities;

namespace UnityTools.Util.Core.Timer.Period
{
    public class PeriodTimerStorageSnapshot
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly DateTime _openEndTime;
        private readonly DateTime _closedEndTime;
        private readonly DateTime _openUpdatedTime;
        private readonly bool _isTamperedFlag;

        //============================================================
        //Properties
        //============================================================
        public DateTime OpenEndTime => _openEndTime;
        public DateTime ClosedEndTime => _closedEndTime;
        public DateTime OpenUpdatedTime => _openUpdatedTime;
        public bool IsTamperedFlag => _isTamperedFlag;

        //============================================================
        //Constructors
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
