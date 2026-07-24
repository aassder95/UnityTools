using System.Collections;
using UnityEngine;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Util.Coroutines
{
    public class CoroutineHelper : MonoBehaviour
    {
        //============================================================
        // Logic
        //============================================================
        public Coroutine StartRoutine(IEnumerator routine)
        {
            if (routine != null)
                return StartCoroutine(routine);

            DebugLogger.LogError("실행할 Coroutine이 비어 있습니다.", this);
            return null;
        }

        public void StopRoutine(Coroutine coroutine)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }

        public void ReplaceRoutine(ref Coroutine coroutine, IEnumerator nextRoutine)
        {
            StopRoutine(coroutine);
            coroutine = nextRoutine == null ? null : StartRoutine(nextRoutine);
        }
    }
}
