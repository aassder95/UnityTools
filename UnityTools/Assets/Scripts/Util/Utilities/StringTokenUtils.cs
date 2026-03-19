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

namespace UnityTools.Util.Utilities
{
    // Exception: stateless utility is kept as a static helper.
    public static class StringTokenUtils
    {
        //============================================================
        //Utilities
        //============================================================
        public static string Normalize(string value, string fallback)
        {
            if(string.IsNullOrWhiteSpace(value))
                return fallback;

            return value.Trim();
        }

        public static string ToLogSafe(string value)
        {
            if(value == null)
                return "null";

            return value.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
        }

        public static bool TryNormalizeNonEmpty(string value, out string normalizedValue)
        {
            normalizedValue = value?.Trim();
            return !string.IsNullOrEmpty(normalizedValue);
        }
    }
}
