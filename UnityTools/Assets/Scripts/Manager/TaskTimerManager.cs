using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityTools.Util;

namespace UnityTools.Manager
{
    public class TaskTimerManager : MonoSingleton<TaskTimerManager>
    {
        //============================================================
        //Constants
        //============================================================
        private const double SEC_PER_MIN = 60d;

        //============================================================
        //Readonly
        //============================================================
        private readonly Dictionary<string, TaskTimerHandle> _handles = new();
        private readonly Dictionary<string, TaskTimerEventBinder> _eventBinders = new();

        //============================================================
        //Inspector Fields
        //============================================================
        [SerializeField] private bool _isEnableLog;

        //============================================================
        //Events
        //============================================================
        public event UnityAction<TaskTimerData> OnAnyTimerUpdated { add => _onAnyTimerUpdated += value; remove => _onAnyTimerUpdated -= value; }
        public event UnityAction<TaskTimerData> OnAnyTimerCompleted { add => _onAnyTimerCompleted += value; remove => _onAnyTimerCompleted -= value; }
        public event UnityAction<TaskTimerData> OnAnyTimerClaimed { add => _onAnyTimerClaimed += value; remove => _onAnyTimerClaimed -= value; }
        private event UnityAction<TaskTimerData> _onAnyTimerUpdated;
        private event UnityAction<TaskTimerData> _onAnyTimerCompleted;
        private event UnityAction<TaskTimerData> _onAnyTimerClaimed;

        //============================================================
        //Unity Methods
        //============================================================
        private void Awake()
        {
            Singletons.TaskTimerManager ??= Instance;
        }

        private void OnDestroy()
        {
            foreach (KeyValuePair<string, TaskTimerHandle> pair in _handles)
            {
                UnbindEvents(pair.Key, pair.Value);
                pair.Value.Release();
            }

            _eventBinders.Clear();
            _handles.Clear();
        }

        //============================================================
        //Init/Register
        //============================================================
        public void InitTimer(TaskTimerHandle handle)
        {
            if(handle == null)
                return;

            if(!TaskTimerStorageKeys.TryNormalizeId(handle.Id, out string id))
            {
                LogInvalidId(nameof(InitTimer), handle.Id);
                return;
            }

            if(_handles.TryGetValue(id, out TaskTimerHandle oldHandle))
            {
                UnbindEvents(id, oldHandle);
                oldHandle.Release();
            }

            _handles[id] = handle;
            BindEvents(id, handle);
            handle.Init();
        }

        private void BindEvents(string id, TaskTimerHandle handle)
        {
            TaskTimerEventBinder eventBinder = new(this, id);
            _eventBinders[id] = eventBinder;

            handle.OnUpdated += eventBinder.OnUpdatedCallback;
            handle.OnCompleted += eventBinder.OnCompletedCallback;
            handle.OnClaimed += eventBinder.OnClaimedCallback;
            handle.OnStateChanged += eventBinder.OnStateChangedCallback;
        }

        private void UnbindEvents(string id, TaskTimerHandle handle)
        {
            if(!_eventBinders.TryGetValue(id, out TaskTimerEventBinder eventBinder))
                return;

            handle.OnUpdated -= eventBinder.OnUpdatedCallback;
            handle.OnCompleted -= eventBinder.OnCompletedCallback;
            handle.OnClaimed -= eventBinder.OnClaimedCallback;
            handle.OnStateChanged -= eventBinder.OnStateChangedCallback;
            _eventBinders.Remove(id);
        }

