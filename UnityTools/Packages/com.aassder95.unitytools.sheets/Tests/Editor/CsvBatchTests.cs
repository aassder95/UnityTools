using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace UnityTools.Sheets.Editor.Tests
{
    public class CsvBatchTests
    {
        //============================================================
        // Logic
        //============================================================
        [Test]
        public void ValidationFailurePreservesEveryOutputAndReportsAllPresets()
        {
            string folder = "Assets/BatchTests-" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", Path.GetFileName(folder));
            TextAsset csv = new TextAsset("Id\nbad\nwrong");
            CsvGeneratorPreset first = ScriptableObject.CreateInstance<CsvGeneratorPreset>();
            CsvGeneratorPreset second = ScriptableObject.CreateInstance<CsvGeneratorPreset>();
            try
            {
                first.Capture(csv, new[] { new CsvColumn("Id", "Id", ECsvColumnType.Int) }, new[] { "" }, "Batch.Data", "First", folder + "/First.cs");
                second.Capture(csv, new[] { new CsvColumn("Id", "Id", ECsvColumnType.Int) }, new[] { "" }, "Batch.Data", "Second", folder + "/Second.cs");
                File.WriteAllText(folder + "/First.cs", "// keep 한글");
                List<string> messages = new List<string>();
                Assert.That(CsvBatchGenerator.TryRun(new[] { first, second }, true, messages), Is.False);
                Assert.That(messages.Count, Is.EqualTo(4));
                Assert.That(File.ReadAllText(folder + "/First.cs"), Is.EqualTo("// keep 한글"));
                Assert.That(File.Exists(folder + "/Second.cs"), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(csv);
                UnityEngine.Object.DestroyImmediate(first);
                UnityEngine.Object.DestroyImmediate(second);
                AssetDatabase.DeleteAsset(folder);
            }
        }

        [Test]
        public void ValidBatchWritesUtf8AndRejectsDuplicateOrEscapingOutputs()
        {
            string folder = "Assets/BatchTests-" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", Path.GetFileName(folder));
            TextAsset csv = new TextAsset("Id\n1");
            CsvGeneratorPreset preset = ScriptableObject.CreateInstance<CsvGeneratorPreset>();
            try
            {
                List<string> messages = new List<string>();
                preset.Capture(csv, new[] { new CsvColumn("Id", "Id", ECsvColumnType.Int) }, new[] { "" }, "Batch.Data", "Entry", folder + "/Entry.cs");
                Assert.That(CsvBatchGenerator.TryRun(new[] { preset, preset }, true, messages), Is.False);
                Assert.That(File.Exists(folder + "/Entry.cs"), Is.False);
                Assert.That(CsvBatchGenerator.TryRun(new[] { preset }, false, messages), Is.True);
                Assert.That(File.Exists(folder + "/Entry.cs"), Is.False);
                Assert.That(CsvBatchGenerator.TryRun(new[] { preset }, true, messages), Is.True);
                Assert.That(File.ReadAllText(folder + "/Entry.cs"), Does.Contain("public class Entry"));
                preset.Capture(csv, new[] { new CsvColumn("Id", "Id", ECsvColumnType.Int) }, new[] { "" }, "Batch.Data", "Entry", "Assets/../Entry.cs");
                Assert.That(CsvBatchGenerator.TryRun(new[] { preset }, true, messages), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(csv);
                UnityEngine.Object.DestroyImmediate(preset);
                AssetDatabase.DeleteAsset(folder);
            }
        }
    }
}
