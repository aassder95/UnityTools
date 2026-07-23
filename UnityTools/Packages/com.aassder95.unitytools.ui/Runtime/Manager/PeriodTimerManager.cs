using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
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
            ClearHandles();
        }

        //============================================================
        // Init/Register
        //============================================================
        public bool TryCreatePeriodTimerHandle(string id, out PeriodTimerHandle handle)
        {
            handle = null;
            if(!StringTokenUtils.TryNormalizeNonEmpty(id, out string normalizedId))
                return false;

            handle = new PeriodTimerHandle(PeriodTimer.Create(normalizedId, this));
            return true;
        }

        public bool TryInitPeriodTimer(PeriodTimerHandle handle, double openMin, double closedMin, Func<IEnumerator> initWaitFunc = null)
        {
            if(handle == null || !StringTokenUtils.TryNormalizeNonEmpty(handle.Id, out string normalizedId))
                return false;

            if(!handle.TryInit(openMin, closedMin, initWaitFunc))
                return false;

            PeriodTimerHandle oldHandle = _handles.SetOrReplace(normalizedId, handle);
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
        public bool TryDeletePeriodTimer(string id)
        {
            if(!StringTokenUtils.TryNormalizeNonEmpty(id, out string normalizedId))
                return false;

            if(_handles.TryRemove(normalizedId, out PeriodTimerHandle handle))
            {
                UnbindEvents(normalizedId, handle);
                handle.Release();
            }

            PeriodTimerPersistence.DeleteAll(normalizedId);
            return true;
        }

        private void BindEvents(string id, PeriodTimerHandle handle)
        {
            PeriodTimerEventBinder eventBinder = new(this, handle);
            _eventBinders[id] = eventBinder;
            handle.OnRemainMinUpdated += eventBinder.OnRemainMinUpdatedCallback;
        }

        private void UnbindEvents(string id, PeriodTimerHandle handle)
        {
            PeriodTimerEventBinder eventBinder = _eventBinders[id];
            handle.OnRemainMinUpdated -= eventBinder.OnRemainMinUpdatedCallback;
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
        private void OnPeriodTimerRemainMinUpdatedCallback(PeriodTimerHandle handle, int remainMin)
        {
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
            return StringTokenUtils.TryNormalizeNonEmpty(id, out string normalizedId) && _handles.TryGet(normalizedId, out handle);
        }

        //============================================================
        // Nested Types
        //============================================================
        private class PeriodTimerEventBinder
        {
            //============================================================
            // Readonly
            //============================================================
            private readonly PeriodTimerManager _manager;
            private readonly PeriodTimerHandle _handle;

            //============================================================
            // Properties
            //============================================================
            public PeriodTimerHandle Handle => _handle;

            //============================================================
            // Constructors
            //============================================================
            public PeriodTimerEventBinder(PeriodTimerManager manager, PeriodTimerHandle handle)
            {
                _manager = manager;
                _handle = handle;
            }

            //============================================================
            // Callbacks
            //============================================================
            public void OnRemainMinUpdatedCallback(int remainMin)
            {
                _manager.OnPeriodTimerRemainMinUpdatedCallback(_handle, remainMin);
            }
        }
    }
}
