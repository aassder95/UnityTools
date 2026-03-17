using System.IO;
using System.Text;
using UnityEngine;

namespace UnityTools.Util
{
    //============================================================
    //Logic
    //============================================================
    public static class CsvJsonConverter
    {
        public static void Convert(string csvPath, string jsonPath)
        {
            string[] lines = File.Exists(csvPath) ? File.ReadAllLines(csvPath) : null;
            if (lines == null || lines.Length <= 1)
            {
                File.WriteAllText(jsonPath, "[]");
                return;
            }

            string[] headers = lines[0].Trim().Split(',');
            StringBuilder sb = new StringBuilder("[");
            bool isFirstLine = true;

            for (int i = 1; i < lines.Length; i++)
            {
                string line = ConvertLineToJson(headers, lines[i]);
                if (line == null)
                    continue;

                if (!isFirstLine)
                    sb.Append(",");

                sb.Append(line);
                isFirstLine = false;
            }

            sb.Append("]");
            File.WriteAllText(jsonPath, sb.ToString());
            Debug.Log("[CsvJsonConverter:Convert] completed: " + jsonPath);
        }

        private static string ConvertLineToJson(string[] headers, string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            string[] values = line.Trim().Split(',');
            StringBuilder sb = new StringBuilder("{");
            bool isFirstPair = true;

            for (int i = 0; i < headers.Length; i++)
            {
                string header = headers[i].Trim();
                if (header.StartsWith("_"))
                    continue;

                string value = (i < values.Length) ? values[i].Trim() : "";

                if (!isFirstPair)
                    sb.Append(",");

                sb.Append("\"").Append(header).Append("\":\"").Append(value).Append("\"");
                isFirstPair = false;
            }

            sb.Append("}");
            return isFirstPair ? null : sb.ToString();
        }
    }
}
