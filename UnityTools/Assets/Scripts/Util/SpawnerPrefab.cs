using UnityEngine;
using System.Collections;

namespace UnityTools.Util
{
    public class SpawnerPrefab : SpawnerBase
    {
        [SerializeField] protected GameObject _goPrefab;

        protected override void Spawn(Vector2 pos)
        {
            GameObject obj = Instantiate(_goPrefab, pos, Quaternion.identity);
            if (obj == null)
                return;

            obj.name = _goPrefab.name;
            obj.transform.SetParent(transform);
            StartCoroutine(CoDestroy(obj));
        }

        IEnumerator CoDestroy(GameObject obj)
        {
            yield return new WaitForSeconds(_duration);
            Destroy(obj);
        }
    }
}
