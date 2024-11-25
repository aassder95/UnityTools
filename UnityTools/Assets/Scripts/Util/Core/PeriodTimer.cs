using System;
using System.Collections;
using UnityEngine;

namespace UnityTools.Util
{
    public enum EPeriodTimerState
    {
        Open,
        Closed,
    }

    public enum EPeriodTimerSubState
    {
        OpenStart,
    }

    public class PeriodTimer
    {
        readonly string OPEN_KEY = "";
        readonly string OPEN_START_KEY = "";
        readonly string CLOSED_KEY = "";
        readonly double OPEN_PERIOD_MINUTES;
        readonly double CLOSED_PERIOD_MINUTES;

        EPeriodTimerState _state = EPeriodTimerState.Closed;
        DateTime _openPeriodEndTime = DateTime.MinValue;
        DateTime _closedPeriodEndTime = DateTime.MinValue;

        Coroutine _coUpdate;

        public event Action<EPeriodTimerState> OnStateUpdated;
        public event Action<EPeriodTimerSubState> OnSubStateUpdated;
        public event Action<int> OnLoopUpdate;
        public event Func<IEnumerator> OnWait;

        public EPeriodTimerState State => _state;
        public DateTime OpenEndTime => _openPeriodEndTime;
        public DateTime ClosedEndTime => _closedPeriodEndTime;

        public PeriodTimer(string openKey, string closedkey, double openPeriodMin, double closedPeriodMin)
        {
            OPEN_KEY = openKey;
            OPEN_START_KEY = "START_" + openKey;
            CLOSED_KEY = closedkey;
            OPEN_PERIOD_MINUTES = openPeriodMin;
            CLOSED_PERIOD_MINUTES = closedPeriodMin;
        }

        public void Init()
        {
            _openPeriodEndTime = LoadPeriodEndTime(OPEN_KEY);
            _closedPeriodEndTime = LoadPeriodEndTime(CLOSED_KEY);

            CoroutineHelper.Start(CoInit());
        }

        void UpdatePeriodState()
        {
            DateTime curTime = DateTime.UtcNow;
            bool isOpen = Utils.CompareWithoutMilliseconds(_openPeriodEndTime, curTime) > 0;
            bool isClosed = Utils.CompareWithoutMilliseconds(_closedPeriodEndTime, curTime) > 0;
            if (!isOpen && !isClosed)
            {
                isOpen = true;
                ResetExpiredPeriod(curTime);
            }

            if (isOpen)
                SetOpenState(curTime);
            else if (isClosed)
                SetClosedState(curTime);
        }

        void ResetExpiredPeriod(DateTime curTime)
        {
            OnSubStateUpdated?.Invoke(EPeriodTimerSubState.OpenStart);
            _openPeriodEndTime = SavePeriodEndTime(OPEN_KEY, curTime.AddMinutes(OPEN_PERIOD_MINUTES));
            _closedPeriodEndTime = SavePeriodEndTime(CLOSED_KEY, _openPeriodEndTime.AddMinutes(CLOSED_PERIOD_MINUTES));
            PlayerPrefs.SetString(OPEN_START_KEY, _openPeriodEndTime.ToString());
        }

        void SetOpenState(DateTime curTime)
        {
            _state = EPeriodTimerState.Open;
            OnStateUpdated?.Invoke(EPeriodTimerState.Open);
            CoroutineHelper.Replace(ref _coUpdate, CoUpdate(curTime, _openPeriodEndTime));
        }

        void SetClosedState(DateTime curTime)
        {
            _state = EPeriodTimerState.Closed;
            OnStateUpdated?.Invoke(EPeriodTimerState.Closed);
            _openPeriodEndTime = SavePeriodEndTime(OPEN_KEY, DateTime.MinValue);
            CoroutineHelper.Replace(ref _coUpdate, CoUpdate(curTime, _closedPeriodEndTime));
        }

        IEnumerator CoInit()
        {
            yield return OnWait?.Invoke();
            UpdatePeriodState();
        }

        IEnumerator CoUpdate(DateTime curTime, DateTime periodEndTime)
        {
            int time = (int)Mathf.Ceil((float)(periodEndTime - curTime).TotalSeconds);
            while (time > 0)
            {
                OnLoopUpdate?.Invoke(time);
                yield return new WaitForSecondsRealtime(1.0f);

                if (--time <= 0)
                    break;
            }

            UpdatePeriodState();
        }

        DateTime SavePeriodEndTime(string key, DateTime time)
        {
            time = Utils.TrimMilliseconds(time);
            PlayerPrefs.SetString(key, time.ToString());
            return time;
        }

        DateTime LoadPeriodEndTime(string key)
        {
            if (PlayerPrefs.HasKey(key))
                return DateTime.Parse(PlayerPrefs.GetString(key));

            return SavePeriodEndTime(key, DateTime.MinValue);
        }
    }
}