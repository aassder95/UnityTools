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
        public const string OPEN_START_KEY = "OPEN_START_KEY";
        public const string OPEN_UPDATED_KEY = "OPEN_UPDATED_KEY";
        public const string OPEN_END_KEY = "OPEN_END_KEY";
        public const string CLOSED_END_KEY = "CLOSED_END_KEY";

        private readonly Persistence _ps;
        private readonly StateMachine<EPeriodTimerState> _fsm;
        private readonly Dictionary<string, DateTime> _periodTimes = new();

        private bool _init;
        private bool _isTimeTamperedFlag;
        private double _openPeriodMin;
        private double _closedPeriodMin;
        private Coroutine _coInit;
        private Coroutine _coUpdate;

        public StateMachine<EPeriodTimerState> FSM => _fsm;
        public bool IsTimeTamperedFlag => _isTimeTamperedFlag;
        public bool IsOpenPeriod => DateTimeUtils.CompareWithoutMilliseconds(DateTime.UtcNow, _periodTimes[OPEN_END_KEY]) < 0;
        public bool IsClosedPeriod => DateTimeUtils.CompareWithoutMilliseconds(DateTime.UtcNow, _periodTimes[CLOSED_END_KEY]) < 0;
        public bool IsTimeTampered => DateTimeUtils.CompareWithoutMilliseconds(DateTime.UtcNow, _periodTimes[OPEN_UPDATED_KEY]) < 0;
        public double OpenPeriodMin => _openPeriodMin;
        public double ClosedPeriodMin => _closedPeriodMin;
        public DateTime OpenStartTime => _periodTimes[OPEN_START_KEY];
        public DateTime OpenUpdatedTime => _periodTimes[OPEN_UPDATED_KEY];
        public DateTime OpenEndTime => _periodTimes[OPEN_END_KEY];
        public DateTime ClosedEndTime => _periodTimes[CLOSED_END_KEY];

        public event UnityAction<int> OnLoopUpdated;
        public event UnityAction<EPeriodTimerState> OnStateChanged { add { _fsm.OnStateChanged += value; } remove { _fsm.OnStateChanged -= value; } }
        public event Func<IEnumerator> OnWait;

        public PeriodTimer(string key, double openPeriodMin, double closedPeriodMin)
        {
            _ps = new(key);
            _fsm = new();
            _fsm.Add(EPeriodTimerState.Reset, new PeriodTimerStates.ResetState(this));
            _fsm.Add(EPeriodTimerState.Open, new PeriodTimerStates.OpenState(this));
            _fsm.Add(EPeriodTimerState.Closed, new PeriodTimerStates.ClosedState(this));
            _periodTimes.AddRange(new[] { OPEN_START_KEY, OPEN_UPDATED_KEY, OPEN_END_KEY, CLOSED_END_KEY }, suffix => _ps.Load<DateTime>(suffix));

            _openPeriodMin = openPeriodMin;
            _closedPeriodMin = closedPeriodMin;
        }

        public void Init()
        {
            CoroutineHelper.Replace(ref _coInit, CoInit());
        }
        
        public void Release()
        {
            if (!_init)
                return;

            CoroutineHelper.Dispose(ref _coInit);
            CoroutineHelper.Dispose(ref _coUpdate);

            _init = false;
            _isTimeTamperedFlag = false;
            _periodTimes.Clear();
    
            OnLoopUpdated = null;
            OnWait = null;
            _fsm.Change(EPeriodTimerState.None);
        }

        private IEnumerator CoInit()
        {
            yield return OnWait?.Invoke();
            _init = true;

            if (IsOpenPeriod)
                _fsm.Change(EPeriodTimerState.Open);
            else if (IsClosedPeriod)
                _fsm.Change(EPeriodTimerState.Closed);
            else
                _fsm.Change(EPeriodTimerState.Reset, true);

            CoroutineHelper.Replace(ref _coUpdate, CoUpdate());
        }

        private IEnumerator CoUpdate()
        {
            while (true)
            {
                _fsm.Update();
                yield return new WaitForSecondsRealtime(60.0f);
            }
        }

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
            SetPeriodTime(CLOSED_END_KEY, DateTime.UtcNow.AddMinutes(_closedPeriodMin));
        }

        public void InvokeLoopUpdated(string key) => OnLoopUpdated?.Invoke(DateTimeUtils.GetRemainingMinutes(_periodTimes[key]));
        public void SetPeriodTime(string key, DateTime time) => _ps.Save(key, _periodTimes[key] = DateTimeUtils.RemoveMilliseconds(time));

        /// <summary>
        /// Open 기간의 시간을 동적으로 변경합니다.
        /// </summary>
        /// <param name="minutes">새로운 Open 기간 (분 단위)</param>
        /// <param name="applyImmediately">즉시 적용 여부 (현재 타이머를 리셋)</param>
        public void SetOpenPeriod(double minutes, bool applyImmediately = false)
        {
            _openPeriodMin = minutes;

            if (applyImmediately && _init)
            {
                _fsm.Change(EPeriodTimerState.Reset, true);
            }
        }

        /// <summary>
        /// Closed 기간의 시간을 동적으로 변경합니다.
        /// </summary>
        /// <param name="minutes">새로운 Closed 기간 (분 단위)</param>
        /// <param name="applyImmediately">즉시 적용 여부 (현재 타이머를 리셋)</param>
        public void SetClosedPeriod(double minutes, bool applyImmediately = false)
        {
            _closedPeriodMin = minutes;

            if (applyImmediately && _init && _fsm.CurType == EPeriodTimerState.Closed)
            {
                SetPeriodTime(CLOSED_END_KEY, DateTime.UtcNow.AddMinutes(_closedPeriodMin));
            }
        }

        /// <summary>
        /// Open과 Closed 기간을 모두 변경합니다.
        /// </summary>
        /// <param name="openMinutes">새로운 Open 기간 (분 단위)</param>
        /// <param name="closedMinutes">새로운 Closed 기간 (분 단위)</param>
        /// <param name="applyImmediately">즉시 적용 여부 (현재 타이머를 리셋)</param>
        public void SetPeriods(double openMinutes, double closedMinutes, bool applyImmediately = false)
        {
            _openPeriodMin = openMinutes;
            _closedPeriodMin = closedMinutes;

            if (applyImmediately && _init)
            {
                _fsm.Change(EPeriodTimerState.Reset, true);
            }
        }
    }
}
