using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityTools.Util.Utilities;

namespace UnityTools.Util.Core.Logging
{
    public static class DebugLogger
    {
        //============================================================
        // Constants
        //============================================================
        private const string UNKNOWN_CLASS = "UnknownClass";
        private const string UNKNOWN_METHOD = "UnknownMethod";

        //============================================================
        // Logic
        //============================================================
        public static void Log(string msg, UnityEngine.Object context = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            if (!DebugLogGate.IsLevelEnabled(ELogLevel.Log))
                return;

            string className = ResolveClassName(filePath);
            string method = ResolveMethod(memberName);
            WriteLog(ELogLevel.Log, className, method, msg, context);
        }

        public static void LogWarning(string msg, UnityEngine.Object context = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            if (!DebugLogGate.IsLevelEnabled(ELogLevel.Warning))
                return;

            string className = ResolveClassName(filePath);
            string method = ResolveMethod(memberName);
            WriteLog(ELogLevel.Warning, className, method, msg, context);
        }

        public static void LogError(string msg, UnityEngine.Object context = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            if (!DebugLogGate.IsLevelEnabled(ELogLevel.Error))
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
        // Utilities
        //============================================================
        private static void WriteLog(ELogLevel level, string className, string method, string msg, UnityEngine.Object context)
        {
            if (!DebugLogGate.IsEnabled(className, level))
                return;

            string message = FormatMessage(className, method, msg);
            switch (level)
            {
                case ELogLevel.Log:
                    if (context == null)
                        Debug.Log(message);
                    else
                        Debug.Log(message, context);
                    break;
                case ELogLevel.Warning:
                    if (context == null)
                        Debug.LogWarning(message);
                    else
                        Debug.LogWarning(message, context);
                    break;
                case ELogLevel.Error:
                    if (context == null)
                        Debug.LogError(message);
                    else
                        Debug.LogError(message, context);
                    break;
            }
        }

        private static void WriteException(string className, string method, Exception ex, UnityEngine.Object context)
        {
            if (ex == null)
            {
                WriteLog(ELogLevel.Error, className, method, "예외 정보가 비어 있습니다.", context);
                return;
            }

            WriteLog(ELogLevel.Error, className, method, $"예외 발생: {ex.Message}", context);
            if (context == null)
                Debug.LogException(ex);
            else
                Debug.LogException(ex, context);
        }

        private static string ResolveClassName(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return UNKNOWN_CLASS;

            string className = Path.GetFileNameWithoutExtension(filePath);
            return string.IsNullOrWhiteSpace(className) ? UNKNOWN_CLASS : className;
        }

        private static string ResolveMethod(string memberName)
        {
            return string.IsNullOrWhiteSpace(memberName) ? UNKNOWN_METHOD : memberName;
        }

        private static string FormatMessage(string className, string method, string msg)
        {
            return $"[{StringTokenUtils.ToLogSafe(className)}.{StringTokenUtils.ToLogSafe(method)}] {msg}";
        }
    }
}
