using System;
using System.Collections;
using UnityEngine.Events;


namespace UnityTools.Util.Core.Timer.Period
{
    public class PeriodTimerHandle : ITimerHandle
    {
        //============================================================
        // Readonly
        //============================================================
        protected readonly IPeriodTimer _timer;

        //============================================================
        // Fields
        //============================================================
        private bool _isRegistered;

        //============================================================
        // Events
        //============================================================
        public event UnityAction OnOpenPeriodPreparing { add => _onOpenPeriodPreparing += value; remove => _onOpenPeriodPreparing -= value; }
        public event UnityAction OnOpenPeriodStarted { add => _onOpenPeriodStarted += value; remove => _onOpenPeriodStarted -= value; }
        public event UnityAction<int> OnRemainMinUpdated { add => _onRemainMinUpdated += value; remove => _onRemainMinUpdated -= value; }
        public event UnityAction OnClosedPeriodStarted { add => _onClosedPeriodStarted += value; remove => _onClosedPeriodStarted -= value; }
        public event UnityAction<EPeriodTimerType, EPeriodTimerType> OnPeriodStateTransition { add => _onPeriodStateTransition += value; remove => _onPeriodStateTransition -= value; }
        private event UnityAction _onOpenPeriodPreparing;
        private event UnityAction _onOpenPeriodStarted;
        private event UnityAction<int> _onRemainMinUpdated;
        private event UnityAction _onClosedPeriodStarted;
        private event UnityAction<EPeriodTimerType, EPeriodTimerType> _onPeriodStateTransition;

        //============================================================
        // Properties
        //============================================================
        public EPeriodTimerType CurType => _timer.CurType;
        public bool IsReady => _timer.IsReady;
        public bool IsOpenPeriod => _timer.IsOpenPeriod;
        public bool IsClosedPeriod => _timer.IsClosedPeriod;
        public int RemainingMin => _timer.RemainingMin;
        public int RemainingSec => _timer.RemainingSec;
        public string Id => _timer.Id;

        //============================================================
        // Constructors
        //============================================================
        public PeriodTimerHandle(IPeriodTimer timer)
        {
            _timer = timer;
        }

        //============================================================
        // Init/Register
        //============================================================

        public virtual bool TryInit(double openMin, double closedMin, Func<IEnumerator> initWaitFunc = null)
        {
            RegisterCallbacks();
            if(_timer.TryInit(openMin, closedMin, initWaitFunc))
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

            _timer.OnOpenPeriodPreparing += OnOpenPeriodPreparingCallback;
            _timer.OnOpenPeriodStarted += OnOpenPeriodStartedCallback;
            _timer.OnRemainMinUpdated += OnRemainMinUpdatedCallback;
            _timer.OnClosedPeriodStarted += OnClosedPeriodStartedCallback;
            _timer.OnPeriodStateTransition += OnPeriodStateTransitionCallback;
            _isRegistered = true;
        }

        private void UnregisterCallbacks()
        {
            if(!_isRegistered)
                return;

            _timer.OnOpenPeriodPreparing -= OnOpenPeriodPreparingCallback;
            _timer.OnOpenPeriodStarted -= OnOpenPeriodStartedCallback;
            _timer.OnRemainMinUpdated -= OnRemainMinUpdatedCallback;
            _timer.OnClosedPeriodStarted -= OnClosedPeriodStartedCallback;
            _timer.OnPeriodStateTransition -= OnPeriodStateTransitionCallback;
            _isRegistered = false;
        }

        //============================================================
        // Logic
        //============================================================
        public bool TryForceOpen()
        {
            return _timer.TryForceOpen();
        }

        public bool TryForceClosed()
        {
            return _timer.TryForceClosed();
        }

        public bool TrySetPeriods(double openMin, double closedMin)
        {
            return _timer.TrySetPeriods(openMin, closedMin);
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OnOpenPeriodPreparingCallback()
        {
            _onOpenPeriodPreparing?.Invoke();
        }

        private void OnOpenPeriodStartedCallback()
        {
            _onOpenPeriodStarted?.Invoke();
        }

        private void OnRemainMinUpdatedCallback(int remainMin)
        {
            _onRemainMinUpdated?.Invoke(remainMin);
        }

        private void OnClosedPeriodStartedCallback()
        {
            _onClosedPeriodStarted?.Invoke();
        }

        private void OnPeriodStateTransitionCallback(EPeriodTimerType prevType, EPeriodTimerType nextType)
        {
            _onPeriodStateTransition?.Invoke(prevType, nextType);
        }

        //============================================================
        // Utilities
        //============================================================
        public virtual PeriodTimerData ToData()
        {
            return new PeriodTimerData(_timer.Id, _timer.CurType);
        }
    }
}
