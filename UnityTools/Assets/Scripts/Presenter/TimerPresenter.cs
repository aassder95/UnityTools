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
            _periodTimer.OnLoopUpdated += OnLoopUpdated;
            _periodTimer.OnStateChanged += OnStateChanged;
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

        void OnStateChanged(EPeriodTimerState state)
        {
            _view.SetTimer(_periodTimer.OpenStartTime, _periodTimer.OpenUpdatedTime, _periodTimer.OpenEndTime, _periodTimer.ClosedEndTime);

            switch (state)
            {
                case EPeriodTimerState.Reset:
                    _view.SetSubState("Reset");
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

        void OnLoopUpdated(int min)
        {
            _view.SetLoop(min, _periodTimer.OpenUpdatedTime);
        }
    }
}