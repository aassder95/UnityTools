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

    public class PeriodTimerPersistence
    {
        readonly string ROOT_KEY;

        public PeriodTimerPersistence(string key)
        {
            ROOT_KEY = key;
        }

        public DateTime Save(string suffix, DateTime time)
        {
            time = Utils.TrimMilliseconds(time);
            PlayerPrefs.SetString(GetKey(suffix), time.ToString());
            return time;
        }

        public DateTime Load(string suffix)
        {
            if (PlayerPrefs.HasKey(GetKey(suffix)))
            {
                if (DateTime.TryParse(PlayerPrefs.GetString(GetKey(suffix)), out DateTime time))
                    return time;
            }

            return DateTime.MinValue;
        }

        string GetKey(string suffix)
        {
            return ROOT_KEY + "_" + suffix;
        }
    }

    public class PeriodTimer
    {
        #region Constants
        const string OPEN_KEY = "OPEN_KEY";
        const string OPEN_START_KEY = "OPEN_START_KEY";
        const string OPEN_UPDATED_KEY = "OPEN_UPDATED_KEY";
        const string CLOSED_KEY = "CLOSED_KEY";
        const float LOOP_INTERVAL_SECONDS = 60.0f;

        readonly double OPEN_PERIOD_MINUTES;
        readonly double CLOSED_PERIOD_MINUTES;
        #endregion //Constants

        #region Fields
        EPeriodTimerState _state = EPeriodTimerState.None;
        PeriodTimerPersistence _persistence;
        bool _init = false;
        Coroutine _coUpdate;
        readonly Dictionary<string, DateTime> _periodEndTimes = new();
        #endregion //Fields

        #region Properties
        public DateTime OpenEndTime => _periodEndTimes[OPEN_KEY];
        public DateTime OpenStartime => _periodEndTimes[OPEN_START_KEY];
        public DateTime OpenUpdatedTime => _periodEndTimes[OPEN_UPDATED_KEY];
        public DateTime ClosedEndTime => _periodEndTimes[CLOSED_KEY];
        #endregion //Properties

        #region Events
        public event UnityAction<EPeriodTimerState> OnStateUpdated;
        public event UnityAction<int> OnLoopUpdate;
        public event Func<IEnumerator> OnWait;
        #endregion //Events

        #region Constructors
        public PeriodTimer(string key, double openPeriodMin, double closedPeriodMin)
        {
            OPEN_PERIOD_MINUTES = openPeriodMin;
            CLOSED_PERIOD_MINUTES = closedPeriodMin;

            _persistence = new(key);
        }
        #endregion //Constructors

        #region Initialization
        public void Init()
        {
            string[] suffixes = { OPEN_KEY, OPEN_START_KEY, OPEN_UPDATED_KEY, CLOSED_KEY };
            foreach (string suffix in suffixes)
            {
                _periodEndTimes[suffix] = _persistence.Load(suffix);
            }

            CoroutineHelper.Start(CoInit());
        }
        #endregion //Initialization

        #region State Management
        void UpdatePeriodState()
        {
            DateTime curTime = Utils.TrimMilliseconds(DateTime.UtcNow);
            bool isOpen = Utils.CompareWithoutMilliseconds(_periodEndTimes[OPEN_KEY], curTime) > 0;
            bool isClosed = Utils.CompareWithoutMilliseconds(_periodEndTimes[CLOSED_KEY], curTime) > 0;
            if (!isOpen && !isClosed)
            {
                isOpen = true;
                SetState(EPeriodTimerState.OpenStart, curTime);
            }

            if (isOpen)
                SetState(EPeriodTimerState.Open, curTime);
            else if (isClosed)
                SetState(EPeriodTimerState.Closed, curTime);
        }

        void SetState(EPeriodTimerState state, DateTime curTime)
        {
            _state = state;

            switch (state)
            {
                case EPeriodTimerState.OpenStart:
                    _periodEndTimes[OPEN_START_KEY] = _persistence.Save(OPEN_START_KEY, curTime);
                    _periodEndTimes[OPEN_KEY] = _persistence.Save(OPEN_KEY, _periodEndTimes[OPEN_START_KEY].AddMinutes(OPEN_PERIOD_MINUTES));
                    _periodEndTimes[CLOSED_KEY] = _persistence.Save(CLOSED_KEY, _periodEndTimes[OPEN_KEY].AddMinutes(CLOSED_PERIOD_MINUTES));
                    break;
                case EPeriodTimerState.Open:
                    CoroutineHelper.Replace(ref _coUpdate, CoUpdate(curTime, _periodEndTimes[OPEN_KEY]));
                    break;
                case EPeriodTimerState.Closed:
                    _periodEndTimes[OPEN_KEY] = _persistence.Save(OPEN_KEY, DateTime.MinValue);
                    CoroutineHelper.Replace(ref _coUpdate, CoUpdate(curTime, _periodEndTimes[CLOSED_KEY]));
                    break;
            }

            OnStateUpdated?.Invoke(state);
        }
        #endregion //State Management

        #region State Control
        public void ForceOpen()
        {
            if (!_init || _state == EPeriodTimerState.Open)
                return;

            DateTime curTime = Utils.TrimMilliseconds(DateTime.UtcNow);
            SetState(EPeriodTimerState.OpenStart, curTime);
            SetState(EPeriodTimerState.Open, curTime);
        }

        public void ForceClosed()
        {
            if (!_init || _state == EPeriodTimerState.Closed)
                return;

            DateTime curTime = Utils.TrimMilliseconds(DateTime.UtcNow);
            _periodEndTimes[OPEN_START_KEY] = _persistence.Save(OPEN_START_KEY, DateTime.MinValue);
            _periodEndTimes[OPEN_UPDATED_KEY] = _persistence.Save(OPEN_UPDATED_KEY, DateTime.MinValue);
            _periodEndTimes[CLOSED_KEY] = _persistence.Save(CLOSED_KEY, curTime.AddMinutes(CLOSED_PERIOD_MINUTES));
            SetState(EPeriodTimerState.Closed, curTime);
        }
        #endregion State Control

        #region Coroutines
        IEnumerator CoInit()
        {
            yield return OnWait?.Invoke();
            _init = true;

            if (Utils.CompareWithoutMilliseconds(_periodEndTimes[OPEN_UPDATED_KEY], DateTime.UtcNow) > 0)
                ForceClosed();
            else
                UpdatePeriodState();
        }

        IEnumerator CoUpdate(DateTime curTime, DateTime periodEndTime)
        {
            int time = (int)Mathf.Ceil((float)(periodEndTime - curTime).TotalMinutes);
            while (time > 0)
            {
                if (_state == EPeriodTimerState.Open)
                    _periodEndTimes[OPEN_UPDATED_KEY] = _persistence.Save(OPEN_UPDATED_KEY, DateTime.UtcNow);

                OnLoopUpdate?.Invoke(time);

                yield return new WaitForSecondsRealtime(LOOP_INTERVAL_SECONDS);

                if (--time <= 0)
                    break;
            }

            UpdatePeriodState();
        }
        #endregion //Coroutines
    }
}