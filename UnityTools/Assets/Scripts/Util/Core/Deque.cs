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

        public void Enqueue(T item) => _deque.AddLast(item);
        public void EnqueueFront(T item) => _deque.AddFirst(item);

        public T Dequeue() => RemoveNode(_deque.First);
        public T DequeueBack() => RemoveNode(_deque.Last);

        public T Peek() => PeekNode(_deque.First);
        public T PeekBack() => PeekNode(_deque.Last);

        public void Clear() => _deque.Clear();

        public void ForEach(UnityAction<T> onAction)
        {
            foreach (var item in _deque)
            {
                onAction(item);
            }
        }

        public T FirstOrDefault(Func<T, bool> cond)
        {
            foreach (var item in _deque)
            {
                if (cond(item))
                    return item;
            }

            return default;
        }

        private T PeekNode(LinkedListNode<T> node)
        {
            if (IsEmpty)
                return default;

            return node.Value;
        }

        private T RemoveNode(LinkedListNode<T> node)
        {
            if (IsEmpty)
                return default;

            T value = node.Value;
            _deque.Remove(node);
            return value;
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