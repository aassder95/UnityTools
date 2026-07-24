using UnityEngine.Events;

namespace UnityTools.Timer.Task
{
    public interface ITaskTimer : ITimerLifecycle
    {
        //============================================================
        // Events
        //============================================================
        event UnityAction OnProgressStarted;
        event UnityAction<int> OnRemainSecUpdated;
        event UnityAction OnCompleted;
        event UnityAction OnClaimed;
        event UnityAction<ETaskTimerType, ETaskTimerType> OnStateTransition;

        //============================================================
        // Properties
        //============================================================
        ETaskTimerType CurType { get; }
        int RemainingSec { get; }
        int DurationSec { get; }
        float Progress { get; }

        //============================================================
        // Logic
        //============================================================
        bool TryInit();
        bool TryStart(double durationSec);
        bool TryReduce(double reduceSec);
        bool TryComplete();
        bool TryClaim();
        bool TryGetClaimed(out bool isClaimed);
        void NotifyCurType();
    }
}
