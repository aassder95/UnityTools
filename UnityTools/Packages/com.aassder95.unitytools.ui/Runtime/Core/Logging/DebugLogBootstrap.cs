using UnityEngine;

namespace UnityTools.Util.Core.Logging
{
    public static class DebugLogBootstrap
    {
        //============================================================
        // Unity Methods
        //============================================================
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            DebugLogGate.Initialize();
            DebugLogGate.Reset();
            DebugLogGate.SetDefaultEnabled(true);
        }
    }
}

