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
        public TaskTimerPersistence(string id)
        {
            if(!TaskTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
                normalizedId = string.Empty;

            _id = normalizedId;
            _storage = new PlayerPrefsStorage();
        }

        //============================================================
        // Persistence
        //============================================================
        public void Save(DateTime startTime, double durationSec, ETaskTimerType stateType, DateTime updatedTime)
        {
            StorageValueUtils.SaveString(_storage, TaskTimerStorageKeys.Start(_id), startTime.Ticks.ToString());
            StorageValueUtils.SaveString(_storage, TaskTimerStorageKeys.Duration(_id), durationSec.ToString(CultureInfo.InvariantCulture));
            StorageValueUtils.SaveString(_storage, TaskTimerStorageKeys.State(_id), ((int)stateType).ToString());
            StorageValueUtils.SaveString(_storage, TaskTimerStorageKeys.Updated(_id), updatedTime.Ticks.ToString());
        }

        public void SaveDuration(double durationSec)
        {
            StorageValueUtils.SaveString(_storage, TaskTimerStorageKeys.Duration(_id), durationSec.ToString(CultureInfo.InvariantCulture));
        }

        public void SaveState(ETaskTimerType stateType)
        {
            StorageValueUtils.SaveString(_storage, TaskTimerStorageKeys.State(_id), ((int)stateType).ToString());
        }

        public void SaveUpdated(DateTime updatedTime)
        {
            StorageValueUtils.SaveString(_storage, TaskTimerStorageKeys.Updated(_id), updatedTime.Ticks.ToString());
        }

        public TaskTimerStorageSnapshot Load()
        {
            DateTime startTime = StorageValueUtils.TryLoadDate(_storage, TaskTimerStorageKeys.Start(_id));
            double durationSec = StorageValueUtils.TryLoadDouble(_storage, TaskTimerStorageKeys.Duration(_id));
            DateTime updatedTime = StorageValueUtils.TryLoadDate(_storage, TaskTimerStorageKeys.Updated(_id));
            int savedStateType = StorageValueUtils.TryLoadInt(_storage, TaskTimerStorageKeys.State(_id));
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
            bool hasStart = StorageValueUtils.HasKey(_storage, TaskTimerStorageKeys.Start(_id));
            bool hasDuration = StorageValueUtils.HasKey(_storage, TaskTimerStorageKeys.Duration(_id));
            bool hasState = StorageValueUtils.HasKey(_storage, TaskTimerStorageKeys.State(_id));
            bool hasUpdated = StorageValueUtils.HasKey(_storage, TaskTimerStorageKeys.Updated(_id));
            return !hasStart && !hasDuration && !hasState && hasUpdated;
        }
    }
}
