using System.Collections.Generic;
using UnityTools.Timer.Persistence;

namespace UnityTools.TimerDashboard
{
    public class TimerDashboardStorage : IStorage
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly Dictionary<string, string> _values = new();

        //============================================================
        // Fields
        //============================================================
        private bool _canSave = true;

        //============================================================
        // Persistence
        //============================================================
        public bool TrySave(string key, string data)
        {
            if (!_canSave || string.IsNullOrWhiteSpace(key) || data == null)
                return false;

            _values[key] = data;
            return true;
        }

        public bool TryLoad(string key, out string data)
        {
            data = null;
            return !string.IsNullOrWhiteSpace(key) && _values.TryGetValue(key, out data);
        }

        public bool TryHasKey(string key, out bool hasKey)
        {
            hasKey = !string.IsNullOrWhiteSpace(key) && _values.ContainsKey(key);
            return !string.IsNullOrWhiteSpace(key);
        }

        public bool TryDelete(string key)
        {
            if (!_canSave || string.IsNullOrWhiteSpace(key))
                return false;

            _values.Remove(key);
            return true;
        }

        //============================================================
        // Logic
        //============================================================
        public void SetSaving(bool canSave)
        {
            _canSave = canSave;
        }
    }
}
