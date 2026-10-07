using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityTools.Sheets;
using UnityTools.Sheets.Editor;

public static class SheetsCompatibilityValidation
{
    //============================================================
    // Logic
    //============================================================
    public static void GenerateFixture()
    {
        string text = "Id,Name,Price,Weight,Enabled,Total,\"설명\n문자\"\n7,\"피자, \"\"맛\"\"\",1.25,2.5,1,9223372036854775807,\"한글\n설명\"";
        if (!CsvParser.TryParse(text, out CsvTable table, out string error))
        {
            Fail(error);
            return;
        }

        CsvColumn[] columns = {
            new CsvColumn("Id", "Id", ECsvColumnType.Int),
            new CsvColumn("Name", "Name", ECsvColumnType.String),
            new CsvColumn("Price", "Price", ECsvColumnType.Float),
            new CsvColumn("Weight", "Weight", ECsvColumnType.Double),
            new CsvColumn("Enabled", "IsEnabled", ECsvColumnType.Bool),
            new CsvColumn("Total", "Total", ECsvColumnType.Long),
            new CsvColumn("설명\n문자", "Description", ECsvColumnType.String)
        };
        if (!CsvCodeGenerator.TryGenerate(table, columns, "Validation.Data", "ItemData", out string source, out error))
        {
            Fail(error);
            return;
        }

        Directory.CreateDirectory("Assets/Generated");
        if (!CsvParser.TryParse("Grade\nRare", out CsvTable enumTable, out error) || !CsvCodeGenerator.TryGenerate(enumTable, new[] { new CsvColumn("Grade", "Grade", ECsvColumnType.Enum, Type.GetType("Validation.Data.ESheetGradeValidation, Assembly-CSharp")) }, "Validation.Data", "GradeData", out string enumSource, out error))
        {
            Fail(error);
            return;
        }

        File.WriteAllText("Assets/Generated/GradeData.cs", enumSource, new UTF8Encoding(false));
        string arrayCsv = "Strings,Ints,Longs,Floats,Doubles,Bools,Grades\n피자|한글,1|2,9223372036854775807|-1,1.25|2.5,1e100|2.5,true|0,Common|Rare";
        if (!CsvParser.TryParse(arrayCsv, out CsvTable arrayTable, out error))
        {
            Fail(error);
            return;
        }

        ECsvColumnType[] arrayTypes = { ECsvColumnType.String, ECsvColumnType.Int, ECsvColumnType.Long, ECsvColumnType.Float, ECsvColumnType.Double, ECsvColumnType.Bool, ECsvColumnType.Enum };
        CsvColumn[] arrayColumns = new CsvColumn[arrayTypes.Length];
        for (int idx = 0; idx < arrayColumns.Length; idx++)
        {
            arrayColumns[idx] = new CsvColumn(arrayTable.Headers[idx], arrayTable.Headers[idx], arrayTypes[idx], arrayTypes[idx] == ECsvColumnType.Enum ? Type.GetType("Validation.Data.ESheetGradeValidation, Assembly-CSharp") : null, true);
        }

        if (!CsvCodeGenerator.TryGenerate(arrayTable, arrayColumns, "Validation.Data", "ArrayData", out string arraySource, out error))
        {
            Fail(error);
            return;
        }

        File.WriteAllText("Assets/Generated/ArrayData.cs", arraySource, new UTF8Encoding(false));
        File.WriteAllText("Assets/Generated/ItemData.cs", source, new UTF8Encoding(false));
        File.WriteAllText("Assets/Generated/Fixture.csv", text, new UTF8Encoding(false));
        File.WriteAllText("Assets/Generated/SheetsSmoke.cs", SmokeSource(), new UTF8Encoding(false));
        ValidateProjectCsv();
        AssetDatabase.Refresh();
    }

