using System;
using System.Globalization;

namespace UnityTools.Timer.Persistence
{
    public static class StorageValueUtils
    {
        //============================================================
        // Persistence
        //============================================================
        public static bool TrySaveString(IStorage storage, string key, string value)
        {
            if (storage == null || string.IsNullOrWhiteSpace(key) || value == null)
                return false;

            return storage.TrySave(key, value);
        }

        public static bool TryHasKey(IStorage storage, string key, out bool hasKey)
        {
            hasKey = false;
            if (storage == null || string.IsNullOrWhiteSpace(key))
                return false;

            return storage.TryHasKey(key, out hasKey);
        }

        public static bool TryLoadStringOrDefault(IStorage storage, string key, out string value)
        {
            value = string.Empty;
            if (storage == null || string.IsNullOrWhiteSpace(key) || !TryLoadRawOrDefault(storage, key, out string raw))
                return false;

            value = raw ?? string.Empty;
            return true;
        }

        public static bool TryLoadDateOrDefault(IStorage storage, string key, out DateTime value)
        {
            value = DateTime.MinValue;
            if (storage == null || string.IsNullOrWhiteSpace(key) || !TryLoadRawOrDefault(storage, key, out string raw))
                return false;

            if (string.IsNullOrEmpty(raw))
                return true;

            if (long.TryParse(raw, out long ticks) && ticks >= DateTime.MinValue.Ticks && ticks <= DateTime.MaxValue.Ticks)
            {
                value = new DateTime(ticks, DateTimeKind.Utc);
                return true;
            }

            return false;
        }

        public static bool TryLoadDoubleOrDefault(IStorage storage, string key, out double value)
        {
            value = 0d;
            if (storage == null || string.IsNullOrWhiteSpace(key) || !TryLoadRawOrDefault(storage, key, out string raw))
                return false;

            if (string.IsNullOrEmpty(raw))
                return true;

            bool isParsed = double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsedValue) || double.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out parsedValue);
            if (!isParsed || parsedValue < 0d || double.IsNaN(parsedValue) || double.IsInfinity(parsedValue))
                return false;

            value = parsedValue;
            return true;
        }

        public static bool TryLoadIntOrDefault(IStorage storage, string key, out int value)
        {
            value = 0;
            if (storage == null || string.IsNullOrWhiteSpace(key) || !TryLoadRawOrDefault(storage, key, out string raw))
                return false;

            return string.IsNullOrEmpty(raw) || int.TryParse(raw, out value);
        }

        //============================================================
        // Utilities
        //============================================================
        private static bool TryLoadRawOrDefault(IStorage storage, string key, out string value)
        {
            value = null;
            if (!storage.TryHasKey(key, out bool hasKey))
                return false;

            return !hasKey || storage.TryLoad(key, out value);
        }
    }
}
