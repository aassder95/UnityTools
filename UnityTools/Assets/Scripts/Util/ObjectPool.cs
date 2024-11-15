using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Util
{
    public interface IPoolable
    {
        void OnGet();
        void OnReturn();
    }

    public class ObjectPool<T> where T : Component, IPoolable
    {
        readonly T _originObj;
        readonly Queue<T> _actives = new Queue<T>();
        readonly Func<T, T> _generator;

        public ObjectPool(Transform trParent, T obj, int size)
        {
            _originObj = obj;
            _generator = (o) =>
            {
                var newObj = UnityEngine.Object.Instantiate(o, trParent);
                if (newObj == null)
                {
                    Debug.LogWarning($"[ObjectPool:_generator] Failed to instantiate: {o.name}");
                    return null;
                }

                newObj.name = o.name;
                newObj.gameObject.SetActive(false);
                return newObj;
            };

            for (int i = 0; i < size; i++)
            {
                T newObj = _generator(_originObj);
                if (newObj != null)
                    _actives.Enqueue(newObj);
            }
        }

        public T Get()
        {
            T obj = _actives.Count > 0 ? _actives.Dequeue() : _generator(_originObj);
            if (obj == null)
            {
                Debug.LogWarning("[ObjectPool:Get] Object null");
                return null;
            }

            obj.gameObject.SetActive(true);
            obj.OnGet();
            return obj;
        }

        public void Return(T obj)
        {
            if (obj == null)
            {
                Debug.LogWarning("[ObjectPool:Return] Object null");
                return;
            }

            obj.gameObject.SetActive(false);
            obj.OnReturn();
            _actives.Enqueue(obj);
        }

        public void Clear()
        {
            _actives.Clear();
        }
    }
}