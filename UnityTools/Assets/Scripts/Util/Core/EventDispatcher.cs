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
        //============================================================
        //Constants
        //============================================================
        private const int DEFAULT_PRIORITY = 0;

        //============================================================
        //Readonly
        //============================================================
        private readonly Dictionary<EEventDispatcherType, SortedList<int, List<Delegate>>> _events = new();

        //============================================================
        //Types
        //============================================================
        public delegate void EventDelegate(object sender);
        public delegate void EventDelegate<T>(object sender, T param);

        //============================================================
        //Logic
        //============================================================
        public void Subscribe(EEventDispatcherType key, EventDelegate listener, int priority = DEFAULT_PRIORITY)
        {
            if(listener == null)
                return;

            SortedList<int, List<Delegate>> priorityList = GetPriorityList(key);
            List<Delegate> listeners = GetListeners(priorityList, priority);
            if(!listeners.Contains(listener))
                listeners.Add(listener);
        }

        public void Subscribe<T>(EEventDispatcherType key, EventDelegate<T> listener, int priority = DEFAULT_PRIORITY)
        {
            if(listener == null)
                return;

            SortedList<int, List<Delegate>> priorityList = GetPriorityList(key);
            List<Delegate> listeners = GetListeners(priorityList, priority);
            if(!listeners.Contains(listener))
                listeners.Add(listener);
        }

        public void Unsubscribe(EEventDispatcherType key, EventDelegate listener)
        {
            if(listener == null || !_events.TryGetValue(key, out SortedList<int, List<Delegate>> priorityList))
                return;

            foreach (List<Delegate> listeners in priorityList.Values)
                listeners.Remove(listener);
        }

        public void Unsubscribe<T>(EEventDispatcherType key, EventDelegate<T> listener)
        {
            if(listener == null || !_events.TryGetValue(key, out SortedList<int, List<Delegate>> priorityList))
                return;

            foreach (List<Delegate> listeners in priorityList.Values)
                listeners.Remove(listener);
        }

        public void Dispatch(EEventDispatcherType key, object sender)
        {
            if(!_events.TryGetValue(key, out SortedList<int, List<Delegate>> priorityList))
                return;

            foreach (List<Delegate> listeners in priorityList.Values)
            {
                foreach (Delegate listener in listeners)
                {
                    try
                    {
                        ((EventDelegate)listener)?.Invoke(sender);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[EventDispatcher:Dispatch] {key}: {ex}");
                    }
                }
            }
        }

        public void Dispatch<T>(EEventDispatcherType key, object sender, T param)
        {
            if(!_events.TryGetValue(key, out SortedList<int, List<Delegate>> priorityList))
                return;

            foreach (List<Delegate> listeners in priorityList.Values)
            {
                foreach (Delegate listener in listeners)
                {
                    try
                    {
                        ((EventDelegate<T>)listener)?.Invoke(sender, param);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[EventDispatcher:Dispatch] {key}: {ex}");
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

