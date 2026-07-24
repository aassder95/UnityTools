using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Util.Core.Pooling
{
    public abstract class Spawner<T> : MonoBehaviour where T : Component, IPoolable
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly Queue<T> _activeObjects = new();

        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Pool")]
        [SerializeField] private T _prefab;
        [SerializeField, Min(0)] private int _initialSize;
        [Header("Spawn")]
        [SerializeField, Min(1)] private int _maxActiveCnt = 10;
        [SerializeField, Min(0.0001f)] private float _intervalSec = 1.0f;

        //============================================================
        // Fields
        //============================================================
        private ObjectPool<T> _pool;
        private WaitForSeconds _spawnWait;
        private Coroutine _coSpawn;
        private bool _isInit;

        //============================================================
        // Unity Methods
        //============================================================
        private void Awake()
        {
            _spawnWait = new WaitForSeconds(_intervalSec);
        }

        private void OnEnable()
        {
            if (_coSpawn != null)
                return;

            Init();
            _coSpawn = StartCoroutine(CoSpawn());
        }

        private void OnDisable()
        {
            if (_coSpawn != null)
            {
                StopCoroutine(_coSpawn);
                _coSpawn = null;
            }

            ReturnActiveObjects();
        }

        private void OnDestroy()
        {
            Release();
        }

        //============================================================
        // Init/Register
        //============================================================
        private void Init()
        {
            if (_isInit)
                return;

            _pool = ObjectPool<T>.Create(_initialSize, _prefab, transform);
            _isInit = true;
        }

        private void Release()
        {
            if (!_isInit)
                return;

            ReturnActiveObjects();
            _pool.Clear();
            _pool = null;
            _isInit = false;
        }

        //============================================================
        // Coroutines
        //============================================================
        private IEnumerator CoSpawn()
        {
            while (true)
            {
                if (_activeObjects.Count >= _maxActiveCnt)
                {
                    T oldestObject = _activeObjects.Peek();
                    if (!_pool.TryReturn(oldestObject))
                    {
                        yield return _spawnWait;
                        continue;
                    }

                    _activeObjects.Dequeue();
                }

                T obj = _pool.Get();
                obj.transform.position = GetSpawnPos();
                _activeObjects.Enqueue(obj);
                yield return _spawnWait;
            }
        }

        //============================================================
        // Utilities
        //============================================================
        private void ReturnActiveObjects()
        {
            while (_activeObjects.Count > 0)
            {
                T obj = _activeObjects.Peek();
                if (!_pool.TryReturn(obj))
                    return;

                _activeObjects.Dequeue();
            }
        }

        protected abstract Vector3 GetSpawnPos();
    }
}
