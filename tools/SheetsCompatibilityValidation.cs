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
        File.WriteAllText("Assets/Generated/ItemData.cs", source, new UTF8Encoding(false));
        File.WriteAllText("Assets/Generated/Fixture.csv", text, new UTF8Encoding(false));
        File.WriteAllText("Assets/Generated/SheetsSmoke.cs", SmokeSource(), new UTF8Encoding(false));
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
            string csv = ""Id,Name,Price,Weight,Enabled,Total,\""설명\n문자\""\n7,\""피자, \""\""맛\""\""\"",1.25,2.5,1,9223372036854775807,\""한글\n설명\"""";
            if (!CsvParser.TryParse(csv, out CsvTable table, out string error))
                return error;

            if (!ItemData.TryRead(table.Rows[0], out ItemData data, out error))
                return error;

            if (data.Id != 7 || data.Name != ""피자, \""맛\"""" || data.Price != 1.25f || data.Weight != 2.5 || !data.IsEnabled || data.Total != long.MaxValue || data.Description != ""한글\n설명"")
                return ""생성 타입의 값이 일치하지 않습니다."";

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
