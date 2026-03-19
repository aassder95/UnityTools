using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityTools.Util;

namespace UnityTools.Manager
{
    public class TimerManager : MonoSingleton<TimerManager>
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly TimerHandleRegistry<TaskTimerHandle> _taskHandles = new();
        private readonly TimerHandleRegistry<PeriodTimerHandle> _periodHandles = new();
        private readonly Dictionary<string, TaskTimerEventBinder> _taskEventBinders = new();
        private readonly Dictionary<string, PeriodTimerEventBinder> _periodEventBinders = new();
        private readonly ITimerHandleFactory<TaskTimerHandle> _taskHandleFactory = new TaskTimerHandleFactory();
        private readonly ITimerHandleFactory<PeriodTimerHandle> _periodHandleFactory = new PeriodTimerHandleFactory();

        //============================================================
        //Events
        //============================================================
        public event UnityAction<TaskTimerData> OnAnyTimerRemainSecUpdated { add => _onAnyTimerRemainSecUpdated += value; remove => _onAnyTimerRemainSecUpdated -= value; }
        public event UnityAction<TaskTimerData> OnAnyTimerCompleted { add => _onAnyTimerCompleted += value; remove => _onAnyTimerCompleted -= value; }
        public event UnityAction<TaskTimerData> OnAnyTimerClaimed { add => _onAnyTimerClaimed += value; remove => _onAnyTimerClaimed -= value; }
        public event UnityAction<PeriodTimerData> OnAnyTimerRemainMinUpdated { add => _onAnyTimerRemainMinUpdated += value; remove => _onAnyTimerRemainMinUpdated -= value; }
        private event UnityAction<TaskTimerData> _onAnyTimerRemainSecUpdated;
        private event UnityAction<TaskTimerData> _onAnyTimerCompleted;
        private event UnityAction<TaskTimerData> _onAnyTimerClaimed;
        private event UnityAction<PeriodTimerData> _onAnyTimerRemainMinUpdated;

        //============================================================
        //Unity Methods
        //============================================================
        private void Awake()
        {
            if(Singletons.TimerManager == null)
                Singletons.RegisterTimerManager(Instance);
        }

        private void OnDestroy()
        {
            ClearTaskHandles();
            ClearPeriodHandles();
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

            return _taskHandleFactory.Create(normalizedId, this);
        }