    public static void VerifyGenerated()
    {
        Type type = Type.GetType("SheetsSmoke, Assembly-CSharp");
        if (type == null)
        {
            Fail("생성한 데이터 타입을 컴파일하지 못했습니다.");
            return;
        }

        string error = (string)type.GetMethod("ValidateData").Invoke(null, null);
        if (!string.IsNullOrEmpty(error))
        {
            Fail(error);
            return;
        }

        File.WriteAllText("generated-result.txt", "Generated type compile and read: Passed");
    }

    public static void PrepareScene()
    {
        CompatibilityValidation.PrepareScenes();
        var scene = EditorSceneManager.OpenScene("Assets/SmokeScene.unity");
        GameObject go = new GameObject("Sheets Smoke");
        go.AddComponent(Type.GetType("SheetsSmoke, Assembly-CSharp"));
        EditorSceneManager.SaveScene(scene);
    }

    //============================================================
    // Utilities
    //============================================================
    private static void ValidateProjectCsv()
    {
        if (!File.Exists("ProjectCsvPaths.txt"))
            return;

        StringBuilder report = new StringBuilder();
        string[] paths = File.ReadAllLines("ProjectCsvPaths.txt");
        for (int idx = 0; idx < paths.Length; idx++)
        {
            string path = paths[idx];
            string csv = File.ReadAllText(path);
            if (!CsvParser.TryParse(csv, out CsvTable table, out string error))
            {
                Fail(Path.GetFileName(path) + ": " + error);
                return;
            }

            CsvColumn[] columns = new CsvColumn[table.Headers.Count];
            for (int columnIdx = 0; columnIdx < columns.Length; columnIdx++)
            {
                string header = table.Headers[columnIdx];
                ECsvColumnType type = ECsvColumnType.String;
                bool isArray = header == "answer";
                if (header == "sauceId" || header == "stageIndex" || header == "reward1Amount" || header == "reward2Amount" || isArray)
                    type = ECsvColumnType.Int;
                else if (header == "randomWeight")
                    type = ECsvColumnType.Float;
                else if (header == "isFinal")
                    type = ECsvColumnType.Bool;

                string property = (type == ECsvColumnType.Bool ? "IsColumn" : "Column") + (columnIdx + 1);
                columns[columnIdx] = new CsvColumn(header, property, type, null, isArray);
            }

            string className = "ProjectCsv" + (idx + 1);
            if (!CsvCodeGenerator.TryGenerate(table, columns, "Validation.ProjectData", className, out string source, out error) || !CsvDataSet<CsvRow>.TryRead(table, table.Headers[0], ReadRow, out CsvDataSet<CsvRow> dataSet, out error))
            {
                Fail(Path.GetFileName(path) + ": " + error);
                return;
            }

            if (table.Rows.Count > 0 && (!table.Rows[0].TryGetCell(table.Headers[0], out string key) || !dataSet.TryGet(key, out CsvRow found) || found != table.Rows[0]))
            {
                Fail("실제 CSV의 키 조회 결과가 일치하지 않습니다.");
                return;
            }

            string groupHeader = table.Headers[0] == "stageId" ? "reward1Key" : "randomWeight";
            if (!CsvDataSet<CsvRow>.TryReadGroups(table, groupHeader, ReadRow, out CsvDataSet<CsvRow> grouped, out error))
            {
                Fail(error);
                return;
            }

            if (table.Rows.Count > 0)
            {
                if (!table.Rows[0].TryGetCell(groupHeader, out string groupKey) || !grouped.TryGetGroup(groupKey, out System.Collections.Generic.IReadOnlyList<CsvRow> group) || group.Count == 0 || group[0] != table.Rows[0])
                {
                    Fail("실제 CSV의 그룹 조회 결과가 일치하지 않습니다.");
                    return;
                }
            }

            File.WriteAllText("Assets/Generated/" + className + ".cs", source, new UTF8Encoding(false));
            report.AppendLine(Path.GetFileName(path) + " | rows=" + dataSet.Items.Count + " | group=" + groupHeader + " | passed");
        }

        File.WriteAllText("project-csv-result.txt", report.ToString(), new UTF8Encoding(false));
    }

