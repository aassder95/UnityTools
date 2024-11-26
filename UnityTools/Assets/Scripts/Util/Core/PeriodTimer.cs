using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Util
{
    public enum EPeriodTimerState
    {
        OpenStart,
        Open,
        Closed,
    }
    public class PeriodTimer
    {
        #region Constant
        const string OPEN_KEY = "OPEN_KEY";
        const string OPEN_START_KEY = "OPEN_START_KEY";
        const string OPEN_UPDATED_KEY = "OPEN_UPDATED_KEY";
        const string CLOSED_KEY = "CLOSED_KEY";
        const float LOOP_INTERVAL_SECONDS = 1.0f;

        readonly string ROOT_KEY;
        readonly double OPEN_PERIOD_MINUTES;
        readonly double CLOSED_PERIOD_MINUTES;
        #endregion //Constant

        #region Variable
        EPeriodTimerState _state = EPeriodTimerState.Closed;
        Coroutine _coUpdate;
        readonly Dictionary<string, DateTime> _periodEndTimes = new();
        #endregion //Variable

        #region Property
        public DateTime OpenEndTime => _periodEndTimes[OPEN_KEY];
        public DateTime ClosedEndTime => _periodEndTimes[CLOSED_KEY];
        #endregion //Property

        #region Event
        public event Action<EPeriodTimerState> OnStateUpdated;
        public event Action<int> OnLoopUpdate;
        public event Func<IEnumerator> OnWait;
        #endregion //Event

        #region Constructor
        public PeriodTimer(string key, double openPeriodMin, double closedPeriodMin)
        {
            ROOT_KEY = key;
            OPEN_PERIOD_MINUTES = openPeriodMin;
            CLOSED_PERIOD_MINUTES = closedPeriodMin;
        }
        #endregion //Constructor

        #region File
        void SavePeriodEndTime(string suffix, DateTime time)
        {
            time = Utils.TrimMilliseconds(time);
            _periodEndTimes[suffix] = time;
            PlayerPrefs.SetString(GetKey(suffix), time.ToString());
        }

        void LoadPeriodEndTime(string suffix)
        {
            if (PlayerPrefs.HasKey(GetKey(suffix)))
                _periodEndTimes[suffix] = DateTime.Parse(PlayerPrefs.GetString(GetKey(suffix)));
            else
                _periodEndTimes[suffix] = DateTime.MinValue;
        }
        #endregion //File

        #region Timer
        public void Init()
        {
            string[] suffixes = { OPEN_KEY, OPEN_START_KEY, OPEN_UPDATED_KEY, CLOSED_KEY };
            foreach (string suffix in suffixes)
            {
                LoadPeriodEndTime(suffix);
            }

            CoroutineHelper.Start(CoInit());
        }

        IEnumerator CoInit()
        {
            yield return OnWait?.Invoke();
            UpdatePeriodState();
        }

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

        IEnumerator CoUpdate(DateTime curTime, DateTime periodEndTime)
        {
            int time = (int)Mathf.Ceil((float)(periodEndTime - curTime).TotalSeconds);
            while (time > 0)
            {
                OnLoopUpdate?.Invoke(time);

                if (_state == EPeriodTimerState.Open)
                    SavePeriodEndTime(OPEN_UPDATED_KEY, curTime);

                yield return new WaitForSecondsRealtime(LOOP_INTERVAL_SECONDS);

                if (--time <= 0)
                    break;
            }

            UpdatePeriodState();
        }
        #endregion //Timer

        #region Get & Set
        string GetKey(string suffix)
        {
            return ROOT_KEY + "_" + suffix;
        }

        void SetState(EPeriodTimerState state, DateTime curTime)
        {
            _state = state;

            switch (state)
            {
                case EPeriodTimerState.OpenStart:
                    SavePeriodEndTime(OPEN_START_KEY, curTime);
                    SavePeriodEndTime(OPEN_KEY, curTime.AddMinutes(OPEN_PERIOD_MINUTES));
                    SavePeriodEndTime(CLOSED_KEY, _periodEndTimes[OPEN_KEY].AddMinutes(CLOSED_PERIOD_MINUTES));
                    break;
                case EPeriodTimerState.Open:
                    CoroutineHelper.Replace(ref _coUpdate, CoUpdate(curTime, _periodEndTimes[OPEN_KEY]));
                    break;
                case EPeriodTimerState.Closed:
                    SavePeriodEndTime(OPEN_KEY, DateTime.MinValue);
                    CoroutineHelper.Replace(ref _coUpdate, CoUpdate(curTime, _periodEndTimes[CLOSED_KEY]));
                    break;
            }

            OnStateUpdated?.Invoke(state);
        }
        #endregion //Get & Set
    }
}