using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityTools.Util.Core.Singleton;
using UnityTools.Util.Core.Timer;
using UnityTools.Util.Core.Timer.Task;
using UnityTools.Util.Utilities;

namespace UnityTools.Manager
{
    public class TaskTimerManager : MonoSingleton<TaskTimerManager>
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly TimerHandleRegistry<TaskTimerHandle> _handles = new();
        private readonly Dictionary<string, TaskTimerEventBinder> _eventBinders = new();

        //============================================================
        // Events
        //============================================================
        public event UnityAction<TaskTimerData> OnAnyTimerRemainSecUpdated { add => _onAnyTimerRemainSecUpdated += value; remove => _onAnyTimerRemainSecUpdated -= value; }
        public event UnityAction<TaskTimerData> OnAnyTimerCompleted { add => _onAnyTimerCompleted += value; remove => _onAnyTimerCompleted -= value; }
        public event UnityAction<TaskTimerData> OnAnyTimerClaimed { add => _onAnyTimerClaimed += value; remove => _onAnyTimerClaimed -= value; }
        private event UnityAction<TaskTimerData> _onAnyTimerRemainSecUpdated;
        private event UnityAction<TaskTimerData> _onAnyTimerCompleted;
        private event UnityAction<TaskTimerData> _onAnyTimerClaimed;

        //============================================================
        // Unity Methods
        //============================================================
        protected override void OnDestroy()
        {
            base.OnDestroy();
            ClearHandles();
        }

        //============================================================
        // Init/Register
        //============================================================
        public bool TryCreateTaskTimerHandle(string id, out TaskTimerHandle handle)
        {
            handle = null;
            if(!StringTokenUtils.TryNormalizeNonEmpty(id, out string normalizedId))
                return false;

            handle = new TaskTimerHandle(TaskTimer.Create(normalizedId, this));
            return true;
        }

        public bool TryInitTaskTimer(TaskTimerHandle handle)
        {
            if(handle == null || !StringTokenUtils.TryNormalizeNonEmpty(handle.Id, out string normalizedId))
                return false;

            if(!handle.TryInit())
                return false;

            TaskTimerHandle oldHandle = _handles.SetOrReplace(normalizedId, handle);
            if(oldHandle != null)
            {
                UnbindEvents(normalizedId, oldHandle);
                if(oldHandle != handle)
                    oldHandle.Release();
            }

            BindEvents(normalizedId, handle);
            return true;
        }

        //============================================================
        // Logic
        //============================================================
        public bool TryStartTaskTimer(string id, double durationSec)
        {
            return TryGetTaskHandle(id, out TaskTimerHandle handle) && handle.TryStart(durationSec);
        }

        public bool TryReduceTaskTimer(string id, double reduceSec)
        {
            return TryGetTaskHandle(id, out TaskTimerHandle handle) && handle.TryReduce(reduceSec);
        }

        public bool TryCompleteTaskTimer(string id)
        {
            return TryGetTaskHandle(id, out TaskTimerHandle handle) && handle.TryComplete();
        }

        public bool TryClaimTaskTimer(string id)
        {
            return TryGetTaskHandle(id, out TaskTimerHandle handle) && handle.TryClaim();
        }

        public bool TryGetTaskTimerClaimed(string id, out bool isClaimed)
        {
            isClaimed = false;
            if(!StringTokenUtils.TryNormalizeNonEmpty(id, out string normalizedId))
                return false;

            if(_handles.TryGet(normalizedId, out TaskTimerHandle handle))
            {
                isClaimed = handle.IsClaimed;
                return true;
            }

            isClaimed = TaskTimerPersistence.LoadClaimed(normalizedId);
            return true;
        }

        private void BindEvents(string id, TaskTimerHandle handle)
        {
            TaskTimerEventBinder eventBinder = new(handle, OnTaskTimerRemainSecUpdatedCallback, OnTaskTimerCompletedCallback, OnTaskTimerClaimedCallback, OnTaskTimerStateTransitionCallback);
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

        private void ClearHandles()
        {
            string[] ids = new string[_eventBinders.Count];
            _eventBinders.Keys.CopyTo(ids, 0);
            foreach(string id in ids)
            {
                UnbindEvents(id, _eventBinders[id].Handle);
            }

            _eventBinders.Clear();
            _handles.Clear();
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OnTaskTimerRemainSecUpdatedCallback(TaskTimerHandle handle, int remainSec)
        {
            if(handle.RemainingSec != remainSec)
                return;

            _onAnyTimerRemainSecUpdated?.Invoke(handle.ToData());
        }

        private void OnTaskTimerCompletedCallback(TaskTimerHandle handle)
        {
            _onAnyTimerCompleted?.Invoke(handle.ToData());
        }

        private void OnTaskTimerClaimedCallback(TaskTimerHandle handle)
        {
            _onAnyTimerClaimed?.Invoke(handle.ToData());
        }

        private void OnTaskTimerStateTransitionCallback(TaskTimerHandle handle, ETaskTimerType prevType, ETaskTimerType nextType)
        {
            if(prevType == nextType || nextType != ETaskTimerType.Processing)
                return;

            _onAnyTimerRemainSecUpdated?.Invoke(handle.ToData());
        }

        //============================================================
        // Utilities
        //============================================================
        public bool TryGetTaskHandle(string id, out TaskTimerHandle handle)
        {
            handle = null;
            return StringTokenUtils.TryNormalizeNonEmpty(id, out string normalizedId) && _handles.TryGet(normalizedId, out handle);
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
            // Properties
            //============================================================
            public TaskTimerHandle Handle => _handle;

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
