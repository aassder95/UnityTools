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
        #region Constants
        public const string OPEN_START_KEY = "OPEN_START_KEY";
        public const string OPEN_UPDATED_KEY = "OPEN_UPDATED_KEY";
        public const string OPEN_END_KEY = "OPEN_END_KEY";
        public const string CLOSED_END_KEY = "CLOSED_END_KEY";
        public readonly double OPEN_PERIOD_MINUTES;
        public readonly double CLOSED_PERIOD_MINUTES;
        #endregion //Constants

        #region Fields
        readonly Persistence _ps;
        readonly StateMachine<EPeriodTimerState> _fsm;
        bool _init = false;
        bool _isTimeTamperedFlag = false;
        readonly Dictionary<string, DateTime> _periodTimes = new();
        #endregion //Fields

        #region Properties
        public StateMachine<EPeriodTimerState> FSM => _fsm;
        public bool IsTimeTamperedFlag => _isTimeTamperedFlag;
        public bool IsOpenPeriod => Utils.CompareWithoutMilliseconds(DateTime.UtcNow, _periodTimes[OPEN_END_KEY]) < 0;
        public bool IsClosedPeriod => Utils.CompareWithoutMilliseconds(DateTime.UtcNow, _periodTimes[CLOSED_END_KEY]) < 0;
        public bool IsTimeTampered => Utils.CompareWithoutMilliseconds(DateTime.UtcNow, _periodTimes[OPEN_UPDATED_KEY]) < 0;
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
            _fsm.Add(EPeriodTimerState.Reset, new PeriodTimerStates.ResetState(this));
            _fsm.Add(EPeriodTimerState.Open, new PeriodTimerStates.OpenState(this));
            _fsm.Add(EPeriodTimerState.Closed, new PeriodTimerStates.ClosedState(this));
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

            _isTimeTamperedFlag = true;
            _fsm.Change(EPeriodTimerState.Closed);
        }

        public void MarkTimeTampered()
        {
            _isTimeTamperedFlag = true;
        }

        public void ClearTimeTampered()
        {
            _isTimeTamperedFlag = false;
            SetPeriodTime(CLOSED_END_KEY, DateTime.UtcNow.AddMinutes(CLOSED_PERIOD_MINUTES));
        }
        #endregion //State Control

        #region Timer Utilities
        public void InvokeLoopUpdated(string key) => OnLoopUpdated?.Invoke(Utils.GetRemainingMinutes(_periodTimes[key]));
        public void SetPeriodTime(string key, DateTime time) => _ps.Save(key, _periodTimes[key] = Utils.TrimMilliseconds(time));
        #endregion //Timer Utilities
    }
}