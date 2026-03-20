using UnityTools.Util.Core.Logging;

namespace UnityTools.Util.Core.Persistence
{
    public class Persistence
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly string _rootKey;
        private readonly ISerializer _serializer;
        private readonly IStorage _storage;

        //============================================================
        //Constructors
        //============================================================
        public Persistence(string rootKey, ISerializer serializer = null, IStorage storage = null)
        {
            _rootKey = rootKey;
            _serializer = serializer ?? new JsonSerializer();
            _storage = storage ?? new PlayerPrefsStorage();
        }

        //============================================================
        //Persistence
        //============================================================
        public void Save<T>(string suffix, T data)
        {
            string key = GetKey(suffix);
            string serialized;

            if(typeof(T) == typeof(string) || typeof(T).IsPrimitive || typeof(T).IsEnum)
                serialized = data?.ToString() ?? string.Empty;
            else
                serialized = _serializer.Serialize(data);

            _storage.Save(key, serialized);
            UnityEngine.PlayerPrefs.Save();
            DebugLogger.Log($"????꾨즺: ??{key}, 媛?{serialized}");
        }

        public T Load<T>(string suffix, T defaultValue = default)
        {
            string key = GetKey(suffix);
            if(!_storage.HasKey(key))
            {
                DebugLogger.Log($"?곗씠?곌? ?놁뒿?덈떎: ??{key}");
                return defaultValue;
            }

            string data = _storage.Load(key);
            if(string.IsNullOrEmpty(data) || data == "{}" || data == "[]")
            {
                DebugLogger.Log($"?곗씠?곌? 鍮꾩뼱 ?덉뒿?덈떎: ??{key}");
                return defaultValue;
            }

            T result;
            if(typeof(T) == typeof(string))
                result = (T)(object)data;
            else if(typeof(T).IsPrimitive || typeof(T).IsEnum)
                result = (T)System.Convert.ChangeType(data, typeof(T));
            else
                result = _serializer.Deserialize<T>(data);

            DebugLogger.Log($"濡쒕뱶 ?꾨즺: ??{key}, 媛?{result}");
            return result;
        }

        public void Delete(string suffix)
        {
            string key = GetKey(suffix);
            _storage.Delete(key);
            DebugLogger.Log($"??젣 ?꾨즺: ??{key}");
        }

        public bool HasKey(string suffix)
        {
            string key = GetKey(suffix);
            return _storage.HasKey(key);
        }

        //============================================================
        //Utilities
        //============================================================
        private string GetKey(string suffix)
        {
            return $"{_rootKey}_{suffix}";
        }
    }
}
