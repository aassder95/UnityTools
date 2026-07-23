using System.Collections;
using UnityEngine;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.Utilities;

namespace UnityTools.Util.Core.Pooling
{
    public class Spawner<T> : MonoBehaviour where T : Component, IPoolable
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private T _prefab;
        [SerializeField] private int _initialSize;
        [SerializeField] private float _intervalSec;
        [SerializeField] private Vector2 _range;

        //============================================================
        // Fields
        //============================================================
        private ObjectPool<T> _pool;
        private WaitForSeconds _spawnWait;
        private Coroutine _coSpawn;
        private bool _isConfigValid;
        private bool _isInit;

        //============================================================
        // Unity Methods
        //============================================================
        private void Awake()
        {
            _isConfigValid = ValidateConfig();
            if(!_isConfigValid)
            {
                enabled = false;
                return;
            }

            _spawnWait = new WaitForSeconds(_intervalSec);
        }

        private void OnEnable()
        {
            if(!_isConfigValid || _coSpawn != null)
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
            _pool?.Clear();
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
                if(!_pool.TryGet(out T obj))
                {
                    _coSpawn = null;
                    enabled = false;
                    yield break;
                }

                obj.transform.position = RandomUtils.GetRandomPosInRange(transform.position, _range);
                yield return _spawnWait;
            }
        }

        //============================================================
        // Utilities
        //============================================================
        private bool ValidateConfig()
        {
            if(_initialSize < 0)
            {
                DebugLogger.LogError("Spawner의 Initial Size는 0 이상이어야 합니다. 값=" + _initialSize, this);
                return false;
            }

            if(_intervalSec <= 0.0f)
            {
                DebugLogger.LogError("Spawner의 Interval은 0초보다 커야 합니다. 값=" + _intervalSec, this);
                return false;
            }

            if(_range.x < 0.0f || _range.y < 0.0f)
            {
                DebugLogger.LogError("Spawner의 Range는 0 이상이어야 합니다. 값=" + _range, this);
                return false;
            }

            return true;
        }
    }
}