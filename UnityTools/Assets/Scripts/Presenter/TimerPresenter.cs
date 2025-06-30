using System.Collections;
using UnityEngine;
using UnityTools.Model;
using UnityTools.UI;
using UnityTools.Util;

namespace UnityTools.Presenter
{
    public class TimerPresenter : BasePresenter<TimerModel, TimerView>
    {
        PeriodTimer _periodTimer;

        public TimerPresenter(TimerView view) : base(new(), view) { }

        protected override void Init()
        {
            _periodTimer = new("TIMER", 1.0, 1.0);
            _periodTimer.Init();
        }

        public override void ShowView()
        {
            base.ShowView();
        }

        public override void HideView()
        {
            base.HideView();
        }

        protected override void BindEvents()
        {
            _view.OnForceOpen += _periodTimer.ForceOpen;
            _view.OnForceClosed += _periodTimer.ForceClosed;
            _periodTimer.OnWait += CoWait;
            _periodTimer.OnLoopUpdated += OnLoopUpdated;
            _periodTimer.OnStateChanged += OnStateChanged;
        }

        protected override void UnbindEvents()
        {
            _view.OnForceOpen -= _periodTimer.ForceOpen;
            _view.OnForceClosed -= _periodTimer.ForceClosed;
            _periodTimer.OnWait -= CoWait;
            _periodTimer.OnLoopUpdated -= OnLoopUpdated;
            _periodTimer.OnStateChanged -= OnStateChanged;
        }

        IEnumerator CoWait() => new WaitForSecondsRealtime(2.0f);

        void OnStateChanged(EPeriodTimerState state)
        {
            _model.SetTimer(_periodTimer.OpenStartTime, _periodTimer.OpenUpdatedTime, _periodTimer.OpenEndTime, _periodTimer.ClosedEndTime);

            switch (state)
            {
                case EPeriodTimerState.Reset:
                    _model.SetSubState("Reset");
                    break;
                case EPeriodTimerState.Open:
                    _model.SetState("Open");
                    break;
                case EPeriodTimerState.Closed:
                    _model.SetState("Closed");
                    _model.SetSubState("Closed");
                    break;
            }

            _view.UpdateView(_model);
        }

        void OnLoopUpdated(int min)
        {
            _model.SetLoop(min, _periodTimer.OpenUpdatedTime);
            _view.UpdateView(_model);
        }
    }
}