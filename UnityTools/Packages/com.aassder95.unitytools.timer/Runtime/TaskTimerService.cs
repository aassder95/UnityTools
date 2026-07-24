using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityTools.Timer.Persistence;
using UnityTools.Timer.Task;

namespace UnityTools.Timer
{
    public class TaskTimerService
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly MonoBehaviour _runner;
        private readonly IStorage _storage;
        private readonly Func<DateTime> _utcNow;
        private readonly Dictionary<string, TaskTimerHandle> _handles = new();
        private readonly Dictionary<string, TaskTimerEventBinder> _eventBinders = new();

        //============================================================
        // Events
        //============================================================
        public event UnityAction<TaskTimerData> OnRemainSecUpdated { add => _onRemainSecUpdated += value; remove => _onRemainSecUpdated -= value; }
        public event UnityAction<TaskTimerData> OnCompleted { add => _onCompleted += value; remove => _onCompleted -= value; }
        public event UnityAction<TaskTimerData> OnClaimed { add => _onClaimed += value; remove => _onClaimed -= value; }
        private event UnityAction<TaskTimerData> _onRemainSecUpdated;
        private event UnityAction<TaskTimerData> _onCompleted;
        private event UnityAction<TaskTimerData> _onClaimed;

        //============================================================
        // Constructors
        //============================================================
        public TaskTimerService(MonoBehaviour runner, IStorage storage = null, Func<DateTime> utcNow = null)
        {
            _runner = runner;
            _storage = storage ?? new PlayerPrefsStorage();
            _utcNow = utcNow;
        }

        //============================================================
        // Init/Register
        //============================================================
        public bool TryCreate(string id, out TaskTimerHandle handle)
        {
            handle = null;
            if (!TryNormalizeId(id, out string normalizedId))
                return false;

            if (!TaskTimer.TryCreate(normalizedId, _runner, out TaskTimer timer, _storage, _utcNow))
                return false;

            handle = new TaskTimerHandle(timer);
            return true;
        }

        public bool TryInit(TaskTimerHandle handle)
        {
            if (handle == null || !TryNormalizeId(handle.Id, out string normalizedId))
                return false;

            if (!handle.TryInit())
                return false;

            if (_handles.TryGetValue(normalizedId, out TaskTimerHandle oldHandle))
            {
                UnbindEvents(normalizedId, oldHandle);
                if (oldHandle != handle)
                    oldHandle.Release();
            }

            _handles[normalizedId] = handle;
            BindEvents(normalizedId, handle);
            return true;
        }

        public void Release()
        {
            string[] ids = new string[_eventBinders.Count];
            _eventBinders.Keys.CopyTo(ids, 0);
            for (int i = 0; i < ids.Length; i++)
            {
                UnbindEvents(ids[i], _handles[ids[i]]);
            }

            foreach (TaskTimerHandle handle in _handles.Values)
            {
                handle.Release();
            }

            _handles.Clear();
            _eventBinders.Clear();
        }

        //============================================================
        // Logic
        //============================================================
        public bool TryStart(string id, double durationSec)
        {
            return TryGetHandle(id, out TaskTimerHandle handle) && handle.TryStart(durationSec);
        }

        public bool TryReduce(string id, double reduceSec)
        {
            return TryGetHandle(id, out TaskTimerHandle handle) && handle.TryReduce(reduceSec);
        }

        public bool TryComplete(string id)
        {
            return TryGetHandle(id, out TaskTimerHandle handle) && handle.TryComplete();
        }

        public bool TryClaim(string id)
        {
            return TryGetHandle(id, out TaskTimerHandle handle) && handle.TryClaim();
        }

        public bool TryGetClaimed(string id, out bool isClaimed)
        {
            isClaimed = false;
            if (!TryNormalizeId(id, out string normalizedId))
                return false;

            if (_handles.TryGetValue(normalizedId, out TaskTimerHandle handle))
                return handle.TryGetClaimed(out isClaimed);

            return TaskTimerPersistence.TryLoadClaimed(normalizedId, out isClaimed, _storage);
        }

