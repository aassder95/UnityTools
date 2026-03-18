using System;
using System.Globalization;

namespace UnityTools.Util
{
    public static class StorageValueUtils
    {
        //============================================================
        //Persistence
        //============================================================
        public static void SaveString(IStorage storage, string key, string value)
        {
            if(storage == null || string.IsNullOrEmpty(key))
                return;

            storage.Save(key, value ?? string.Empty);
        }

        public static bool HasKey(IStorage storage, string key)
        {
            if(storage == null || string.IsNullOrEmpty(key))
                return false;

            return storage.HasKey(key);
        }

        public static string LoadString(IStorage storage, string key)
        {
            if(!HasKey(storage, key))
                return string.Empty;

            return storage.Load(key) ?? string.Empty;
        }

        //============================================================
        //Utilities
        //============================================================
        public static DateTime TryLoadDate(IStorage storage, string key)
        {
            string raw = LoadString(storage, key);
            if(!long.TryParse(raw, out long ticks))
                return DateTime.MinValue;

            if(ticks == DateTime.MinValue.Ticks)
                return DateTime.MinValue;
            if(ticks < DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Ticks)
                return DateTime.MinValue;

            return new DateTime(ticks, DateTimeKind.Utc);
        }

        public static double TryLoadDouble(IStorage storage, string key)
        {
            string raw = LoadString(storage, key);
            bool isSuccess = double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) ||
                             double.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
            if(!isSuccess || double.IsNaN(value) || double.IsInfinity(value))
                return 0d;

            return Math.Max(0d, value);
        }

        public static int TryLoadInt(IStorage storage, string key)
        {
            string raw = LoadString(storage, key);
            if(!int.TryParse(raw, out int value))
                return 0;

            return value;
        }
    }
}
