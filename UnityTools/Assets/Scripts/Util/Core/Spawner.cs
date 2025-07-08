using UnityEngine;
using System.Collections;

namespace UnityTools.Util
{
    public class Spawner<T> : MonoBehaviour where T : Component, IPoolable
    {
        [SerializeField] private T _prefab;
        [SerializeField] private int _cnt;
        [SerializeField] private float _interval;
        [SerializeField] private Vector2 _range;

        private ObjectPool<T> _pool;

        private void Awake()
        {
            _pool = new ObjectPool<T>(_cnt, _prefab, transform);
        }

        private void Start()
        {
            StartCoroutine(CoSpawn());
        }

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