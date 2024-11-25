using System.Collections;
using UnityEngine;

namespace UnityTools.Util
{
    public class CoroutineHelper : MonoBehaviour
    {
        static CoroutineHelper _instance;

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else if (_instance != this)
            {
                Destroy(this.gameObject);
            }
        }

        public static Coroutine Start(IEnumerator enumerator)
        {
            return _instance.StartCoroutine(enumerator);
        }

        public static void Stop(Coroutine coroutine)
        {
            _instance.StopCoroutine(coroutine);
        }

        public static void Replace(ref Coroutine coroutine, IEnumerator enumerator)
        {
            if (coroutine != null)
                Stop(coroutine);

            coroutine = enumerator == null ? null : Start(enumerator);
        }

        public static void Dispose(ref Coroutine coroutine)
        {
            if (coroutine != null)
                Stop(coroutine);

            coroutine = null;
        }
    }
}