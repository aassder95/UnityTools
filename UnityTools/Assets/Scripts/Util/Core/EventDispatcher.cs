using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Util
{
    // Exception: type-centric file uses Types section.
    //============================================================
    //Types
    //============================================================
    public enum EEventDispatcherType
    {

    }

    public class EventDispatcher : MonoSingleton<EventDispatcher>
    {
        private const int DEFAULT_PRIORITY = 0;
        private readonly Dictionary<EEventDispatcherType, SortedList<int, List<Delegate>>> _events = new();

        public delegate void EventDelegate(object sender);
        public delegate void EventDelegate<T>(object sender, T param);

        public void Subscribe(EEventDispatcherType key, EventDelegate listener, int priority = DEFAULT_PRIORITY) => Add(key, listener, priority);
        public void Subscribe<T>(EEventDispatcherType key, EventDelegate<T> listener, int priority = DEFAULT_PRIORITY) => Add(key, listener, priority);
        public void Unsubscribe(EEventDispatcherType key, EventDelegate listener) => Remove(key, listener);
        public void Unsubscribe<T>(EEventDispatcherType key, EventDelegate<T> listener) => Remove(key, listener);
        public void Dispatch(EEventDispatcherType key, object sender) => Invoke(key, del => ((EventDelegate)del)?.Invoke(sender));
        public void Dispatch<T>(EEventDispatcherType key, object sender, T param) => Invoke(key, del => ((EventDelegate<T>)del)?.Invoke(sender, param));

        private void Add(EEventDispatcherType key, Delegate listener, int priority)
        {
            var priorityList = GetPriorityList(key);
            var listeners = GetListeners(priorityList, priority);

            if (!listeners.Contains(listener))
                listeners.Add(listener);
        }

        private void Remove(EEventDispatcherType key, Delegate listener)
        {
            if (!_events.TryGetValue(key, out var priorityList))
                return;

            foreach (var listeners in priorityList.Values)
            {
                listeners.Remove(listener);
            }
        }

        private void Invoke(EEventDispatcherType key, Action<Delegate> onAction)
        {
            if (!_events.TryGetValue(key, out var priorityList))
                return;

            foreach (var listeners in priorityList.Values)
            {
                foreach (var listener in listeners)
                {
                    try
                    {
                        onAction(listener);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[EventDispatcher:Invoke] {key}: {ex}");
                    }
                }
            }
        }

        private SortedList<int, List<Delegate>> GetPriorityList(EEventDispatcherType key)
        {
            if (!_events.TryGetValue(key, out var priorityList))
            {
                priorityList = new SortedList<int, List<Delegate>>();
                _events[key] = priorityList;
            }

            return priorityList;
        }

        private List<Delegate> GetListeners(SortedList<int, List<Delegate>> priorityList, int priority)
        {
            if (!priorityList.TryGetValue(priority, out var listeners))
            {
                listeners = new List<Delegate>();
                priorityList[priority] = listeners;
            }

            return listeners;
        }
    }
}

