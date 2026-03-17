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
        public event UnityAction<PeriodTimerData> OnAnyTimerUpdated { add => _onAnyTimerUpdated += value; remove => _onAnyTimerUpdated -= value; }
        private event UnityAction<PeriodTimerData> _onAnyTimerUpdated;

        //============================================================
        //Unity Methods
        //============================================================
        private void Awake()
        {
            Singletons.PeriodTimerManager ??= Instance;
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
        public void InitTimer(PeriodTimerHandle handle, double openMin, double closedMin)
        {
            if(handle == null)
                return;

            if(!PeriodTimerStorageKeys.TryNormalizeId(handle.Id, out string id))
            {
                LogInvalidId(nameof(InitTimer), handle.Id);
                return;
            }

            if(_handles.TryGetValue(id, out PeriodTimerHandle oldHandle))
            {
                UnbindEvents(id, oldHandle);
                oldHandle.Release();
            }

            _handles[id] = handle;
            BindEvents(id, handle);
            handle.Init(openMin, closedMin);
        }

        private void BindEvents(string id, PeriodTimerHandle handle)
        {
            PeriodTimerEventBinder eventBinder = new(this, id);
            _eventBinders[id] = eventBinder;
            handle.OnUpdated += eventBinder.OnUpdatedCallback;
        }

        private void UnbindEvents(string id, PeriodTimerHandle handle)
        {
            if(!_eventBinders.TryGetValue(id, out PeriodTimerEventBinder eventBinder))
                return;

            handle.OnUpdated -= eventBinder.OnUpdatedCallback;
            _eventBinders.Remove(id);
        }

        //============================================================
        //Callbacks
        //============================================================
        private void OnTimerUpdatedCallback(string id, int remainMin)
        {
            if(!_handles.TryGetValue(id, out PeriodTimerHandle handle))
                return;

            _onAnyTimerUpdated?.Invoke(handle.ToData());
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
            if(!PeriodTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
            {
                LogInvalidId(nameof(CreatePeriodTimerHandle), id);
                return null;
            }

            return new PeriodTimerHandle(new PeriodTimer(normalizedId, this, _isEnableLog));
        }

        private void LogInvalidId(string method, string id)
        {
            if(!_isEnableLog)
                return;

            string safeId = id == null ? "null" : id.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
            Debug.LogWarning($"[PeriodTimerManager:{method}] 유효하지 않은 ID 요청을 무시합니다: '{safeId}'");
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
            public void OnUpdatedCallback(int remainMin)
            {
                if(_manager == null)
                    return;

                _manager.OnTimerUpdatedCallback(_id, remainMin);
            }
        }
    }
}
