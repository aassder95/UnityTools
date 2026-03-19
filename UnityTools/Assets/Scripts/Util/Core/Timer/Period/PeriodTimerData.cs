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
