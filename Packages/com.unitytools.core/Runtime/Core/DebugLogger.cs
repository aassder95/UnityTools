using System;
using UnityEngine;

namespace UnityTools.Util
{
    public static class DebugLogger
    {
        //============================================================
        //Logic
        //============================================================
        public static void Log(bool isEnabled, string className, string method, string msg, string category = null, UnityEngine.Object context = null)
        {
            if(!CanLog(isEnabled, className, category))
                return;

            string message = FormatMessage(className, method, msg);
            if(context == null)
                Debug.Log(message);
            else
                Debug.Log(message, context);
        }

        public static void LogWarning(bool isEnabled, string className, string method, string msg, string category = null, UnityEngine.Object context = null)
        {
            if(!CanLog(isEnabled, className, category))
                return;

            string message = FormatMessage(className, method, msg);
            if(context == null)
                Debug.LogWarning(message);
            else
                Debug.LogWarning(message, context);
        }

        public static void LogError(bool isEnabled, string className, string method, string msg, string category = null, UnityEngine.Object context = null)
        {
            if(!CanLog(isEnabled, className, category))
                return;

            string message = FormatMessage(className, method, msg);
            if(context == null)
                Debug.LogError(message);
            else
                Debug.LogError(message, context);
        }

        public static void LogException(bool isEnabled, string className, string method, Exception ex, string category = null, UnityEngine.Object context = null)
        {
            if(!CanLog(isEnabled, className, category))
                return;

            if(ex == null)
            {
                LogError(isEnabled, className, method, "예외 정보가 null입니다.", category, context);
                return;
            }

            LogError(isEnabled, className, method, $"예외 발생: {ex.Message}", category, context);
            if(context == null)
                Debug.LogException(ex);
            else
                Debug.LogException(ex, context);
        }

        //============================================================
        //Utilities
        //============================================================
        private static bool CanLog(bool isEnabled, string className, string category)
        {
            if(!isEnabled)
                return false;

            string resolvedCategory = string.IsNullOrWhiteSpace(category) ? NormalizeToken(className, "UnknownClass") : category.Trim();
            return DebugLogGate.IsEnabled(resolvedCategory);
        }

        private static string FormatMessage(string className, string method, string msg)
        {
            string safeClassName = NormalizeToken(className, "UnknownClass");
            string safeMethod = NormalizeToken(method, "UnknownMethod");
            string safeMsg = msg ?? string.Empty;
            return $"[{safeClassName}:{safeMethod}] {safeMsg}";
        }

        private static string NormalizeToken(string value, string fallback)
        {
            if(string.IsNullOrWhiteSpace(value))
                return fallback;

            return value.Trim();
        }
    }
}
