using System;
using System.Collections.Generic;

namespace UnityTools.Sheets.Editor
{
    public static class CsvTableDiff
    {
        //============================================================
        // Logic
        //============================================================
        public static bool TryCompare(CsvTable before, CsvTable after, string keyHeader, out IReadOnlyList<CsvChange> changes, out string error)
        {
            changes = null;
            error = string.Empty;
            if (!TryIndex(before, keyHeader, out Dictionary<string, CsvRow> oldRows, out error) || !TryIndex(after, keyHeader, out Dictionary<string, CsvRow> newRows, out error))
                return false;

            List<CsvChange> result = new List<CsvChange>();
            HashSet<string> oldHeaders = new HashSet<string>(before.Headers, StringComparer.Ordinal);
            HashSet<string> newHeaders = new HashSet<string>(after.Headers, StringComparer.Ordinal);
            for (int idx = 0; idx < before.Headers.Count; idx++)
            {
                string header = before.Headers[idx];
                if (!newHeaders.Contains(header))
                    result.Add(new CsvChange(ECsvChange.HeaderRemoved, string.Empty, header, header, string.Empty));
            }

            for (int idx = 0; idx < after.Headers.Count; idx++)
            {
                string header = after.Headers[idx];
                if (!oldHeaders.Contains(header))
                    result.Add(new CsvChange(ECsvChange.HeaderAdded, string.Empty, header, string.Empty, header));
            }

            foreach (KeyValuePair<string, CsvRow> entry in oldRows)
            {
                if (!newRows.TryGetValue(entry.Key, out CsvRow row))
                {
                    for (int idx = 0; idx < before.Headers.Count; idx++)
                    {
                        string header = before.Headers[idx];
                        if (entry.Value.TryGetCell(header, out string value))
                            result.Add(new CsvChange(ECsvChange.Removed, entry.Key, header, value, string.Empty));
                    }

                    continue;
                }

                for (int idx = 0; idx < after.Headers.Count; idx++)
                {
                    string header = after.Headers[idx];
                    bool hasBefore = entry.Value.TryGetCell(header, out string oldValue);
                    bool hasAfter = row.TryGetCell(header, out string newValue);
                    if (hasBefore != hasAfter || oldValue != newValue)
                        result.Add(new CsvChange(ECsvChange.Changed, entry.Key, header, oldValue ?? string.Empty, newValue ?? string.Empty));
                }

                for (int idx = 0; idx < before.Headers.Count; idx++)
                {
                    string header = before.Headers[idx];
                    if (!newHeaders.Contains(header) && entry.Value.TryGetCell(header, out string value))
                        result.Add(new CsvChange(ECsvChange.Changed, entry.Key, header, value, string.Empty));
                }
            }

            foreach (KeyValuePair<string, CsvRow> entry in newRows)
            {
                if (oldRows.ContainsKey(entry.Key))
                    continue;

                for (int idx = 0; idx < after.Headers.Count; idx++)
                {
                    string header = after.Headers[idx];
                    if (entry.Value.TryGetCell(header, out string value))
                        result.Add(new CsvChange(ECsvChange.Added, entry.Key, header, string.Empty, value));
                }
            }

            result.Sort((left, right) =>
            {
                int order = left.Kind.CompareTo(right.Kind);
                if (order == 0)
                    order = string.CompareOrdinal(left.Key, right.Key);

                return order != 0 ? order : string.CompareOrdinal(left.Header, right.Header);
            });
            changes = result.AsReadOnly();
            return true;
        }

        //============================================================
        // Utilities
        //============================================================
        private static bool TryIndex(CsvTable table, string keyHeader, out Dictionary<string, CsvRow> rows, out string error)
        {
            rows = new Dictionary<string, CsvRow>(StringComparer.Ordinal);
            error = string.Empty;
            if (table == null || string.IsNullOrEmpty(keyHeader) || !new HashSet<string>(table.Headers, StringComparer.Ordinal).Contains(keyHeader))
            {
                error = "양쪽 CSV에 고유 키 열이 필요합니다.";
                return false;
            }

            for (int idx = 0; idx < table.Rows.Count; idx++)
            {
                CsvRow row = table.Rows[idx];
                if (!row.TryGetCell(keyHeader, out string key) || string.IsNullOrEmpty(key) || rows.ContainsKey(key))
                {
                    error = row.LineNum + "행: 키가 비어 있거나 중복됩니다.";
                    return false;
                }

                rows.Add(key, row);
            }

            return true;
        }
    }
}
