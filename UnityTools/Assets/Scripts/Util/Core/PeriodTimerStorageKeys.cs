namespace UnityTools.Util
{
    public static class PeriodTimerStorageKeys
    {
        //============================================================
        // Constants
        //============================================================
        private const string PREFIX = "PeriodTimer_";
        private const string OPEN_START_TIME_SUFFIX = "_OPEN_START";
        private const string OPEN_END_TIME_SUFFIX = "_OPEN_END";
        private const string CLOSED_END_TIME_SUFFIX = "_CLOSED_END";
        private const string OPEN_UPDATED_TIME_SUFFIX = "_OPEN_UPDATED";
        private const string TAMPERED_SUFFIX = "_TAMPERED";

        //============================================================
        // Logic
        //============================================================
        public static bool TryNormalizeId(string rawId, out string normalizedId)
        {
            normalizedId = rawId?.Trim();
            return !string.IsNullOrEmpty(normalizedId);
        }

        public static string OpenStart(string id) => Build(id, OPEN_START_TIME_SUFFIX);
        public static string OpenEnd(string id) => Build(id, OPEN_END_TIME_SUFFIX);
        public static string ClosedEnd(string id) => Build(id, CLOSED_END_TIME_SUFFIX);
        public static string OpenUpdated(string id) => Build(id, OPEN_UPDATED_TIME_SUFFIX);
        public static string Tampered(string id) => Build(id, TAMPERED_SUFFIX);

        private static string Build(string id, string suffix)
        {
            TryNormalizeId(id, out string normalizedId);
            return $"{PREFIX}{normalizedId}{suffix}";
        }
    }
}
