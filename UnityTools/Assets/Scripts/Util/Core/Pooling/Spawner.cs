using System.Collections;
using UnityEngine;
using UnityTools.Util.Utilities;

namespace UnityTools.Util.Core.Pooling
{
    public class Spawner<T> : MonoBehaviour where T : Component, IPoolable
    {
        //============================================================
        //Inspector Fields
        //============================================================
        [SerializeField] private T _prefab;
        [SerializeField] private int _cnt;
        [SerializeField] private float _interval;
        [SerializeField] private Vector2 _range;

        //============================================================
        //Fields
        //============================================================
        private ObjectPool<T> _pool;

        //============================================================
        //Unity Methods
        //============================================================
        private void Awake()
        {
            _pool = new ObjectPool<T>(_cnt, _prefab, transform);
        }

        private void Start()
        {
            if(_pool == null || _cnt <= 0)
                return;

            StartCoroutine(CoSpawn());
        }

        //============================================================
        //Coroutines
        //============================================================
        private IEnumerator CoSpawn()
        {
            while (true)
            {
                T obj = _pool.Get();
                if (obj != null)
                    obj.transform.position = RandomUtils.GetRandomPositionInRange(transform.position, _range);

                yield return new WaitForSeconds(_interval);
            }
        }
    }
}
