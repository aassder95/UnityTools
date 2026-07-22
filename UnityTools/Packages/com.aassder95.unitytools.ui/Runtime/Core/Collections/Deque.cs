using System.Collections;
using System.Collections.Generic;

namespace UnityTools.Util.Core.Collections
{
    public class Deque<T> : IEnumerable<T>
    {
        //============================================================
        // Fields
        //============================================================
        private LinkedList<T> _deque = new();

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
            if(_deque == null)
                _deque = new LinkedList<T>();

            _deque.AddLast(item);
        }

        public void EnqueueFront(T item)
        {
            if(_deque == null)
                _deque = new LinkedList<T>();

            _deque.AddFirst(item);
        }

        public T Dequeue()
        {
            if(IsEmpty)
                return default;

            T value = _deque.First.Value;
            _deque.RemoveFirst();
            return value;
        }

        public T DequeueBack()
        {
            if(IsEmpty)
                return default;

            T value = _deque.Last.Value;
            _deque.RemoveLast();
            return value;
        }

        public T Peek()
        {
            if(IsEmpty)
                return default;

            return _deque.First.Value;
        }

        public void Clear()
        {
            if(IsEmpty)
                return;

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
