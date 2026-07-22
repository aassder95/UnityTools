using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityTools.Util.Core.Singleton;
using UnityTools.Util.Core.Timer;
using UnityTools.Util.Core.Timer.Period;
using UnityTools.Util.Utilities;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Manager
{
    // Exception: Period timer values are minute-based by product requirement.
    public class PeriodTimerManager : MonoSingleton<PeriodTimerManager>
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly TimerHandleRegistry<PeriodTimerHandle> _handles = new();
        private readonly Dictionary<string, PeriodTimerEventBinder> _eventBinders = new();
        private readonly ITimerHandleFactory<PeriodTimerHandle> _handleFactory = new PeriodTimerHandleFactory();

        //============================================================
        // Events
        //============================================================
        public event UnityAction<PeriodTimerData> OnAnyTimerRemainMinUpdated { add => _onAnyTimerRemainMinUpdated += value; remove => _onAnyTimerRemainMinUpdated -= value; }
        private event UnityAction<PeriodTimerData> _onAnyTimerRemainMinUpdated;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnDestroy()
        {
            ClearHandles();
        }

        //============================================================
        // Init/Register
        //============================================================
        public PeriodTimerHandle CreatePeriodTimerHandle(string id)
        {
            if(!PeriodTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
            {
                LogInvalidId(nameof(CreatePeriodTimerHandle), id);
                return null;
            }

            return _handleFactory.Create(normalizedId, this);
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

            PeriodTimerHandle oldHandle = _handles.SetOrReplace(normalizedId, handle);
            if(oldHandle != null)
            {
                UnbindEvents(normalizedId, oldHandle);
                oldHandle.Release();
            }

            BindEvents(normalizedId, handle);
            handle.Init(openMin, closedMin, initWaitFunc);
        }

        //============================================================
        // Logic
        //============================================================
        public void DeletePeriodTimer(string id)
        {
            if(!PeriodTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
            {
                LogInvalidId(nameof(DeletePeriodTimer), id);
                return;
            }

            if(_handles.Remove(normalizedId, out PeriodTimerHandle handle))
            {
                UnbindEvents(normalizedId, handle);
                handle.Release();
            }
            else
            {
                _eventBinders.Remove(normalizedId);
            }

            PeriodTimerPersistence.DeleteAll(normalizedId);
        }

        private void BindEvents(string id, PeriodTimerHandle handle)
        {
            PeriodTimerEventBinder eventBinder = new(this, id);
            _eventBinders[id] = eventBinder;
            handle.OnRemainMinUpdated += eventBinder.OnRemainMinUpdatedCallback;
        }

        private void UnbindEvents(string id, PeriodTimerHandle handle)
        {
            if(!_eventBinders.TryGetValue(id, out PeriodTimerEventBinder eventBinder))
                return;

            handle.OnRemainMinUpdated -= eventBinder.OnRemainMinUpdatedCallback;
            _eventBinders.Remove(id);
        }

        private void ClearHandles()
        {
            string[] ids = new string[_eventBinders.Count];
            _eventBinders.Keys.CopyTo(ids, 0);
            foreach (string id in ids)
            {
                if(_handles.TryGet(id, out PeriodTimerHandle handle))
                    UnbindEvents(id, handle);
            }

            _eventBinders.Clear();
            _handles.ClearAll();
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OnPeriodTimerRemainMinUpdatedCallback(string id, int remainMin)
        {
            if(!_handles.TryGet(id, out PeriodTimerHandle handle))
                return;

            if(handle.GetRemainingMin() != remainMin)
                return;

            _onAnyTimerRemainMinUpdated?.Invoke(handle.ToData());
        }

        //============================================================
        // Utilities
        //============================================================
        public PeriodTimerHandle GetPeriodHandle(string id)
        {
            if(!PeriodTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
                return null;

            return _handles.GetOrDefault(normalizedId);
        }

        private void LogInvalidId(string method, string id)
        {
            string safeId = StringTokenUtils.ToLogSafe(id);
            DebugLogger.LogWarning($"[{method}] 유효하지 않은 ID 입력: '{safeId}'");
        }

        //============================================================
        // Nested Types
        //============================================================
        private class PeriodTimerEventBinder
        {
            //============================================================
            // Readonly
            //============================================================
            private readonly string _id;
            private readonly PeriodTimerManager _manager;

            //============================================================
            // Constructors
            //============================================================
            public PeriodTimerEventBinder(PeriodTimerManager manager, string id)
            {
                _manager = manager;
                _id = id;
            }

            //============================================================
            // Callbacks
            //============================================================
            public void OnRemainMinUpdatedCallback(int remainMin)
            {
                if(_manager == null)
                    return;

                _manager.OnPeriodTimerRemainMinUpdatedCallback(_id, remainMin);
            }
        }
    }
}
