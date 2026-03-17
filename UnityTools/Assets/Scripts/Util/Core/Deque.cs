using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityTools.Util
{
    //============================================================
    //Logic
    //============================================================
    public class Deque<T> : IEnumerable<T>
    {
        private LinkedList<T> _deque = new LinkedList<T>();

        public bool IsEmpty => Count == 0;
        public int Count => _deque.Count;

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

        public T PeekBack()
        {
            if(IsEmpty)
                return default;

            return _deque.Last.Value;
        }

        public void Clear()
        {
            if(IsEmpty)
                return;

            _deque.Clear();
        }

        public void ForEach(UnityAction<T> onAction)
        {
            if(onAction == null || IsEmpty)
                return;

            foreach (var item in _deque)
            {
                onAction(item);
            }
        }

        public T FirstOrDefault(Func<T, bool> cond)
        {
            if(cond == null || IsEmpty)
                return default;

            foreach (var item in _deque)
            {
                if (cond(item))
                    return item;
            }

            return default;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in _deque)
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
