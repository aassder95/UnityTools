using System;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Util.Constants;
using UnityTools.Util.Core.Collections;
using UnityTools.Util.Core.Events;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.Core.Persistence;
using UnityTools.Util.Core.Pooling;
using UnityTools.Util.Core.Singleton;
using UnityTools.Util.Core.State;
using UnityTools.Util.Core.Timer.Period;
using UnityTools.Util.Core.Timer.Shared;
using UnityTools.Util.Core.Timer.Task;
using UnityTools.Util.Coroutines;
using UnityTools.Util.Extensions;
using UnityTools.Util.UIFramework;
using UnityTools.Util.Utilities;

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
                DebugLogger.LogError("?�리??참조가 비어 ?�습?�다.");
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
                DebugLogger.LogWarning("반환 ?�???�브?�트가 비어 ?�습?�다.");
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
