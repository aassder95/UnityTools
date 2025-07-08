using System;
using UnityEngine;

namespace UnityTools.Util
{
    public class Persistence
    {
        private readonly string ROOT_KEY;

        public Persistence(string key)
        {
            ROOT_KEY = key;
        }

        public void Save(string suffix, DateTime time)
        {
            PlayerPrefs.SetString(GetKey(suffix), time.ToString());
        }

        public DateTime Load(string suffix)
        {
            if (PlayerPrefs.HasKey(GetKey(suffix)))
            {
                if (DateTime.TryParse(PlayerPrefs.GetString(GetKey(suffix)), out DateTime time))
                    return time;
            }

            return DateTime.MinValue;
        }

        private string GetKey(string suffix)
        {
            return ROOT_KEY + "_" + suffix;
        }
    }
}