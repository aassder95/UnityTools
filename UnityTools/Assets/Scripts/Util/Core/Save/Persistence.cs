using UnityEngine;

namespace UnityTools.Util
{
    //============================================================
    //Logic
    //============================================================
    public class Persistence
    {
        private readonly string ROOT_KEY;
        private readonly ISerializer SERIALIZER;
        private readonly IStorage STORAGE;

        public Persistence(string rootKey, ISerializer serializer = null, IStorage storage = null)
        {
            ROOT_KEY = rootKey;
            SERIALIZER = serializer ?? new JsonSerializer();
            STORAGE = storage ?? new PlayerPrefsStorage();
        }

        public void Save<T>(string suffix, T data)
        {
            string key = GetKey(suffix);
            string serialized;

            if (typeof(T) == typeof(string) || typeof(T).IsPrimitive || typeof(T).IsEnum)
            {
                serialized = data?.ToString() ?? "";
            }
            else
            {
                serialized = SERIALIZER.Serialize(data);
            }

            STORAGE.Save(key, serialized);
            PlayerPrefs.Save();
            Debug.Log($"[Persistence:Save] 저장 완료 key={key}, data={serialized}");
        }

        public T Load<T>(string suffix, T defaultValue = default)
        {
            string key = GetKey(suffix);

            if (!STORAGE.HasKey(key))
            {
                Debug.Log($"[Persistence:Load] 데이터가 없습니다. key={key}");
                return defaultValue;
            }

            string data = STORAGE.Load(key);
            if (string.IsNullOrEmpty(data) || data == "{}" || data == "[]")
            {
                Debug.Log($"[Persistence:Load] 데이터가 비어 있습니다. key={key}");
                return defaultValue;
            }

            T result;

            if (typeof(T) == typeof(string))
            {
                result = (T)(object)data;
            }
            else if (typeof(T).IsPrimitive || typeof(T).IsEnum)
            {
                result = (T)System.Convert.ChangeType(data, typeof(T));
            }
            else
            {
                result = SERIALIZER.Deserialize<T>(data);
            }

            Debug.Log($"[Persistence:Load] 로드 완료 key={key}, data={result}");
            return result;
        }

        public void Delete(string suffix)
        {
            string key = GetKey(suffix);
            STORAGE.Delete(key);
            Debug.Log($"[Persistence:Delete] 삭제 완료 key={key}");
        }

        public bool HasKey(string suffix)
        {
            string key = GetKey(suffix);
            return STORAGE.HasKey(key);
        }

        private string GetKey(string suffix)
        {
            return $"{ROOT_KEY}_{suffix}";
        }
    }
}
