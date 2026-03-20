using UnityEngine;

namespace UnityTools.Util.Core.Logging
{
    // Exception: stateless utility is kept as a static helper.
    public static class DebugLogBootstrap
    {
        //============================================================
        //Unity Methods
        //============================================================
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            DebugLogGate.Initialize();
            DebugLogGate.Reset();
            DebugLogGate.SetDefaultEnabled(true);
            ApplyClassOverrides();
        }

        //============================================================
        //Logic
        //============================================================
        private static void ApplyClassOverrides()
        {
            // 필요 시 특정 클래스 로그를 개별 제어합니다.
            // 예시: DebugLogGate.SetEnabled("TaskTimer", false);
        }
    }
}

