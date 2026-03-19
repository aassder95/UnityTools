using System;
using System.Collections;
using UnityEngine.Events;

namespace UnityTools.Util
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
