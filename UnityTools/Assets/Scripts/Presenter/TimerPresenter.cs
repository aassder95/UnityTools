using System;
using System.Collections;
using UnityEngine;
using UnityTools.UI;
using UnityTools.Util;

namespace UnityTools.Presenter
{
    public class TimerPresenter
    {
        readonly TimerView _view;

        PeriodTimer _periodTimer;

        public TimerPresenter(TimerView view)
        {
            _view = view;

            _periodTimer = new("OPEN_KEY", "CLOSED_KEY", 1.0, 1.0);
            _periodTimer.OnWait += CoWait;
            _periodTimer.OnStateUpdated += OnStateUpdated;
            _periodTimer.OnSubStateUpdated += OnSubStateUpdated;
            _periodTimer.OnLoopUpdate += OnLoopUpdate;
            _periodTimer.Init();
        }

        IEnumerator CoWait()
        {
            yield return new WaitForSecondsRealtime(2.0f);
        }

        void OnStateUpdated(EPeriodTimerState state)
        {
            _view.SetTimer(Utils.TrimMilliseconds(DateTime.UtcNow), _periodTimer.OpenEndTime, _periodTimer.ClosedEndTime);

            switch (state)
            {
                case EPeriodTimerState.Open:
                    _view.SetState("Open");
                    break;
                case EPeriodTimerState.Closed:
                    _view.SetState("Closed");
                    _view.SetSubState("Closed");
                    break;
            }
        }

        void OnSubStateUpdated(EPeriodTimerSubState state)
        {
            _view.SetTimer(Utils.TrimMilliseconds(DateTime.UtcNow), _periodTimer.OpenEndTime, _periodTimer.ClosedEndTime);

            switch (state)
            {
                case EPeriodTimerSubState.OpenStart:
                    _view.SetSubState("OpenStart");
                    break;
            }
        }

        void OnLoopUpdate(int min)
        {
            _view.SetLoop(min);
        }
    }
}