using System;
using UnityEngine;

namespace UnityTools.Util
{
    public static class Utils
    {
        public static Vector2 GetRandomPos(Vector2 center, Vector2 range)
        {
            float x = UnityEngine.Random.Range(center.x - range.x * 0.5f, center.x + range.x * 0.5f);
            float y = UnityEngine.Random.Range(center.y - range.y * 0.5f, center.y + range.y * 0.5f);
            return new Vector2(x, y);
        }

        public static int ClampIndex(int idx, int lastIdx)
        {
            return Mathf.Clamp(idx, 0, Mathf.Max(0, lastIdx));
        }

        public static int ClampIndexFromPosition(float pos, float itemSize, int lastIdx)
        {
            return ClampIndex(Mathf.FloorToInt(pos / itemSize + 0.0001f), lastIdx);
        }

        public static DateTime TrimMilliseconds(DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, time.Kind);
        }

        public static int CompareWithoutMilliseconds(DateTime dt1, DateTime dt2)
        {
            return TrimMilliseconds(dt1).CompareTo(TrimMilliseconds(dt2));
        }

        public static int GetEnumLength<T>() where T : Enum
        {
            return Enum.GetNames(typeof(T)).Length;
        }

        public static int GetRemainingMinutes(DateTime time)
        {
            return Mathf.Max(0, Mathf.CeilToInt((float)CalculateTimeSpan(time).TotalMinutes));
        }

        public static int GetRemainingSeconds(DateTime time)
        {
            return Mathf.Max(0, Mathf.CeilToInt((float)CalculateTimeSpan(time).TotalSeconds));
        }

        public static TimeSpan CalculateTimeSpan(DateTime time)
        {
            return time - TrimMilliseconds(DateTime.UtcNow);
        }
    }
}