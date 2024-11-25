using System;
using System.Collections;
using UnityEngine;

namespace UnityTools.Util
{
    public class PeriodTimer
    {
        readonly string OPEN_KEY = "";
        readonly string CLOSED_KEY = "";
        readonly double OPEN_PERIOD_MINUTES;
        readonly double CLOSED_PERIOD_MINUTES;

        DateTime _openPeriodEndTime = DateTime.MinValue;
        DateTime _closedPeriodEndTime = DateTime.MinValue;

        Coroutine _coCoroutine;
        public event Func<IEnumerator> OnWaitFunc;

        public PeriodTimer(string openKey, string closedkey, double openPeriodMin, double closedPeriodMin)
        {
            OPEN_KEY = openKey;
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

            Debug.Log($"[UpdatePeriodState] cur: {curTime}");
            Debug.Log($"[UpdatePeriodState] open: {_openPeriodEndTime}, closed: {_closedPeriodEndTime}");
            Debug.Log($"[UpdatePeriodState] open: {isOpen}, closed: {isClosed}");

            if (!isOpen && !isClosed)
            {
                Debug.Log($"[UpdatePeriodState] expired");

                isOpen = true;

                _openPeriodEndTime = SavePeriodEndTime(OPEN_KEY, curTime.AddMinutes(OPEN_PERIOD_MINUTES));
                _closedPeriodEndTime = SavePeriodEndTime(CLOSED_KEY, _openPeriodEndTime.AddMinutes(CLOSED_PERIOD_MINUTES));

                Debug.Log($"[UpdatePeriodState] open: {_openPeriodEndTime}, closed: {_closedPeriodEndTime}");
                Debug.Log($"[UpdatePeriodState] open: {isOpen}, closed: {isClosed}");
            }

            if (isOpen)
            {
                Debug.Log($"[UpdatePeriodState] open");

                CoroutineHelper.Replace(ref _coCoroutine, CoUpdate(curTime, _openPeriodEndTime));
            }
            else if (isClosed)
            {
                Debug.Log($"[UpdatePeriodState] closed");

                _openPeriodEndTime = SavePeriodEndTime(OPEN_KEY, DateTime.MinValue);
                CoroutineHelper.Replace(ref _coCoroutine, CoUpdate(curTime, _closedPeriodEndTime));
            }
        }

        IEnumerator CoInit()
        {
            Debug.Log("[CoInit] wait");

            yield return OnWaitFunc?.Invoke();

            Debug.Log("[CoInit] start");

            UpdatePeriodState();
        }

        IEnumerator CoUpdate(DateTime curTime, DateTime periodEndTime)
        {
            int time = (int)Mathf.Ceil((float)(periodEndTime - curTime).TotalMinutes);

            Debug.Log($"[CoUpdate] start: {time}");

            while (time > 0)
            {
                yield return new WaitForSecondsRealtime(60.0f);

                if (--time <= 0)
                    break;
            }

            Debug.Log($"[CoUpdate] end");

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
            DateTime time = DateTime.MinValue;

            if (PlayerPrefs.HasKey(key))
                time = DateTime.Parse(PlayerPrefs.GetString(key));
            else
                PlayerPrefs.SetString(key, time.ToString());

            Debug.Log($"[LoadPeriodEndTime] {key}: {time}");

            return time;
        }
    }
}