using System.Collections;
using UnityEngine;

namespace UnityTools.Util
{
    public abstract class SpawnerBase : MonoBehaviour
    {
        [SerializeField] float _interval;
        [SerializeField] protected float _duration;
        [SerializeField] Vector2 _range;

        void Start()
        {
            StartCoroutine(CoSpawn());
        }

        protected abstract void Spawn(Vector2 pos);

        IEnumerator CoSpawn()
        {
            while (true)
            {
                Spawn(Utils.GetRandomPos(transform.position, _range));
                yield return new WaitForSeconds(_interval);
            }
        }
    }
}
