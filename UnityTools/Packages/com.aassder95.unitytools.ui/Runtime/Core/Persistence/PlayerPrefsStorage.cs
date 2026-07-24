using System;
using UnityEngine;

namespace UnityTools.Util.Core.Persistence
{
    public class PlayerPrefsStorage : IStorage
    {
        //============================================================
        // Persistence
        //============================================================
        public bool TrySave(string key, string data)
        {
            if (string.IsNullOrWhiteSpace(key) || data == null)
                return false;

            try
            {
                PlayerPrefs.SetString(key, data);
                PlayerPrefs.Save();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool TryLoad(string key, out string data)
        {
            data = null;
            if (string.IsNullOrWhiteSpace(key))
                return false;

            try
            {
                if (!PlayerPrefs.HasKey(key))
                    return false;

                data = PlayerPrefs.GetString(key);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool TryHasKey(string key, out bool hasKey)
        {
            hasKey = false;
            if (string.IsNullOrWhiteSpace(key))
                return false;

            try
            {
                hasKey = PlayerPrefs.HasKey(key);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool TryDelete(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            try
            {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
