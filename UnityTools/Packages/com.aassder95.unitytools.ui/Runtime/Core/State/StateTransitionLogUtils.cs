using System;
using System.Runtime.CompilerServices;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Util.Core.State
{
    public static class StateTransitionLogUtils
    {
        //============================================================
        // Utilities
        //============================================================
        public static void LogMissingState<TType>(string method, TType type, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "") where TType : Enum
        {
            DebugLogger.LogError($"[{method}] 상태를 찾을 수 없습니다: {type}", null, memberName, filePath);
        }

        public static void LogInitialSetFailed<TType>(string method, TType type, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "") where TType : Enum
        {
            DebugLogger.LogError($"[{method}] 초기 상태 설정 실패: {type}", null, memberName, filePath);
        }

        public static void LogTransitionFailed<TType>(string method, TType fromType, TType toType, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "") where TType : Enum
        {
            DebugLogger.LogError($"[{method}] 상태 전환 실패: {fromType} -> {toType}", null, memberName, filePath);
        }
    }
}
