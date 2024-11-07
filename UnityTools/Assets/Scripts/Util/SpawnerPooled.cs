using UnityEngine;
using System.Collections;

namespace UnityTools.Util
{
    public class SpawnerPooled<T> : SpawnerBase where T : Component, IPoolable
    {
        [SerializeField] protected T _prefab;
        [SerializeField] int _size;

        ObjectPool<T> _pool;

        void Awake()
        {
            _pool = new ObjectPool<T>(transform, _prefab, _size);
        }

        protected override void Spawn(Vector2 pos)
        {
            T obj = _pool.Get();
            if (obj == null)
                return;

            obj.transform.position = pos;
            StartCoroutine(CoDestroy(obj));
        }

        private IEnumerator CoDestroy(T obj)
        {
            yield return new WaitForSeconds(_duration);
            _pool.Return(obj);
        }
    }
}