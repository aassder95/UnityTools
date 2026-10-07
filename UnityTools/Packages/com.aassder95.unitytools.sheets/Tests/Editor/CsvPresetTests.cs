using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace UnityTools.Sheets.Editor.Tests
{
    public class CsvPresetTests
    {
        //============================================================
        // Logic
        //============================================================
        [Test]
        public void AssetRoundTripRestoresEnumArrayAndOutputAfterCsvRename()
        {
            string folder = "Assets/PresetTests-" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", Path.GetFileName(folder));
            try
            {
                File.WriteAllText(folder + "/Days.csv", "Days\nMonday|Tuesday");
                AssetDatabase.ImportAsset(folder + "/Days.csv");
                TextAsset csv = AssetDatabase.LoadAssetAtPath<TextAsset>(folder + "/Days.csv");
                CsvGeneratorPreset preset = ScriptableObject.CreateInstance<CsvGeneratorPreset>();
                preset.Capture(csv, new[] { new CsvColumn("Days", "Days", ECsvColumnType.Enum, typeof(DayOfWeek), true) }, new[] { "System.DayOfWeek" }, "Game.Data", "DaysData", "Assets/한글/DaysData.cs");
                AssetDatabase.CreateAsset(preset, folder + "/Preset.asset");
                AssetDatabase.SaveAssets();
                Assert.That(AssetDatabase.MoveAsset(folder + "/Days.csv", folder + "/Renamed.csv"), Is.Empty);
                AssetDatabase.ImportAsset(folder + "/Preset.asset", ImportAssetOptions.ForceUpdate);
                preset = AssetDatabase.LoadAssetAtPath<CsvGeneratorPreset>(folder + "/Preset.asset");
                Assert.That(AssetDatabase.GetAssetPath(preset.Csv), Does.EndWith("Renamed.csv"));
                Assert.That(preset.OutputPath, Is.EqualTo("Assets/한글/DaysData.cs"));
                Assert.That(preset.ClassName, Is.EqualTo("DaysData"));
                Assert.That(CsvParser.TryParse(preset.Csv.text, out CsvTable table, out string error), Is.True, error);
                List<CsvColumn> columns = new List<CsvColumn>();
                List<string> names = new List<string>();
                Assert.That(preset.TryRestore(table, columns, names, out error), Is.True, error);
                Assert.That(columns[0].EnumType, Is.EqualTo(typeof(DayOfWeek)));
                Assert.That(columns[0].IsArray, Is.True);
                Assert.That(CsvCodeGenerator.TryGenerate(table, columns, preset.NamespaceName, preset.ClassName, out string source, out error), Is.True, error);
            }
            finally
            {
                AssetDatabase.DeleteAsset(folder);
            }
        }

        [TestCase("Renamed\n1")]
        [TestCase("Id,New\n1,2")]
        public void HeaderChangesDoNotPartiallyApplyPreset(string text)
        {
            CsvGeneratorPreset preset = ScriptableObject.CreateInstance<CsvGeneratorPreset>();
            try
            {
                preset.Capture(null, new[] { new CsvColumn("Id", "Id", ECsvColumnType.Int) }, new[] { "" }, "Game.Data", "ItemData", "");
                Assert.That(CsvParser.TryParse(text, out CsvTable table, out string error), Is.True, error);
                List<CsvColumn> columns = new List<CsvColumn> { new CsvColumn("Keep", "Keep", ECsvColumnType.String) };
                List<string> names = new List<string> { "Keep" };
                Assert.That(preset.TryRestore(table, columns, names, out error), Is.False);
                Assert.That(columns[0].Header, Is.EqualTo("Keep"));
                Assert.That(names[0], Is.EqualTo("Keep"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(preset);
            }
        }
    }
}
