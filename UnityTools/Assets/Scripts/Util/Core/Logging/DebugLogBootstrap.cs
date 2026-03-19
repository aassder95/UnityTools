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

