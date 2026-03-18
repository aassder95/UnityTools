using System;
using System.Globalization;

namespace UnityTools.Util
{
    public class TaskTimerPersistence
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly string _id;
        private readonly PlayerPrefsStorage _storage;

        //============================================================
        //Constructors
        //============================================================
        public TaskTimerPersistence(string id)
        {
            if(!TaskTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
                normalizedId = string.Empty;

            _id = normalizedId;
            _storage = new PlayerPrefsStorage();
        }

        //============================================================
        //Persistence
        //============================================================
        public void Save(DateTime startTime, double durationSec, ETaskTimerType stateType, DateTime updatedTime)
        {
            SaveString(TaskTimerStorageKeys.Start(_id), startTime.Ticks.ToString());
            SaveString(TaskTimerStorageKeys.Duration(_id), durationSec.ToString(CultureInfo.InvariantCulture));
            SaveString(TaskTimerStorageKeys.State(_id), ((int)stateType).ToString());
            SaveString(TaskTimerStorageKeys.Updated(_id), updatedTime.Ticks.ToString());
        }

        public void SaveDuration(double durationSec)
        {
            SaveString(TaskTimerStorageKeys.Duration(_id), durationSec.ToString(CultureInfo.InvariantCulture));
        }

        public void SaveState(ETaskTimerType stateType)
        {
            SaveString(TaskTimerStorageKeys.State(_id), ((int)stateType).ToString());
        }

        public void SaveUpdated(DateTime updatedTime)
        {
            SaveString(TaskTimerStorageKeys.Updated(_id), updatedTime.Ticks.ToString());
        }

        public TaskTimerStorageSnapshot Load()
        {
            DateTime startTime = TryLoadDate(TaskTimerStorageKeys.Start(_id));
            double durationSec = TryLoadDouble(TaskTimerStorageKeys.Duration(_id));
            DateTime updatedTime = TryLoadDate(TaskTimerStorageKeys.Updated(_id));
            int savedStateType = TryLoadInt(TaskTimerStorageKeys.State(_id));
            return new TaskTimerStorageSnapshot(startTime, durationSec, updatedTime, savedStateType);
        }

        public void ClearRuntimeData()
        {
            _storage.Delete(TaskTimerStorageKeys.Start(_id));
            _storage.Delete(TaskTimerStorageKeys.Duration(_id));
            _storage.Delete(TaskTimerStorageKeys.State(_id));
        }

        public bool IsClaimed()
        {
            bool hasStart = HasKey(TaskTimerStorageKeys.Start(_id));
            bool hasDuration = HasKey(TaskTimerStorageKeys.Duration(_id));
            bool hasState = HasKey(TaskTimerStorageKeys.State(_id));
            bool hasUpdated = HasKey(TaskTimerStorageKeys.Updated(_id));
            return !hasStart && !hasDuration && !hasState && hasUpdated;
        }

        //============================================================
        //Utilities
        //============================================================
        private void SaveString(string key, string value)
        {
            if(string.IsNullOrEmpty(key))
                return;

            _storage.Save(key, value ?? string.Empty);
        }

        private string LoadString(string key)
        {
            if(!HasKey(key))
                return string.Empty;

            return _storage.Load(key);
        }

        private bool HasKey(string key)
        {
            if(string.IsNullOrEmpty(key))
                return false;

            return _storage.HasKey(key);
        }

        private DateTime TryLoadDate(string key)
        {
            string raw = LoadString(key);
            if(!long.TryParse(raw, out long ticks))
                return DateTime.MinValue;

            if(ticks == DateTime.MinValue.Ticks)
                return DateTime.MinValue;
            if(ticks < DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Ticks)
                return DateTime.MinValue;

            return new DateTime(ticks, DateTimeKind.Utc);
        }

        private double TryLoadDouble(string key)
        {
            string raw = LoadString(key);
            bool isSuccess = double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) ||
                             double.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
            if(!isSuccess || double.IsNaN(value) || double.IsInfinity(value))
                return 0d;

            return Math.Max(0d, value);
        }

        private int TryLoadInt(string key)
        {
            string raw = LoadString(key);
            if(!int.TryParse(raw, out int value))
                return 0;

            return value;
        }
    }
}
