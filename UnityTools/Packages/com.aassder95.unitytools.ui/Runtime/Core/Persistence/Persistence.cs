using System;
using UnityTools.Util.Core.Logging;

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
            if(!_isValid)
                DebugLogger.LogError("Persistence Root Key가 비어 있습니다.");
        }

        //============================================================
        // Persistence
        //============================================================
        public void Save<T>(string suffix, T data)
        {
            if(!CanUseSuffix(suffix))
                return;

            string key = GetKey(suffix);
            try
            {
                string serialized;
                if(typeof(T) == typeof(string) || typeof(T).IsPrimitive || typeof(T).IsEnum)
                    serialized = data?.ToString() ?? string.Empty;
                else
                    serialized = _serializer.Serialize(data);

                _storage.Save(key, serialized);
                DebugLogger.Log($"저장 완료: 키={key}, 값={serialized}");
            }
            catch(Exception exception)
            {
                DebugLogger.LogError("데이터 저장에 실패했습니다. 키=" + key + ", 원인=" + exception.Message);
            }
        }

        public T Load<T>(string suffix, T defaultValue = default)
        {
            if(!CanUseSuffix(suffix))
                return defaultValue;

            string key = GetKey(suffix);
            try
            {
                if(!_storage.HasKey(key))
                {
                    DebugLogger.Log($"데이터가 없습니다: 키={key}");
                    return defaultValue;
                }

                string data = _storage.Load(key);
                if(string.IsNullOrEmpty(data) || data == "{}" || data == "[]")
                {
                    DebugLogger.Log($"데이터가 비어 있습니다: 키={key}");
                    return defaultValue;
                }

                T result;
                if(data is T typedData)
                    result = typedData;
                else if(typeof(T).IsEnum)
                    result = (T)Enum.Parse(typeof(T), data);
                else if(typeof(T).IsPrimitive)
                    result = (T)Convert.ChangeType(data, typeof(T));
                else
                    result = _serializer.Deserialize<T>(data);

                DebugLogger.Log($"로드 완료: 키={key}, 값={result}");
                return result;
            }
            catch(Exception exception)
            {
                DebugLogger.LogError("데이터 로드에 실패했습니다. 키=" + key + ", 원인=" + exception.Message);
                return defaultValue;
            }
        }

        public void Delete(string suffix)
        {
            if(!CanUseSuffix(suffix))
                return;

            string key = GetKey(suffix);
            try
            {
                _storage.Delete(key);
                DebugLogger.Log($"삭제 완료: 키={key}");
            }
            catch(Exception exception)
            {
                DebugLogger.LogError("데이터 삭제에 실패했습니다. 키=" + key + ", 원인=" + exception.Message);
            }
        }

        public bool HasKey(string suffix)
        {
            if(!CanUseSuffix(suffix))
                return false;

            try
            {
                return _storage.HasKey(GetKey(suffix));
            }
            catch(Exception exception)
            {
                DebugLogger.LogError("데이터 존재 여부 조회에 실패했습니다. 원인=" + exception.Message);
                return false;
            }
        }

        //============================================================
        // Utilities
        //============================================================
        private bool CanUseSuffix(string suffix)
        {
            if(_isValid && !string.IsNullOrWhiteSpace(suffix))
                return true;

            DebugLogger.LogError("Persistence가 유효하지 않거나 Suffix가 비어 있습니다. Root=" + (_rootKey ?? "null"));
            return false;
        }

        private string GetKey(string suffix)
        {
            return $"{_rootKey}_{suffix}";
        }
    }
}
