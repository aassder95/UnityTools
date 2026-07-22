using System;
using System.Collections;
using UnityEngine.Events;

namespace UnityTools.Util.Core.Timer.Period
{
    // Exception: Period timer values are minute-based by product requirement.
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
        public virtual void Init(double openMin, double closedMin, Func<IEnumerator> initWaitFunc = null)
        {
            RegisterCallbacks();
            _timer.Init(openMin, closedMin, initWaitFunc);
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
        public void ForceOpen()
        {
            if(!IsReady)
                return;

            _timer.ForceOpen();
        }

        public void ForceClosed()
        {
            if(!IsReady)
                return;

            _timer.ForceClosed();
        }

        public void SetPeriods(double openMin, double closedMin)
        {
            if(!IsReady)
                return;

            _timer.SetPeriods(openMin, closedMin);
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
        public int GetRemainingMin()
        {
            return !IsReady ? 0 : _timer.GetRemainingMin();
        }

        public int GetRemainingSec()
        {
            return !IsReady ? 0 : _timer.GetRemainingSec();
        }

        public virtual PeriodTimerData ToData()
        {
            return new PeriodTimerData(_timer.Id, _timer.CurType);
        }
    }
}
