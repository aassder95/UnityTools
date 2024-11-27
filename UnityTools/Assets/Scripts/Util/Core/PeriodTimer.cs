using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityTools.Util
{
    public enum EPeriodTimerState
    {
        None,
        Reset,
        Open,
        Closed,
    }

    public class PeriodTimer
    {
        #region State Classes
        class BaseState : IState
        {
            protected PeriodTimer _timer;
            public BaseState(PeriodTimer timer) { _timer = timer; }
            public virtual void Enter() { }
            public virtual void Execute() { }
            public virtual void Exit() { }
        }

        class ResetState : BaseState
        {
            public ResetState(PeriodTimer timer) : base(timer) { }
            public override void Enter()
            {
                DateTime now = DateTime.UtcNow;
                _timer.SetPeriodTime(OPEN_START_KEY, now);
                _timer.SetPeriodTime(OPEN_END_KEY, now.AddMinutes(_timer.OPEN_PERIOD_MINUTES));
                _timer.SetPeriodTime(CLOSED_END_KEY, now.AddMinutes(_timer.OPEN_PERIOD_MINUTES + _timer.CLOSED_PERIOD_MINUTES));
            }

            public override void Execute()
            {
                _timer._fsm.Change(EPeriodTimerState.Open);
            }
        }

        class OpenState : BaseState
        {
            public OpenState(PeriodTimer timer) : base(timer) { }
            public override void Execute()
            {
                if (_timer.IsCheatPeriod)
                {
                    _timer._isCheat = true;
                    _timer._fsm.Change(EPeriodTimerState.Closed);
                    return;
                }
                else if (!_timer.IsOpenPeriod)
                {
                    _timer._fsm.Change(EPeriodTimerState.Closed);
                    return;
                }

                _timer.SetPeriodTime(OPEN_UPDATED_KEY, DateTime.UtcNow);
                _timer.OnLoopUpdated?.Invoke(_timer.GetRemainTime(OPEN_END_KEY));
            }
        }

        class ClosedState : BaseState
        {
            public ClosedState(PeriodTimer timer) : base(timer) { }
            public override void Enter()
            {
                if (_timer._isCheat)
                {
                    _timer._isCheat = false;
                    _timer.SetPeriodTime(CLOSED_END_KEY, DateTime.UtcNow.AddMinutes(_timer.CLOSED_PERIOD_MINUTES));
                }

                _timer.SetPeriodTime(OPEN_START_KEY, DateTime.MinValue);
                _timer.SetPeriodTime(OPEN_UPDATED_KEY, DateTime.MinValue);
                _timer.SetPeriodTime(OPEN_END_KEY, DateTime.MinValue);
            }

            public override void Execute()
            {
                if (!_timer.IsClosedPeriod)
                {
                    _timer._fsm.Change(EPeriodTimerState.Reset, true);
                    return;
                }

                _timer.OnLoopUpdated?.Invoke(_timer.GetRemainTime(CLOSED_END_KEY));
            }
        }
        #endregion //State Classes

        #region Constants
        const string OPEN_START_KEY = "OPEN_START_KEY";
        const string OPEN_UPDATED_KEY = "OPEN_UPDATED_KEY";
        const string OPEN_END_KEY = "OPEN_END_KEY";
        const string CLOSED_END_KEY = "CLOSED_END_KEY";
        readonly double OPEN_PERIOD_MINUTES;
        readonly double CLOSED_PERIOD_MINUTES;
        #endregion //Constants

        #region Fields
        readonly Persistence _ps;
        readonly StateMachine<EPeriodTimerState> _fsm;
        bool _init = false;
        bool _isCheat = false;
        readonly Dictionary<string, DateTime> _periodTimes = new();
        #endregion //Fields

        #region Properties
        bool IsOpenPeriod => Utils.CompareWithoutMilliseconds(DateTime.UtcNow, _periodTimes[OPEN_END_KEY]) < 0;
        bool IsClosedPeriod => Utils.CompareWithoutMilliseconds(DateTime.UtcNow, _periodTimes[CLOSED_END_KEY]) < 0;
        bool IsCheatPeriod => Utils.CompareWithoutMilliseconds(DateTime.UtcNow, _periodTimes[OPEN_UPDATED_KEY]) < 0;
        public DateTime OpenStartTime => _periodTimes[OPEN_START_KEY];
        public DateTime OpenUpdatedTime => _periodTimes[OPEN_UPDATED_KEY];
        public DateTime OpenEndTime => _periodTimes[OPEN_END_KEY];
        public DateTime ClosedEndTime => _periodTimes[CLOSED_END_KEY];
        #endregion //Properties

        #region Events
        public event UnityAction<int> OnLoopUpdated;
        public event UnityAction<EPeriodTimerState> OnStateChanged { add { _fsm.OnStateChanged += value; } remove { _fsm.OnStateChanged -= value; } }
        public event Func<IEnumerator> OnWait;
        #endregion //Events

        #region Constructors
        public PeriodTimer(string key, double openPeriodMin, double closedPeriodMin)
        {
            OPEN_PERIOD_MINUTES = openPeriodMin;
            CLOSED_PERIOD_MINUTES = closedPeriodMin;

            _ps = new(key);
            _periodTimes.AddRange(new[] { OPEN_START_KEY, OPEN_UPDATED_KEY, OPEN_END_KEY, CLOSED_END_KEY }, suffix => _ps.Load(suffix));

            _fsm = new();
            _fsm.Add(EPeriodTimerState.Reset, new ResetState(this));
            _fsm.Add(EPeriodTimerState.Open, new OpenState(this));
            _fsm.Add(EPeriodTimerState.Closed, new ClosedState(this));
        }
        #endregion //Constructors

        #region Initialization
        public void Init()
        {
            CoroutineHelper.Start(CoInit());
        }

        IEnumerator CoInit()
        {
            yield return OnWait?.Invoke();
            _init = true;

            if (IsOpenPeriod)
                _fsm.Change(EPeriodTimerState.Open);
            else if (IsClosedPeriod)
                _fsm.Change(EPeriodTimerState.Closed);
            else
                _fsm.Change(EPeriodTimerState.Reset, true);

            CoroutineHelper.Start(CoUpdate());
        }

        IEnumerator CoUpdate()
        {
            while (true)
            {
                _fsm.Update();
                yield return new WaitForSecondsRealtime(60.0f);
            }
        }
        #endregion //Initialization

        #region State Control
        public void ForceOpen()
        {
            if (!_init || _fsm.CurType == EPeriodTimerState.Open)
                return;

            _fsm.Change(EPeriodTimerState.Reset, true);
        }

        public void ForceClosed()
        {
            if (!_init || _fsm.CurType == EPeriodTimerState.Closed)
                return;

            _isCheat = true;
            _fsm.Change(EPeriodTimerState.Closed);
        }
        #endregion //State Control

        #region Timer Utilities
        int GetRemainTime(string key) => Mathf.Max(0, Mathf.CeilToInt((float)(_periodTimes[key] - Utils.TrimMilliseconds(DateTime.UtcNow)).TotalMinutes));
        void SetPeriodTime(string key, DateTime time) => _ps.Save(key, _periodTimes[key] = Utils.TrimMilliseconds(time));
        #endregion //Timer Utilities
    }
}