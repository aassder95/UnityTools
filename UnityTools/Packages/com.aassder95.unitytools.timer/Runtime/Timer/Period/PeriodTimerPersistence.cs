using System;
using System.Globalization;
using UnityTools.Timer.Persistence;

namespace UnityTools.Timer.Period
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
        public PeriodTimerPersistence(string normalizedId, IStorage storage = null)
        {
            _id = normalizedId;
            _storage = storage ?? new PlayerPrefsStorage();
        }

        //============================================================
        // Persistence
        //============================================================
        public bool TrySave(DateTime openEndTime, DateTime closedEndTime, DateTime openUpdatedTime, bool isTamperedFlag)
        {
            bool isOpenEndSaved = StorageValueUtils.TrySaveString(_storage, PeriodTimerStorageKeys.OpenEnd(_id), openEndTime.Ticks.ToString(CultureInfo.InvariantCulture));
            bool isClosedEndSaved = StorageValueUtils.TrySaveString(_storage, PeriodTimerStorageKeys.ClosedEnd(_id), closedEndTime.Ticks.ToString(CultureInfo.InvariantCulture));
            bool isOpenUpdatedSaved = StorageValueUtils.TrySaveString(_storage, PeriodTimerStorageKeys.OpenUpdated(_id), openUpdatedTime.Ticks.ToString(CultureInfo.InvariantCulture));
            bool isTamperedSaved = StorageValueUtils.TrySaveString(_storage, PeriodTimerStorageKeys.Tampered(_id), isTamperedFlag ? "1" : "0");
            return isOpenEndSaved && isClosedEndSaved && isOpenUpdatedSaved && isTamperedSaved;
        }

        public bool TryLoad(out PeriodTimerStorageSnapshot snapshot)
        {
            bool isOpenEndLoaded = StorageValueUtils.TryLoadDateOrDefault(_storage, PeriodTimerStorageKeys.OpenEnd(_id), out DateTime openEndTime);
            bool isClosedEndLoaded = StorageValueUtils.TryLoadDateOrDefault(_storage, PeriodTimerStorageKeys.ClosedEnd(_id), out DateTime closedEndTime);
            bool isOpenUpdatedLoaded = StorageValueUtils.TryLoadDateOrDefault(_storage, PeriodTimerStorageKeys.OpenUpdated(_id), out DateTime openUpdatedTime);
            bool isTamperedLoaded = StorageValueUtils.TryLoadStringOrDefault(_storage, PeriodTimerStorageKeys.Tampered(_id), out string rawTampered);
            snapshot = new PeriodTimerStorageSnapshot(openEndTime, closedEndTime, openUpdatedTime, rawTampered == "1");
            return isOpenEndLoaded && isClosedEndLoaded && isOpenUpdatedLoaded && isTamperedLoaded;
        }

        public static bool TryDeleteAll(string normalizedId, IStorage storage = null)
        {
            IStorage targetStorage = storage ?? new PlayerPrefsStorage();
            return PeriodTimerStorageKeys.TryDeleteAll(normalizedId, targetStorage);
        }
    }
}
