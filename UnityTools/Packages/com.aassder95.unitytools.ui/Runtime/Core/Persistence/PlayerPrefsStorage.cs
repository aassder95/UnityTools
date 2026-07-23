using UnityEngine;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Util.Core.Persistence
{
    public class PlayerPrefsStorage : IStorage
    {
        //============================================================
        // Persistence
        //============================================================
        public void Save(string key, string data)
        {
            if(string.IsNullOrEmpty(key) || data == null)
            {
                DebugLogger.LogError("PlayerPrefs에 저장할 Key 또는 Data가 유효하지 않습니다.");
                return;
            }

            PlayerPrefs.SetString(key, data);
            PlayerPrefs.Save();
        }

        public string Load(string key)
        {
            if(string.IsNullOrEmpty(key))
            {
                DebugLogger.LogError("PlayerPrefs에서 로드할 Key가 비어 있습니다.");
                return string.Empty;
            }

            return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : string.Empty;
        }

        public bool HasKey(string key)
        {
            if(!string.IsNullOrEmpty(key))
                return PlayerPrefs.HasKey(key);

            DebugLogger.LogError("PlayerPrefs에서 조회할 Key가 비어 있습니다.");
            return false;
        }

        public void Delete(string key)
        {
            if(string.IsNullOrEmpty(key))
            {
                DebugLogger.LogError("PlayerPrefs에서 삭제할 Key가 비어 있습니다.");
                return;
            }

            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }
    }
}
