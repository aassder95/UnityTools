using System.Collections.Generic;
using UnityEngine;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Util.Core.Pooling
{
    public class ObjectPool<T> where T : Component, IPoolable
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _objects = new();
        private readonly HashSet<T> _pooledObjects = new();

        //============================================================
        // Constructors
        //============================================================
        private ObjectPool(T prefab, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;
        }

        //============================================================
        // Init/Register
        //============================================================
        public static ObjectPool<T> Create(int initialSize, T prefab, Transform parent)
        {
            if(initialSize < 0)
            {
                DebugLogger.LogError("오브젝트 풀 초기 크기는 0 이상이어야 합니다. 값=" + initialSize);
                return null;
            }

            ObjectPool<T> pool = new(prefab, parent);
            for(int i = 0; i < initialSize; i++)
            {
                T newObj = pool.CreateObject();
                pool._objects.Enqueue(newObj);
                pool._pooledObjects.Add(newObj);
            }

            return pool;
        }

        //============================================================
        // Logic
        //============================================================
        public T Get()
        {
            T obj;
            if(_objects.Count > 0)
            {
                obj = _objects.Dequeue();
                _pooledObjects.Remove(obj);
            }
            else
            {
                obj = CreateObject();
            }

            obj.gameObject.SetActive(true);
            obj.OnGet();
            return obj;
        }

        public bool TryReturn(T obj)
        {
            if(obj == null)
            {
                DebugLogger.LogError("오브젝트 풀에 반환할 객체가 비어 있습니다.");
                return false;
            }

            if(_pooledObjects.Contains(obj))
            {
                DebugLogger.LogError("이미 풀에 들어 있는 객체를 중복 반환했습니다. 이름=" + obj.name);
                return false;
            }

            obj.OnReturn();
            obj.gameObject.SetActive(false);
            _objects.Enqueue(obj);
            _pooledObjects.Add(obj);
            return true;
        }

        public void Clear()
        {
            while(_objects.Count > 0)
            {
                DestroyObject(_objects.Dequeue());
            }

            _pooledObjects.Clear();
        }

        //============================================================
        // Utilities
        //============================================================
        private T CreateObject()
        {
            T newObj = UnityEngine.Object.Instantiate(_prefab, _parent);
            newObj.gameObject.SetActive(false);
            return newObj;
        }

        private static void DestroyObject(T obj)
        {
            if(obj != null)
                UnityEngine.Object.Destroy(obj.gameObject);
        }
    }
}
