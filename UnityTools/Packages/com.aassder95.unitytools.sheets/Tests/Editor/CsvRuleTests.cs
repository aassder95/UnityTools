using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace UnityTools.Sheets.Editor.Tests
{
    public class CsvRuleTests
    {
        //============================================================
        // Logic
        //============================================================
        [Test]
        public void RequiredRangeAndLengthCollectEveryFailure()
        {
            Assert.That(CsvParser.TryParse("Name,Price\n피자,0\n소스,10\n   ,-1\n너무긴이름,NaN", out CsvTable table, out string error), Is.True, error);
            CsvValueRule[] rules = { new CsvValueRule("Name", ECsvRule.Required), new CsvValueRule("Name", ECsvRule.TextLength, 1.0, 3.0), new CsvValueRule("Price", ECsvRule.NumberRange, 0.0, 10.0) };
            List<CsvValidationIssue> issues = new List<CsvValidationIssue>();
            CsvRuleValidator.Validate(table, rules, issues);
            Assert.That(issues.Count, Is.EqualTo(4));
            Assert.That(issues, Has.Some.Matches<CsvValidationIssue>(item => item.LineNum == 4 && item.Header == "Name"));
            Assert.That(issues, Has.Some.Matches<CsvValidationIssue>(item => item.LineNum == 5 && item.Header == "Price"));
        }

        [TestCase(ECsvRule.NumberRange, 10.0, 1.0)]
        [TestCase(ECsvRule.NumberRange, double.NaN, 1.0)]
        [TestCase(ECsvRule.TextLength, -1.0, 2.0)]
        [TestCase(ECsvRule.TextLength, 0.5, 2.0)]
        public void InvalidConfigurationIsRejectedEvenWithoutRows(ECsvRule kind, double min, double max)
        {
            Assert.That(CsvParser.TryParse("Id", out CsvTable table, out string error), Is.True, error);
            List<CsvValidationIssue> issues = new List<CsvValidationIssue>();
            CsvRuleValidator.Validate(table, new[] { new CsvValueRule("Id", kind, min, max) }, issues);
            Assert.That(issues.Count, Is.EqualTo(1));
            Assert.That(issues[0].LineNum, Is.Zero);
        }

        [Test]
        public void RulesSurvivePresetSerializationAndBlockBatchGeneration()
        {
            TextAsset csv = new TextAsset("Id\n99");
            CsvGeneratorPreset preset = ScriptableObject.CreateInstance<CsvGeneratorPreset>();
            try
            {
                preset.Capture(csv, new[] { new CsvColumn("Id", "Id", ECsvColumnType.Int) }, new[] { "" }, "Game.Data", "ItemData", "Assets/ItemData.cs", "", null, new[] { new CsvValueRule("Id", ECsvRule.NumberRange, 0.0, 10.0) });
                string json = JsonUtility.ToJson(preset);
                JsonUtility.FromJsonOverwrite(json, preset);
                Assert.That(preset.Rules.Count, Is.EqualTo(1));
                Assert.That(preset.Rules[0].Max, Is.EqualTo(10.0));
                List<CsvValidationIssue> issues = new List<CsvValidationIssue>();
                Assert.That(CsvBatchGenerator.TryPrepare(preset, out string source, issues), Is.False);
                Assert.That(source, Is.Null);
                Assert.That(issues.Count, Is.EqualTo(1));
                Assert.That(issues[0].Header, Is.EqualTo("Id"));
            }
            finally
            {
                Object.DestroyImmediate(csv);
                Object.DestroyImmediate(preset);
            }
        }
    }
}
