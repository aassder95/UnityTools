using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityTools.Sheets.Editor;

namespace UnityTools.Sheets.Editor.Tests
{
    public class CsvGeneratorTests
    {
        [Test]
        public void GenerateValidatesDataAndProducesReadonlyType()
        {
            Assert.That(CsvParser.TryParse("Id,Name,Price,Enabled\n1,피자,1.25,true", out CsvTable table, out string error), Is.True, error);
            CsvColumn[] columns = {
                new CsvColumn("Id", "Id", ECsvColumnType.Int),
                new CsvColumn("Name", "Name", ECsvColumnType.String),
                new CsvColumn("Price", "Price", ECsvColumnType.Float),
                new CsvColumn("Enabled", "IsEnabled", ECsvColumnType.Bool)
            };
            Assert.That(CsvCodeGenerator.TryGenerate(table, columns, "Game.Data", "ItemData", out string source, out error), Is.True, error);
            Assert.That(source, Does.Contain("public int Id => _id;"));
            Assert.That(source, Does.Contain("public static bool TryRead"));
            Assert.That(source, Does.Not.Contain("public int Id;"));
            Assert.That(source, Does.Not.Contain("throw"));
        }

        [TestCase("bad name")]
        [TestCase("1Bad")]
        [TestCase("class")]
        [TestCase("TryRead")]
        [TestCase("ItemData")]
        [TestCase("CsvCellValue")]
        [TestCase("CsvSourceRow")]
        public void InvalidPropertyNamesCannotGenerateBrokenCode(string name)
        {
            Assert.That(CsvParser.TryParse("Id\n1", out CsvTable table, out string error), Is.True, error);
            Assert.That(CsvCodeGenerator.TryGenerate(table, new[] { new CsvColumn("Id", name, ECsvColumnType.Int) }, "Game.Data", "ItemData", out string source, out error), Is.False);
            Assert.That(source, Is.Null);
            Assert.That(error, Is.Not.Empty);
        }

        [TestCase("Game..Data", "ItemData")]
        [TestCase("class.Data", "ItemData")]
        [TestCase("Game.Data", "itemData")]
        [TestCase("Game.Data", "CsvSourceRow")]
        [TestCase("", "ItemData")]
        [TestCase("__arglist", "ItemData")]
        [TestCase("__makeref", "ItemData")]
        [TestCase("__reftype", "ItemData")]
        [TestCase("__refvalue", "ItemData")]
        public void InvalidNamespaceOrClassIsRejected(string namespaceName, string className)
        {
            Assert.That(CsvParser.TryParse("Id\n1", out CsvTable table, out string error), Is.True, error);
            Assert.That(CsvCodeGenerator.TryGenerate(table, new[] { new CsvColumn("Id", "Id", ECsvColumnType.Int) }, namespaceName, className, out string source, out error), Is.False);
        }

        [Test]
        public void SchemaMustCoverEachHeaderExactlyOnce()
        {
            Assert.That(CsvParser.TryParse("Id,Name\n1,Pizza", out CsvTable table, out string error), Is.True, error);
            Assert.That(CsvCodeGenerator.TryGenerate(table, new[] { new CsvColumn("Id", "Id", ECsvColumnType.Int) }, "Game.Data", "ItemData", out string source, out error), Is.False);
            Assert.That(CsvCodeGenerator.TryGenerate(table, new[] { new CsvColumn("Id", "Id", ECsvColumnType.Int), new CsvColumn("Id", "Name", ECsvColumnType.String) }, "Game.Data", "ItemData", out source, out error), Is.False);
            Assert.That(CsvCodeGenerator.TryGenerate(table, new[] { new CsvColumn("Id", "Id", ECsvColumnType.Int), new CsvColumn("Name", "Id", ECsvColumnType.String) }, "Game.Data", "ItemData", out source, out error), Is.False);
        }

        [Test]
        public void FailedConversionReportsPhysicalRowAndColumn()
        {
            Assert.That(CsvParser.TryParse("Id\n10\nwrong", out CsvTable table, out string error), Is.True, error);
            Assert.That(CsvCodeGenerator.TryGenerate(table, new[] { new CsvColumn("Id", "Id", ECsvColumnType.Int) }, "Game.Data", "ItemData", out string source, out error), Is.False);
            Assert.That(error, Does.Contain("3행").And.Contain("Id"));
        }

        [Test]
        public void QuotedHeaderIsEscapedAsCSharpLiteral()
        {
            Assert.That(CsvParser.TryParse("\"줄\n이름\"\ntext", out CsvTable table, out string error), Is.True, error);
            Assert.That(CsvCodeGenerator.TryGenerate(table, new[] { new CsvColumn("줄\n이름", "Name", ECsvColumnType.String) }, "Game.Data", "ItemData", out string source, out error), Is.True, error);
            Assert.That(source, Does.Contain("줄\\u000a이름"));
        }

        [Test]
        public void BrowserReadActionBuildsExplicitColumnMappings()
        {
            CsvGeneratorWindow window = ScriptableObject.CreateInstance<CsvGeneratorWindow>();
            TextAsset csv = new TextAsset("Id,Name\n1,피자");
            try
            {
                window.Show();
                typeof(CsvGeneratorWindow).GetField("_csv", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(window, csv);
                typeof(CsvGeneratorWindow).GetMethod("ReadCsv", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(window, null);
                CsvTable table = (CsvTable)typeof(CsvGeneratorWindow).GetField("_table", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(window);
                Assert.That(table.Rows.Count, Is.EqualTo(1));
                var columns = (System.Collections.Generic.IReadOnlyList<CsvColumn>)typeof(CsvGeneratorWindow).GetField("_columns", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(window);
                Assert.That(columns.Count, Is.EqualTo(2));
                Assert.That(columns[0].Header, Is.EqualTo("Id"));
                Assert.That(columns[0].Name, Is.EqualTo("Column1"));
                Assert.That(columns[0].Type, Is.EqualTo(ECsvColumnType.String));
            }
            finally
            {
                window.Close();
                UnityEngine.Object.DestroyImmediate(csv);
            }
        }

        [Test]
        public void BoolPropertyUsesStatePrefix()
        {
            Assert.That(CsvParser.TryParse("Enabled\n1", out CsvTable table, out string error), Is.True, error);
            Assert.That(CsvCodeGenerator.TryGenerate(table, new[] { new CsvColumn("Enabled", "Enabled", ECsvColumnType.Bool) }, "Game.Data", "ItemData", out string source, out error), Is.False);
            Assert.That(error, Does.Contain("bool Property"));
        }
    }
}
