using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Util
{
    public class Deque<T>
    {
        LinkedList<T> _list = new LinkedList<T>();

        public bool IsEmpty => Count == 0;
        public int Count => _list.Count;

        public void Enqueue(T item) => _list.AddLast(item);
        public void EnqueueFront(T item) => _list.AddFirst(item);

        public T Dequeue() => RemoveNode(_list.First);
        public T DequeueBack() => RemoveNode(_list.Last);

        public T Peek() => PeekNode(_list.First);
        public T PeekBack() => PeekNode(_list.Last);

        public void Clear() => _list.Clear();

        T PeekNode(LinkedListNode<T> node)
        {
            if (IsEmpty)
            {
                Debug.LogWarning("[Deque:PeekNode] IsEmpty");
                return default;
            }

            return node.Value;
        }

        T RemoveNode(LinkedListNode<T> node)
        {
            if (IsEmpty)
            {
                Debug.LogWarning("[Deque:RemoveNode] IsEmpty");
                return default;
            }

            T value = node.Value;
            _list.Remove(node);
            return value;
        }
    }
}