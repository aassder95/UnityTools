using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityTools.Util;

namespace UnityTools.Manager
{
    public class PeriodTimerManager : MonoSingleton<PeriodTimerManager>
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly Dictionary<string, PeriodTimerHandle> _handles = new();
        private readonly Dictionary<string, PeriodTimerEventBinder> _eventBinders = new();

        //============================================================
        //Inspector Fields
        //============================================================
        [SerializeField] private bool _isEnableLog;

        //============================================================
        //Events
        //============================================================
        public event UnityAction<PeriodTimerData> OnAnyTimerRemainMinUpdated { add => _onAnyTimerRemainMinUpdated += value; remove => _onAnyTimerRemainMinUpdated -= value; }
        private event UnityAction<PeriodTimerData> _onAnyTimerRemainMinUpdated;

        //============================================================
        //Unity Methods
        //============================================================
        private void Awake()
        {
            if(Singletons.PeriodTimerManager == null)
                Singletons.RegisterPeriodTimerManager(Instance);
        }

        private void OnDestroy()
        {
            foreach (KeyValuePair<string, PeriodTimerHandle> pair in _handles)
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
        public void InitTimer(PeriodTimerHandle handle, double openMin, double closedMin, Func<IEnumerator> initWaitFunc = null)
        {
            if(handle == null)
                return;

            if(!TryNormalizeId(nameof(InitTimer), handle.Id, out string normalizedId))
                return;

            if(_handles.TryGetValue(normalizedId, out PeriodTimerHandle oldHandle))
            {
                UnbindEvents(normalizedId, oldHandle);
                oldHandle.Release();
            }

            _handles[normalizedId] = handle;
            BindEvents(normalizedId, handle);
            handle.Init(openMin, closedMin, initWaitFunc);
        }

        public void DeleteTimer(string id)
        {
            if(!TryNormalizeId(nameof(DeleteTimer), id, out string normalizedId))
                return;

            if(_handles.TryGetValue(normalizedId, out PeriodTimerHandle handle))
            {
                UnbindEvents(normalizedId, handle);
                handle.Release();
                _handles.Remove(normalizedId);
            }
            else
            {
                _eventBinders.Remove(normalizedId);
            }

            PeriodTimerStorageKeys.DeleteAll(normalizedId);
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

        //============================================================
        //Callbacks
        //============================================================
        private void OnTimerRemainMinUpdatedCallback(string id, int remainMin)
        {
            if(!_handles.TryGetValue(id, out PeriodTimerHandle handle))
                return;

            _onAnyTimerRemainMinUpdated?.Invoke(handle.ToData());
        }

        //============================================================
        //Utilities
        //============================================================
        public PeriodTimerHandle GetHandle(string id)
        {
            if(!PeriodTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
                return null;

            return _handles.GetValueOrDefault(normalizedId);
        }

        public PeriodTimerHandle CreatePeriodTimerHandle(string id)
        {
            if(!TryNormalizeId(nameof(CreatePeriodTimerHandle), id, out string normalizedId))
                return null;

            return new PeriodTimerHandle(new PeriodTimer(normalizedId, this, _isEnableLog));
        }

        private bool TryNormalizeId(string method, string id, out string normalizedId)
        {
            if(PeriodTimerStorageKeys.TryNormalizeId(id, out normalizedId))
                return true;

            LogInvalidId(method, id);
            normalizedId = string.Empty;
            return false;
        }

        private void LogInvalidId(string method, string id)
        {
            if(!_isEnableLog)
                return;

            string safeId = StringTokenUtils.ToLogSafe(id);
            DebugLogger.LogWarning($"[{method}] 유효하지 않은 ID 입력: '{safeId}'");
        }

        private class PeriodTimerEventBinder
        {
            //============================================================
            //Readonly
            //============================================================
            private readonly string _id;
            private readonly PeriodTimerManager _manager;

            //============================================================
            //Constructors
            //============================================================
            public PeriodTimerEventBinder(PeriodTimerManager manager, string id)
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

                _manager.OnTimerRemainMinUpdatedCallback(_id, remainMin);
            }
        }
    }
}
