namespace UnityTools.Timer.Task
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
        public static string Start(string normalizedId)
        {
            return $"{PREFIX}{normalizedId}{START_TIME_SUFFIX}";
        }

        public static string Updated(string normalizedId)
        {
            return $"{PREFIX}{normalizedId}{UPDATED_TIME_SUFFIX}";
        }

        public static string Duration(string normalizedId)
        {
            return $"{PREFIX}{normalizedId}{DURATION_SUFFIX}";
        }

        public static string State(string normalizedId)
        {
            return $"{PREFIX}{normalizedId}{STATE_SUFFIX}";
        }
    }
}
