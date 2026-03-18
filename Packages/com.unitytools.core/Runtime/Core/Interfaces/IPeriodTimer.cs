using System;
using System.Collections;
using UnityEngine.Events;

namespace UnityTools.Util
{
    public interface IPeriodTimer : ITimerLifecycle
    {
        event UnityAction OnOpenPeriodPreparing;
        event UnityAction OnOpenPeriodStarted;
        event UnityAction<int> OnRemainMinUpdated;
        event UnityAction OnClosedPeriodStarted;
        event UnityAction<EPeriodTimerType, EPeriodTimerType> OnPeriodStateTransition;

        EPeriodTimerType CurType { get; }
        bool IsReady { get; }
        bool IsOpenPeriod { get; }
        bool IsClosedPeriod { get; }

        void Init(double openMin, double closedMin, Func<IEnumerator> initWaitFunc = null);
        void ForceOpen();
        void ForceClosed();
        void SetPeriods(double openMin, double closedMin);
        int GetRemainingMin();
        int GetRemainingSec();
    }
}
