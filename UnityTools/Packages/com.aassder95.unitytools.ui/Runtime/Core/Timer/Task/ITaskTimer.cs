using UnityEngine.Events;

namespace UnityTools.Util.Core.Timer.Task
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
        bool IsClaimed { get; }
        int RemainingSec { get; }
        int DurationSec { get; }
        float Progress { get; }

        //============================================================
        // Logic
        //============================================================
        bool Init();
        bool TryStart(double durationSec);
        bool TryReduce(double reduceSec);
        bool TryComplete();
        bool TryClaim();
        void NotifyCurType();
    }
}
