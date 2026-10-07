using System;
using System.Collections.Generic;

namespace UnityTools.Sheets.Editor
{
    public static class CsvConstraintValidator
    {
        //============================================================
        // Logic
        //============================================================
        public static void Validate(CsvTable table, string keyHeader, IReadOnlyList<CsvReferenceRule> references, List<CsvValidationIssue> issues)
        {
            if (table == null || issues == null)
                return;

            if (!string.IsNullOrEmpty(keyHeader))
            {
                if (!HasHeader(table, keyHeader))
                {
                    issues.Add(new CsvValidationIssue(0, keyHeader, "키 열이 CSV에 없습니다: " + keyHeader));
                }
                else
                {
                    HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);
                    for (int idx = 0; idx < table.Rows.Count; idx++)
                    {
                        CsvRow row = table.Rows[idx];
                        if (!row.TryGetCell(keyHeader, out string value) || string.IsNullOrEmpty(value) || !keys.Add(value))
                            issues.Add(new CsvValidationIssue(row.LineNum, keyHeader, "키가 비어 있거나 중복됩니다."));
                    }
                }
            }

            if (references == null)
                return;

            for (int idx = 0; idx < references.Count; idx++)
            {
                CsvReferenceRule rule = references[idx];
                if (rule == null || !HasHeader(table, rule.Header) || rule.TargetCsv == null)
                {
                    issues.Add(new CsvValidationIssue(0, rule == null ? string.Empty : rule.Header, "참조 규칙의 열과 대상 CSV를 연결하세요."));
                    continue;
                }

                if (!CsvParser.TryParse(rule.TargetCsv.text, out CsvTable target, out string error))
                {
                    issues.Add(new CsvValidationIssue(0, rule.Header, "대상 CSV 파싱 실패: " + error));
                    continue;
                }

                if (!HasHeader(target, rule.TargetHeader))
                {
                    issues.Add(new CsvValidationIssue(0, rule.Header, "대상 CSV에 참조 키 열이 없습니다: " + rule.TargetHeader));
                    continue;
                }

                HashSet<string> targets = new HashSet<string>(StringComparer.Ordinal);
                for (int rowIdx = 0; rowIdx < target.Rows.Count; rowIdx++)
                {
                    if (target.Rows[rowIdx].TryGetCell(rule.TargetHeader, out string value))
                        targets.Add(value);
                }

                for (int rowIdx = 0; rowIdx < table.Rows.Count; rowIdx++)
                {
                    CsvRow row = table.Rows[rowIdx];
                    if (!row.TryGetCell(rule.Header, out string value))
                    {
                        issues.Add(new CsvValidationIssue(row.LineNum, rule.Header, "참조 값을 읽을 수 없습니다."));
                        continue;
                    }
                    if (rule.IsArray && value.Length == 0)
                        continue;

                    string[] values = rule.IsArray ? value.Split('|') : new[] { value };
                    for (int valueIdx = 0; valueIdx < values.Length; valueIdx++)
                    {
                        if (string.IsNullOrEmpty(values[valueIdx]) || !targets.Contains(values[valueIdx]))
                            issues.Add(new CsvValidationIssue(row.LineNum, rule.Header, "대상 CSV에 참조 ID가 없습니다: " + values[valueIdx]));
                    }
                }
            }
        }

        //============================================================
        // Utilities
        //============================================================
        private static bool HasHeader(CsvTable table, string header)
        {
            for (int idx = 0; idx < table.Headers.Count; idx++)
            {
                if (table.Headers[idx] == header)
                    return true;
            }

            return false;
        }
    }
}
