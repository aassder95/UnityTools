using UnityEngine.Events;
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
    // Exception: interface-centric file uses Types section.
    //============================================================
    //Types
    //============================================================
    public interface ITaskTimer : ITimerLifecycle
    {
        //============================================================
        //Events
        //============================================================
        event UnityAction OnProgressStarted;
        event UnityAction<int> OnRemainSecUpdated;
        event UnityAction OnCompleted;
        event UnityAction OnClaimed;
        event UnityAction<ETaskTimerType, ETaskTimerType> OnStateTransition;

        //============================================================
        //Properties
        //============================================================
        ETaskTimerType CurType { get; }
        bool IsClaimed { get; }
        int RemainingSec { get; }
        int DurationSec { get; }

        //============================================================
        //Logic
        //============================================================
        void Init();
        bool Start(double durationSec);
        bool Reduce(double reduceSec);
        bool CompleteImmediately();
        bool Claim();
        void NotifyCurType();
        float GetProgress();
    }
}
