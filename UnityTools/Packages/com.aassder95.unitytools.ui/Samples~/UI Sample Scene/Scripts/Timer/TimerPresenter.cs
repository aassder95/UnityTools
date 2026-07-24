using UnityEngine;
using UnityEngine.Events;
using UnityTools.Util.Core.Logging;
using UnityTools.Timer.Period;
using UnityTools.Util.UiFramework;

namespace UnityTools.Samples.Timer
{
    public class TimerPresenter : BasePresenter<TimerModel, TimerView>
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly UnityAction<int> _onTimerUpdated;
        private readonly UnityAction _onForceOpen;
        private readonly UnityAction _onForceClosed;

        //============================================================
        // Fields
        //============================================================
        private PeriodTimer _periodTimer;

        //============================================================
        // Constructors
        //============================================================
        public TimerPresenter(TimerModel model, TimerView view) : base(model, view)
        {
            _onTimerUpdated = OnTimerUpdatedCallback;
            _onForceOpen = OnForceOpenCallback;
            _onForceClosed = OnForceClosedCallback;
        }

        //============================================================
        // Init/Register
        //============================================================
        protected override void OnInit()
        {
            bool isCreated = PeriodTimer.TryCreate("TIMER", _view, out _periodTimer);
            Debug.Assert(isCreated);
        }

        protected override void OnRelease()
        {
            _periodTimer.Release();
            _periodTimer = null;
        }

        protected override void BindEvents()
        {
            base.BindEvents();
            _view.OnForceOpen += _onForceOpen;
            _view.OnForceClosed += _onForceClosed;
            _periodTimer.OnRemainMinUpdated += _onTimerUpdated;
            _periodTimer.OnPeriodStateTransition += OnPeriodStateTransitionCallback;
        }

        protected override void UnbindEvents()
        {
            _view.OnForceOpen -= _onForceOpen;
            _view.OnForceClosed -= _onForceClosed;
            _periodTimer.OnRemainMinUpdated -= _onTimerUpdated;
            _periodTimer.OnPeriodStateTransition -= OnPeriodStateTransitionCallback;
            base.UnbindEvents();
        }

        //============================================================
        // Logic
        //============================================================
        protected override void OnShow()
        {
            if (!_periodTimer.TryInit(1.0, 1.0))
                DebugLogger.LogError("PeriodTimer 샘플 초기화에 실패했습니다.", _view);
        }

        protected override void OnHide()
        {
            _periodTimer.Release();
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OnTimerUpdatedCallback(int remainMin)
        {
            _model.SetLoop(remainMin, _periodTimer.OpenUpdatedTime);
        }

        private void OnForceOpenCallback()
        {
            if (!_periodTimer.TryForceOpen())
                DebugLogger.LogError("PeriodTimer Open 전환에 실패했습니다.", _view);
        }

        private void OnForceClosedCallback()
        {
            if (!_periodTimer.TryForceClosed())
                DebugLogger.LogError("PeriodTimer Closed 전환에 실패했습니다.", _view);
        }

        private void OnPeriodStateTransitionCallback(EPeriodTimerType prevType, EPeriodTimerType nextType)
        {
            if (prevType == nextType)
                return;

            switch (nextType)
            {
                case EPeriodTimerType.Reset:
                    _model.SetSnapshot(_periodTimer.OpenUpdatedTime, _periodTimer.OpenEndTime, _periodTimer.ClosedEndTime, "Reset", "Reset", true);
                    break;
                case EPeriodTimerType.Open:
                    _model.SetSnapshot(_periodTimer.OpenUpdatedTime, _periodTimer.OpenEndTime, _periodTimer.ClosedEndTime, "Open", null, false);
                    break;
                case EPeriodTimerType.Closed:
                    _model.SetSnapshot(_periodTimer.OpenUpdatedTime, _periodTimer.OpenEndTime, _periodTimer.ClosedEndTime, "Closed", "Closed", true);
                    break;
            }
        }
    }
}
