using System;
using System.Globalization;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Util.Core.Persistence
{
    public static class StorageValueUtils
    {
        //============================================================
        // Persistence
        //============================================================
        public static void SaveString(IStorage storage, string key, string value)
        {
            if(storage == null || string.IsNullOrEmpty(key) || value == null)
            {
                DebugLogger.LogError("저장할 Storage, Key, Value가 유효하지 않습니다.");
                return;
            }

            storage.Save(key, value);
        }

        public static bool HasKey(IStorage storage, string key)
        {
            if(storage != null && !string.IsNullOrEmpty(key))
                return storage.HasKey(key);

            DebugLogger.LogError("조회할 Storage 또는 Key가 유효하지 않습니다.");
            return false;
        }

        public static string LoadString(IStorage storage, string key)
        {
            if(storage == null || string.IsNullOrEmpty(key))
            {
                DebugLogger.LogError("로드할 Storage 또는 Key가 유효하지 않습니다.");
                return string.Empty;
            }

            if(!storage.HasKey(key))
                return string.Empty;

            string value = storage.Load(key);
            if(value != null)
                return value;

            DebugLogger.LogError("Storage가 null 값을 반환했습니다. 키=" + key);
            return string.Empty;
        }

        public static DateTime LoadDateOrDefault(IStorage storage, string key)
        {
            string raw = LoadString(storage, key);
            if(string.IsNullOrEmpty(raw))
                return DateTime.MinValue;

            if(long.TryParse(raw, out long ticks) && ticks >= DateTime.MinValue.Ticks && ticks <= DateTime.MaxValue.Ticks)
                return new DateTime(ticks, DateTimeKind.Utc);

            DebugLogger.LogError("저장된 DateTime 값이 유효하지 않습니다. 키=" + key + ", 값=" + raw);
            return DateTime.MinValue;
        }

        public static double LoadDoubleOrDefault(IStorage storage, string key)
        {
            string raw = LoadString(storage, key);
            if(string.IsNullOrEmpty(raw))
                return 0d;

            bool isParsed = double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) || double.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
            if(isParsed && value >= 0d && !double.IsNaN(value) && !double.IsInfinity(value))
                return value;

            DebugLogger.LogError("저장된 double 값이 유효하지 않습니다. 키=" + key + ", 값=" + raw);
            return 0d;
        }

        public static int LoadIntOrDefault(IStorage storage, string key)
        {
            string raw = LoadString(storage, key);
            if(string.IsNullOrEmpty(raw))
                return 0;

            if(int.TryParse(raw, out int value))
                return value;

            DebugLogger.LogError("저장된 int 값이 유효하지 않습니다. 키=" + key + ", 값=" + raw);
            return 0;
        }
    }
}
