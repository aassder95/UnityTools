using System;
using System.Collections.Generic;

namespace UnityTools.Sheets
{
    public class CsvDataSet<T> where T : class
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly IReadOnlyList<T> _items;
        private readonly Dictionary<string, IReadOnlyList<T>> _groups;

        //============================================================
        // Properties
        //============================================================
        public IReadOnlyList<T> Items => _items;

        //============================================================
        // Constructors
        //============================================================
        private CsvDataSet(List<T> items, Dictionary<string, List<T>> groups)
        {
            _items = items.AsReadOnly();
            _groups = new Dictionary<string, IReadOnlyList<T>>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, List<T>> group in groups)
            {
                _groups.Add(group.Key, group.Value.AsReadOnly());
            }
        }

        //============================================================
        // Logic
        //============================================================
        public static bool TryRead(CsvTable table, string keyHeader, CsvRowReader<T> reader, out CsvDataSet<T> dataSet, out string error)
        {
            return TryRead(table, keyHeader, reader, false, out dataSet, out error);
        }

        public static bool TryReadGroups(CsvTable table, string keyHeader, CsvRowReader<T> reader, out CsvDataSet<T> dataSet, out string error)
        {
            return TryRead(table, keyHeader, reader, true, out dataSet, out error);
        }

        public bool TryGet(string key, out T item)
        {
            item = null;
            if (key == null || !_groups.TryGetValue(key, out IReadOnlyList<T> group) || group.Count != 1)
                return false;

            item = group[0];
            return true;
        }

        public bool TryGetGroup(string key, out IReadOnlyList<T> items)
        {
            items = null;
            return key != null && _groups.TryGetValue(key, out items);
        }

        //============================================================
        // Utilities
        //============================================================
        private static bool TryRead(CsvTable table, string keyHeader, CsvRowReader<T> reader, bool canRepeatKeys, out CsvDataSet<T> dataSet, out string error)
        {
            dataSet = null;
            error = string.Empty;
            if (table == null || string.IsNullOrWhiteSpace(keyHeader) || reader == null)
            {
                error = "CSV 테이블, 키 헤더와 행 변환 함수가 필요합니다.";
                return false;
            }

            bool hasKeyHeader = false;
            for (int idx = 0; idx < table.Headers.Count; idx++)
            {
                if (table.Headers[idx] == keyHeader)
                {
                    hasKeyHeader = true;
                    break;
                }
            }

            if (!hasKeyHeader)
            {
                error = "키 헤더가 없습니다: " + keyHeader;
                return false;
            }

            List<T> items = new List<T>(table.Rows.Count);
            Dictionary<string, List<T>> groups = new Dictionary<string, List<T>>(StringComparer.Ordinal);
            for (int idx = 0; idx < table.Rows.Count; idx++)
            {
                CsvRow row = table.Rows[idx];
                if (row == null)
                {
                    error = "CSV 행이 없습니다: " + (idx + 1);
                    return false;
                }

                if (!row.TryGetCell(keyHeader, out string key) || string.IsNullOrWhiteSpace(key))
                {
                    error = row.LineNum + "행의 키가 비어 있거나 없습니다: " + keyHeader;
                    return false;
                }

                if (!canRepeatKeys && groups.ContainsKey(key))
                {
                    error = row.LineNum + "행의 키가 중복됩니다: " + key;
                    return false;
                }

                if (!reader(row, out T item, out string rowError))
                {
                    error = row.LineNum + "행 변환 실패: " + rowError;
                    return false;
                }

                if (item == null)
                {
                    error = row.LineNum + "행 변환 결과가 없습니다.";
                    return false;
                }

                items.Add(item);
                if (!groups.TryGetValue(key, out List<T> group))
                {
                    group = new List<T>();
                    groups.Add(key, group);
                }

                group.Add(item);
            }

            dataSet = new CsvDataSet<T>(items, groups);
            return true;
        }
    }
}
