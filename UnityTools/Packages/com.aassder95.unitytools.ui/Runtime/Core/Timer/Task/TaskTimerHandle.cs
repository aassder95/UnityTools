using UnityEngine;
using UnityEngine.Events;


namespace UnityTools.Util.Core.Timer.Task
{
    public class TaskTimerHandle : ITimerHandle
    {
        //============================================================
        // Readonly
        //============================================================
        protected readonly ITaskTimer _timer;

        //============================================================
        // Fields
        //============================================================
        private bool _isRegistered;

        //============================================================
        // Events
        //============================================================
        public event UnityAction OnProgressStarted { add => _onProgressStarted += value; remove => _onProgressStarted -= value; }
        public event UnityAction<int> OnRemainSecUpdated { add => _onRemainSecUpdated += value; remove => _onRemainSecUpdated -= value; }
        public event UnityAction OnCompleted { add => _onCompleted += value; remove => _onCompleted -= value; }
        public event UnityAction OnClaimed { add => _onClaimed += value; remove => _onClaimed -= value; }
        public event UnityAction<ETaskTimerType, ETaskTimerType> OnStateTransition { add => _onStateTransition += value; remove => _onStateTransition -= value; }
        private event UnityAction _onProgressStarted;
        private event UnityAction<int> _onRemainSecUpdated;
        private event UnityAction _onCompleted;
        private event UnityAction _onClaimed;
        private event UnityAction<ETaskTimerType, ETaskTimerType> _onStateTransition;

        //============================================================
        // Properties
        //============================================================
        public ETaskTimerType CurType => _timer.CurType;
        public bool IsClaimed => _timer.IsClaimed;
        public int RemainingSec => _timer.RemainingSec;
        public string Id => _timer.Id;

        //============================================================
        // Constructors
        //============================================================
        public TaskTimerHandle(ITaskTimer timer)
        {
            _timer = timer;
        }

        //============================================================
        // Init/Register
        //============================================================

        public virtual bool TryInit()
        {
            RegisterCallbacks();
            if(_timer.TryInit())
                return true;

            UnregisterCallbacks();
            return false;
        }

        public virtual bool TryRelease()
        {
            bool isSuccess = _timer.TryRelease();
            UnregisterCallbacks();
            return isSuccess;
        }

        private void RegisterCallbacks()
        {
            if(_isRegistered)
                return;

            _timer.OnProgressStarted += OnProgressStartedCallback;
            _timer.OnRemainSecUpdated += OnRemainSecUpdatedCallback;
            _timer.OnCompleted += OnCompletedCallback;
            _timer.OnClaimed += OnClaimedCallback;
            _timer.OnStateTransition += OnStateTransitionCallback;
            _isRegistered = true;
        }

        private void UnregisterCallbacks()
        {
            if(!_isRegistered)
                return;

            _timer.OnProgressStarted -= OnProgressStartedCallback;
            _timer.OnRemainSecUpdated -= OnRemainSecUpdatedCallback;
            _timer.OnCompleted -= OnCompletedCallback;
            _timer.OnClaimed -= OnClaimedCallback;
            _timer.OnStateTransition -= OnStateTransitionCallback;
            _isRegistered = false;
        }

        //============================================================
        // Logic
        //============================================================
        public bool TryStart(double durationSec)
        {
            return _timer.TryStart(durationSec);
        }

        public virtual bool TryReduce(double reduceSec)
        {
            return _timer.TryReduce(reduceSec);
        }

        public bool TryComplete()
        {
            return _timer.TryComplete();
        }

        public virtual bool TryClaim()
        {
            return _timer.TryClaim();
        }

        public void NotifyCurType()
        {
            _timer.NotifyCurType();
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OnProgressStartedCallback()
        {
            _onProgressStarted?.Invoke();
        }

        private void OnRemainSecUpdatedCallback(int remainingSec)
        {
            _onRemainSecUpdated?.Invoke(remainingSec);
        }

        private void OnCompletedCallback()
        {
            _onCompleted?.Invoke();
        }

        private void OnClaimedCallback()
        {
            _onClaimed?.Invoke();
        }

        private void OnStateTransitionCallback(ETaskTimerType prevType, ETaskTimerType nextType)
        {
            _onStateTransition?.Invoke(prevType, nextType);
        }

        //============================================================
        // Utilities
        //============================================================
        public virtual TaskTimerData ToData()
        {
            return new TaskTimerData(_timer.Id, _timer.CurType, Mathf.Max(_timer.RemainingSec, 0), _timer.DurationSec, _timer.Progress);
        }
    }
}
