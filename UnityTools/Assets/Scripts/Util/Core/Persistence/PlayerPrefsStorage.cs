using UnityEngine;

namespace UnityTools.Util.Core.Persistence
{
    public class PlayerPrefsStorage : IStorage
    {
        //============================================================
        //Persistence
        //============================================================
        public void Save(string key, string data)
        {
            PlayerPrefs.SetString(key, data);
            PlayerPrefs.Save();
        }

        public string Load(string key)
        {
            if(string.IsNullOrEmpty(key) || !PlayerPrefs.HasKey(key))
                return string.Empty;

            return PlayerPrefs.GetString(key);
        }

        public bool HasKey(string key)
        {
            if(string.IsNullOrEmpty(key))
                return false;

            return PlayerPrefs.HasKey(key);
        }

        public void Delete(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }
    }
}
