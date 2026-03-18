namespace UnityTools.Util
{
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
            return TimerIdUtils.TryNormalizeId(rawId, out normalizedId);
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

