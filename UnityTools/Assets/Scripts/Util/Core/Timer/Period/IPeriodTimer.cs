using System;
using System.Collections;
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

namespace UnityTools.Util.Core.Timer.Period
{
    // Exception: Period timer domain uses minute-based period values by design.
    // Exception: interface-centric file uses Types section.
    //============================================================
    //Types
    //============================================================
    public interface IPeriodTimer : ITimerLifecycle
    {
        //============================================================
        //Events
        //============================================================
        event UnityAction OnOpenPeriodPreparing;
        event UnityAction OnOpenPeriodStarted;
        event UnityAction<int> OnRemainMinUpdated;
        event UnityAction OnClosedPeriodStarted;
        event UnityAction<EPeriodTimerType, EPeriodTimerType> OnPeriodStateTransition;

        //============================================================
        //Properties
        //============================================================
        EPeriodTimerType CurType { get; }
        bool IsReady { get; }
        bool IsOpenPeriod { get; }
        bool IsClosedPeriod { get; }

        //============================================================
        //Logic
        //============================================================
        void Init(double openMin, double closedMin, Func<IEnumerator> initWaitFunc = null);
        void ForceOpen();
        void ForceClosed();
        void SetPeriods(double openMin, double closedMin);
        int GetRemainingMin();
        int GetRemainingSec();
    }
}
