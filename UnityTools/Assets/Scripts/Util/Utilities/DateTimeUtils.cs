using System;
using UnityEngine;

namespace UnityTools.Util.Utilities
{
    // Exception: stateless utility is kept as a static helper.
    public static class DateTimeUtils
    {
        //============================================================
        //Logic
        //============================================================
        public static DateTime RemoveMilliseconds(DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day,
                              time.Hour, time.Minute, time.Second, time.Kind);
        }

        public static int CompareWithoutMilliseconds(DateTime first, DateTime second)
        {
            DateTime firstWithoutMilliseconds = RemoveMilliseconds(first);
            DateTime secondWithoutMilliseconds = RemoveMilliseconds(second);
            if(firstWithoutMilliseconds < secondWithoutMilliseconds)
                return -1;
            if(firstWithoutMilliseconds > secondWithoutMilliseconds)
                return 1;

            return 0;
        }

        public static int GetRemainingMinutes(DateTime targetTime)
        {
            TimeSpan remainingTime = targetTime - RemoveMilliseconds(DateTime.UtcNow);
            if(remainingTime.TotalMinutes <= 0d)
                return 0;

            return Mathf.CeilToInt((float)remainingTime.TotalMinutes);
        }

        public static int GetRemainingSeconds(DateTime targetTime)
        {
            TimeSpan remainingTime = targetTime - RemoveMilliseconds(DateTime.UtcNow);
            if(remainingTime.TotalSeconds <= 0d)
                return 0;

            return Mathf.CeilToInt((float)remainingTime.TotalSeconds);
        }
    }
}
