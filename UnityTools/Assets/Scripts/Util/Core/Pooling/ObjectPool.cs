using System;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Util.Core.Pooling
{
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
                DebugLogger.LogError("?留??꾨━??李몄“媛 鍮꾩뼱 ?덉뒿?덈떎.");
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
                DebugLogger.LogWarning("諛섑솚 ????ㅻ툕?앺듃媛 鍮꾩뼱 ?덉뒿?덈떎.");
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
