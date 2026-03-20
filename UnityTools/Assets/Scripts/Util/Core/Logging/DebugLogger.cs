using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityTools.Util.Utilities;

namespace UnityTools.Util.Core.Logging
{
    // Exception: stateless utility is kept as a static helper.
    public static class DebugLogger
    {
        //============================================================
        //Constants
        //============================================================
        private const string UNKNOWN_CLASS = "UnknownClass";
        private const string UNKNOWN_METHOD = "UnknownMethod";

        //============================================================
        //Logic
        //============================================================
        public static void Log(string msg, UnityEngine.Object context = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            if(!DebugLogGate.IsLevelEnabled(ELogLevel.Log))
                return;

            string className = ResolveClassName(filePath);
            string method = ResolveMethod(memberName);
            WriteLog(ELogLevel.Log, className, method, msg, context);
        }

        public static void LogWarning(string msg, UnityEngine.Object context = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            if(!DebugLogGate.IsLevelEnabled(ELogLevel.Warning))
                return;

            string className = ResolveClassName(filePath);
            string method = ResolveMethod(memberName);
            WriteLog(ELogLevel.Warning, className, method, msg, context);
        }

        public static void LogError(string msg, UnityEngine.Object context = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            if(!DebugLogGate.IsLevelEnabled(ELogLevel.Error))
                return;

            string className = ResolveClassName(filePath);
            string method = ResolveMethod(memberName);
            WriteLog(ELogLevel.Error, className, method, msg, context);
        }

        public static void LogException(Exception ex, UnityEngine.Object context = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            string className = ResolveClassName(filePath);
            string method = ResolveMethod(memberName);
            WriteException(className, method, ex, context);
        }

        //============================================================
        //Utilities
        //============================================================
        private static void WriteLog(ELogLevel level, string className, string method, string msg, UnityEngine.Object context)
        {
            if(!DebugLogGate.IsEnabled(className, level))
                return;

            string message = FormatMessage(className, method, msg);
            switch(level)
            {
                case ELogLevel.Log:
                    if(context == null)
                        Debug.Log(message);
                    else
                        Debug.Log(message, context);
                    break;
                case ELogLevel.Warning:
                    if(context == null)
                        Debug.LogWarning(message);
                    else
                        Debug.LogWarning(message, context);
                    break;
                case ELogLevel.Error:
                    if(context == null)
                        Debug.LogError(message);
                    else
                        Debug.LogError(message, context);
                    break;
            }
        }

        private static void WriteException(string className, string method, Exception ex, UnityEngine.Object context)
        {
            if(ex == null)
            {
                WriteLog(ELogLevel.Error, className, method, "?덉쇅 ?뺣낫媛 鍮꾩뼱 ?덉뒿?덈떎.", context);
                return;
            }

            WriteLog(ELogLevel.Error, className, method, $"?덉쇅 諛쒖깮: {ex.Message}", context);
            if(context == null)
                Debug.LogException(ex);
            else
                Debug.LogException(ex, context);
        }

        private static string ResolveClassName(string filePath)
        {
            if(string.IsNullOrWhiteSpace(filePath))
                return UNKNOWN_CLASS;

            string className = Path.GetFileNameWithoutExtension(filePath);
            return StringTokenUtils.Normalize(className, UNKNOWN_CLASS);
        }

        private static string ResolveMethod(string memberName)
        {
            string method = StringTokenUtils.Normalize(memberName, UNKNOWN_METHOD);
            if(method == ".ctor")
                return "Ctor";

            return method;
        }

        private static string FormatMessage(string className, string method, string msg)
        {
            string safeClassName = StringTokenUtils.Normalize(className, UNKNOWN_CLASS);
            string safeMethod = StringTokenUtils.Normalize(method, UNKNOWN_METHOD);
            string safeMsg = msg ?? string.Empty;
            return $"[{safeClassName}:{safeMethod}] {safeMsg}";
        }
    }
}
