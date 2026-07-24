using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityTools.Timer.Period;
using UnityTools.Timer.Persistence;

namespace UnityTools.Timer
{
    public class PeriodTimerService
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly MonoBehaviour _runner;
        private readonly IStorage _storage;
        private readonly Func<DateTime> _utcNow;
        private readonly Dictionary<string, PeriodTimerHandle> _handles = new();
        private readonly Dictionary<string, PeriodTimerEventBinder> _eventBinders = new();

        //============================================================
        // Events
        //============================================================
        public event UnityAction<PeriodTimerData> OnRemainMinUpdated { add => _onRemainMinUpdated += value; remove => _onRemainMinUpdated -= value; }
        private event UnityAction<PeriodTimerData> _onRemainMinUpdated;

        //============================================================
        // Constructors
        //============================================================
        public PeriodTimerService(MonoBehaviour runner, IStorage storage = null, Func<DateTime> utcNow = null)
        {
            _runner = runner;
            _storage = storage ?? new PlayerPrefsStorage();
            _utcNow = utcNow;
        }

        //============================================================
        // Init/Register
        //============================================================
        public bool TryCreate(string id, out PeriodTimerHandle handle)
        {
            handle = null;
            if (!TryNormalizeId(id, out string normalizedId))
                return false;

            if (!PeriodTimer.TryCreate(normalizedId, _runner, out PeriodTimer timer, _storage, _utcNow))
                return false;

            handle = new PeriodTimerHandle(timer);
            return true;
        }

        public bool TryInit(PeriodTimerHandle handle, double openMin, double closedMin, Func<IEnumerator> initWaitFunc = null)
        {
            if (handle == null || !TryNormalizeId(handle.Id, out string normalizedId))
                return false;

            if (!handle.TryInit(openMin, closedMin, initWaitFunc))
                return false;

            if (_handles.TryGetValue(normalizedId, out PeriodTimerHandle oldHandle))
            {
                UnbindEvents(normalizedId, oldHandle);
                if (oldHandle != handle)
                    oldHandle.Release();
            }

            _handles[normalizedId] = handle;
            BindEvents(normalizedId, handle);
            return true;
        }

        public void Release()
        {
            string[] ids = new string[_eventBinders.Count];
            _eventBinders.Keys.CopyTo(ids, 0);
            for (int i = 0; i < ids.Length; i++)
            {
                UnbindEvents(ids[i], _handles[ids[i]]);
            }

            foreach (PeriodTimerHandle handle in _handles.Values)
            {
                handle.Release();
            }

            _handles.Clear();
            _eventBinders.Clear();
        }

        //============================================================
        // Logic
        //============================================================
        public bool TryDelete(string id)
        {
            if (!TryNormalizeId(id, out string normalizedId))
                return false;

            if (!PeriodTimerPersistence.TryDeleteAll(normalizedId, _storage))
                return false;

            if (_handles.Remove(normalizedId, out PeriodTimerHandle handle))
            {
                UnbindEvents(normalizedId, handle);
                handle.Release();
            }

            return true;
        }

        public bool TryGetHandle(string id, out PeriodTimerHandle handle)
        {
            handle = null;
            return TryNormalizeId(id, out string normalizedId) && _handles.TryGetValue(normalizedId, out handle);
        }

        private void BindEvents(string id, PeriodTimerHandle handle)
        {
            PeriodTimerEventBinder eventBinder = new(handle, OnRemainMinUpdatedCallback);
            _eventBinders[id] = eventBinder;
            handle.OnRemainMinUpdated += eventBinder.OnRemainMinUpdatedCallback;
        }

        private void UnbindEvents(string id, PeriodTimerHandle handle)
        {
            PeriodTimerEventBinder eventBinder = _eventBinders[id];
            handle.OnRemainMinUpdated -= eventBinder.OnRemainMinUpdatedCallback;
            _eventBinders.Remove(id);
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OnRemainMinUpdatedCallback(PeriodTimerHandle handle, int remainMin)
        {
            if (handle.RemainingMin != remainMin)
                return;

            _onRemainMinUpdated?.Invoke(handle.ToData());
        }

        //============================================================
        // Utilities
        //============================================================
        private static bool TryNormalizeId(string id, out string normalizedId)
        {
            normalizedId = id?.Trim();
            return !string.IsNullOrEmpty(normalizedId);
        }

        //============================================================
        // Nested Types
        //============================================================
        private class PeriodTimerEventBinder
        {
            //============================================================
            // Readonly
            //============================================================
            private readonly PeriodTimerHandle _handle;
            private readonly Action<PeriodTimerHandle, int> _onRemainMinUpdated;

            //============================================================
            // Constructors
            //============================================================
            public PeriodTimerEventBinder(PeriodTimerHandle handle, Action<PeriodTimerHandle, int> onRemainMinUpdated)
            {
                _handle = handle;
                _onRemainMinUpdated = onRemainMinUpdated;
            }

            //============================================================
            // Callbacks
            //============================================================
            public void OnRemainMinUpdatedCallback(int remainMin)
            {
                _onRemainMinUpdated.Invoke(_handle, remainMin);
            }
        }
    }
}
