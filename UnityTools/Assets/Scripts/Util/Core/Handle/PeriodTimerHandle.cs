using UnityEngine.Events;

namespace UnityTools.Util
{
    public class PeriodTimerHandle
    {
        //============================================================
        //Readonly
        //============================================================
        protected readonly PeriodTimer _timer;

        //============================================================
        //Fields
        //============================================================
        private bool _isRegistered;

        //============================================================
        //Events
        //============================================================
        public event UnityAction OnOpenStarted { add => _onOpenStarted += value; remove => _onOpenStarted -= value; }
        public event UnityAction<int> OnUpdated { add => _onUpdated += value; remove => _onUpdated -= value; }
        public event UnityAction OnClosedStarted { add => _onClosedStarted += value; remove => _onClosedStarted -= value; }
        public event UnityAction<EPeriodTimerType> OnStateChanged { add => _timer.FSM.OnStateChanged += value; remove => _timer.FSM.OnStateChanged -= value; }
        private event UnityAction _onOpenStarted;
        private event UnityAction<int> _onUpdated;
        private event UnityAction _onClosedStarted;

        //============================================================
        //Properties
        //============================================================
        public EPeriodTimerType CurType => _timer.FSM.CurType;
        public string Id => _timer.Id;
        public int RemainingMin => _timer.GetRemainingMin();
        public int RemainingSec => _timer.GetRemainingSec();

        //============================================================
        //Constructors
        //============================================================
        public PeriodTimerHandle(PeriodTimer timer)
        {
            _timer = timer;
        }

        //============================================================
        //Init/Register
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

            _timer.OnUpdated += OnUpdatedCallback;
            _timer.OnOpenStarted += OnOpenStartedCallback;
            _timer.OnClosedStarted += OnClosedStartedCallback;
            _isRegistered = true;
        }

        private void UnregisterCallbacks()
        {
            if(!_isRegistered)
                return;

            _timer.OnUpdated -= OnUpdatedCallback;
            _timer.OnOpenStarted -= OnOpenStartedCallback;
            _timer.OnClosedStarted -= OnClosedStartedCallback;
            _isRegistered = false;
        }

        //============================================================
        //Logic
        //============================================================
        public void ForceOpen()
        {
            if(_timer == null)
                return;

            _timer.ForceOpen();
        }

        public void ForceClosed()
        {
            if(_timer == null)
                return;

            _timer.ForceClosed();
        }

        public void SetPeriods(double openMin, double closedMin)
        {
            if(_timer == null)
                return;

            _timer.SetPeriods(openMin, closedMin);
        }

        //============================================================
        //Callbacks
        //============================================================
        private void OnOpenStartedCallback()
        {
            _onOpenStarted?.Invoke();
        }

        private void OnUpdatedCallback(int remainMin)
        {
            _onUpdated?.Invoke(remainMin);
        }

        private void OnClosedStartedCallback()
        {
            _onClosedStarted?.Invoke();
        }

        //============================================================
        //Utilities
        //============================================================
        public virtual PeriodTimerData ToData()
        {
            return new PeriodTimerData(_timer.Id, _timer.FSM.CurType);
        }
    }
}
