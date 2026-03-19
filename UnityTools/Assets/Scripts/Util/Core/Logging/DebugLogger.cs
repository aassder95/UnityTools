using System;
using System.IO;
using System.Runtime.CompilerServices;
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
    public static class DebugLogger
    {
        //============================================================
        //Constants
        //============================================================
        private const string UNKNOWN_CLASS = "알수없는클래스";
        private const string UNKNOWN_METHOD = "알수없는메서드";

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
                WriteLog(ELogLevel.Error, className, method, "예외 정보가 비어 있습니다.", context);
                return;
            }

            WriteLog(ELogLevel.Error, className, method, $"예외 발생: {ex.Message}", context);
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
