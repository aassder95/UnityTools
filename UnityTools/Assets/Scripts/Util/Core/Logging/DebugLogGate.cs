using System;
using System.Collections.Generic;
using UnityTools.Util.Utilities;

namespace UnityTools.Util.Core.Logging
{
    public enum ELogLevel
    {
        Log,
        Warning,
        Error
    }

    // Exception: stateless utility is kept as a static helper.
    public static class DebugLogGate
    {
        //============================================================
        //Constants
        //============================================================
        private const string DEFAULT_CLASS = "__default__";

        //============================================================
        //Readonly
        //============================================================
        private static readonly Dictionary<string, bool> _classEnabled = new(StringComparer.Ordinal);

        //============================================================
        //Fields
        //============================================================
        private static bool _isInitialized;
        private static bool _isDefaultEnabled = true;
        private static bool _isLogEnabled = true;
        private static bool _isWarningEnabled = true;
        private static bool _isErrorEnabled = true;

        //============================================================
        //Properties
        //============================================================
        public static bool IsDefaultEnabled => _isDefaultEnabled;

        //============================================================
        //Init/Register
        //============================================================
        public static void Initialize()
        {
            if(_isInitialized)
                return;

            ApplyDefaultBuildPreset();
            _isInitialized = true;
        }

        //============================================================
        //Logic
        //============================================================
        public static void SetEnabled(string className, bool isEnabled)
        {
            string key = StringTokenUtils.Normalize(className, DEFAULT_CLASS);
            _classEnabled[key] = isEnabled;
        }

        public static bool IsEnabled(string className)
        {
            return IsEnabled(className, ELogLevel.Log);
        }

        public static bool IsLevelEnabled(ELogLevel level)
        {
            Initialize();
            return IsLevelEnabledInternal(level);
        }

        public static bool IsEnabled(string className, ELogLevel level)
        {
            Initialize();
            if(!IsLevelEnabledInternal(level))
                return false;

            string key = StringTokenUtils.Normalize(className, DEFAULT_CLASS);
            if(_classEnabled.TryGetValue(key, out bool isEnabled))
                return isEnabled;

            return _isDefaultEnabled;
        }

        public static void Reset()
        {
            _classEnabled.Clear();
        }

        public static void SetDefaultEnabled(bool isEnabled)
        {
            _isDefaultEnabled = isEnabled;
        }

        //============================================================
        //Utilities
        //============================================================
        private static void ApplyDefaultBuildPreset()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            SetLevelEnabled(true, true, true);
#else
            SetLevelEnabled(false, false, true);
#endif
        }

        private static bool IsLevelEnabledInternal(ELogLevel level)
        {
            switch(level)
            {
                case ELogLevel.Log:
                    return _isLogEnabled;
                case ELogLevel.Warning:
                    return _isWarningEnabled;
                case ELogLevel.Error:
                    return _isErrorEnabled;
                default:
                    return false;
            }
        }

        private static void SetLevelEnabled(bool isLogEnabled, bool isWarningEnabled, bool isErrorEnabled)
        {
            _isLogEnabled = isLogEnabled;
            _isWarningEnabled = isWarningEnabled;
            _isErrorEnabled = isErrorEnabled;
        }

    }
}
