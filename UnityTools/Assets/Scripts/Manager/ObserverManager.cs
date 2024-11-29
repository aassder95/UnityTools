using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityTools.Util;

namespace UnityTools.Manager
{
    public class ObserverManager : Singleton<ObserverManager>
    {
        readonly Dictionary<string, SortedList<int, List<Delegate>>> _events = new();

        public void Subscribe(string key, UnityAction listener, int priority = 0) => Add(key, listener, priority);
        public void Subscribe<T>(string key, UnityAction<T> listener, int priority = 0) => Add(key, listener, priority);

        public void Unsubscribe(string key, UnityAction listener) => Remove(key, listener);
        public void Unsubscribe<T>(string key, UnityAction<T> listener) => Remove(key, listener);

        public void Dispatch(string key) => Invoke(key, listener => ((UnityAction)listener)?.Invoke());
        public void Dispatch<T>(string key, T param) => Invoke(key, listener => ((UnityAction<T>)listener)?.Invoke(param));

        void Add(string key, Delegate listener, int priority)
        {
            var priorityList = GetOrCreatePriorityList(key);
            var listeners = GetOrCreateListeners(priorityList, priority);

            if (!listeners.Contains(listener))
                listeners.Add(listener);
        }

        void Remove(string key, Delegate listener)
        {
            if (_events.TryGetValue(key, out var priorityList))
            {
                foreach (var listeners in priorityList.Values)
                {
                    listeners.Remove(listener);
                }
            }
        }

        void Invoke(string key, UnityAction<Delegate> onInvoke)
        {
            if (_events.TryGetValue(key, out var priorityList))
            {
                foreach (var listeners in priorityList.Values)
                {
                    foreach (var listener in listeners)
                    {
                        onInvoke(listener);
                    }
                }
            }
        }

        SortedList<int, List<Delegate>> GetOrCreatePriorityList(string key)
        {
            if (!_events.TryGetValue(key, out var priorityList))
            {
                priorityList = new();
                _events[key] = priorityList;
            }

            return priorityList;
        }

        List<Delegate> GetOrCreateListeners(SortedList<int, List<Delegate>> priorityList, int priority)
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