        public bool TryGetHandle(string id, out TaskTimerHandle handle)
        {
            handle = null;
            return TryNormalizeId(id, out string normalizedId) && _handles.TryGetValue(normalizedId, out handle);
        }

        private void BindEvents(string id, TaskTimerHandle handle)
        {
            TaskTimerEventBinder eventBinder = new(handle, OnRemainSecUpdatedCallback, OnCompletedCallback, OnClaimedCallback, OnStateTransitionCallback);
            _eventBinders[id] = eventBinder;
            handle.OnRemainSecUpdated += eventBinder.OnRemainSecUpdatedCallback;
            handle.OnCompleted += eventBinder.OnCompletedCallback;
            handle.OnClaimed += eventBinder.OnClaimedCallback;
            handle.OnStateTransition += eventBinder.OnStateTransitionCallback;
        }

        private void UnbindEvents(string id, TaskTimerHandle handle)
        {
            TaskTimerEventBinder eventBinder = _eventBinders[id];
            handle.OnRemainSecUpdated -= eventBinder.OnRemainSecUpdatedCallback;
            handle.OnCompleted -= eventBinder.OnCompletedCallback;
            handle.OnClaimed -= eventBinder.OnClaimedCallback;
            handle.OnStateTransition -= eventBinder.OnStateTransitionCallback;
            _eventBinders.Remove(id);
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OnRemainSecUpdatedCallback(TaskTimerHandle handle, int remainSec)
        {
            if (handle.RemainingSec != remainSec)
                return;

            _onRemainSecUpdated?.Invoke(handle.ToData());
        }

        private void OnCompletedCallback(TaskTimerHandle handle)
        {
            _onCompleted?.Invoke(handle.ToData());
        }

        private void OnClaimedCallback(TaskTimerHandle handle)
        {
            _onClaimed?.Invoke(handle.ToData());
        }

        private void OnStateTransitionCallback(TaskTimerHandle handle, ETaskTimerType prevType, ETaskTimerType nextType)
        {
            if (prevType == nextType || nextType != ETaskTimerType.Processing)
                return;

            _onRemainSecUpdated?.Invoke(handle.ToData());
        }

        //============================================================
        // Utilities
        //============================================================
        private static bool TryNormalizeId(string id, out string normalizedId)
        {
            normalizedId = id?.Trim();
            return !string.IsNullOrEmpty(normalizedId);
        }

        //============================================================
        // Nested Types
        //============================================================
        private class TaskTimerEventBinder
        {
            //============================================================
            // Readonly
            //============================================================
            private readonly TaskTimerHandle _handle;
            private readonly Action<TaskTimerHandle, int> _onRemainSecUpdated;
            private readonly Action<TaskTimerHandle> _onCompleted;
            private readonly Action<TaskTimerHandle> _onClaimed;
            private readonly Action<TaskTimerHandle, ETaskTimerType, ETaskTimerType> _onStateTransition;

            //============================================================
            // Constructors
            //============================================================
            public TaskTimerEventBinder(TaskTimerHandle handle, Action<TaskTimerHandle, int> onRemainSecUpdated, Action<TaskTimerHandle> onCompleted, Action<TaskTimerHandle> onClaimed, Action<TaskTimerHandle, ETaskTimerType, ETaskTimerType> onStateTransition)
            {
                _handle = handle;
                _onRemainSecUpdated = onRemainSecUpdated;
                _onCompleted = onCompleted;
                _onClaimed = onClaimed;
                _onStateTransition = onStateTransition;
            }

            //============================================================
            // Callbacks
            //============================================================
            public void OnRemainSecUpdatedCallback(int remainSec)
            {
                _onRemainSecUpdated.Invoke(_handle, remainSec);
            }

            public void OnCompletedCallback()
            {
                _onCompleted.Invoke(_handle);
            }

            public void OnClaimedCallback()
            {
                _onClaimed.Invoke(_handle);
            }

            public void OnStateTransitionCallback(ETaskTimerType prevType, ETaskTimerType nextType)
            {
                _onStateTransition.Invoke(_handle, prevType, nextType);
            }
        }
    }
}
