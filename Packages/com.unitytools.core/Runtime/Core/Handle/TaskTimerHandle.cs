using UnityEngine;
using UnityEngine.Events;

namespace UnityTools.Util
{
    public class TaskTimerHandle
    {
        //============================================================
        //Readonly
        //============================================================
        protected readonly TaskTimer _timer;

        //============================================================
        //Fields
        //============================================================
        private bool _isRegistered;

        //============================================================
        //Events
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
        //Properties
        //============================================================
        public ETaskTimerType CurType => _timer.FSM.CurType;
        public bool IsClaimed => _timer.IsClaimed;
        public int RemainingSec => _timer.RemainingSec;
        public string Id => _timer.Id;

        //============================================================
        //Constructors
        //============================================================
        public TaskTimerHandle(TaskTimer timer)
        {
            _timer = timer;
        }

        //============================================================
        //Init/Register
        //============================================================
        public virtual void Init()
        {
            RegisterCallbacks();
            _timer.Init();
        }

        public virtual void Release()
        {
            _timer.Release();
            UnregisterCallbacks();
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
        //Logic
        //============================================================
        public bool Start(double durationSec)
        {
            if(durationSec <= 0d || double.IsNaN(durationSec) || double.IsInfinity(durationSec))
                return false;

            return _timer.Start(durationSec);
        }

        public virtual bool Reduce(double reduceSec)
        {
            if(reduceSec <= 0d || double.IsNaN(reduceSec) || double.IsInfinity(reduceSec))
                return false;

            return _timer.Reduce(reduceSec);
        }

        public bool CompleteImmediately()
        {
            if(CurType != ETaskTimerType.Processing)
                return false;

            return _timer.CompleteImmediately();
        }

        public virtual bool Claim()
        {
            if(CurType != ETaskTimerType.Completed)
                return false;

            return _timer.Claim();
        }

        public void NotifyCurType()
        {
            _timer.NotifyCurType();
        }

        //============================================================
        //Callbacks
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
        //Utilities
        //============================================================
        public virtual TaskTimerData ToData()
        {
            return new TaskTimerData(_timer.Id, Mathf.Max(_timer.RemainingSec, 0), _timer.DurationSec, _timer.GetProgress());
        }
    }
}
