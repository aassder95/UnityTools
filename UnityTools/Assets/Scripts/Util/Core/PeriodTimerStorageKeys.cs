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

        public static string OpenStart(string id)
        {
            TryNormalizeId(id, out string normalizedId);
            return $"{PREFIX}{normalizedId ?? string.Empty}{OPEN_START_TIME_SUFFIX}";
        }

        public static string OpenEnd(string id)
        {
            TryNormalizeId(id, out string normalizedId);
            return $"{PREFIX}{normalizedId ?? string.Empty}{OPEN_END_TIME_SUFFIX}";
        }

        public static string ClosedEnd(string id)
        {
            TryNormalizeId(id, out string normalizedId);
            return $"{PREFIX}{normalizedId ?? string.Empty}{CLOSED_END_TIME_SUFFIX}";
        }

        public static string OpenUpdated(string id)
        {
            TryNormalizeId(id, out string normalizedId);
            return $"{PREFIX}{normalizedId ?? string.Empty}{OPEN_UPDATED_TIME_SUFFIX}";
        }

        public static string Tampered(string id)
        {
            TryNormalizeId(id, out string normalizedId);
            return $"{PREFIX}{normalizedId ?? string.Empty}{TAMPERED_SUFFIX}";
        }
    }
}
