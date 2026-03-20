using System;
using System.Runtime.CompilerServices;
using UnityTools.Util.Core.Logging;

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
            DebugLogger.LogWarning($"[{method}] ?源낆쨯??? ??? ?怨밴묶 ?袁⑹뵠 ?遺욧퍕: {type}", null, memberName, filePath);
        }

        public static void LogInitialSetFailed<TType>(
            string method,
            TType type,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "") where TType : Enum
        {
            DebugLogger.LogWarning($"[{method}] ?λ뜃由??怨밴묶 ??쇱젟 ??쎈솭: {type}", null, memberName, filePath);
        }

        public static void LogTransitionFailed<TType>(
            string method,
            TType fromType,
            TType toType,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "") where TType : Enum
        {
            DebugLogger.LogWarning($"[{method}] ?怨밴묶 ?袁⑹뵠 ??쎈솭: {fromType} -> {toType}", null, memberName, filePath);
        }
    }
}
