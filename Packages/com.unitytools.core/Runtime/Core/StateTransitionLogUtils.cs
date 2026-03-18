using System;

namespace UnityTools.Util
{
    public static class StateTransitionLogUtils
    {
        //============================================================
        //Utilities
        //============================================================
        public static void LogMissingState<TType>(string method, TType type) where TType : Enum
        {
            DebugLogger.LogWarning($"[{method}] 등록되지 않은 상태 전이 요청: {type}");
        }

        public static void LogInitialSetFailed<TType>(string method, TType type) where TType : Enum
        {
            DebugLogger.LogWarning($"[{method}] 초기 상태 설정 실패: {type}");
        }

        public static void LogTransitionFailed<TType>(string method, TType fromType, TType toType) where TType : Enum
        {
            DebugLogger.LogWarning($"[{method}] 상태 전이 실패: {fromType} -> {toType}");
        }
    }
}
