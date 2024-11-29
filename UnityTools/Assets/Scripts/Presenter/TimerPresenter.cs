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
            BindEvents();

            _view = view;
            _periodTimer = new("TIMER", 1.0, 1.0);
            _periodTimer.OnWait += CoWait;
            _periodTimer.Init();
        }

        void BindEvents()
        {
            EventDispatcher.Instance.Subscribe(EEventDispatcherType.PeriodTimerForceOpen, OnForceOpen);
            EventDispatcher.Instance.Subscribe(EEventDispatcherType.PeriodTimerForceClose, OnForceClosed);
            EventDispatcher.Instance.Subscribe<int>(EEventDispatcherType.PeriodTimerLoopUpdated, OnLoopUpdated);
            EventDispatcher.Instance.Subscribe<EPeriodTimerState>(EEventDispatcherType.PeriodTimerStateChanged, OnStateChanged);
        }

        void OnForceOpen(object sender)
        {
            _periodTimer.ForceOpen();
        }

        void OnForceClosed(object sender)
        {
            _periodTimer.ForceClosed();
        }

        IEnumerator CoWait()
        {
            yield return new WaitForSecondsRealtime(2.0f);
        }

        void OnLoopUpdated(object sender, int min)
        {
            _view.SetLoop(min, _periodTimer.OpenUpdatedTime);
        }

        void OnStateChanged(object sender, EPeriodTimerState state)
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
    }
}