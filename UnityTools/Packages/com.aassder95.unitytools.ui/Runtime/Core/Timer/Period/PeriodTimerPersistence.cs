using System;
using UnityTools.Util.Core.Persistence;

namespace UnityTools.Util.Core.Timer.Period
{
    public class PeriodTimerPersistence
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly string _id;
        private readonly IStorage _storage;

        //============================================================
        // Constructors
        //============================================================
        public PeriodTimerPersistence(string id)
        {
            if(!PeriodTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
                normalizedId = string.Empty;

            _id = normalizedId;
            _storage = new PlayerPrefsStorage();
        }

        //============================================================
        // Persistence
        //============================================================
        public void Save(DateTime openEndTime, DateTime closedEndTime, DateTime openUpdatedTime, bool isTamperedFlag)
        {
            StorageValueUtils.SaveString(_storage, PeriodTimerStorageKeys.OpenEnd(_id), openEndTime.Ticks.ToString());
            StorageValueUtils.SaveString(_storage, PeriodTimerStorageKeys.ClosedEnd(_id), closedEndTime.Ticks.ToString());
            StorageValueUtils.SaveString(_storage, PeriodTimerStorageKeys.OpenUpdated(_id), openUpdatedTime.Ticks.ToString());
            StorageValueUtils.SaveString(_storage, PeriodTimerStorageKeys.Tampered(_id), isTamperedFlag ? "1" : "0");
        }

        public PeriodTimerStorageSnapshot Load()
        {
            DateTime openEndTime = StorageValueUtils.TryLoadDate(_storage, PeriodTimerStorageKeys.OpenEnd(_id));
            DateTime closedEndTime = StorageValueUtils.TryLoadDate(_storage, PeriodTimerStorageKeys.ClosedEnd(_id));
            DateTime openUpdatedTime = StorageValueUtils.TryLoadDate(_storage, PeriodTimerStorageKeys.OpenUpdated(_id));
            bool isTamperedFlag = StorageValueUtils.LoadString(_storage, PeriodTimerStorageKeys.Tampered(_id)) == "1";
            return new PeriodTimerStorageSnapshot(openEndTime, closedEndTime, openUpdatedTime, isTamperedFlag);
        }

        public static void DeleteAll(string id, IStorage storage = null)
        {
            IStorage targetStorage = storage ?? new PlayerPrefsStorage();
            PeriodTimerStorageKeys.DeleteAll(id, targetStorage);
        }
    }
}
