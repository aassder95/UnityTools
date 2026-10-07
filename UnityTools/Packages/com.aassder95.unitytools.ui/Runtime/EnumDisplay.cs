using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityTools.Ui
{
    public static class EnumDisplay<T> where T : struct, Enum
    {
        //============================================================
        // Readonly
        //============================================================
        private static readonly Dictionary<T, string> _names = new Dictionary<T, string>();
        private static readonly Dictionary<string, T> _values = new Dictionary<string, T>(StringComparer.Ordinal);
        private static readonly HashSet<string> _ambiguousNames = new HashSet<string>(StringComparer.Ordinal);

        //============================================================
        // Constructors
        //============================================================
        static EnumDisplay()
        {
            FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static);
            Array.Sort(fields, (left, right) => left.MetadataToken.CompareTo(right.MetadataToken));
            foreach (FieldInfo field in fields)
            {
                T value = (T)field.GetValue(null);
                var attribute = field.GetCustomAttribute<EnumDisplayNameAttribute>();
                string name = string.IsNullOrEmpty(attribute?.Name) ? field.Name : attribute.Name;
                if (!_names.ContainsKey(value))
                    _names.Add(value, name);

                if (_values.TryGetValue(name, out T existing) && !EqualityComparer<T>.Default.Equals(value, existing))
                    _ambiguousNames.Add(name);
                else
                    _values[name] = value;
            }
        }

        //============================================================
        // Logic
        //============================================================
        public static bool TryGetName(T value, out string name)
        {
            return _names.TryGetValue(value, out name);
        }

        public static bool TryParseName(string name, out T value)
        {
            value = default;
            if (name == null || _ambiguousNames.Contains(name))
                return false;

            return _values.TryGetValue(name, out value);
        }
    }
}
