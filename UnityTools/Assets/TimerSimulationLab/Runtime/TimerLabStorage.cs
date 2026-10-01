using System.Collections.Generic;
using System.Text;
using UnityTools.Timer.Persistence;

namespace UnityTools.Timer.Samples
{
    public class TimerLabStorage : IStorage
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly SortedDictionary<string, string> _entries = new SortedDictionary<string, string>(System.StringComparer.Ordinal);

        //============================================================
        // Fields
        //============================================================
        private bool _canSave = true;
        private int _saveCnt;
        private int _rejectedSaveCnt;

        //============================================================
        // Properties
        //============================================================
        public int SaveCnt => _saveCnt;
        public int RejectedSaveCnt => _rejectedSaveCnt;

        //============================================================
        // Persistence
        //============================================================
        public bool TrySave(string key, string data)
        {
            if (string.IsNullOrEmpty(key) || data == null)
                return false;

            if (!_canSave)
            {
                ++_rejectedSaveCnt;
                return false;
            }

            _entries[key] = data;
            ++_saveCnt;
            return true;
        }

        public bool TryLoad(string key, out string data)
        {
            data = null;
            return !string.IsNullOrEmpty(key) && _entries.TryGetValue(key, out data);
        }
        public bool TryHasKey(string key, out bool hasKey)
        {
            hasKey = !string.IsNullOrEmpty(key) && _entries.ContainsKey(key);
            if (string.IsNullOrEmpty(key))
                return false;

            return true;
        }

        public bool TryDelete(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            _entries.Remove(key);
            return true;
        }

        //============================================================
        // Logic
        //============================================================
        public void SetSaving(bool canSave)
        {
            _canSave = canSave;
        }

        public string Capture()
        {
            if (_entries.Count == 0)
                return "<empty>";

            StringBuilder text = new StringBuilder();
            foreach (KeyValuePair<string, string> entry in _entries)
            {
                text.Append(entry.Key).Append("\n").Append(entry.Value).Append("\n");
            }

            return text.ToString();
        }
    }
}
