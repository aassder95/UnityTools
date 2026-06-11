using UnityTools.Util.Core.Persistence;
using UnityTools.Util.Utilities;

namespace UnityTools.Util.Core.Timer.Period
{
    // Exception: stateless utility is kept as a static helper.
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
        public static bool TryNormalizeId(string rawId, out string normalizedId)
        {
            return StringTokenUtils.TryNormalizeNonEmpty(rawId, out normalizedId);
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

        public static void DeleteAll(string id, IStorage storage)
        {
            if(storage == null)
                return;

            if(!TryNormalizeId(id, out string normalizedId))
                return;

            storage.Delete(OpenEnd(normalizedId));
            storage.Delete(ClosedEnd(normalizedId));
            storage.Delete(OpenUpdated(normalizedId));
            storage.Delete(Tampered(normalizedId));
        }
    }
}

