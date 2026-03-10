using UnityEngine;
using UnityEngine.Events;

namespace UnityTools.Util
{
    public class TaskTimerHandle
    {
        //============================================================
        // Constants
        //============================================================
        private const double SEC_PER_MIN = 60d;

        //============================================================
        // Readonly
        //============================================================
        protected readonly TaskTimer _timer;

        //============================================================
        // Fields
        //============================================================
        private bool _isRegistered;

        //============================================================
        // Events
        //============================================================
        public event UnityAction OnProgressStarted { add => _onProgressStarted += value; remove => _onProgressStarted -= value; }
        public event UnityAction<int> OnUpdated { add => _onUpdated += value; remove => _onUpdated -= value; }
        public event UnityAction OnCompleted { add => _onCompleted += value; remove => _onCompleted -= value; }
        public event UnityAction OnClaimed { add => _onClaimed += value; remove => _onClaimed -= value; }
        public event UnityAction<ETaskTimerType> OnStateChanged { add => _timer.FSM.OnStateChanged += value; remove => _timer.FSM.OnStateChanged -= value; }
        private event UnityAction _onProgressStarted;
        private event UnityAction<int> _onUpdated;
        private event UnityAction _onCompleted;
        private event UnityAction _onClaimed;

        //============================================================
        // Properties
        //============================================================
        public ETaskTimerType CurType => _timer.FSM.CurType;
        public bool IsClaimed => _timer.IsClaimed;
        public int RemainingSec => _timer.RemainingSec;
        public string Id => _timer.Id;

        //============================================================
        // Constructors
        //============================================================
        public TaskTimerHandle(TaskTimer timer)
        {
            _timer = timer;
        }

        //============================================================
        // Init/Register
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

            _timer.OnProgressStarted += onProgressStartedCallback;
            _timer.OnUpdated += onUpdatedCallback;
            _timer.OnCompleted += onCompletedCallback;
            _timer.OnClaimed += onClaimedCallback;
            _isRegistered = true;
        }

        private void UnregisterCallbacks()
        {
            if(!_isRegistered)
                return;

            _timer.OnProgressStarted -= onProgressStartedCallback;
            _timer.OnUpdated -= onUpdatedCallback;
            _timer.OnCompleted -= onCompletedCallback;
            _timer.OnClaimed -= onClaimedCallback;
            _isRegistered = false;
        }

        //============================================================
        // Logic
        //============================================================
        public bool Start(double durationSec)
        {
            return _timer.Start(durationSec);
        }

        public bool StartMinutes(double durationMin)
        {
            if(double.IsNaN(durationMin) || double.IsInfinity(durationMin))
                return false;

            return _timer.Start(durationMin * SEC_PER_MIN);
        }

        public virtual bool Reduce(double reduceSec)
        {
            return _timer.Reduce(reduceSec);
        }

        public virtual bool ReduceMinutes(double reduceMin)
        {
            if(double.IsNaN(reduceMin) || double.IsInfinity(reduceMin))
                return false;

            return _timer.Reduce(reduceMin * SEC_PER_MIN);
        }

        public bool CompleteImmediately()
        {
            return _timer.CompleteImmediately();
        }

        public virtual bool Claim()
        {
            return _timer.Claim();
        }

        //============================================================
        // Callbacks
        //============================================================
        private void onProgressStartedCallback()
        {
            _onProgressStarted?.Invoke();
        }

        private void onUpdatedCallback(int remainingSec)
        {
            _onUpdated?.Invoke(remainingSec);
        }

        private void onCompletedCallback()
        {
            _onCompleted?.Invoke();
        }

        private void onClaimedCallback()
        {
            _onClaimed?.Invoke();
        }

        //============================================================
        // Utilities
        //============================================================
        public virtual TaskTimerData ToData()
        {
            return new TaskTimerData(_timer.Id, Mathf.Max(_timer.RemainingSec, 0), _timer.DurationSec, _timer.GetProgress());
        }

        public void NotifyCurType()
        {
            _timer.NotifyCurType();
        }
    }
}
