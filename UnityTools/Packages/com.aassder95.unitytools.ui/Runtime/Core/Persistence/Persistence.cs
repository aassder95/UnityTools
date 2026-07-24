using System;

namespace UnityTools.Util.Core.Persistence
{
    public class Persistence
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly string _rootKey;
        private readonly ISerializer _serializer;
        private readonly IStorage _storage;
        private readonly bool _isValid;

        //============================================================
        // Constructors
        //============================================================
        public Persistence(string rootKey, ISerializer serializer = null, IStorage storage = null)
        {
            _rootKey = rootKey;
            _serializer = serializer ?? new JsonSerializer();
            _storage = storage ?? new PlayerPrefsStorage();
            _isValid = !string.IsNullOrWhiteSpace(rootKey);
        }

        //============================================================
        // Persistence
        //============================================================
        public bool TrySave<T>(string suffix, T data)
        {
            if (!CanUseSuffix(suffix))
                return false;

            try
            {
                string serialized;
                if (typeof(T) == typeof(string) || typeof(T).IsPrimitive || typeof(T).IsEnum)
                    serialized = data?.ToString() ?? string.Empty;
                else
                    serialized = _serializer.Serialize(data);

                return _storage.TrySave(GetKey(suffix), serialized);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool TryLoad<T>(string suffix, out T value, T defaultValue = default)
        {
            value = defaultValue;
            if (!CanUseSuffix(suffix))
                return false;

            try
            {
                string key = GetKey(suffix);
                if (!_storage.TryHasKey(key, out bool hasKey))
                    return false;

                if (!hasKey)
                    return true;

                if (!_storage.TryLoad(key, out string data))
                    return false;

                if (string.IsNullOrEmpty(data) || data == "{}" || data == "[]")
                    return true;

                if (data is T typedData)
                {
                    value = typedData;
                    return true;
                }

                if (typeof(T).IsEnum)
                {
                    if (!Enum.TryParse(typeof(T), data, out object enumValue))
                        return false;

                    value = (T)enumValue;
                    return true;
                }

                value = typeof(T).IsPrimitive ? (T)Convert.ChangeType(data, typeof(T)) : _serializer.Deserialize<T>(data);
                return true;
            }
            catch (Exception)
            {
                value = defaultValue;
                return false;
            }
        }

        public bool TryDelete(string suffix)
        {
            if (!CanUseSuffix(suffix))
                return false;

            try
            {
                return _storage.TryDelete(GetKey(suffix));
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool TryHasKey(string suffix, out bool hasKey)
        {
            hasKey = false;
            if (!CanUseSuffix(suffix))
                return false;

            try
            {
                return _storage.TryHasKey(GetKey(suffix), out hasKey);
            }
            catch (Exception)
            {
                return false;
            }
        }

        //============================================================
        // Utilities
        //============================================================
        private bool CanUseSuffix(string suffix)
        {
            return _isValid && !string.IsNullOrWhiteSpace(suffix);
        }

        private string GetKey(string suffix)
        {
            return $"{_rootKey}_{suffix}";
        }
    }
}
