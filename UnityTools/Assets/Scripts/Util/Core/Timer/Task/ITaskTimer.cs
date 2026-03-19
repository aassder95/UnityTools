using UnityEngine.Events;

namespace UnityTools.Util
{
    public interface ITaskTimer : ITimerLifecycle
    {
        event UnityAction OnProgressStarted;
        event UnityAction<int> OnRemainSecUpdated;
        event UnityAction OnCompleted;
        event UnityAction OnClaimed;
        event UnityAction<ETaskTimerType, ETaskTimerType> OnStateTransition;

        ETaskTimerType CurType { get; }
        bool IsClaimed { get; }
        int RemainingSec { get; }
        int DurationSec { get; }

        void Init();
        bool Start(double durationSec);
        bool Reduce(double reduceSec);
        bool CompleteImmediately();
        bool Claim();
        void NotifyCurType();
        float GetProgress();
    }
}
