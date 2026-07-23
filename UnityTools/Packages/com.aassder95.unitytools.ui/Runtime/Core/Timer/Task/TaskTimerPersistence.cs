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
        // Properties
        //============================================================
        public bool IsClaimed => LoadClaimed(_id, _storage);

        //============================================================
        // Constructors
        //============================================================
        public TaskTimerPersistence(string normalizedId)
        {
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
            DateTime startTime = StorageValueUtils.LoadDateOrDefault(_storage, TaskTimerStorageKeys.Start(_id));
            double durationSec = StorageValueUtils.LoadDoubleOrDefault(_storage, TaskTimerStorageKeys.Duration(_id));
            DateTime updatedTime = StorageValueUtils.LoadDateOrDefault(_storage, TaskTimerStorageKeys.Updated(_id));
            int savedStateType = StorageValueUtils.LoadIntOrDefault(_storage, TaskTimerStorageKeys.State(_id));
            return new TaskTimerStorageSnapshot(startTime, durationSec, updatedTime, savedStateType);
        }

        public void ClearRuntimeData()
        {
            _storage.Delete(TaskTimerStorageKeys.Start(_id));
            _storage.Delete(TaskTimerStorageKeys.Duration(_id));
            _storage.Delete(TaskTimerStorageKeys.State(_id));
        }

        public static bool LoadClaimed(string normalizedId, IStorage storage = null)
        {
            IStorage targetStorage = storage ?? new PlayerPrefsStorage();
            bool hasStart = StorageValueUtils.HasKey(targetStorage, TaskTimerStorageKeys.Start(normalizedId));
            bool hasDuration = StorageValueUtils.HasKey(targetStorage, TaskTimerStorageKeys.Duration(normalizedId));
            bool hasState = StorageValueUtils.HasKey(targetStorage, TaskTimerStorageKeys.State(normalizedId));
            bool hasUpdated = StorageValueUtils.HasKey(targetStorage, TaskTimerStorageKeys.Updated(normalizedId));
            return !hasStart && !hasDuration && !hasState && hasUpdated;
        }
    }
}