        //============================================================
        //Logic
        //============================================================
        public void StartTimer(string id, double durationSec)
        {
            if(!TaskTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
            {
                LogInvalidId(nameof(StartTimer), id);
                return;
            }

            if(!_handles.TryGetValue(normalizedId, out TaskTimerHandle handle))
                return;

            handle.Start(durationSec);
        }

        public void StartTimerMinutes(string id, double durationMin)
        {
            if(double.IsNaN(durationMin) || double.IsInfinity(durationMin))
                return;

            if(!TaskTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
            {
                LogInvalidId(nameof(StartTimerMinutes), id);
                return;
            }

            if(!_handles.TryGetValue(normalizedId, out TaskTimerHandle handle))
                return;

            handle.Start(durationMin * SEC_PER_MIN);
        }

        public void Reduce(string id, double reduceSec)
        {
            if(!TaskTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
            {
                LogInvalidId(nameof(Reduce), id);
                return;
            }

            if(!_handles.TryGetValue(normalizedId, out TaskTimerHandle handle))
                return;

            handle.Reduce(reduceSec);
        }

        public void ReduceMinutes(string id, double reduceMin)
        {
            if(double.IsNaN(reduceMin) || double.IsInfinity(reduceMin))
                return;

            if(!TaskTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
            {
                LogInvalidId(nameof(ReduceMinutes), id);
                return;
            }

            if(!_handles.TryGetValue(normalizedId, out TaskTimerHandle handle))
                return;

            handle.Reduce(reduceMin * SEC_PER_MIN);
        }

        public void CompleteImmediately(string id)
        {
            if(!TaskTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
            {
                LogInvalidId(nameof(CompleteImmediately), id);
                return;
            }

            if(!_handles.TryGetValue(normalizedId, out TaskTimerHandle handle))
                return;

            handle.CompleteImmediately();
        }

        public void Claim(string id)
        {
            if(!TaskTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
            {
                LogInvalidId(nameof(Claim), id);
                return;
            }

            if(!_handles.TryGetValue(normalizedId, out TaskTimerHandle handle))
                return;

            handle.Claim();
        }

        //============================================================
        //Callbacks
        //============================================================
        private void OnTimerUpdatedCallback(string id, int remainSec)
        {
            if(!_handles.TryGetValue(id, out TaskTimerHandle handle))
                return;

            _onAnyTimerUpdated?.Invoke(handle.ToData());
        }

        private void OnTimerCompletedCallback(string id)
        {
            if(!_handles.TryGetValue(id, out TaskTimerHandle handle))
                return;

            _onAnyTimerCompleted?.Invoke(handle.ToData());
        }

        private void OnTimerClaimedCallback(string id)
        {
            if(!_handles.TryGetValue(id, out TaskTimerHandle handle))
                return;

            _onAnyTimerClaimed?.Invoke(handle.ToData());
        }

        private void OnStateChangedCallback(string id, ETaskTimerType type)
        {
            if(type != ETaskTimerType.Processing)
                return;

            if(!_handles.TryGetValue(id, out TaskTimerHandle handle))
                return;

            _onAnyTimerUpdated?.Invoke(handle.ToData());
        }

        //============================================================
        //Utilities
        //============================================================
        public bool IsClaimed(string id)
        {
            if(!TaskTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
                return false;

            if(_handles.TryGetValue(normalizedId, out TaskTimerHandle handle))
                return handle.IsClaimed;

            return TaskTimer.IsClaimedStatic(normalizedId);
        }

        public TaskTimerHandle GetHandle(string id)
        {
            if(!TaskTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
                return null;

            return _handles.GetValueOrDefault(normalizedId);
        }

        public TaskTimerHandle CreateTaskTimerHandle(string id)
        {
            if(!TaskTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
            {
                LogInvalidId(nameof(CreateTaskTimerHandle), id);
                return null;
            }

            return new TaskTimerHandle(new TaskTimer(normalizedId, this, _isEnableLog));
        }

        private void LogInvalidId(string method, string id)
        {
            if(!_isEnableLog)
                return;

            string safeId = id == null ? "null" : id.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
            Debug.LogWarning($"[TaskTimerManager:{method}] 유효하지 않은 ID 요청을 무시합니다: '{safeId}'");
        }

        private class TaskTimerEventBinder
        {
            //============================================================
            //Readonly
            //============================================================
            private readonly string _id;
            private readonly TaskTimerManager _manager;

            //============================================================
            //Constructors
            //============================================================
            public TaskTimerEventBinder(TaskTimerManager manager, string id)
            {
                _manager = manager;
                _id = id;
            }

            //============================================================
            //Callbacks
            //============================================================
            public void OnUpdatedCallback(int remainSec)
            {
                if(_manager == null)
                    return;

                _manager.OnTimerUpdatedCallback(_id, remainSec);
            }

            public void OnCompletedCallback()
            {
                if(_manager == null)
                    return;

                _manager.OnTimerCompletedCallback(_id);
            }

            public void OnClaimedCallback()
            {
                if(_manager == null)
                    return;

                _manager.OnTimerClaimedCallback(_id);
            }

            public void OnStateChangedCallback(ETaskTimerType type)
            {
                if(_manager == null)
                    return;

                _manager.OnStateChangedCallback(_id, type);
            }
        }
    }
}