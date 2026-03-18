using System;
using System.Collections.Generic;

namespace UnityTools.Util
{
    public static class DebugLogGate
    {
        //============================================================
        //Constants
        //============================================================
        private const string DEFAULT_CATEGORY = "__default__";

        //============================================================
        //Readonly
        //============================================================
        private static readonly Dictionary<string, bool> _categoryEnabled = new(StringComparer.Ordinal);

        //============================================================
        //Fields
        //============================================================
        private static bool _defaultEnabled = true;

        //============================================================
        //Properties
        //============================================================
        public static bool DefaultEnabled
        {
            get => _defaultEnabled;
            set => _defaultEnabled = value;
        }

        //============================================================
        //Logic
        //============================================================
        public static void SetEnabled(string category, bool isEnabled)
        {
            string key = NormalizeCategory(category);
            _categoryEnabled[key] = isEnabled;
        }

        public static bool IsEnabled(string category)
        {
            string key = NormalizeCategory(category);
            if(_categoryEnabled.TryGetValue(key, out bool isEnabled))
                return isEnabled;

            return _defaultEnabled;
        }

        public static void Reset()
        {
            _categoryEnabled.Clear();
        }

        //============================================================
        //Utilities
        //============================================================
        private static string NormalizeCategory(string category)
        {
            if(string.IsNullOrWhiteSpace(category))
                return DEFAULT_CATEGORY;

            return category.Trim();
        }
    }
}
