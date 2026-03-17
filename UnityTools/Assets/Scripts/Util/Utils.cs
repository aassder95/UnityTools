using System;
using UnityEngine;

namespace UnityTools.Util
{
    //============================================================
    //Logic
    //============================================================
    public static class RandomUtils
    {
        private const float HALF = 0.5f;
        
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
        private const float POSITION_EPSILON = 0.0001f;
        
        public static int ClampIndex(int index, int lastIndex)
        {
            return Mathf.Clamp(index, 0, Mathf.Max(0, lastIndex));
        }

        public static int CalculateClampedIndexFromPosition(float position, float itemSize, int lastIndex)
        {
            return ClampIndex(Mathf.FloorToInt(position / itemSize + POSITION_EPSILON), lastIndex);
        }
    }

    public static class DateTimeUtils
    {
        public static DateTime RemoveMilliseconds(DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, 
                              time.Hour, time.Minute, time.Second, time.Kind);
        }

        public static int CompareWithoutMilliseconds(DateTime first, DateTime second)
        {
            return RemoveMilliseconds(first).CompareTo(RemoveMilliseconds(second));
        }

        public static int GetRemainingMinutes(DateTime targetTime)
        {
            return Mathf.Max(0, Mathf.CeilToInt((float)CalculateTimeUntil(targetTime).TotalMinutes));
        }

        public static int GetRemainingSeconds(DateTime targetTime)
        {
            return Mathf.Max(0, Mathf.CeilToInt((float)CalculateTimeUntil(targetTime).TotalSeconds));
        }

        private static TimeSpan CalculateTimeUntil(DateTime targetTime)
        {
            return targetTime - RemoveMilliseconds(DateTime.UtcNow);
        }
    }

    public static class EnumUtils
    {
        public static int GetCount<T>() where T : Enum
        {
            return Enum.GetNames(typeof(T)).Length;
        }
    }
}
