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
            _view.OnForceOpen += OnForceOpen;
            _view.OnForceClosed += OnForceClosed;

            _periodTimer = new("TIMER", 1.0, 1.0);
            _periodTimer.OnWait += CoWait;
            _periodTimer.OnStateUpdated += OnStateUpdated;
            _periodTimer.OnLoopUpdate += OnLoopUpdate;
            _periodTimer.Init();
        }

        void OnForceOpen()
        {
            _periodTimer.ForceOpen();
        }

        void OnForceClosed()
        {
            _periodTimer.ForceClosed();
        }

        IEnumerator CoWait()
        {
            yield return new WaitForSecondsRealtime(2.0f);
        }

        void OnStateUpdated(EPeriodTimerState state)
        {
            _view.SetTimer(_periodTimer.OpenStartime, _periodTimer.OpenEndTime, _periodTimer.ClosedEndTime);

            switch (state)
            {
                case EPeriodTimerState.OpenStart:
                    _view.SetSubState("OpenStart");
                    break;
                case EPeriodTimerState.Open:
                    _view.SetState("Open");
                    break;
                case EPeriodTimerState.Closed:
                    _view.SetState("Closed");
                    _view.SetSubState("Closed");
                    break;
            }
        }

        void OnLoopUpdate(int min)
        {
            _view.SetLoop(min, _periodTimer.OpenUpdatedTime);
        }
    }
}