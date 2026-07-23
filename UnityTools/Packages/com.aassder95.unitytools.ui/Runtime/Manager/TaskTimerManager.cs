using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityTools.Util.Core.Logging;
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
        private readonly ITimerHandleFactory<TaskTimerHandle> _handleFactory = new TaskTimerHandleFactory();

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
            {
                LogInvalidId(nameof(TryCreateTaskTimerHandle), id);
                return false;
            }

            return _handleFactory.TryCreate(normalizedId, this, out handle);
        }

        public bool InitTaskTimer(TaskTimerHandle handle)
        {
            if(handle == null)
            {
                DebugLogger.LogError("초기화할 TaskTimerHandle이 비어 있습니다.", this);
                return false;
            }

            if(!StringTokenUtils.TryNormalizeNonEmpty(handle.Id, out string normalizedId))
            {
                LogInvalidId(nameof(InitTaskTimer), handle.Id);
                return false;
            }

            if(!handle.Init())
                return false;

            if(_handles.TryGet(normalizedId, out TaskTimerHandle oldHandle))
            {
                UnbindEvents(normalizedId, oldHandle);
                oldHandle.Release();
            }

            if(!_handles.TrySetOrReplace(normalizedId, handle, out _))
            {
                handle.Release();
                return false;
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
            {
                LogInvalidId(nameof(TryGetTaskTimerClaimed), id);
                return false;
            }

            if(_handles.TryGet(normalizedId, out TaskTimerHandle handle))
            {
                isClaimed = handle.IsClaimed;
                return true;
            }

            return TaskTimer.TryGetClaimed(normalizedId, out isClaimed);
        }

        private void BindEvents(string id, TaskTimerHandle handle)
        {
            TaskTimerEventBinder eventBinder = new(id, OnTaskTimerRemainSecUpdatedCallback, OnTaskTimerCompletedCallback, OnTaskTimerClaimedCallback, OnTaskTimerStateTransitionCallback);
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
            foreach(string id in ids)
            {
                if(_handles.TryGet(id, out TaskTimerHandle handle))
                    UnbindEvents(id, handle);
            }

            _eventBinders.Clear();
            _handles.Clear();
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OnTaskTimerRemainSecUpdatedCallback(string id, int remainSec)
        {
            if(!ValidateCallbackHandle(id, out TaskTimerHandle handle))
                return;

            if(handle.RemainingSec != remainSec)
                return;

            _onAnyTimerRemainSecUpdated?.Invoke(handle.ToData());
        }

        private void OnTaskTimerCompletedCallback(string id)
        {
            if(ValidateCallbackHandle(id, out TaskTimerHandle handle))
                _onAnyTimerCompleted?.Invoke(handle.ToData());
        }

        private void OnTaskTimerClaimedCallback(string id)
        {
            if(ValidateCallbackHandle(id, out TaskTimerHandle handle))
                _onAnyTimerClaimed?.Invoke(handle.ToData());
        }

        private void OnTaskTimerStateTransitionCallback(string id, ETaskTimerType prevType, ETaskTimerType nextType)
        {
            if(prevType == nextType || nextType != ETaskTimerType.Processing)
                return;

            if(ValidateCallbackHandle(id, out TaskTimerHandle handle))
                _onAnyTimerRemainSecUpdated?.Invoke(handle.ToData());
        }

        //============================================================
        // Utilities
        //============================================================
        public bool TryGetTaskHandle(string id, out TaskTimerHandle handle)
        {
            handle = null;
            if(!StringTokenUtils.TryNormalizeNonEmpty(id, out string normalizedId))
            {
                LogInvalidId(nameof(TryGetTaskHandle), id);
                return false;
            }

            if(_handles.TryGet(normalizedId, out handle))
                return true;

            DebugLogger.LogError("등록된 TaskTimerHandle을 찾을 수 없습니다. ID=" + normalizedId, this);
            return false;
        }

        private bool ValidateCallbackHandle(string id, out TaskTimerHandle handle)
        {
            if(_handles.TryGet(id, out handle))
                return true;

            DebugLogger.LogError("TaskTimer Callback 대상 Handle을 찾을 수 없습니다. ID=" + StringTokenUtils.ToLogSafe(id), this);
            return false;
        }

        private void LogInvalidId(string method, string id)
        {
            string safeId = StringTokenUtils.ToLogSafe(id);
            DebugLogger.LogError($"[{method}] 유효하지 않은 ID 입력: '{safeId}'", this);
        }

        //============================================================
        // Nested Types
        //============================================================
        private class TaskTimerEventBinder
        {
            //============================================================
            // Readonly
            //============================================================
            private readonly string _id;
            private readonly Action<string, int> _onRemainSecUpdated;
            private readonly Action<string> _onCompleted;
            private readonly Action<string> _onClaimed;
            private readonly Action<string, ETaskTimerType, ETaskTimerType> _onStateTransition;

            //============================================================
            // Constructors
            //============================================================
            public TaskTimerEventBinder(string id, Action<string, int> onRemainSecUpdated, Action<string> onCompleted, Action<string> onClaimed, Action<string, ETaskTimerType, ETaskTimerType> onStateTransition)
            {
                _id = id;
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
                _onRemainSecUpdated.Invoke(_id, remainSec);
            }

            public void OnCompletedCallback()
            {
                _onCompleted.Invoke(_id);
            }

            public void OnClaimedCallback()
            {
                _onClaimed.Invoke(_id);
            }

            public void OnStateTransitionCallback(ETaskTimerType prevType, ETaskTimerType nextType)
            {
                _onStateTransition.Invoke(_id, prevType, nextType);
            }
        }
    }
}
