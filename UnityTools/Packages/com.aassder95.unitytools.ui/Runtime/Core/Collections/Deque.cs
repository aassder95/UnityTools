using System.Collections;
using System.Collections.Generic;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Util.Core.Collections
{
    public class Deque<T> : IEnumerable<T>
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly LinkedList<T> _deque = new();

        //============================================================
        // Properties
        //============================================================
        public bool IsEmpty => Count == 0;
        public int Count => _deque.Count;

        //============================================================
        // Logic
        //============================================================
        public void Enqueue(T item)
        {
            _deque.AddLast(item);
        }

        public void EnqueueFront(T item)
        {
            _deque.AddFirst(item);
        }

        public T Dequeue()
        {
            if(IsEmpty)
            {
                DebugLogger.LogError("비어 있는 Deque에서 앞쪽 Item을 제거할 수 없습니다.");
                return default;
            }

            T value = _deque.First.Value;
            _deque.RemoveFirst();
            return value;
        }

        public T DequeueBack()
        {
            if(IsEmpty)
            {
                DebugLogger.LogError("비어 있는 Deque에서 뒤쪽 Item을 제거할 수 없습니다.");
                return default;
            }

            T value = _deque.Last.Value;
            _deque.RemoveLast();
            return value;
        }

        public T Peek()
        {
            if(IsEmpty)
            {
                DebugLogger.LogError("비어 있는 Deque의 앞쪽 Item을 조회할 수 없습니다.");
                return default;
            }

            return _deque.First.Value;
        }

        public T PeekBack()
        {
            if(IsEmpty)
            {
                DebugLogger.LogError("비어 있는 Deque의 뒤쪽 Item을 조회할 수 없습니다.");
                return default;
            }

            return _deque.Last.Value;
        }

        public void Clear()
        {
            if(!IsEmpty)
                _deque.Clear();
        }

        //============================================================
        // Utilities
        //============================================================
        public IEnumerator<T> GetEnumerator()
        {
            foreach(T item in _deque)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
