using UnityTools.Model;
using UnityTools.UI;
using UnityTools.Util;
using UnityEngine.Events;

namespace UnityTools.Presenter
{
    public class TimerPresenter : BasePresenter<TimerModel, TimerView>
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly UnityAction<int> _onTimerUpdated;

        //============================================================
        //Fields
        //============================================================
        private PeriodTimer _periodTimer;

        //============================================================
        //Constructors
        //============================================================
        public TimerPresenter(TimerModel model, TimerView view) : base(model, view)
        {
            _onTimerUpdated = remainMin =>
            {
                if(_periodTimer == null)
                    return;

                _model.SetLoop(remainMin, _periodTimer.OpenUpdatedTime);
            };
        }

        //============================================================
        //Init/Register
        //============================================================
        public override void Init()
        {
            _periodTimer = new PeriodTimer("TIMER", _view);
            _periodTimer.Init(1.0, 1.0);
            base.Init();
        }

        public override void Release()
        {
            base.Release();
            _periodTimer?.Release();
            _periodTimer = null;
        }

        protected override void BindEvents()
        {
            base.BindEvents();

            if(_periodTimer == null)
                return;

            _view.OnForceOpen += _periodTimer.ForceOpen;
            _view.OnForceClosed += _periodTimer.ForceClosed;
            _periodTimer.OnUpdated += _onTimerUpdated;
            _periodTimer.OnStateChanged += OnStateChangedCallback;
        }

        protected override void UnbindEvents()
        {
            if(_periodTimer != null)
            {
                _view.OnForceOpen -= _periodTimer.ForceOpen;
                _view.OnForceClosed -= _periodTimer.ForceClosed;
                _periodTimer.OnUpdated -= _onTimerUpdated;
                _periodTimer.OnStateChanged -= OnStateChangedCallback;
            }

            base.UnbindEvents();
        }

        //============================================================
        //Callbacks
        //============================================================
        private void OnStateChangedCallback(EPeriodTimerType type)
        {
            _model.SetTimer(_periodTimer.OpenStartTime, _periodTimer.OpenUpdatedTime, _periodTimer.OpenEndTime, _periodTimer.ClosedEndTime);

            switch (type)
            {
                case EPeriodTimerType.Reset:
                    _model.SetState("Reset");
                    _model.SetSubState("Reset");
                    break;
                case EPeriodTimerType.Open:
                    _model.SetState("Open");
                    break;
                case EPeriodTimerType.Closed:
                    _model.SetState("Closed");
                    _model.SetSubState("Closed");
                    break;
            }
        }
    }
}
