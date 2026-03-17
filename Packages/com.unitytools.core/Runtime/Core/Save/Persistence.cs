using UnityEngine;

namespace UnityTools.Util
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
            PlayerPrefs.Save();
            Debug.Log($"[Persistence:Save] 저장 완료 key={key}, data={serialized}");
        }

        public T Load<T>(string suffix, T defaultValue = default)
        {
            string key = GetKey(suffix);
            if(!_storage.HasKey(key))
            {
                Debug.Log($"[Persistence:Load] 데이터가 없습니다. key={key}");
                return defaultValue;
            }

            string data = _storage.Load(key);
            if(string.IsNullOrEmpty(data) || data == "{}" || data == "[]")
            {
                Debug.Log($"[Persistence:Load] 데이터가 비어 있습니다. key={key}");
                return defaultValue;
            }

            T result;
            if(typeof(T) == typeof(string))
                result = (T)(object)data;
            else if(typeof(T).IsPrimitive || typeof(T).IsEnum)
                result = (T)System.Convert.ChangeType(data, typeof(T));
            else
                result = _serializer.Deserialize<T>(data);

            Debug.Log($"[Persistence:Load] 로드 완료 key={key}, data={result}");
            return result;
        }

        public void Delete(string suffix)
        {
            string key = GetKey(suffix);
            _storage.Delete(key);
            Debug.Log($"[Persistence:Delete] 삭제 완료 key={key}");
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
