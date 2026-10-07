using System;
using NUnit.Framework;

namespace UnityTools.Sheets.Editor.Tests
{
    public class CsvEnumGeneratorTests
    {
        [Test]
        public void GeneratesTypedEnumAndReaderAliases()
        {
            Assert.That(CsvParser.TryParse("Day\nMonday", out CsvTable table, out string error), Is.True, error);
            CsvColumn column = new CsvColumn("Day", "Day", ECsvColumnType.Enum, typeof(DayOfWeek));
            Assert.That(CsvCodeGenerator.TryGenerate(table, new[] { column }, "Game.Data", "ItemData", out string source, out error), Is.True, error);
            Assert.That(source, Does.Contain("using CsvEnumType0 = System.DayOfWeek;"));
            Assert.That(source, Does.Contain("public CsvEnumType0 Day => _day;"));
            Assert.That(source, Does.Contain("!CsvEnumReader0.TryParse(raw0, out CsvEnumType0 value0)"));
        }

        [TestCase("1")]
        [TestCase(" monday")]
        [TestCase("Monday ")]
        [TestCase("Monday,Tuesday")]
        public void InvalidEnumCellsPreventGeneration(string text)
        {
            CsvTable table = new CsvTable(new[] { "Day" }, new[] { new CsvRow(8, new System.Collections.Generic.Dictionary<string, string> { { "Day", text } }) });
            Assert.That(CsvCodeGenerator.TryGenerate(table, new[] { new CsvColumn("Day", "Day", ECsvColumnType.Enum, typeof(DayOfWeek)) }, "Game.Data", "ItemData", out string source, out string error), Is.False);
            Assert.That(source, Is.Null);
            Assert.That(error, Does.Contain("8행"));
        }

        [Test]
        public void MissingNonEnumAndEditorTypesAreRejectedEvenForEmptyTables()
        {
            CsvTable table = new CsvTable(new[] { "Day" }, new CsvRow[0]);
            foreach (Type type in new[] { null, typeof(string), typeof(ECsvColumnType) })
            {
                Assert.That(CsvCodeGenerator.TryGenerate(table, new[] { new CsvColumn("Day", "Day", ECsvColumnType.Enum, type) }, "Game.Data", "ItemData", out string source, out string error), Is.False);
                Assert.That(source, Is.Null);
            }
        }

        [TestCase("CsvEnumType0")]
        [TestCase("CsvEnumReader0")]
        public void GeneratedAliasNamesAreReserved(string name)
        {
            CsvTable table = new CsvTable(new[] { "Day" }, new CsvRow[0]);
            Assert.That(CsvCodeGenerator.TryGenerate(table, new[] { new CsvColumn("Day", name, ECsvColumnType.Enum, typeof(DayOfWeek)) }, "Game.Data", "ItemData", out string source, out string error), Is.False);
            Assert.That(CsvCodeGenerator.TryGenerate(table, new[] { new CsvColumn("Day", "Day", ECsvColumnType.Enum, typeof(DayOfWeek)) }, "Game.Data", name, out source, out error), Is.False);
        }
    }
}
