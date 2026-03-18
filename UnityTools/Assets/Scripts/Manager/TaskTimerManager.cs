using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityTools.Util;

namespace UnityTools.Manager
{
    public class TaskTimerEventBinder
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly string _id;
        private readonly Action<string, int> _onRemainSecUpdated;
        private readonly Action<string> _onCompleted;
        private readonly Action<string> _onClaimed;
        private readonly Action<string, ETaskTimerType, ETaskTimerType> _onStateTransition;

        //============================================================
        //Constructors
        //============================================================
        public TaskTimerEventBinder(
            string id,
            Action<string, int> onRemainSecUpdated,
            Action<string> onCompleted,
            Action<string> onClaimed,
            Action<string, ETaskTimerType, ETaskTimerType> onStateTransition)
        {
            _id = id;
            _onRemainSecUpdated = onRemainSecUpdated;
            _onCompleted = onCompleted;
            _onClaimed = onClaimed;
            _onStateTransition = onStateTransition;
        }

        //============================================================
        //Callbacks
        //============================================================
        public void OnRemainSecUpdatedCallback(int remainSec)
        {
            _onRemainSecUpdated?.Invoke(_id, remainSec);
        }

        public void OnCompletedCallback()
        {
            _onCompleted?.Invoke(_id);
        }

        public void OnClaimedCallback()
        {
            _onClaimed?.Invoke(_id);
        }

        public void OnStateTransitionCallback(ETaskTimerType prevType, ETaskTimerType nextType)
        {
            _onStateTransition?.Invoke(_id, prevType, nextType);
        }
    }

    public class TaskTimerManager : MonoSingleton<TaskTimerManager>
    {
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
        public event UnityAction<TaskTimerData> OnAnyTimerRemainSecUpdated { add => _onAnyTimerRemainSecUpdated += value; remove => _onAnyTimerRemainSecUpdated -= value; }
        public event UnityAction<TaskTimerData> OnAnyTimerCompleted { add => _onAnyTimerCompleted += value; remove => _onAnyTimerCompleted -= value; }
        public event UnityAction<TaskTimerData> OnAnyTimerClaimed { add => _onAnyTimerClaimed += value; remove => _onAnyTimerClaimed -= value; }
        private event UnityAction<TaskTimerData> _onAnyTimerRemainSecUpdated;
        private event UnityAction<TaskTimerData> _onAnyTimerCompleted;
        private event UnityAction<TaskTimerData> _onAnyTimerClaimed;

        //============================================================
        //Unity Methods
        //============================================================
        private void Awake()
        {
            if(Singletons.TaskTimerManager == null)
                Singletons.RegisterTaskTimerManager(Instance);
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
            TaskTimerEventBinder eventBinder = new(
                id,
                OnTimerRemainSecUpdatedCallback,
                OnTimerCompletedCallback,
                OnTimerClaimedCallback,
                OnStateTransitionCallback);
            _eventBinders[id] = eventBinder;

            handle.OnRemainSecUpdated += eventBinder.OnRemainSecUpdatedCallback;
            handle.OnCompleted += eventBinder.OnCompletedCallback;
            handle.OnClaimed += eventBinder.OnClaimedCallback;
            handle.OnStateTransition += eventBinder.OnStateTransitionCallback;
        }

        private void UnbindEvents(string id, TaskTimerHandle handle)
        {
            if(!_eventBinders.TryGetValue(id, out TaskTimerEventBinder eventBinder))
                return;

            handle.OnRemainSecUpdated -= eventBinder.OnRemainSecUpdatedCallback;
            handle.OnCompleted -= eventBinder.OnCompletedCallback;
            handle.OnClaimed -= eventBinder.OnClaimedCallback;
            handle.OnStateTransition -= eventBinder.OnStateTransitionCallback;
            _eventBinders.Remove(id);
        }

        //============================================================
        //Logic
        //============================================================
        public void StartTimer(string id, double durationSec)
        {
            if(!TryGetHandle(nameof(StartTimer), id, out TaskTimerHandle handle))
                return;

            handle.Start(durationSec);
        }

        public void Reduce(string id, double reduceSec)
        {
            if(!TryGetHandle(nameof(Reduce), id, out TaskTimerHandle handle))
                return;

            handle.Reduce(reduceSec);
        }

        public void CompleteImmediately(string id)
        {
            if(!TryGetHandle(nameof(CompleteImmediately), id, out TaskTimerHandle handle))
                return;

            handle.CompleteImmediately();
        }

        public void Claim(string id)
        {
            if(!TryGetHandle(nameof(Claim), id, out TaskTimerHandle handle))
                return;

            handle.Claim();
        }

        //============================================================
        //Callbacks
        //============================================================
        private void OnTimerRemainSecUpdatedCallback(string id, int remainSec)
        {
            if(!_handles.TryGetValue(id, out TaskTimerHandle handle))
                return;

            _onAnyTimerRemainSecUpdated?.Invoke(handle.ToData());
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

        private void OnStateTransitionCallback(string id, ETaskTimerType prevType, ETaskTimerType nextType)
        {
            if(nextType != ETaskTimerType.Processing)
                return;

            if(!_handles.TryGetValue(id, out TaskTimerHandle handle))
                return;

            _onAnyTimerRemainSecUpdated?.Invoke(handle.ToData());
        }

        //============================================================
        //Utilities
        //============================================================
        private bool TryGetHandle(string method, string id, out TaskTimerHandle handle)
        {
            handle = null;
            if(!TaskTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
            {
                LogInvalidId(method, id);
                return false;
            }

            return _handles.TryGetValue(normalizedId, out handle);
        }

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

            string safeId = StringTokenUtils.ToLogSafe(id);
            DebugLogger.LogWarning($"[{method}] 유효하지 않은 ID 입력: '{safeId}'");
        }
    }
}
