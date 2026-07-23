using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.Core.Singleton;
using UnityTools.Util.Core.Timer;
using UnityTools.Util.Core.Timer.Period;
using UnityTools.Util.Utilities;

namespace UnityTools.Manager
{
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
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(!TryClearHandles())
                DebugLogger.LogError("PeriodTimerManager Handle 해제에 실패했습니다.", this);
        }

        //============================================================
        // Init/Register
        //============================================================
        public bool TryCreatePeriodTimerHandle(string id, out PeriodTimerHandle handle)
        {
            handle = null;
            if(!StringTokenUtils.TryNormalizeNonEmpty(id, out string normalizedId))
            {
                LogInvalidId(nameof(TryCreatePeriodTimerHandle), id);
                return false;
            }

            return _handleFactory.TryCreate(normalizedId, this, out handle);
        }

        public bool TryInitPeriodTimer(PeriodTimerHandle handle, double openMin, double closedMin, Func<IEnumerator> initWaitFunc = null)
        {
            if(handle == null)
            {
                DebugLogger.LogError("초기화할 PeriodTimerHandle이 비어 있습니다.", this);
                return false;
            }

            if(!StringTokenUtils.TryNormalizeNonEmpty(handle.Id, out string normalizedId))
            {
                LogInvalidId(nameof(TryInitPeriodTimer), handle.Id);
                return false;
            }

            if(!handle.TryInit(openMin, closedMin, initWaitFunc))
                return false;

            if(_handles.TryGet(normalizedId, out PeriodTimerHandle oldHandle))
            {
                UnbindEvents(normalizedId, oldHandle);
                if(!oldHandle.TryRelease())
                {
                    if(!handle.TryRelease())
                        DebugLogger.LogError("PeriodTimer 기존 Handle 교체 실패 후 새 Handle도 해제하지 못했습니다. ID=" + normalizedId, this);

                    return false;
                }
            }

            if(!_handles.TrySetOrReplace(normalizedId, handle, out _))
            {
                if(!handle.TryRelease())
                    DebugLogger.LogError("PeriodTimer Handle 등록 실패 후 새 Handle을 해제하지 못했습니다. ID=" + normalizedId, this);

                return false;
            }

            BindEvents(normalizedId, handle);
            return true;
        }

        //============================================================
        // Logic
        //============================================================
        public bool TryDeletePeriodTimer(string id)
        {
            if(!StringTokenUtils.TryNormalizeNonEmpty(id, out string normalizedId))
            {
                LogInvalidId(nameof(TryDeletePeriodTimer), id);
                return false;
            }

            bool isSuccess = true;
            if(_handles.TryRemove(normalizedId, out PeriodTimerHandle handle))
            {
                UnbindEvents(normalizedId, handle);
                isSuccess = handle.TryRelease();
            }
            else
            {
                _eventBinders.Remove(normalizedId);
            }

            return PeriodTimerPersistence.TryDeleteAll(normalizedId) && isSuccess;
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

        private bool TryClearHandles()
        {
            string[] ids = new string[_eventBinders.Count];
            _eventBinders.Keys.CopyTo(ids, 0);
            foreach(string id in ids)
            {
                if(_handles.TryGet(id, out PeriodTimerHandle handle))
                    UnbindEvents(id, handle);
            }

            _eventBinders.Clear();
            return _handles.TryClear();
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OnPeriodTimerRemainMinUpdatedCallback(string id, int remainMin)
        {
            if(!_handles.TryGet(id, out PeriodTimerHandle handle))
            {
                DebugLogger.LogError("PeriodTimer Callback 대상 Handle을 찾을 수 없습니다. ID=" + StringTokenUtils.ToLogSafe(id), this);
                return;
            }

            if(handle.RemainingMin != remainMin)
                return;

            _onAnyTimerRemainMinUpdated?.Invoke(handle.ToData());
        }

        //============================================================
        // Utilities
        //============================================================
        public bool TryGetPeriodHandle(string id, out PeriodTimerHandle handle)
        {
            handle = null;
            if(!StringTokenUtils.TryNormalizeNonEmpty(id, out string normalizedId))
            {
                LogInvalidId(nameof(TryGetPeriodHandle), id);
                return false;
            }

            if(_handles.TryGet(normalizedId, out handle))
                return true;

            DebugLogger.LogError("등록된 PeriodTimerHandle을 찾을 수 없습니다. ID=" + normalizedId, this);
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
                _manager.OnPeriodTimerRemainMinUpdatedCallback(_id, remainMin);
            }
        }
    }
}
