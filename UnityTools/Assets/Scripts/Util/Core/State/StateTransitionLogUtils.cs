using System;
using System.Runtime.CompilerServices;
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

namespace UnityTools.Util.Core.State
{
    // Exception: stateless utility is kept as a static helper.
    public static class StateTransitionLogUtils
    {
        //============================================================
        //Utilities
        //============================================================
        public static void LogMissingState<TType>(
            string method,
            TType type,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "") where TType : Enum
        {
            DebugLogger.LogWarning($"[{method}] ?±Î°ù?òÏ? ?äÏ? ?ÅÌÉú ?ÑÏù¥ ?îÏ≤≠: {type}", null, memberName, filePath);
        }

        public static void LogInitialSetFailed<TType>(
            string method,
            TType type,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "") where TType : Enum
        {
            DebugLogger.LogWarning($"[{method}] Ï¥àÍ∏∞ ?ÅÌÉú ?§Ï†ï ?§Ìå®: {type}", null, memberName, filePath);
        }

        public static void LogTransitionFailed<TType>(
            string method,
            TType fromType,
            TType toType,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "") where TType : Enum
        {
            DebugLogger.LogWarning($"[{method}] ?ÅÌÉú ?ÑÏù¥ ?§Ìå®: {fromType} -> {toType}", null, memberName, filePath);
        }
    }
}
