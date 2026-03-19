using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityTools.Util;
using UnityTools.Util.Constants;
using UnityTools.Util.Core.Collections;
using UnityTools.Util.Core.Events;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.Core.Persistence;
using UnityTools.Util.Core.Pooling;
using UnityTools.Util.Core.Singleton;
using UnityTools.Util.Core.State;
using UnityTools.Util.Core.Timer.Period;
using UnityTools.Util.Core.Timer.Shared;
using UnityTools.Util.Core.Timer.Task;
using UnityTools.Util.Coroutines;
using UnityTools.Util.Extensions;
using UnityTools.Util.UIFramework;
using UnityTools.Util.Utilities;

namespace UnityTools.Manager
{
    public class TaskTimerManager : MonoSingleton<TaskTimerManager>
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly TimerHandleRegistry<TaskTimerHandle> _handles = new();
        private readonly Dictionary<string, TaskTimerEventBinder> _eventBinders = new();
        private readonly ITimerHandleFactory<TaskTimerHandle> _handleFactory = new TaskTimerHandleFactory();

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
        private void OnDestroy()
        {
            ClearHandles();
        }

        //============================================================
        //Init/Register
        //============================================================
        public TaskTimerHandle CreateTaskTimerHandle(string id)
        {
            if(!TaskTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
            {
                LogInvalidId(nameof(CreateTaskTimerHandle), id);
                return null;
            }

            return _handleFactory.Create(normalizedId, this);
        }

        public void InitTaskTimer(TaskTimerHandle handle)
        {
            if(handle == null)
                return;

            if(!TaskTimerStorageKeys.TryNormalizeId(handle.Id, out string normalizedId))
            {
                LogInvalidId(nameof(InitTaskTimer), handle.Id);
                return;
            }

            TaskTimerHandle oldHandle = _handles.SetOrReplace(normalizedId, handle);
            if(oldHandle != null)
            {
                UnbindEvents(normalizedId, oldHandle);
                oldHandle.Release();
            }

            BindEvents(normalizedId, handle);
            handle.Init();
        }

        //============================================================
        //Logic
        //============================================================
        public void StartTaskTimer(string id, double durationSec)
        {
            if(!TryGetHandle(nameof(StartTaskTimer), id, out TaskTimerHandle handle))
                return;

            handle.Start(durationSec);
        }

        public void ReduceTaskTimer(string id, double reduceSec)
        {
            if(!TryGetHandle(nameof(ReduceTaskTimer), id, out TaskTimerHandle handle))
                return;

            handle.Reduce(reduceSec);
        }

        public void CompleteTaskTimerImmediately(string id)
        {
            if(!TryGetHandle(nameof(CompleteTaskTimerImmediately), id, out TaskTimerHandle handle))
                return;

            handle.CompleteImmediately();
        }

        public void ClaimTaskTimer(string id)
        {
            if(!TryGetHandle(nameof(ClaimTaskTimer), id, out TaskTimerHandle handle))
                return;

            handle.Claim();
        }

        public bool IsTaskTimerClaimed(string id)
        {
            if(!TaskTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
                return false;

            if(_handles.TryGet(normalizedId, out TaskTimerHandle handle))
                return handle.IsClaimed;

            return TaskTimer.IsClaimedStatic(normalizedId);
        }

        private void BindEvents(string id, TaskTimerHandle handle)
        {
            TaskTimerEventBinder eventBinder = new(
                id,
                OnTaskTimerRemainSecUpdatedCallback,
                OnTaskTimerCompletedCallback,
                OnTaskTimerClaimedCallback,
                OnTaskTimerStateTransitionCallback);
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

        private void ClearHandles()
        {
            string[] ids = new string[_eventBinders.Count];
            _eventBinders.Keys.CopyTo(ids, 0);
            foreach (string id in ids)
            {
                if(_handles.TryGet(id, out TaskTimerHandle handle))
                    UnbindEvents(id, handle);
            }

            _eventBinders.Clear();
            _handles.ClearAll();
        }

        //============================================================
        //Callbacks
        //============================================================
        private void OnTaskTimerRemainSecUpdatedCallback(string id, int remainSec)
        {
            if(!_handles.TryGet(id, out TaskTimerHandle handle))
                return;

            if(handle.RemainingSec != remainSec)
                return;

            _onAnyTimerRemainSecUpdated?.Invoke(handle.ToData());
        }

        private void OnTaskTimerCompletedCallback(string id)
        {
            if(!_handles.TryGet(id, out TaskTimerHandle handle))
                return;

            _onAnyTimerCompleted?.Invoke(handle.ToData());
        }

        private void OnTaskTimerClaimedCallback(string id)
        {
            if(!_handles.TryGet(id, out TaskTimerHandle handle))
                return;

            _onAnyTimerClaimed?.Invoke(handle.ToData());
        }

        private void OnTaskTimerStateTransitionCallback(string id, ETaskTimerType prevType, ETaskTimerType nextType)
        {
            if(prevType == nextType)
                return;

            if(nextType != ETaskTimerType.Processing)
                return;

            if(!_handles.TryGet(id, out TaskTimerHandle handle))
                return;

            _onAnyTimerRemainSecUpdated?.Invoke(handle.ToData());
        }

        //============================================================
        //Utilities
        //============================================================
        public TaskTimerHandle GetTaskHandle(string id)
        {
            if(!TaskTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
                return null;

            return _handles.GetOrDefault(normalizedId);
        }

        private bool TryGetHandle(string method, string id, out TaskTimerHandle handle)
        {
            handle = null;
            if(!TaskTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
            {
                LogInvalidId(method, id);
                return false;
            }

            return _handles.TryGet(normalizedId, out handle);
        }

        private void LogInvalidId(string method, string id)
        {
            string safeId = StringTokenUtils.ToLogSafe(id);
            DebugLogger.LogWarning($"[{method}] 유효하지 않은 ID 입력: '{safeId}'");
        }

        private class TaskTimerEventBinder
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
    }
}
