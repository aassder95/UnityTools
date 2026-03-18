namespace UnityTools.Util
{
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
