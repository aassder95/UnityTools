using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Util
{
    // Exception: type-centric file uses Types section.
    //============================================================
    //Types
    //============================================================
    public interface IPoolable
    {
        //============================================================
        //Callbacks
        //============================================================
        void OnGet();
        void OnReturn();
    }

    public class ObjectPool<T> where T : Component, IPoolable
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly T _originObj;
        private readonly Queue<T> _pool = new();
        private readonly Func<T, T> _generator;

        //============================================================
        //Constructors
        //============================================================
        public ObjectPool(int initialSize, T prefab, Transform parent)
        {
            if(prefab == null)
            {
                DebugLogger.LogError("프리팹 참조가 비어 있습니다.");
                return;
            }

            _originObj = prefab;
            _generator = original =>
            {
                T newObj = UnityEngine.Object.Instantiate(original, parent);
                newObj.gameObject.SetActive(false);
                return newObj;
            };

            for(int i = 0; i < initialSize; i++)
            {
                T newObj = _generator(_originObj);
                _pool.Enqueue(newObj);
            }
        }

        //============================================================
        //Logic
        //============================================================
        public T Get()
        {
            T obj = _pool.Count > 0 ? _pool.Dequeue() : _generator(_originObj);
            obj.gameObject.SetActive(true);
            obj.OnGet();
            return obj;
        }

        public void Return(T obj)
        {
            if(obj == null)
            {
                DebugLogger.LogWarning("반환 대상 오브젝트가 비어 있습니다.");
                return;
            }

            obj.OnReturn();
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }

        public void Clear()
        {
            while(_pool.Count > 0)
            {
                T obj = _pool.Dequeue();
                if(obj != null)
                    UnityEngine.Object.Destroy(obj.gameObject);
            }
        }
    }
}