    private static bool ReadRow(CsvRow row, out CsvRow item, out string error)
    {
        item = row;
        error = string.Empty;
        return true;
    }

    private static void Fail(string error)
    {
        Debug.LogError(error);
        EditorApplication.Exit(1);
    }

    private static string SmokeSource()
    {
        return @"using System;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityTools.Sheets;
using Validation.Data;

public class SheetsSmoke : MonoBehaviour
{
    private void Start()
    {
        string[] args = Environment.GetCommandLineArgs();
        int idx = Array.IndexOf(args, ""--sheet-result"");
        string error = ValidateData();
        if (idx >= 0 && idx + 1 < args.Length)
            File.WriteAllText(args[idx + 1], error.Length == 0 ? ""Passed"" : error);

        Application.Quit(error.Length == 0 ? 0 : 1);
    }

    public static string ValidateData()
    {
        CultureInfo prev = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(""fr-FR"");
            string arrayCsv = ""Strings,Ints,Longs,Floats,Doubles,Bools,Grades\n피자|한글,1|2,9223372036854775807|-1,1.25|2.5,1e100|2.5,true|0,Common|Rare"";
            if (!CsvParser.TryParse(arrayCsv, out CsvTable arrayTable, out string arrayError) || !ArrayData.TryRead(arrayTable.Rows[0], out ArrayData arrays, out arrayError))
                return ""생성 배열 타입을 읽지 못했습니다: "" + arrayError;

            if (arrays.Strings[1] != ""한글"" || arrays.Ints[1] != 2 || arrays.Longs[0] != long.MaxValue || arrays.Floats[0] != 1.25f || arrays.Doubles[0] != 1e100 || arrays.Bools[1] || arrays.Grades[1] != ESheetGradeValidation.Rare)
                return ""생성 배열 값이 일치하지 않습니다."";

            string[] badArrays = { ""a|"", ""1||2"", ""1|9223372036854775808"", ""1|NaN"", ""1|Infinity"", ""true|yes"", ""Common|20"" };
            for (int arrayIdx = 0; arrayIdx < badArrays.Length; arrayIdx++)
            {
                var arrayCells = new System.Collections.Generic.Dictionary<string, string>(arrayTable.Rows[0].Cells);
                arrayCells[arrayTable.Headers[arrayIdx]] = badArrays[arrayIdx];
                if (ArrayData.TryRead(new CsvRow(6, arrayCells), out arrays, out arrayError) || arrays != null || string.IsNullOrEmpty(arrayError))
                    return ""잘못된 배열의 부분 결과가 공개됐습니다."";
            }

            var emptyCells = new System.Collections.Generic.Dictionary<string, string>();
            for (int arrayIdx = 0; arrayIdx < arrayTable.Headers.Count; arrayIdx++)
            {
                emptyCells.Add(arrayTable.Headers[arrayIdx], string.Empty);
            }

            if (!ArrayData.TryRead(new CsvRow(7, emptyCells), out arrays, out arrayError) || arrays.Strings.Count != 0 || arrays.Ints.Count != 0 || arrays.Longs.Count != 0 || arrays.Floats.Count != 0 || arrays.Doubles.Count != 0 || arrays.Bools.Count != 0 || arrays.Grades.Count != 0)
                return ""빈 배열을 읽지 못했습니다: "" + arrayError;

            if (!CsvParser.TryParse(""Grade\nRare"", out CsvTable gradeTable, out string gradeError) || !GradeData.TryRead(gradeTable.Rows[0], out GradeData grade, out gradeError) || grade.Grade != ESheetGradeValidation.Rare)
                return ""생성 enum 타입을 읽지 못했습니다: "" + gradeError;

            string[] badGrades = { ""20"", ""rare"", "" Rare "", ""Rare,Common"", ""Unknown"", """" };
            for (int gradeIdx = 0; gradeIdx < badGrades.Length; gradeIdx++)
            {
                var cells = new System.Collections.Generic.Dictionary<string, string> { { ""Grade"", badGrades[gradeIdx] } };
                if (GradeData.TryRead(new CsvRow(4, cells), out grade, out gradeError) || grade != null || string.IsNullOrEmpty(gradeError))
                    return ""잘못된 enum 값을 정상 데이터로 읽었습니다."";
            }

            string csv = ""Id,Name,Price,Weight,Enabled,Total,\""설명\n문자\""\n7,\""피자, \""\""맛\""\""\"",1.25,2.5,1,9223372036854775807,\""한글\n설명\"""";
            if (!CsvParser.TryParse(csv, out CsvTable table, out string error))
                return error;

            if (!ItemData.TryRead(table.Rows[0], out ItemData data, out error))
                return error;

            if (data.Id != 7 || data.Name != ""피자, \""맛\"""" || data.Price != 1.25f || data.Weight != 2.5 || !data.IsEnabled || data.Total != long.MaxValue || data.Description != ""한글\n설명"")
                return ""생성 타입의 값이 일치하지 않습니다."";

            if (!CsvDataSet<ItemData>.TryRead(table, ""Id"", ItemData.TryRead, out CsvDataSet<ItemData> dataSet, out error))
                return error;

            if (dataSet.Items.Count != 1 || !dataSet.TryGet(""7"", out ItemData found) || found.Name != data.Name || dataSet.TryGet(""missing"", out found))
                return ""생성 타입의 키 조회 결과가 일치하지 않습니다."";

            string duplicateCsv = csv + ""\n"" + csv.Substring(csv.IndexOf(""\n7,"", StringComparison.Ordinal) + 1);
            if (!CsvParser.TryParse(duplicateCsv, out CsvTable duplicateTable, out error))
                return error;

            if (CsvDataSet<ItemData>.TryRead(duplicateTable, ""Id"", ItemData.TryRead, out dataSet, out error) || dataSet != null || string.IsNullOrEmpty(error))
                return ""중복 키 데이터가 부분 결과로 공개됐습니다."";

            if (!CsvDataSet<ItemData>.TryReadGroups(duplicateTable, ""Id"", ItemData.TryRead, out dataSet, out error))
                return error;

            if (!dataSet.TryGetGroup(""7"", out System.Collections.Generic.IReadOnlyList<ItemData> group) || group.Count != 2 || group[0].Name != data.Name || dataSet.TryGet(""7"", out found))
                return ""그룹 조회 또는 중복 키의 단일 조회 결과가 일치하지 않습니다."";

            string[] badValues = { ""wrong"", ""피자"", ""NaN"", ""Infinity"", ""yes"", ""9223372036854775808"", ""내용"" };
            for (int idx = 0; idx < 6; idx++)
            {
                if (idx == 1)
                    continue;

                string[] values = { ""7"", ""피자"", ""1.25"", ""2.5"", ""1"", ""100"", ""내용"" };
                values[idx] = badValues[idx];
                string invalid = ""Id,Name,Price,Weight,Enabled,Total,\""설명\n문자\""\n"" + string.Join("","", values);
                if (!CsvParser.TryParse(invalid, out table, out error))
                    return error;

                if (ItemData.TryRead(table.Rows[0], out data, out error) || data != null || string.IsNullOrEmpty(error))
                    return ""잘못된 셀을 정상 데이터로 읽었습니다."";
            }

            if (!CsvParser.TryParse(""Id\n7"", out table, out error))
                return error;

            if (ItemData.TryRead(table.Rows[0], out data, out error) || string.IsNullOrEmpty(error))
                return ""누락된 열을 정상 데이터로 읽었습니다."";

            return string.Empty;
        }
        finally
        {
            CultureInfo.CurrentCulture = prev;
        }
    }
}
";
    }
}
