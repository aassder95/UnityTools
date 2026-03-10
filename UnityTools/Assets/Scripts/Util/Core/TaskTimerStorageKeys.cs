namespace UnityTools.Util
{
    public static class TaskTimerStorageKeys
    {
        //============================================================
        // Constants
        //============================================================
        private const string PREFIX = "TaskTimer_";
        private const string START_TIME_SUFFIX = "_START";
        private const string UPDATED_TIME_SUFFIX = "_UPDATED";
        private const string DURATION_SUFFIX = "_DURATION";
        private const string STATE_SUFFIX = "_STATE";

        //============================================================
        // Logic
        //============================================================
        public static bool TryNormalizeId(string rawId, out string normalizedId)
        {
            normalizedId = rawId?.Trim();
            return !string.IsNullOrEmpty(normalizedId);
        }

        public static string Start(string id) => Build(id, START_TIME_SUFFIX);
        public static string Updated(string id) => Build(id, UPDATED_TIME_SUFFIX);
        public static string Duration(string id) => Build(id, DURATION_SUFFIX);
        public static string State(string id) => Build(id, STATE_SUFFIX);

        private static string Build(string id, string suffix)
        {
            TryNormalizeId(id, out string normalizedId);
            return $"{PREFIX}{normalizedId}{suffix}";
        }
    }
}
