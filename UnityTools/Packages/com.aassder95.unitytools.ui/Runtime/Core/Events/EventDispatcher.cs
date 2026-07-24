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
            if (!TryValidateKey(key) || listener == null)
            {
                if (listener == null)
                    DebugLogger.LogError("구독할 이벤트 Listener가 비어 있습니다.");

                return false;
            }

            if (!CanUseListenerType(key, typeof(EventDelegate)))
                return false;

            SortedList<int, List<Delegate>> listenersByPriority = GetListenersByPriority(key);
            List<Delegate> listeners = GetListeners(listenersByPriority, priority);
            if (!listeners.Contains(listener))
                listeners.Add(listener);

            return true;
        }

        public bool TrySubscribe<T>(string key, EventDelegate<T> listener, int priority = DEFAULT_PRIORITY)
        {
            if (!TryValidateKey(key) || listener == null)
            {
                if (listener == null)
                    DebugLogger.LogError("구독할 이벤트 Listener가 비어 있습니다.");

                return false;
            }

            if (!CanUseListenerType(key, typeof(EventDelegate<T>)))
                return false;

            SortedList<int, List<Delegate>> listenersByPriority = GetListenersByPriority(key);
            List<Delegate> listeners = GetListeners(listenersByPriority, priority);
            if (!listeners.Contains(listener))
                listeners.Add(listener);

            return true;
        }

        public bool TryUnsubscribe(string key, EventDelegate listener)
        {
            if (!TryValidateKey(key) || listener == null)
            {
                if (listener == null)
                    DebugLogger.LogError("해제할 이벤트 Listener가 비어 있습니다.");

                return false;
            }

            return TryRemoveListener(key, listener);
        }

        public bool TryUnsubscribe<T>(string key, EventDelegate<T> listener)
        {
            if (!TryValidateKey(key) || listener == null)
            {
                if (listener == null)
                    DebugLogger.LogError("해제할 이벤트 Listener가 비어 있습니다.");

                return false;
            }

            return TryRemoveListener(key, listener);
        }

        public bool TryDispatch(string key)
        {
            if (!TryValidateKey(key))
                return false;

            if (!_events.TryGetValue(key, out SortedList<int, List<Delegate>> listenersByPriority))
                return true;

            foreach (List<Delegate> listeners in listenersByPriority.Values)
            {
                Delegate[] listenersSnapshot = listeners.ToArray();
                for (int i = 0; i < listenersSnapshot.Length; i++)
                {
                    if (listenersSnapshot[i] is not EventDelegate listener)
                    {
                        DebugLogger.LogError("이벤트 Listener 형식이 Dispatch 인자와 일치하지 않습니다. 키=" + key);
                        return false;
                    }

                    listener();
                }
            }

            return true;
        }

        public bool TryDispatch<T>(string key, T param)
        {
            if (!TryValidateKey(key))
                return false;

            if (!_events.TryGetValue(key, out SortedList<int, List<Delegate>> listenersByPriority))
                return true;

            foreach (List<Delegate> listeners in listenersByPriority.Values)
            {
                Delegate[] listenersSnapshot = listeners.ToArray();
                for (int i = 0; i < listenersSnapshot.Length; i++)
                {
                    if (listenersSnapshot[i] is not EventDelegate<T> listener)
                    {
                        DebugLogger.LogError("이벤트 Listener 형식이 Dispatch 인자와 일치하지 않습니다. 키=" + key);
                        return false;
                    }

                    listener(param);
                }
            }

            return true;
        }

        private bool CanUseListenerType(string key, Type listenerType)
        {
            if (!_events.TryGetValue(key, out SortedList<int, List<Delegate>> listenersByPriority))
                return true;

            foreach (List<Delegate> listeners in listenersByPriority.Values)
            {
                for (int i = 0; i < listeners.Count; i++)
                {
                    if (listeners[i].GetType() == listenerType)
                        continue;

                    DebugLogger.LogError("같은 이벤트 키에 서로 다른 Listener 형식을 등록할 수 없습니다. 키=" + key);
                    return false;
                }
            }

            return true;
        }

        private bool TryRemoveListener(string key, Delegate listener)
        {
            if (!_events.TryGetValue(key, out SortedList<int, List<Delegate>> listenersByPriority))
            {
                DebugLogger.LogError("해제할 이벤트 키가 등록되어 있지 않습니다. 키=" + key);
                return false;
            }

            bool isRemoved = false;
            List<int> emptyPriorities = null;
            foreach (KeyValuePair<int, List<Delegate>> pair in listenersByPriority)
            {
                isRemoved |= pair.Value.Remove(listener);
                if (pair.Value.Count > 0)
                    continue;

                emptyPriorities ??= new List<int>();
                emptyPriorities.Add(pair.Key);
            }

            if (!isRemoved)
            {
                DebugLogger.LogError("해제할 이벤트 Listener가 등록되어 있지 않습니다. 키=" + key);
                return false;
            }

            if (emptyPriorities != null)
            {
                for (int i = 0; i < emptyPriorities.Count; i++)
                {
                    listenersByPriority.Remove(emptyPriorities[i]);
                }
            }

            if (listenersByPriority.Count == 0)
                _events.Remove(key);

            return true;
        }

        private static bool TryValidateKey(string key)
        {
            if (!string.IsNullOrWhiteSpace(key))
                return true;

            DebugLogger.LogError("이벤트 키가 비어 있습니다.");
            return false;
        }

        //============================================================
        // Utilities
        //============================================================
        private SortedList<int, List<Delegate>> GetListenersByPriority(string key)
        {
            if (!_events.TryGetValue(key, out SortedList<int, List<Delegate>> listenersByPriority))
            {
                listenersByPriority = new SortedList<int, List<Delegate>>();
                _events[key] = listenersByPriority;
            }

            return listenersByPriority;
        }

        private static List<Delegate> GetListeners(SortedList<int, List<Delegate>> listenersByPriority, int priority)
        {
            if (!listenersByPriority.TryGetValue(priority, out List<Delegate> listeners))
            {
                listeners = new List<Delegate>();
                listenersByPriority[priority] = listeners;
            }

            return listeners;
        }
    }
}
