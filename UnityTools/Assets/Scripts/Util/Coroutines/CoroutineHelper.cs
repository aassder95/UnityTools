using System.Collections;
using UnityEngine;
using UnityTools.Util.Constants;
using UnityTools.Util.Core.Collections;
using UnityTools.Util.Core.Events;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.Core.Persistence;
using UnityTools.Util.Core.Pooling;
using UnityTools.Util.Core.Singleton;
using UnityTools.Util.Core.State;
using UnityTools.Util.Core.Timer.Period;
using UnityTools.Util.Core.Timer.Shared;
using UnityTools.Util.Core.Timer.Task;
using UnityTools.Util.Coroutines;
using UnityTools.Util.Extensions;
using UnityTools.Util.UIFramework;
using UnityTools.Util.Utilities;

namespace UnityTools.Util.Coroutines
{
    //============================================================
    //Logic
    //============================================================
    public class CoroutineHelper : MonoBehaviour
    {
        //============================================================
        //Fields
        //============================================================
        private static CoroutineHelper _instance;

        //============================================================
        //Unity Methods
        //============================================================
        private void Awake()
        {
            if (_instance == null)
                _instance = this;
            else if (_instance != this)
                Destroy(gameObject);
        }

        //============================================================
        //Logic
        //============================================================
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
