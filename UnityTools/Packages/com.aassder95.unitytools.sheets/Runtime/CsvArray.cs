using System;
using System.Collections.Generic;

namespace UnityTools.Sheets
{
    public static class CsvArray<T>
    {
        //============================================================
        // Logic
        //============================================================
        public static bool TryParse(string text, CsvCellReader<T> reader, out IReadOnlyList<T> values)
        {
            values = null;
            if (text == null || reader == null)
                return false;

            if (text.Length == 0)
            {
                values = Array.AsReadOnly(Array.Empty<T>());
                return true;
            }

            string[] cells = text.Split('|');
            T[] items = new T[cells.Length];
            for (int idx = 0; idx < cells.Length; idx++)
            {
                if (cells[idx].Length == 0 || !reader(cells[idx], out items[idx]))
                    return false;
            }

            values = Array.AsReadOnly(items);
            return true;
        }
    }
}
