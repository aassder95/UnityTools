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
