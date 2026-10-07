using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;

namespace UnityTools.Sheets.Editor
{
    public static class CsvBatchGenerator
    {
        //============================================================
        // Logic
        //============================================================
        public static bool TryRun(IReadOnlyList<CsvGeneratorPreset> presets, bool shouldWrite, List<string> messages)
        {
            if (presets == null || messages == null)
                return false;

            messages.Clear();
            List<string> paths = new List<string>();
            List<string> sources = new List<string>();
            HashSet<string> outputPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> typeNames = new HashSet<string>(StringComparer.Ordinal);
            if (presets.Count == 0)
                messages.Add("프리셋을 한 개 이상 연결하세요.");

            for (int idx = 0; idx < presets.Count; idx++)
            {
                CsvGeneratorPreset preset = presets[idx];
                string label = preset == null ? (idx + 1) + "번째 프리셋" : preset.name;
                List<CsvValidationIssue> issues = new List<CsvValidationIssue>();
                if (!TryPrepare(preset, out string source, issues))
                {
                    for (int issueIdx = 0; issueIdx < issues.Count; issueIdx++)
                    {
                        messages.Add(label + ": " + issues[issueIdx].LineNum + "행 / " + issues[issueIdx].Header + " / " + issues[issueIdx].Message);
                    }
                }

                if (preset == null)
                    continue;

                string path = (preset.OutputPath ?? string.Empty).Replace('\\', '/');
                if (!IsOutputPathValid(path, preset.ClassName))
                    messages.Add(label + ": 출력은 Assets 아래의 기존 폴더와 클래스명.cs 경로여야 합니다.");

                if (!outputPaths.Add(path))
                    messages.Add(label + ": 출력 경로가 다른 프리셋과 중복됩니다.");

                if (!typeNames.Add(preset.NamespaceName + "." + preset.ClassName))
                    messages.Add(label + ": 생성 타입이 다른 프리셋과 중복됩니다.");

                paths.Add(path);
                sources.Add(source);
            }

            if (messages.Count > 0)
                return false;

            if (shouldWrite)
            {
                for (int idx = 0; idx < paths.Count; idx++)
                {
                    try
                    {
                        File.WriteAllText(paths[idx], sources[idx], new UTF8Encoding(false));
                        AssetDatabase.ImportAsset(paths[idx]);
                    }
                    catch (IOException ex)
                    {
                        messages.Add("파일 저장 실패: " + paths[idx] + " / " + ex.Message + " (앞서 저장된 파일은 유지됩니다.)");
                        return false;
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        messages.Add("파일 저장 권한 없음: " + paths[idx] + " / " + ex.Message + " (앞서 저장된 파일은 유지됩니다.)");
                        return false;
                    }
                }
            }

            messages.Add(presets.Count + "개 프리셋 " + (shouldWrite ? "생성 완료" : "검증 통과"));
            return true;
        }

        public static bool TryPrepare(CsvGeneratorPreset preset, out string source, List<CsvValidationIssue> issues)
        {
            source = null;
            if (issues == null)
                return false;

            issues.Clear();
            if (preset == null || preset.Csv == null)
            {
                issues.Add(new CsvValidationIssue(0, string.Empty, "프리셋과 CSV를 연결하세요."));
                return false;
            }

            if (!CsvParser.TryParse(preset.Csv.text, out CsvTable table, out string error))
            {
                issues.Add(new CsvValidationIssue(0, string.Empty, error));
                return false;
            }

            List<CsvColumn> columns = new List<CsvColumn>();
            List<string> enumNames = new List<string>();
            if (!preset.TryRestore(table, columns, enumNames, out error))
            {
                issues.Add(new CsvValidationIssue(0, string.Empty, error));
                return false;
            }

            issues.AddRange(CsvCodeGenerator.Validate(table, columns, preset.NamespaceName, preset.ClassName));
            CsvConstraintValidator.Validate(table, preset.KeyHeader, preset.References, issues);
            if (issues.Count > 0)
                return false;

            if (CsvCodeGenerator.TryGenerate(table, columns, preset.NamespaceName, preset.ClassName, out source, out error))
                return true;

            issues.Add(new CsvValidationIssue(0, string.Empty, error));
            return false;
        }

        //============================================================
        // Utilities
        //============================================================
        private static bool IsOutputPathValid(string path, string className)
        {
            if (!path.StartsWith("Assets/", StringComparison.Ordinal) || path.Contains("/../") || path.Contains("/./") || path.Contains("//") || path.IndexOfAny(new[] { ':', '*', '?', '"', '<', '>', '|' }) >= 0 || Path.GetExtension(path) != ".cs" || Path.GetFileNameWithoutExtension(path) != className)
                return false;

            string parent = Path.GetDirectoryName(path);
            return !string.IsNullOrEmpty(parent) && Directory.Exists(parent);
        }
    }
}
