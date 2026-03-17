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
            int safeLastIndex = lastIndex < 0 ? 0 : lastIndex;
            if(index < 0)
                return 0;
            if(index > safeLastIndex)
                return safeLastIndex;

            return index;
        }

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

    public static class EnumUtils
    {
        public static int GetCount<T>() where T : Enum
        {
            return Enum.GetNames(typeof(T)).Length;
        }
    }
}
