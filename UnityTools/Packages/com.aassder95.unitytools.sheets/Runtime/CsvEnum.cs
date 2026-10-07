using System;
using System.Collections.Generic;

namespace UnityTools.Sheets
{
    public static class CsvEnum<T> where T : struct, Enum
    {
        //============================================================
        // Readonly
        //============================================================
        private static readonly Dictionary<string, T> _values = CreateValues();

        //============================================================
        // Logic
        //============================================================
        public static bool TryParse(string text, out T value)
        {
            value = default;
            return text != null && _values.TryGetValue(text, out value);
        }

        //============================================================
        // Utilities
        //============================================================
        private static Dictionary<string, T> CreateValues()
        {
            Dictionary<string, T> values = new Dictionary<string, T>(StringComparer.Ordinal);
            string[] names = Enum.GetNames(typeof(T));
            for (int idx = 0; idx < names.Length; idx++)
            {
                if (Enum.TryParse(names[idx], out T value))
                    values.Add(names[idx], value);
            }

            return values;
        }
    }
}
