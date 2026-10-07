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
        // Constants
        //============================================================
        private const string STORAGE_PREFIX = "PeriodTimer_";
        private const string SNAPSHOT_SUFFIX = "_SNAPSHOT";

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

        private event Action _onTimersChanged;
        public event Action OnTimersChanged { add => _onTimersChanged += value; remove => _onTimersChanged -= value; }

        //============================================================
        // Properties
        //============================================================
        public int TimerCnt => _handles.Count;

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
            _onTimersChanged?.Invoke();
            return true;
        }

        public bool TryUnregister(string id)
        {
            if (!TryNormalizeId(id, out string normalizedId) || !_handles.Remove(normalizedId, out PeriodTimerHandle handle))
                return false;

            UnbindEvents(normalizedId, handle);
            handle.Release();
            _onTimersChanged?.Invoke();
            return true;
        }

        public void Release()
        {
            bool hasTimers = _handles.Count > 0;
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
            if (hasTimers)
                _onTimersChanged?.Invoke();
        }

        //============================================================
        // Logic
        //============================================================
        public bool TryDelete(string id)
        {
            if (!TryNormalizeId(id, out string normalizedId))
                return false;

            if (!TryMarkDeleted(normalizedId))
                return false;

            if (_handles.Remove(normalizedId, out PeriodTimerHandle handle))
            {
                UnbindEvents(normalizedId, handle);
                handle.Release();
                _onTimersChanged?.Invoke();
            }

            return true;
        }

        public bool TryGetHandle(string id, out PeriodTimerHandle handle)
        {
            handle = null;
            return TryNormalizeId(id, out string normalizedId) && _handles.TryGetValue(normalizedId, out handle);
        }

        public PeriodTimerData[] GetSnapshots()
        {
            PeriodTimerData[] snapshots = new PeriodTimerData[_handles.Count];
            int idx = 0;
            foreach (PeriodTimerHandle handle in _handles.Values)
            {
                snapshots[idx++] = handle.ToData();
            }

            return snapshots;
        }

        private void BindEvents(string id, PeriodTimerHandle handle)
        {
            PeriodTimerEventBinder eventBinder = new(handle, OnRemainMinUpdatedCallback, OnPeriodStateTransitionCallback);
            _eventBinders[id] = eventBinder;
            handle.OnRemainMinUpdated += eventBinder.OnRemainMinUpdatedCallback;
            handle.OnPeriodStateTransition += eventBinder.OnPeriodStateTransitionCallback;
        }

        private void UnbindEvents(string id, PeriodTimerHandle handle)
        {
            PeriodTimerEventBinder eventBinder = _eventBinders[id];
            handle.OnRemainMinUpdated -= eventBinder.OnRemainMinUpdatedCallback;
            handle.OnPeriodStateTransition -= eventBinder.OnPeriodStateTransitionCallback;
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
            _onTimersChanged?.Invoke();
        }

        private void OnPeriodStateTransitionCallback(PeriodTimerHandle handle, EPeriodTimerType prevType, EPeriodTimerType nextType)
        {
            if (prevType == nextType)
                return;

            _onTimersChanged?.Invoke();
        }

        //============================================================
        // Utilities
        //============================================================
        private static bool TryNormalizeId(string id, out string normalizedId)
        {
            normalizedId = id?.Trim();
            return !string.IsNullOrEmpty(normalizedId);
        }

        private bool TryMarkDeleted(string id)
        {
            return _storage.TrySave($"{STORAGE_PREFIX}{id}{SNAPSHOT_SUFFIX}", "1|DELETED");
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
            private readonly Action<PeriodTimerHandle, EPeriodTimerType, EPeriodTimerType> _onPeriodStateTransition;

            //============================================================
            // Constructors
            //============================================================
            public PeriodTimerEventBinder(PeriodTimerHandle handle, Action<PeriodTimerHandle, int> onRemainMinUpdated, Action<PeriodTimerHandle, EPeriodTimerType, EPeriodTimerType> onPeriodStateTransition)
            {
                _handle = handle;
                _onRemainMinUpdated = onRemainMinUpdated;
                _onPeriodStateTransition = onPeriodStateTransition;
            }

            //============================================================
            // Callbacks
            //============================================================
            public void OnPeriodStateTransitionCallback(EPeriodTimerType prevType, EPeriodTimerType nextType)
            {
                _onPeriodStateTransition.Invoke(_handle, prevType, nextType);
            }

            public void OnRemainMinUpdatedCallback(int remainMin)
            {
                _onRemainMinUpdated.Invoke(_handle, remainMin);
            }
        }
    }
}
