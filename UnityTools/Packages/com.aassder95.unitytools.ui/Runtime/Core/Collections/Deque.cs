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

        public T Dequeue()
        {
            T value = _deque.First.Value;
            _deque.RemoveFirst();
            return value;
        }

        public T DequeueBack()
        {
            T value = _deque.Last.Value;
            _deque.RemoveLast();
            return value;
        }

        public T Peek()
        {
            return _deque.First.Value;
        }

        public T PeekBack()
        {
            return _deque.Last.Value;
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
