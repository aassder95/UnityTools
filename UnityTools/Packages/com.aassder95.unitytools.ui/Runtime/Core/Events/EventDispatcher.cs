using System;
using System.Collections.Generic;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.Core.Singleton;

namespace UnityTools.Util.Core.Events
{
    public class EventDispatcher : MonoSingleton<EventDispatcher>
    {
        //============================================================
        // Constants
        //============================================================
        private const int DEFAULT_PRIORITY = 0;

        //============================================================
        // Readonly
        //============================================================
        private readonly Dictionary<string, SortedList<int, List<Delegate>>> _events = new(StringComparer.Ordinal);

        //============================================================
        // Logic
        //============================================================
        public bool TrySubscribe(string key, EventDelegate listener, int priority = DEFAULT_PRIORITY)
        {
            if(!ValidateKey(key) || listener == null)
            {
                if(listener == null)
                    DebugLogger.LogError("구독할 이벤트 Listener가 비어 있습니다.");

                return false;
            }

            if(!CanUseListenerType(key, typeof(EventDelegate)))
                return false;

            SortedList<int, List<Delegate>> priorityList = GetPriorityList(key);
            List<Delegate> listeners = GetListeners(priorityList, priority);
            if(!listeners.Contains(listener))
                listeners.Add(listener);

            return true;
        }

        public bool TrySubscribe<T>(string key, EventDelegate<T> listener, int priority = DEFAULT_PRIORITY)
        {
            if(!ValidateKey(key) || listener == null)
            {
                if(listener == null)
                    DebugLogger.LogError("구독할 이벤트 Listener가 비어 있습니다.");

                return false;
            }

            if(!CanUseListenerType(key, typeof(EventDelegate<T>)))
                return false;

            SortedList<int, List<Delegate>> priorityList = GetPriorityList(key);
            List<Delegate> listeners = GetListeners(priorityList, priority);
            if(!listeners.Contains(listener))
                listeners.Add(listener);

            return true;
        }

        public bool TryUnsubscribe(string key, EventDelegate listener)
        {
            if(!ValidateKey(key) || listener == null)
            {
                if(listener == null)
                    DebugLogger.LogError("해제할 이벤트 Listener가 비어 있습니다.");

                return false;
            }

            return TryRemoveListener(key, listener);
        }

        public bool TryUnsubscribe<T>(string key, EventDelegate<T> listener)
        {
            if(!ValidateKey(key) || listener == null)
            {
                if(listener == null)
                    DebugLogger.LogError("해제할 이벤트 Listener가 비어 있습니다.");

                return false;
            }

            return TryRemoveListener(key, listener);
        }

        public bool TryDispatch(string key)
        {
            if(!ValidateKey(key))
                return false;

            if(!_events.TryGetValue(key, out SortedList<int, List<Delegate>> priorityList))
                return true;

            foreach(List<Delegate> listeners in priorityList.Values)
            {
                Delegate[] listenerSnapshot = listeners.ToArray();
                for(int i = 0; i < listenerSnapshot.Length; i++)
                {
                    if(listenerSnapshot[i] is not EventDelegate listener)
                    {
                        DebugLogger.LogError("이벤트 Listener 형식이 Dispatch 인자와 일치하지 않습니다. 키=" + key);
                        return false;
                    }

                    try
                    {
                        listener();
                    }
                    catch(Exception exception)
                    {
                        DebugLogger.LogError("이벤트 Listener 실행에 실패했습니다. 키=" + key + ", 원인=" + exception.Message);
                        return false;
                    }
                }
            }

            return true;
        }

        public bool TryDispatch<T>(string key, T param)
        {
            if(!ValidateKey(key))
                return false;

            if(!_events.TryGetValue(key, out SortedList<int, List<Delegate>> priorityList))
                return true;

            foreach(List<Delegate> listeners in priorityList.Values)
            {
                Delegate[] listenerSnapshot = listeners.ToArray();
                for(int i = 0; i < listenerSnapshot.Length; i++)
                {
                    if(listenerSnapshot[i] is not EventDelegate<T> listener)
                    {
                        DebugLogger.LogError("이벤트 Listener 형식이 Dispatch 인자와 일치하지 않습니다. 키=" + key);
                        return false;
                    }

                    try
                    {
                        listener(param);
                    }
                    catch(Exception exception)
                    {
                        DebugLogger.LogError("이벤트 Listener 실행에 실패했습니다. 키=" + key + ", 원인=" + exception.Message);
                        return false;
                    }
                }
            }

            return true;
        }

        private bool CanUseListenerType(string key, Type listenerType)
        {
            if(!_events.TryGetValue(key, out SortedList<int, List<Delegate>> priorityList))
                return true;

            foreach(List<Delegate> listeners in priorityList.Values)
            {
                for(int i = 0; i < listeners.Count; i++)
                {
                    if(listeners[i].GetType() == listenerType)
                        continue;

                    DebugLogger.LogError("같은 이벤트 키에 서로 다른 Listener 형식을 등록할 수 없습니다. 키=" + key);
                    return false;
                }
            }

            return true;
        }

        private bool TryRemoveListener(string key, Delegate listener)
        {
            if(!_events.TryGetValue(key, out SortedList<int, List<Delegate>> priorityList))
            {
                DebugLogger.LogError("해제할 이벤트 키가 등록되어 있지 않습니다. 키=" + key);
                return false;
            }

            bool isRemoved = false;
            List<int> emptyPriorities = null;
            foreach(KeyValuePair<int, List<Delegate>> pair in priorityList)
            {
                isRemoved |= pair.Value.Remove(listener);
                if(pair.Value.Count > 0)
                    continue;

                emptyPriorities ??= new List<int>();
                emptyPriorities.Add(pair.Key);
            }

            if(!isRemoved)
            {
                DebugLogger.LogError("해제할 이벤트 Listener가 등록되어 있지 않습니다. 키=" + key);
                return false;
            }

            if(emptyPriorities != null)
            {
                for(int i = 0; i < emptyPriorities.Count; i++)
                {
                    priorityList.Remove(emptyPriorities[i]);
                }
            }

            if(priorityList.Count == 0)
                _events.Remove(key);

            return true;
        }

        private static bool ValidateKey(string key)
        {
            if(!string.IsNullOrWhiteSpace(key))
                return true;

            DebugLogger.LogError("이벤트 키가 비어 있습니다.");
            return false;
        }

        //============================================================
        // Utilities
        //============================================================
        private SortedList<int, List<Delegate>> GetPriorityList(string key)
        {
            if(!_events.TryGetValue(key, out SortedList<int, List<Delegate>> priorityList))
            {
                priorityList = new SortedList<int, List<Delegate>>();
                _events[key] = priorityList;
            }

            return priorityList;
        }

        private static List<Delegate> GetListeners(SortedList<int, List<Delegate>> priorityList, int priority)
        {
            if(!priorityList.TryGetValue(priority, out List<Delegate> listeners))
            {
                listeners = new List<Delegate>();
                priorityList[priority] = listeners;
            }

            return listeners;
        }
    }
}
