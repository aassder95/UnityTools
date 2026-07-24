using System.Collections;
using System.Collections.Generic;

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

        public bool TryDequeue(out T item)
        {
            if (_deque.Count == 0)
            {
                item = default;
                return false;
            }

            item = _deque.First.Value;
            _deque.RemoveFirst();
            return true;
        }

        public bool TryDequeueBack(out T item)
        {
            if (_deque.Count == 0)
            {
                item = default;
                return false;
            }

            item = _deque.Last.Value;
            _deque.RemoveLast();
            return true;
        }

        public bool TryPeek(out T item)
        {
            if (_deque.Count == 0)
            {
                item = default;
                return false;
            }

            item = _deque.First.Value;
            return true;
        }

        public bool TryPeekBack(out T item)
        {
            if (_deque.Count == 0)
            {
                item = default;
                return false;
            }

            item = _deque.Last.Value;
            return true;
        }

        public void Clear()
        {
            _deque.Clear();
        }

        //============================================================
        // Utilities
        //============================================================
        public IEnumerator<T> GetEnumerator()
        {
            foreach (T item in _deque)
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
