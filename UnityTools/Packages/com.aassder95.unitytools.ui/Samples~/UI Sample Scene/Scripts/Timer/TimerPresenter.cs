using UnityEngine.Events;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.Core.Timer.Period;
using UnityTools.Util.UIFramework;

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
        protected override bool OnInit()
        {
            return PeriodTimer.TryCreate("TIMER", _view, out _periodTimer);
        }

        protected override bool OnRelease()
        {
            bool isSuccess = _periodTimer.TryRelease();
            _periodTimer = null;
            return isSuccess;
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
        protected override bool OnShow()
        {
            return _periodTimer.IsReady || _periodTimer.TryInit(1.0, 1.0);
        }

        protected override bool OnHide()
        {
            return _periodTimer.TryRelease();
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
            if(_periodTimer.TryForceOpen())
                return;

            DebugLogger.LogError("PeriodTimer 강제 Open에 실패했습니다.");
            StopAfterFailure();
        }

        private void OnForceClosedCallback()
        {
            if(_periodTimer.TryForceClosed())
                return;

            DebugLogger.LogError("PeriodTimer 강제 Closed에 실패했습니다.");
            StopAfterFailure();
        }

        private void OnPeriodStateTransitionCallback(EPeriodTimerType prevType, EPeriodTimerType nextType)
        {
            if(prevType == nextType)
                return;

            switch(nextType)
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
