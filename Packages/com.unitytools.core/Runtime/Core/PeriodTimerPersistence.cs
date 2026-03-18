using System;

namespace UnityTools.Util
{
    public class PeriodTimerPersistence
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly string _id;
        private readonly IStorage _storage;

        //============================================================
        //Constructors
        //============================================================
        public PeriodTimerPersistence(string id)
        {
            if(!PeriodTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
                normalizedId = string.Empty;

            _id = normalizedId;
            _storage = new PlayerPrefsStorage();
        }

        //============================================================
        //Persistence
        //============================================================
        public void Save(DateTime openEndTime, DateTime closedEndTime, DateTime openUpdatedTime, bool isTamperedFlag)
        {
            SaveString(PeriodTimerStorageKeys.OpenEnd(_id), openEndTime.Ticks.ToString());
            SaveString(PeriodTimerStorageKeys.ClosedEnd(_id), closedEndTime.Ticks.ToString());
            SaveString(PeriodTimerStorageKeys.OpenUpdated(_id), openUpdatedTime.Ticks.ToString());
            SaveString(PeriodTimerStorageKeys.Tampered(_id), isTamperedFlag ? "1" : "0");
        }

        public PeriodTimerStorageSnapshot Load()
        {
            DateTime openEndTime = TryLoadDate(PeriodTimerStorageKeys.OpenEnd(_id));
            DateTime closedEndTime = TryLoadDate(PeriodTimerStorageKeys.ClosedEnd(_id));
            DateTime openUpdatedTime = TryLoadDate(PeriodTimerStorageKeys.OpenUpdated(_id));
            bool isTamperedFlag = LoadString(PeriodTimerStorageKeys.Tampered(_id)) == "1";
            return new PeriodTimerStorageSnapshot(openEndTime, closedEndTime, openUpdatedTime, isTamperedFlag);
        }

        public static void DeleteAll(string id, IStorage storage = null)
        {
            IStorage targetStorage = storage ?? new PlayerPrefsStorage();
            PeriodTimerStorageKeys.DeleteAll(id, targetStorage);
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
            if(string.IsNullOrEmpty(key) || !_storage.HasKey(key))
                return string.Empty;

            return _storage.Load(key);
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
    }
}
