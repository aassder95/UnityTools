using UnityTools.Util.Core.Persistence;

namespace UnityTools.Util.Core.Timer.Period
{
    public static class PeriodTimerStorageKeys
    {
        //============================================================
        // Constants
        //============================================================
        private const string PREFIX = "PeriodTimer_";
        private const string OPEN_END_TIME_SUFFIX = "_OPEN_END";
        private const string CLOSED_END_TIME_SUFFIX = "_CLOSED_END";
        private const string OPEN_UPDATED_TIME_SUFFIX = "_OPEN_UPDATED";
        private const string TAMPERED_SUFFIX = "_TAMPERED";

        //============================================================
        // Logic
        //============================================================
        public static string OpenEnd(string normalizedId)
        {
            return $"{PREFIX}{normalizedId}{OPEN_END_TIME_SUFFIX}";
        }

        public static string ClosedEnd(string normalizedId)
        {
            return $"{PREFIX}{normalizedId}{CLOSED_END_TIME_SUFFIX}";
        }

        public static string OpenUpdated(string normalizedId)
        {
            return $"{PREFIX}{normalizedId}{OPEN_UPDATED_TIME_SUFFIX}";
        }

        public static string Tampered(string normalizedId)
        {
            return $"{PREFIX}{normalizedId}{TAMPERED_SUFFIX}";
        }

        public static void DeleteAll(string normalizedId, IStorage storage)
        {
            storage.Delete(OpenEnd(normalizedId));
            storage.Delete(ClosedEnd(normalizedId));
            storage.Delete(OpenUpdated(normalizedId));
            storage.Delete(Tampered(normalizedId));
        }
    }
}