        public PeriodTimerHandle CreatePeriodTimerHandle(string id)
        {
            if(!PeriodTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
            {
                LogInvalidId(nameof(CreatePeriodTimerHandle), id);
                return null;
            }

            return _periodHandleFactory.Create(normalizedId, this);
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

            TaskTimerHandle oldHandle = _taskHandles.SetOrReplace(normalizedId, handle);
            if(oldHandle != null)
            {
                UnbindTaskEvents(normalizedId, oldHandle);
                oldHandle.Release();
            }

            BindTaskEvents(normalizedId, handle);
            handle.Init();
        }

        public void InitPeriodTimer(PeriodTimerHandle handle, double openMin, double closedMin, Func<IEnumerator> initWaitFunc = null)
        {
            if(handle == null)
                return;

            if(!PeriodTimerStorageKeys.TryNormalizeId(handle.Id, out string normalizedId))
            {
                LogInvalidId(nameof(InitPeriodTimer), handle.Id);
                return;
            }

            PeriodTimerHandle oldHandle = _periodHandles.SetOrReplace(normalizedId, handle);
            if(oldHandle != null)
            {
                UnbindPeriodEvents(normalizedId, oldHandle);
                oldHandle.Release();
            }

            BindPeriodEvents(normalizedId, handle);
            handle.Init(openMin, closedMin, initWaitFunc);
        }

        //============================================================
        //Logic
        //============================================================
        public void StartTaskTimer(string id, double durationSec)
        {
            if(!TryGetTaskHandle(nameof(StartTaskTimer), id, out TaskTimerHandle handle))
                return;

            handle.Start(durationSec);
        }

        public void ReduceTaskTimer(string id, double reduceSec)
        {
            if(!TryGetTaskHandle(nameof(ReduceTaskTimer), id, out TaskTimerHandle handle))
                return;

            handle.Reduce(reduceSec);
        }

        public void CompleteTaskTimerImmediately(string id)
        {
            if(!TryGetTaskHandle(nameof(CompleteTaskTimerImmediately), id, out TaskTimerHandle handle))
                return;

            handle.CompleteImmediately();
        }

        public void ClaimTaskTimer(string id)
        {
            if(!TryGetTaskHandle(nameof(ClaimTaskTimer), id, out TaskTimerHandle handle))
                return;

            handle.Claim();
        }

        public void DeletePeriodTimer(string id)
        {
            if(!PeriodTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
            {
                LogInvalidId(nameof(DeletePeriodTimer), id);
                return;
            }

            if(_periodHandles.Remove(normalizedId, out PeriodTimerHandle handle))
            {
                UnbindPeriodEvents(normalizedId, handle);
                handle.Release();
            }
            else
            {
                _periodEventBinders.Remove(normalizedId);
            }

            PeriodTimerPersistence.DeleteAll(normalizedId);
        }

        public bool IsTaskTimerClaimed(string id)
        {
            if(!TaskTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
                return false;

            if(_taskHandles.TryGet(normalizedId, out TaskTimerHandle handle))
                return handle.IsClaimed;

            return TaskTimer.IsClaimedStatic(normalizedId);
        }

        private void BindTaskEvents(string id, TaskTimerHandle handle)
        {
            TaskTimerEventBinder eventBinder = new(
                id,
                OnTaskTimerRemainSecUpdatedCallback,
                OnTaskTimerCompletedCallback,
                OnTaskTimerClaimedCallback,
                OnTaskTimerStateTransitionCallback);
            _taskEventBinders[id] = eventBinder;

            handle.OnRemainSecUpdated += eventBinder.OnRemainSecUpdatedCallback;
            handle.OnCompleted += eventBinder.OnCompletedCallback;
            handle.OnClaimed += eventBinder.OnClaimedCallback;
            handle.OnStateTransition += eventBinder.OnStateTransitionCallback;
        }

        private void UnbindTaskEvents(string id, TaskTimerHandle handle)
        {
            if(!_taskEventBinders.TryGetValue(id, out TaskTimerEventBinder eventBinder))
                return;

            handle.OnRemainSecUpdated -= eventBinder.OnRemainSecUpdatedCallback;
            handle.OnCompleted -= eventBinder.OnCompletedCallback;
            handle.OnClaimed -= eventBinder.OnClaimedCallback;
            handle.OnStateTransition -= eventBinder.OnStateTransitionCallback;
            _taskEventBinders.Remove(id);
        }

        private void BindPeriodEvents(string id, PeriodTimerHandle handle)
        {
            PeriodTimerEventBinder eventBinder = new(this, id);
            _periodEventBinders[id] = eventBinder;
            handle.OnRemainMinUpdated += eventBinder.OnRemainMinUpdatedCallback;
        }

        private void UnbindPeriodEvents(string id, PeriodTimerHandle handle)
        {
            if(!_periodEventBinders.TryGetValue(id, out PeriodTimerEventBinder eventBinder))
                return;

            handle.OnRemainMinUpdated -= eventBinder.OnRemainMinUpdatedCallback;
            _periodEventBinders.Remove(id);
        }

        private void ClearTaskHandles()
        {
            string[] ids = new string[_taskEventBinders.Count];
            _taskEventBinders.Keys.CopyTo(ids, 0);
            foreach (string id in ids)
            {
                if(_taskHandles.TryGet(id, out TaskTimerHandle handle))
                    UnbindTaskEvents(id, handle);
            }

            _taskEventBinders.Clear();
            _taskHandles.ClearAll();
        }

        private void ClearPeriodHandles()
        {
            string[] ids = new string[_periodEventBinders.Count];
            _periodEventBinders.Keys.CopyTo(ids, 0);
            foreach (string id in ids)
            {
                if(_periodHandles.TryGet(id, out PeriodTimerHandle handle))
                    UnbindPeriodEvents(id, handle);
            }

            _periodEventBinders.Clear();
            _periodHandles.ClearAll();
        }

        //============================================================
        //Callbacks
        //============================================================
        private void OnTaskTimerRemainSecUpdatedCallback(string id, int remainSec)
        {
            if(!_taskHandles.TryGet(id, out TaskTimerHandle handle))
                return;

            if(handle.RemainingSec != remainSec)
                return;

            _onAnyTimerRemainSecUpdated?.Invoke(handle.ToData());
        }

        private void OnTaskTimerCompletedCallback(string id)
        {
            if(!_taskHandles.TryGet(id, out TaskTimerHandle handle))
                return;

            _onAnyTimerCompleted?.Invoke(handle.ToData());
        }

        private void OnTaskTimerClaimedCallback(string id)
        {
            if(!_taskHandles.TryGet(id, out TaskTimerHandle handle))
                return;

            _onAnyTimerClaimed?.Invoke(handle.ToData());
        }

        private void OnTaskTimerStateTransitionCallback(string id, ETaskTimerType prevType, ETaskTimerType nextType)
        {
            if(prevType == nextType)
                return;

            if(nextType != ETaskTimerType.Processing)
                return;

            if(!_taskHandles.TryGet(id, out TaskTimerHandle handle))
                return;

            _onAnyTimerRemainSecUpdated?.Invoke(handle.ToData());
        }

        private void OnPeriodTimerRemainMinUpdatedCallback(string id, int remainMin)
        {
            if(!_periodHandles.TryGet(id, out PeriodTimerHandle handle))
                return;

            if(handle.GetRemainingMin() != remainMin)
                return;

            _onAnyTimerRemainMinUpdated?.Invoke(handle.ToData());
        }

        //============================================================
        //Utilities
        //============================================================
        public TaskTimerHandle GetTaskHandle(string id)
        {
            if(!TaskTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
                return null;

            return _taskHandles.GetOrDefault(normalizedId);
        }

        public PeriodTimerHandle GetPeriodHandle(string id)
        {
            if(!PeriodTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
                return null;

            return _periodHandles.GetOrDefault(normalizedId);
        }

        private bool TryGetTaskHandle(string method, string id, out TaskTimerHandle handle)
        {
            handle = null;
            if(!TaskTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
            {
                LogInvalidId(method, id);
                return false;
            }

            return _taskHandles.TryGet(normalizedId, out handle);
        }

        private void LogInvalidId(string method, string id)
        {
            string safeId = StringTokenUtils.ToLogSafe(id);
            DebugLogger.LogWarning($"[{method}] 유효하지 않은 ID 입력: '{safeId}'");
        }

        private class PeriodTimerEventBinder
        {
            //============================================================
            //Readonly
            //============================================================
            private readonly string _id;
            private readonly TimerManager _manager;

            //============================================================
            //Constructors
            //============================================================
            public PeriodTimerEventBinder(TimerManager manager, string id)
            {
                _manager = manager;
                _id = id;
            }

            //============================================================
            //Callbacks
            //============================================================
            public void OnRemainMinUpdatedCallback(int remainMin)
            {
                if(_manager == null)
                    return;

                _manager.OnPeriodTimerRemainMinUpdatedCallback(_id, remainMin);
            }
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


