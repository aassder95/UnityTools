using UnityEngine;
using System.Collections;

namespace UnityTools.Util
{
    public class Spawner<T> : MonoBehaviour where T : Component, IPoolable
    {
        [SerializeField] T _prefab;
        [SerializeField] int _cnt;
        [SerializeField] float _interval;
        [SerializeField] Vector2 _range;

        ObjectPool<T> _pool;

        void Awake()
        {
            _pool = new ObjectPool<T>(transform, _prefab, _cnt);
        }

        void Start()
        {
            StartCoroutine(CoSpawn());
        }

        IEnumerator CoSpawn()
        {
            while (true)
            {
                T obj = _pool.Get();
                if (obj != null)
                {
                    obj.transform.position = Utils.GetRandomPos(transform.position, _range);
                }

                yield return new WaitForSeconds(_interval);
            }
        }
    }
}