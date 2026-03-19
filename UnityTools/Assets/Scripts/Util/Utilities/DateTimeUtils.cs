using System;
using UnityEngine;
using UnityTools.Util.Constants;
using UnityTools.Util.Core.Collections;
using UnityTools.Util.Core.Events;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.Core.Persistence;
using UnityTools.Util.Core.Pooling;
using UnityTools.Util.Core.Singleton;
using UnityTools.Util.Core.State;
using UnityTools.Util.Core.Timer.Period;
using UnityTools.Util.Core.Timer.Shared;
using UnityTools.Util.Core.Timer.Task;
using UnityTools.Util.Coroutines;
using UnityTools.Util.Extensions;
using UnityTools.Util.UIFramework;
using UnityTools.Util.Utilities;

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
