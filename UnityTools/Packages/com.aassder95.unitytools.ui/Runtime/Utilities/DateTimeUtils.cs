using System;
using UnityEngine;

namespace UnityTools.Util.Utilities
{
    public static class DateTimeUtils
    {
        //============================================================
        // Logic
        //============================================================
        public static DateTime RemoveMs(DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, time.Kind);
        }

        public static int CompareWithoutMs(DateTime first, DateTime second)
        {
            DateTime firstWithoutMs = RemoveMs(first);
            DateTime secondWithoutMs = RemoveMs(second);
            if(firstWithoutMs < secondWithoutMs)
                return -1;
            if(firstWithoutMs > secondWithoutMs)
                return 1;

            return 0;
        }

        public static int GetRemainingMin(DateTime targetTime)
        {
            return GetRemainingMin(targetTime, DateTime.UtcNow);
        }

        public static int GetRemainingMin(DateTime targetTime, DateTime utcNow)
        {
            TimeSpan remainingTime = targetTime - RemoveMs(utcNow);
            if(remainingTime.TotalMinutes <= 0d)
                return 0;

            return Mathf.CeilToInt((float)remainingTime.TotalMinutes);
        }

        public static int GetRemainingSec(DateTime targetTime)
        {
            return GetRemainingSec(targetTime, DateTime.UtcNow);
        }

        public static int GetRemainingSec(DateTime targetTime, DateTime utcNow)
        {
            TimeSpan remainingTime = targetTime - RemoveMs(utcNow);
            if(remainingTime.TotalSeconds <= 0d)
                return 0;

            return Mathf.CeilToInt((float)remainingTime.TotalSeconds);
        }
    }
}
