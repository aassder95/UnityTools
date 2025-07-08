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
        private readonly T _originObj;
        private readonly Queue<T> _pool = new Queue<T>();
        private readonly Func<T, T> _generator;

        public ObjectPool(int initialSize, T prefab, Transform parent)
        {
            if (prefab == null)
            {
                Debug.LogError("[ObjectPool] Prefab is null.");
                return;
            }

            _originObj = prefab;
            _generator = (original) =>
            {
                var newObj = UnityEngine.Object.Instantiate(original, parent);
                newObj.gameObject.SetActive(false);
                return newObj;
            };

            for (int i = 0; i < initialSize; i++)
            {
                T newObj = _generator(_originObj);
                _pool.Enqueue(newObj);
            }
        }

        public T Get()
        {
            T obj = _pool.Count > 0 ? _pool.Dequeue() : _generator(_originObj);
            obj.gameObject.SetActive(true);
            obj.OnGet();
            return obj;
        }

        public void Return(T obj)
        {
            if (obj == null)
            {
                Debug.LogWarning("[ObjectPool:Return] Object is null");
                return;
            }

            obj.OnReturn();
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }

        public void Clear()
        {
            _pool.Clear();
        }
    }
}
