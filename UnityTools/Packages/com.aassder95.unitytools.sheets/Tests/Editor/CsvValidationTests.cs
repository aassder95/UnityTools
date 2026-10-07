using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace UnityTools.Sheets.Editor.Tests
{
    public class CsvValidationTests
    {
        //============================================================
        // Logic
        //============================================================
        [Test]
        public void AllBadCellsAreReportedWithPhysicalLines()
        {
            Assert.That(CsvParser.TryParse("Id,Price,Name\nbad,no,\"two\nlines\"\nwrong,invalid,end", out CsvTable table, out string error), Is.True, error);
            CsvColumn[] columns = { new CsvColumn("Id", "Id", ECsvColumnType.Int), new CsvColumn("Price", "Price", ECsvColumnType.Float), new CsvColumn("Name", "Name", ECsvColumnType.String) };
            IReadOnlyList<CsvValidationIssue> issues = CsvCodeGenerator.Validate(table, columns, "Game.Data", "ItemData");
            Assert.That(issues.Count, Is.EqualTo(4));
            Assert.That(issues[0].LineNum, Is.EqualTo(2));
            Assert.That(issues[1].LineNum, Is.EqualTo(4));
            Assert.That(issues[2].Header, Is.EqualTo("Price"));
            Assert.That(CsvCodeGenerator.TryGenerate(table, columns, "Game.Data", "ItemData", out string source, out error), Is.False);
            Assert.That(source, Is.Null);
        }

        [Test]
        public void DuplicateEmptyKeysAndEveryMissingArrayReferenceAreReported()
        {
            TextAsset target = new TextAsset("Id\n10\n20");
            try
            {
                Assert.That(CsvParser.TryParse("Id,Refs\n1,10|30\n1,40|20\n,", out CsvTable table, out string error), Is.True, error);
                List<CsvValidationIssue> issues = new List<CsvValidationIssue>();
                CsvConstraintValidator.Validate(table, "Id", new[] { new CsvReferenceRule("Refs", target, "Id", true) }, issues);
                Assert.That(issues.Count, Is.EqualTo(4));
                Assert.That(issues[0].LineNum, Is.EqualTo(3));
                Assert.That(issues[1].LineNum, Is.EqualTo(4));
                Assert.That(issues[2].Message, Does.Contain("30"));
                Assert.That(issues[3].Message, Does.Contain("40"));
            }
            finally
            {
                Object.DestroyImmediate(target);
            }
        }

        [Test]
        public void MissingHeadersAreReportedEvenForEmptyTables()
        {
            TextAsset target = new TextAsset("Id\n10");
            try
            {
                Assert.That(CsvParser.TryParse("Id,Ref", out CsvTable table, out string error), Is.True, error);
                List<CsvValidationIssue> issues = new List<CsvValidationIssue>();
                CsvConstraintValidator.Validate(table, "Missing", new[] { new CsvReferenceRule("Ref", target, "Missing", false) }, issues);
                Assert.That(issues.Count, Is.EqualTo(2));
                Assert.That(issues[0].LineNum, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(target);
            }
        }

        [Test]
        public void ConstraintConfigurationIsSavedInPreset()
        {
            TextAsset target = new TextAsset("Id\n10");
            CsvGeneratorPreset preset = ScriptableObject.CreateInstance<CsvGeneratorPreset>();
            try
            {
                preset.Capture(null, new[] { new CsvColumn("Id", "Id", ECsvColumnType.Int) }, new[] { "" }, "Game.Data", "ItemData", "", "Id", new[] { new CsvReferenceRule("Id", target, "Id", false) });
                string json = JsonUtility.ToJson(preset);
                JsonUtility.FromJsonOverwrite(json, preset);
                Assert.That(preset.KeyHeader, Is.EqualTo("Id"));
                Assert.That(preset.References.Count, Is.EqualTo(1));
                Assert.That(preset.References[0].TargetCsv, Is.EqualTo(target));
            }
            finally
            {
                Object.DestroyImmediate(preset);
                Object.DestroyImmediate(target);
            }
        }
    }
}
