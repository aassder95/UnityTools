using System;
using UnityEngine;

namespace UnityTools.Util
{
    //============================================================
    //Logic
    //============================================================
    public static class RandomUtils
    {
        //============================================================
        //Constants
        //============================================================
        private const float HALF = 0.5f;

        //============================================================
        //Logic
        //============================================================
        public static Vector2 GetRandomPositionInRange(Vector2 center, Vector2 range)
        {
            float x = UnityEngine.Random.Range(center.x - range.x * HALF, center.x + range.x * HALF);
            float y = UnityEngine.Random.Range(center.y - range.y * HALF, center.y + range.y * HALF);
            return new Vector2(x, y);
        }
        
        public static Color GetRandomColor()
        {
            return new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f),UnityEngine.Random.Range(0.0f, 1.0f));
        }
    }

    public static class IndexUtils
    {
        //============================================================
        //Constants
        //============================================================
        private const float POSITION_EPSILON = 0.0001f;

        //============================================================
        //Logic
        //============================================================
        public static int CalculateClampedIndexFromPosition(float position, float itemSize, int lastIndex)
        {
            if(itemSize <= 0f)
                return 0;

            int index = Mathf.FloorToInt(position / itemSize + POSITION_EPSILON);
            int safeLastIndex = lastIndex < 0 ? 0 : lastIndex;
            if(index < 0)
                return 0;
            if(index > safeLastIndex)
                return safeLastIndex;

            return index;
        }
    }

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

    public static class StringTokenUtils
    {
        //============================================================
        //Utilities
        //============================================================
        public static string Normalize(string value, string fallback)
        {
            if(string.IsNullOrWhiteSpace(value))
                return fallback;

            return value.Trim();
        }

        public static string ToLogSafe(string value)
        {
            if(value == null)
                return "null";

            return value.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
        }
    }

    public static class TimerIdUtils
    {
        //============================================================
        //Utilities
        //============================================================
        public static bool TryNormalizeId(string rawId, out string normalizedId)
        {
            normalizedId = rawId?.Trim();
            return !string.IsNullOrEmpty(normalizedId);
        }
    }

}
