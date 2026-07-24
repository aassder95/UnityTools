using System;
using System.Collections;
using UnityEngine.Events;

namespace UnityTools.Timer.Period
{
    public interface IPeriodTimer : ITimerLifecycle
    {
        //============================================================
        // Events
        //============================================================
        event UnityAction OnOpenPeriodPreparing;
        event UnityAction OnOpenPeriodStarted;
        event UnityAction<int> OnRemainMinUpdated;
        event UnityAction OnClosedPeriodStarted;
        event UnityAction<EPeriodTimerType, EPeriodTimerType> OnPeriodStateTransition;

        //============================================================
        // Properties
        //============================================================
        EPeriodTimerType CurType { get; }
        bool IsReady { get; }
        bool IsOpenPeriod { get; }
        bool IsClosedPeriod { get; }
        int RemainingMin { get; }
        int RemainingSec { get; }

        //============================================================
        // Logic
        //============================================================
        bool TryInit(double openMin, double closedMin, Func<IEnumerator> initWaitFunc = null);
        bool TryForceOpen();
        bool TryForceClosed();
        bool TrySetPeriods(double openMin, double closedMin);
    }
}
