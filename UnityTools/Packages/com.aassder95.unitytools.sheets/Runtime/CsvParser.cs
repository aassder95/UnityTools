using System;
using System.Collections.Generic;
using System.Text;

namespace UnityTools.Sheets
{
    public static class CsvParser
    {
        //============================================================
        // Type Declarations
        //============================================================
        private enum ECellState { Start, Unquoted, Quoted, Closed }

        //============================================================
        // Logic
        //============================================================
        public static bool TryParse(string text, out CsvTable table, out string error)
        {
            table = null;
            error = string.Empty;
            if (string.IsNullOrEmpty(text))
            {
                error = "CSV 내용이 비어 있습니다.";
                return false;
            }

            List<string> headers = null;
            List<CsvRow> rows = new List<CsvRow>();
            List<string> cells = new List<string>();
            StringBuilder cell = new StringBuilder();
            ECellState state = ECellState.Start;
            int lineNum = 1;
            int rowLineNum = 1;
            bool hasContent = false;
            int startIdx = text[0] == '\uFEFF' ? 1 : 0;
            for (int idx = startIdx; idx <= text.Length; idx++)
            {
                bool isEnd = idx == text.Length;
                char ch = isEnd ? '\0' : text[idx];
                bool isNewline = ch == '\r' || ch == '\n';
                if (state == ECellState.Quoted)
                {
                    if (isEnd)
                    {
                        error = $"{rowLineNum}행: 따옴표가 닫히지 않았습니다.";
                        return false;
                    }

                    if (ch == '"')
                    {
                        if (idx + 1 < text.Length && text[idx + 1] == '"')
                        {
                            cell.Append('"');
                            idx++;
                        }
                        else
                        {
                            state = ECellState.Closed;
                        }
                    }
                    else
                    {
                        cell.Append(ch);
                        if (isNewline)
                        {
                            if (ch == '\r' && idx + 1 < text.Length && text[idx + 1] == '\n')
                                cell.Append(text[++idx]);

                            lineNum++;
                        }
                    }

                    continue;
                }

                if (isEnd || isNewline || ch == ',')
                {
                    cells.Add(cell.ToString());
                    cell.Clear();
                    state = ECellState.Start;
                    if (ch == ',')
                    {
                        hasContent = true;
                        continue;
                    }

                    if (hasContent && !TryAddRow(cells, rowLineNum, ref headers, rows, out error))
                        return false;

                    cells.Clear();
                    hasContent = false;
                    if (ch == '\r' && idx + 1 < text.Length && text[idx + 1] == '\n')
                        idx++;

                    if (isNewline)
                        lineNum++;

                    rowLineNum = lineNum;
                    continue;
                }

                if (state == ECellState.Closed || (ch == '"' && state != ECellState.Start))
                {
                    error = $"{lineNum}행, {cells.Count + 1}열: 따옴표 위치가 잘못되었습니다.";
                    return false;
                }

                hasContent = true;
                if (ch == '"')
                {
                    state = ECellState.Quoted;
                }
                else
                {
                    state = ECellState.Unquoted;
                    cell.Append(ch);
                }
            }

            if (headers == null)
            {
                error = "CSV 헤더가 없습니다.";
                return false;
            }

            table = new CsvTable(headers, rows);
            return true;
        }

        //============================================================
        // Utilities
        //============================================================
        private static bool TryAddRow(List<string> cells, int lineNum, ref List<string> headers, List<CsvRow> rows, out string error)
        {
            error = string.Empty;
            if (headers == null)
            {
                HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);
                for (int idx = 0; idx < cells.Count; idx++)
                {
                    if (string.IsNullOrWhiteSpace(cells[idx]) || !names.Add(cells[idx]))
                    {
                        error = $"{lineNum}행, {idx + 1}열: 비어 있거나 중복된 헤더입니다.";
                        return false;
                    }
                }

                headers = new List<string>(cells);
                return true;
            }

            if (cells.Count != headers.Count)
            {
                error = $"{lineNum}행: 열 개수 {cells.Count}개가 헤더 {headers.Count}개와 다릅니다.";
                return false;
            }

            Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int idx = 0; idx < headers.Count; idx++)
            {
                values.Add(headers[idx], cells[idx]);
            }

            rows.Add(new CsvRow(lineNum, values));
            return true;
        }
    }
}
