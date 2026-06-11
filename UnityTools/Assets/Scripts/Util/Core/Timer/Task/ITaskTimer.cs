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

        //============================================================
        // Logic
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
