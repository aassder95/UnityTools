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
        OpenStart,
        Open,
        Closed,
    }

    public class PeriodTimer
    {
        #region State Classes
        class OpenStartState : IState
        {
            PeriodTimer _timer;

            public OpenStartState(PeriodTimer timer)
            {
                _timer = timer;
            }

            public void Enter()
            {
                Debug.Log($"[OpenStart:Enter]");
                DateTime now = DateTime.UtcNow;
                _timer.SetPeriodTime(OPEN_START_KEY, now);
                _timer.SetPeriodTime(OPEN_END_KEY, now.AddMinutes(_timer.OPEN_PERIOD_MINUTES));
                _timer.SetPeriodTime(CLOSED_END_KEY, now.AddMinutes(_timer.OPEN_PERIOD_MINUTES + _timer.CLOSED_PERIOD_MINUTES));

                Execute();
            }

            public void Execute()
            {
                Debug.Log($"[OpenStart:Execute]");
                if (Utils.CompareWithoutMilliseconds(_timer.GetPeriodTime(OPEN_END_KEY), DateTime.UtcNow) > 0)
                    _timer._stateMachine.Change(EPeriodTimerState.Open);
            }

            public void Exit()
            {
                Debug.Log($"[OpenStart:Exit]");
            }
        }

        class OpenState : IState
        {
            PeriodTimer _timer;

            public OpenState(PeriodTimer timer)
            {
                _timer = timer;
            }

            public void Enter()
            {
                Debug.Log($"[Open:Enter]");
                _timer.StartPeriodUpdate(OPEN_END_KEY);
            }

            public void Execute()
            {
                Debug.Log($"[Open:Execute]");
                DateTime now = DateTime.UtcNow;
                if (Utils.CompareWithoutMilliseconds(_timer.GetPeriodTime(OPEN_UPDATED_KEY), now) > 0)
                {
                    Debug.LogError("Cheating detected");
                    _timer._stateMachine.Change(EPeriodTimerState.Closed);
                }
                else if (Utils.CompareWithoutMilliseconds(_timer.GetPeriodTime(OPEN_END_KEY), now) <= 0)
                {
                    _timer._stateMachine.Change(EPeriodTimerState.Closed);
                }
            }

            public void Exit()
            {
                Debug.Log($"[Open:Exit]");
            }
        }

        class ClosedState : IState
        {
            PeriodTimer _timer;

            public ClosedState(PeriodTimer timer)
            {
                _timer = timer;
            }

            public void Enter()
            {
                Debug.Log($"[Closed:Enter]");
                _timer.SetPeriodTime(OPEN_START_KEY, DateTime.MinValue);
                _timer.SetPeriodTime(OPEN_UPDATED_KEY, DateTime.MinValue);
                _timer.SetPeriodTime(OPEN_END_KEY, DateTime.MinValue);
                _timer.StartPeriodUpdate(CLOSED_END_KEY);
            }

            public void Execute()
            {
                Debug.Log($"[Closed:Execute]");
                if (Utils.CompareWithoutMilliseconds(_timer.GetPeriodTime(CLOSED_END_KEY), DateTime.UtcNow) <= 0)
                    _timer._stateMachine.Change(EPeriodTimerState.OpenStart);
            }

            public void Exit()
            {
                Debug.Log($"[Closed:Exit]");
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
        readonly Persistence _persistence;
        readonly StateMachine<EPeriodTimerState> _stateMachine;
        bool _init = false;
        Coroutine _coUpdate;
        readonly Dictionary<string, DateTime> _periodTimes = new();
        #endregion //Fields

        #region Properties
        public DateTime OpenStartTime => GetPeriodTime(OPEN_START_KEY);
        public DateTime OpenUpdatedTime => GetPeriodTime(OPEN_UPDATED_KEY);
        public DateTime OpenEndTime => GetPeriodTime(OPEN_END_KEY);
        public DateTime ClosedEndTime => GetPeriodTime(CLOSED_END_KEY);
        #endregion //Properties

        #region Events
        public event UnityAction<EPeriodTimerState> OnStateUpdated
        {
            add { _stateMachine.OnStateChanged += value; }
            remove { _stateMachine.OnStateChanged -= value; }
        }
        public event UnityAction<int> OnLoopUpdate;
        public event Func<IEnumerator> OnWait;
        #endregion //Events

        #region Constructors
        public PeriodTimer(string key, double openPeriodMin, double closedPeriodMin)
        {
            OPEN_PERIOD_MINUTES = openPeriodMin;
            CLOSED_PERIOD_MINUTES = closedPeriodMin;

            _persistence = new(key);
            _stateMachine = new();
            _stateMachine.Add(EPeriodTimerState.OpenStart, new OpenStartState(this));
            _stateMachine.Add(EPeriodTimerState.Open, new OpenState(this));
            _stateMachine.Add(EPeriodTimerState.Closed, new ClosedState(this));
        }
        #endregion //Constructors

        #region Initialization
        public void Init()
        {
            string[] suffixes = { OPEN_END_KEY, OPEN_START_KEY, OPEN_UPDATED_KEY, CLOSED_END_KEY };
            foreach (string suffix in suffixes)
            {
                _periodTimes[suffix] = _persistence.Load(suffix);
            }

            CoroutineHelper.Start(CoInit());
        }
        #endregion //Initialization

        #region State Logic
        public void ForceOpen()
        {
            if (!_init || _stateMachine.CurType == EPeriodTimerState.Open)
                return;

            _stateMachine.Change(EPeriodTimerState.OpenStart);
        }

        public void ForceClosed()
        {
            if (!_init || _stateMachine.CurType == EPeriodTimerState.Closed)
                return;

            SetPeriodTime(CLOSED_END_KEY, DateTime.UtcNow.AddMinutes(CLOSED_PERIOD_MINUTES));
            _stateMachine.Change(EPeriodTimerState.Closed);
        }
        #endregion //State Logic

        #region Timer Utilities
        DateTime GetPeriodTime(string key)
        {
            return _periodTimes.TryGetValue(key, out DateTime time) ? time : DateTime.MinValue;
        }

        void SetPeriodTime(string key, DateTime time)
        {
            _periodTimes[key] = Utils.TrimMilliseconds(time);
            _persistence.Save(key, _periodTimes[key]);
        }
        #endregion //Timer Utilities

        #region Coroutines
        void StartPeriodUpdate(string key)
        {
            CoroutineHelper.Replace(ref _coUpdate, CoUpdate(_periodTimes[key]));
        }

        IEnumerator CoInit()
        {
            yield return OnWait?.Invoke();
            _init = true;

            Debug.Log($"[CoInit]");

            DateTime now = DateTime.UtcNow;
            if (Utils.CompareWithoutMilliseconds(_periodTimes[OPEN_UPDATED_KEY], now) > 0)
            {
                Debug.LogError("Cheating detected");
                _stateMachine.Change(EPeriodTimerState.Closed);
            }
            else if (Utils.CompareWithoutMilliseconds(_periodTimes[OPEN_END_KEY], now) > 0)
            {
                _stateMachine.Change(EPeriodTimerState.Open);
            }
            else if (Utils.CompareWithoutMilliseconds(_periodTimes[CLOSED_END_KEY], now) > 0)
            {
                _stateMachine.Change(EPeriodTimerState.Closed);
            }
            else
            {
                _stateMachine.Change(EPeriodTimerState.OpenStart);
            }
        }

        IEnumerator CoUpdate(DateTime periodEndTime)
        {
            Debug.Log($"[CoUpdate]");
            while (true)
            {
                DateTime now = DateTime.UtcNow;
                int time = (int)Mathf.Ceil((float)(periodEndTime - now).TotalSeconds);

                if (time <= 0)
                    break;

                if (_stateMachine.CurType == EPeriodTimerState.Open)
                    SetPeriodTime(OPEN_UPDATED_KEY, now);

                OnLoopUpdate?.Invoke(time);
                yield return null;
            }

            _stateMachine.Update();
        }
        #endregion //Coroutines
    }
}