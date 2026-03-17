using System.Collections;
using UnityEngine;

namespace UnityTools.Util
{
    //============================================================
    //Logic
    //============================================================
    public class CoroutineHelper : MonoBehaviour
    {
        private static CoroutineHelper _instance;

        private void Awake()
        {
            if (_instance == null)
                _instance = this;
            else if (_instance != this)
                Destroy(gameObject);
        }

        public static Coroutine Start(IEnumerator enumerator)
        {
            if(enumerator == null)
                return null;

            if(_instance == null)
                _instance = FindFirstObjectByType<CoroutineHelper>();
            if(_instance == null)
                return null;

            return _instance.StartCoroutine(enumerator);
        }

        public static void Stop(Coroutine coroutine)
        {
            if(coroutine == null)
                return;

            if(_instance == null)
                _instance = FindFirstObjectByType<CoroutineHelper>();
            if(_instance == null)
                return;

            _instance.StopCoroutine(coroutine);
        }

        public static void Replace(ref Coroutine coroutine, IEnumerator enumerator)
        {
            Stop(coroutine);
            coroutine = enumerator == null ? null : Start(enumerator);
        }
    }
}
