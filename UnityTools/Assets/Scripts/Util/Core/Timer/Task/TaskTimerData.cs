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
    public class TaskTimerData
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly string _id;
        private readonly ETaskTimerType _curType;
        private readonly int _remainingSec;
        private readonly int _durationSec;
        private readonly float _progress;

        //============================================================
        //Properties
        //============================================================
        public string Id => _id;
        public ETaskTimerType CurType => _curType;
        public int RemainingSec => _remainingSec;
        public int DurationSec => _durationSec;
        public float Progress => _progress;

        //============================================================
        //Constructors
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
