using System;
using System.Collections.Generic;
using System.Globalization;

namespace UnityTools.Sheets.Editor
{
    public static class CsvRuleValidator
    {
        //============================================================
        // Logic
        //============================================================
        public static void Validate(CsvTable table, IReadOnlyList<CsvValueRule> rules, List<CsvValidationIssue> issues)
        {
            if (table == null || rules == null || issues == null)
                return;

            HashSet<string> headers = new HashSet<string>(table.Headers, StringComparer.Ordinal);
            for (int idx = 0; idx < rules.Count; idx++)
            {
                CsvValueRule rule = rules[idx];
                if (rule == null || !headers.Contains(rule.Header) || !Enum.IsDefined(typeof(ECsvRule), rule.Kind))
                {
                    issues.Add(new CsvValidationIssue(0, rule == null ? string.Empty : rule.Header, "검증 규칙의 열 또는 종류가 잘못되었습니다."));
                    continue;
                }

                if (rule.Kind != ECsvRule.Required && (double.IsNaN(rule.Min) || double.IsNaN(rule.Max) || double.IsInfinity(rule.Min) || double.IsInfinity(rule.Max) || rule.Min > rule.Max || rule.Kind == ECsvRule.TextLength && (rule.Min < 0.0 || rule.Min != Math.Floor(rule.Min) || rule.Max != Math.Floor(rule.Max))))
                {
                    issues.Add(new CsvValidationIssue(0, rule.Header, "규칙의 최소/최대 범위를 확인하세요. 문자열 길이는 0 이상의 정수여야 합니다."));
                    continue;
                }

                for (int rowIdx = 0; rowIdx < table.Rows.Count; rowIdx++)
                {
                    CsvRow row = table.Rows[rowIdx];
                    bool isValid = row.TryGetCell(rule.Header, out string value);
                    if (isValid)
                    {
                        switch (rule.Kind)
                        {
                            case ECsvRule.Required:
                                isValid = !string.IsNullOrWhiteSpace(value);
                                break;
                            case ECsvRule.TextLength:
                                isValid = value.Length >= rule.Min && value.Length <= rule.Max;
                                break;
                            case ECsvRule.NumberRange:
                                isValid = double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double number) && !double.IsNaN(number) && !double.IsInfinity(number) && number >= rule.Min && number <= rule.Max;
                                break;
                        }
                    }

                    if (!isValid)
                        issues.Add(new CsvValidationIssue(row.LineNum, rule.Header, "값이 검증 규칙을 충족하지 않습니다: " + rule.Kind));
                }
            }
        }
    }
}
