using System.Globalization;

namespace UnityTools.Sheets
{
    public static class CsvValue
    {
        //============================================================
        // Logic
        //============================================================
        public static bool TryParse(string text, out int value)
        {
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        public static bool TryParse(string text, out long value)
        {
            return long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        public static bool TryParse(string text, out float value)
        {
            return float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) && !float.IsNaN(value) && !float.IsInfinity(value);
        }

        public static bool TryParse(string text, out double value)
        {
            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) && !double.IsNaN(value) && !double.IsInfinity(value);
        }

        public static bool TryParse(string text, out bool value)
        {
            string normalized = text?.Trim();
            if (normalized == "1" || normalized == "0")
            {
                value = normalized == "1";
                return true;
            }

            return bool.TryParse(normalized, out value);
        }
    }
}
