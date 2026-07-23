using System.Collections;
using UnityEngine;

namespace UnityTools.Util.Core.Pooling
{
    public abstract class Spawner<T> : MonoBehaviour where T : Component, IPoolable
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private T _prefab;
        [Min(0)] [SerializeField] private int _initialSize;
        [Min(0.0001f)] [SerializeField] private float _intervalSec = 1.0f;

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
            if(_coSpawn != null)
                return;

            Init();
            _coSpawn = StartCoroutine(CoSpawn());
        }

        private void OnDisable()
        {
            if(_coSpawn == null)
                return;

            StopCoroutine(_coSpawn);
            _coSpawn = null;
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
            if(_isInit)
                return;

            _pool = ObjectPool<T>.Create(_initialSize, _prefab, transform);
            _isInit = true;
        }

        private void Release()
        {
            if(!_isInit)
                return;

            _pool.Clear();
            _pool = null;
            _isInit = false;
        }

        //============================================================
        // Coroutines
        //============================================================
        private IEnumerator CoSpawn()
        {
            while(true)
            {
                T obj = _pool.Get();
                obj.transform.position = GetSpawnPos();
                yield return _spawnWait;
            }
        }

        //============================================================
        // Utilities
        //============================================================
        protected abstract Vector3 GetSpawnPos();
    }
}
