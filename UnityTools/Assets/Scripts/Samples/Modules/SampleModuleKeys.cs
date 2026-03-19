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

namespace UnityTools.Samples.Modules
{
    // Exception: constants-only type uses Types section.
    //============================================================
    //Types
    //============================================================
    public static class SampleModuleKeys
    {
        //============================================================
        //Constants
        //============================================================
        public const string ALL = "All";
        public const string RANK = "Rank";
        public const string RANK_OSA = "RankOSA";
        public const string TIMER = "Timer";
        public const string INVEN = "Inven";
    }
}
