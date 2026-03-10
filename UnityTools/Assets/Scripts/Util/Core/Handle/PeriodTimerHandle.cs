using UnityEngine.Events;

namespace UnityTools.Util
{
    public class PeriodTimerHandle
    {
        //============================================================
        // Readonly
        //============================================================
        protected readonly PeriodTimer _timer;

        //============================================================
        // Fields
        //============================================================
        private bool _isRegistered;

        //============================================================
        // Events
        //============================================================
        public event UnityAction OnOpenStarted { add => _onOpenStarted += value; remove => _onOpenStarted -= value; }
        public event UnityAction<int> OnUpdated { add => _onUpdated += value; remove => _onUpdated -= value; }
        public event UnityAction OnClosedStarted { add => _onClosedStarted += value; remove => _onClosedStarted -= value; }
        public event UnityAction<EPeriodTimerType> OnStateChanged { add => _timer.FSM.OnStateChanged += value; remove => _timer.FSM.OnStateChanged -= value; }
        private event UnityAction _onOpenStarted;
        private event UnityAction<int> _onUpdated;
        private event UnityAction _onClosedStarted;

        //============================================================
        // Properties
        //============================================================
        public EPeriodTimerType CurType => _timer.FSM.CurType;
        public string Id => _timer.Id;

        //============================================================
        // Constructors
        //============================================================
        public PeriodTimerHandle(PeriodTimer timer)
        {
            _timer = timer;
        }

        //============================================================
        // Init/Register
        //============================================================
        public virtual void Init(double openMin, double closedMin)
        {
            RegisterCallbacks();
            _timer.Init(openMin, closedMin);
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

            _timer.OnUpdated += onUpdatedCallback;
            _timer.OnOpenStarted += onOpenStartedCallback;
            _timer.OnClosedStarted += onClosedStartedCallback;
            _isRegistered = true;
        }

        private void UnregisterCallbacks()
        {
            if(!_isRegistered)
                return;

            _timer.OnUpdated -= onUpdatedCallback;
            _timer.OnOpenStarted -= onOpenStartedCallback;
            _timer.OnClosedStarted -= onClosedStartedCallback;
            _isRegistered = false;
        }

        //============================================================
        // Logic
        //============================================================
        public void ForceOpen()
        {
            _timer.ForceOpen();
        }

        public void ForceClosed()
        {
            _timer.ForceClosed();
        }

        public void SetPeriods(double openMin, double closedMin)
        {
            _timer.SetPeriods(openMin, closedMin);
        }

        //============================================================
        // Callbacks
        //============================================================
        private void onOpenStartedCallback()
        {
            _onOpenStarted?.Invoke();
        }

        private void onUpdatedCallback(int remainMin)
        {
            _onUpdated?.Invoke(remainMin);
        }

        private void onClosedStartedCallback()
        {
            _onClosedStarted?.Invoke();
        }

        //============================================================
        // Utilities
        //============================================================
        public int GetRemainingMin()
        {
            return _timer.GetRemainingMin();
        }

        public int GetRemainingSec()
        {
            return _timer.GetRemainingSec();
        }

        public virtual PeriodTimerData ToData()
        {
            return new PeriodTimerData(_timer.Id, _timer.FSM.CurType);
        }
    }
}
