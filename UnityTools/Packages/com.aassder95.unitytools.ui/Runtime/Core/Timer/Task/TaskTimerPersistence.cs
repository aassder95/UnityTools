using System;
using System.Globalization;
using UnityTools.Util.Core.Persistence;

namespace UnityTools.Util.Core.Timer.Task
{
    public class TaskTimerPersistence
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly string _id;
        private readonly IStorage _storage;

        //============================================================
        // Constructors
        //============================================================
        public TaskTimerPersistence(string normalizedId, IStorage storage = null)
        {
            _id = normalizedId;
            _storage = storage ?? new PlayerPrefsStorage();
        }

        //============================================================
        // Persistence
        //============================================================
        public bool TrySave(DateTime startTime, double durationSec, ETaskTimerType stateType, DateTime updatedTime)
        {
            bool isStartSaved = StorageValueUtils.TrySaveString(_storage, TaskTimerStorageKeys.Start(_id), startTime.Ticks.ToString(CultureInfo.InvariantCulture));
            bool isDurationSaved = TrySaveDuration(durationSec);
            bool isStateSaved = TrySaveState(stateType);
            bool isUpdatedSaved = TrySaveUpdated(updatedTime);
            return isStartSaved && isDurationSaved && isStateSaved && isUpdatedSaved;
        }

        public bool TrySaveDuration(double durationSec)
        {
            return StorageValueUtils.TrySaveString(_storage, TaskTimerStorageKeys.Duration(_id), durationSec.ToString(CultureInfo.InvariantCulture));
        }

        public bool TrySaveState(ETaskTimerType stateType)
        {
            return StorageValueUtils.TrySaveString(_storage, TaskTimerStorageKeys.State(_id), ((int)stateType).ToString(CultureInfo.InvariantCulture));
        }

        public bool TrySaveUpdated(DateTime updatedTime)
        {
            return StorageValueUtils.TrySaveString(_storage, TaskTimerStorageKeys.Updated(_id), updatedTime.Ticks.ToString(CultureInfo.InvariantCulture));
        }

        public bool TryLoad(out TaskTimerStorageSnapshot snapshot)
        {
            bool isStartLoaded = StorageValueUtils.TryLoadDateOrDefault(_storage, TaskTimerStorageKeys.Start(_id), out DateTime startTime);
            bool isDurationLoaded = StorageValueUtils.TryLoadDoubleOrDefault(_storage, TaskTimerStorageKeys.Duration(_id), out double durationSec);
            bool isUpdatedLoaded = StorageValueUtils.TryLoadDateOrDefault(_storage, TaskTimerStorageKeys.Updated(_id), out DateTime updatedTime);
            bool isStateLoaded = StorageValueUtils.TryLoadIntOrDefault(_storage, TaskTimerStorageKeys.State(_id), out int savedStateType);
            snapshot = new TaskTimerStorageSnapshot(startTime, durationSec, updatedTime, savedStateType);
            return isStartLoaded && isDurationLoaded && isUpdatedLoaded && isStateLoaded;
        }

        public bool TryClearRuntimeData()
        {
            bool isStartDeleted = _storage.TryDelete(TaskTimerStorageKeys.Start(_id));
            bool isDurationDeleted = _storage.TryDelete(TaskTimerStorageKeys.Duration(_id));
            bool isStateDeleted = _storage.TryDelete(TaskTimerStorageKeys.State(_id));
            return isStartDeleted && isDurationDeleted && isStateDeleted;
        }

        public bool TryLoadClaimed(out bool isClaimed)
        {
            return TryLoadClaimed(_id, out isClaimed, _storage);
        }

        public static bool TryLoadClaimed(string normalizedId, out bool isClaimed, IStorage storage = null)
        {
            isClaimed = false;
            if(string.IsNullOrWhiteSpace(normalizedId))
                return false;

            IStorage targetStorage = storage ?? new PlayerPrefsStorage();
            bool isStartChecked = StorageValueUtils.TryHasKey(targetStorage, TaskTimerStorageKeys.Start(normalizedId), out bool hasStart);
            bool isDurationChecked = StorageValueUtils.TryHasKey(targetStorage, TaskTimerStorageKeys.Duration(normalizedId), out bool hasDuration);
            bool isStateChecked = StorageValueUtils.TryHasKey(targetStorage, TaskTimerStorageKeys.State(normalizedId), out bool hasState);
            bool isUpdatedChecked = StorageValueUtils.TryHasKey(targetStorage, TaskTimerStorageKeys.Updated(normalizedId), out bool hasUpdated);
            if(!isStartChecked || !isDurationChecked || !isStateChecked || !isUpdatedChecked)
                return false;

            isClaimed = !hasStart && !hasDuration && !hasState && hasUpdated;
            return true;
        }
    }
}
