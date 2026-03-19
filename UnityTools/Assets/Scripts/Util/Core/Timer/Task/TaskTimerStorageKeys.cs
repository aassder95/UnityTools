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

namespace UnityTools.Util.Core.Timer.Task
{
    // Exception: stateless utility is kept as a static helper.
    public static class TaskTimerStorageKeys
    {
        //============================================================
        //Constants
        //============================================================
        private const string PREFIX = "TaskTimer_";
        private const string START_TIME_SUFFIX = "_START";
        private const string UPDATED_TIME_SUFFIX = "_UPDATED";
        private const string DURATION_SUFFIX = "_DURATION";
        private const string STATE_SUFFIX = "_STATE";

        //============================================================
        //Logic
        //============================================================
        public static bool TryNormalizeId(string rawId, out string normalizedId)
        {
            return StringTokenUtils.TryNormalizeNonEmpty(rawId, out normalizedId);
        }

        public static string Start(string id)
        {
            TryNormalizeId(id, out string normalizedId);
            return $"{PREFIX}{normalizedId ?? string.Empty}{START_TIME_SUFFIX}";
        }

        public static string Updated(string id)
        {
            TryNormalizeId(id, out string normalizedId);
            return $"{PREFIX}{normalizedId ?? string.Empty}{UPDATED_TIME_SUFFIX}";
        }

        public static string Duration(string id)
        {
            TryNormalizeId(id, out string normalizedId);
            return $"{PREFIX}{normalizedId ?? string.Empty}{DURATION_SUFFIX}";
        }

        public static string State(string id)
        {
            TryNormalizeId(id, out string normalizedId);
            return $"{PREFIX}{normalizedId ?? string.Empty}{STATE_SUFFIX}";
        }
    }
}

