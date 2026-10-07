using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace UnityTools.Sheets.Editor
{
    public static class CsvEnumResolver
    {
        //============================================================
        // Logic
        //============================================================
        public static Type Resolve(string name)
        {
            if (string.IsNullOrEmpty(name) || !Regex.IsMatch(name, @"^[A-Za-z_][A-Za-z0-9_]*(\.[A-Za-z_][A-Za-z0-9_]*)*$"))
                return null;

            Type result = null;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int idx = 0; idx < assemblies.Length; idx++)
            {
                Type candidate = assemblies[idx].GetType(name, false, false);
                if (candidate == null || candidate == result)
                    continue;

                if (result != null)
                    return null;

                result = candidate;
            }

            return result;
        }
    }
}
