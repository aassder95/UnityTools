using System;
using NUnit.Framework;

namespace UnityTools.Sheets.Editor.Tests
{
    public class CsvArrayGeneratorTests
    {
        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator WindowDisplaysEnumArraySchemaAndGeneratedPreview()
        {
            CsvGeneratorWindow window = UnityEngine.ScriptableObject.CreateInstance<CsvGeneratorWindow>();
            UnityEngine.TextAsset csv = new UnityEngine.TextAsset("Days,Ids\nMonday|Tuesday,1|2");
            System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            try
            {
                typeof(CsvGeneratorWindow).GetField("_csv", flags).SetValue(window, csv);
                typeof(CsvGeneratorWindow).GetMethod("ReadCsv", flags).Invoke(window, null);
                var columns = (System.Collections.Generic.List<CsvColumn>)typeof(CsvGeneratorWindow).GetField("_columns", flags).GetValue(window);
                var enumNames = (System.Collections.Generic.List<string>)typeof(CsvGeneratorWindow).GetField("_enumNames", flags).GetValue(window);
                columns[0] = new CsvColumn("Days", "Days", ECsvColumnType.Enum, typeof(DayOfWeek), true);
                columns[1] = new CsvColumn("Ids", "Ids", ECsvColumnType.Int, null, true);
                enumNames[0] = typeof(DayOfWeek).FullName;
                CsvTable table = (CsvTable)typeof(CsvGeneratorWindow).GetField("_table", flags).GetValue(window);
                Assert.That(CsvCodeGenerator.TryGenerate(table, columns, "Game.Data", "ItemData", out string source, out string error), Is.True, error);
                typeof(CsvGeneratorWindow).GetField("_source", flags).SetValue(window, source);
                window.Show();
                window.Repaint();
                yield return null;
                window.Repaint();
                yield return null;
                Assert.That(columns[0].IsArray, Is.True);
                Assert.That(columns[0].EnumType, Is.EqualTo(typeof(DayOfWeek)));
                Assert.That(typeof(CsvGeneratorWindow).GetField("_source", flags).GetValue(window), Is.EqualTo(source));
            }
            finally
            {
                window.Close();
                UnityEngine.Object.DestroyImmediate(csv);
            }
        }

        [TestCase(ECsvColumnType.String, "피자|한글")]
        [TestCase(ECsvColumnType.Int, "1|2|3")]
        [TestCase(ECsvColumnType.Long, "9223372036854775807|2")]
        [TestCase(ECsvColumnType.Float, "1.25|2.5")]
        [TestCase(ECsvColumnType.Double, "1e100|2.5")]
        [TestCase(ECsvColumnType.Bool, "true|0")]
        [TestCase(ECsvColumnType.Enum, "Monday|Tuesday")]
        public void GeneratesReadonlyListsWithTypedReaders(ECsvColumnType type, string text)
        {
            Assert.That(CsvParser.TryParse("Values\n" + text, out CsvTable table, out string error), Is.True, error);
            CsvColumn column = new CsvColumn("Values", "Values", type, type == ECsvColumnType.Enum ? typeof(DayOfWeek) : null, true);
            Assert.That(CsvCodeGenerator.TryGenerate(table, new[] { column }, "Game.Data", "ItemData", out string source, out error), Is.True, error);
            Assert.That(source, Does.Contain("public CsvArrayType0 Values => _values;"));
            Assert.That(source, Does.Contain("!CsvArrayReader0.TryParse(raw0,"));
        }

        [TestCase("1||2", ECsvColumnType.Int)]
        [TestCase("1|bad", ECsvColumnType.Int)]
        [TestCase("1|NaN", ECsvColumnType.Float)]
        [TestCase("Monday|1", ECsvColumnType.Enum)]
        [TestCase("a|", ECsvColumnType.String)]
        public void BadArrayElementsPreventGeneration(string text, ECsvColumnType type)
        {
            Assert.That(CsvParser.TryParse("Values\n" + text, out CsvTable table, out string error), Is.True, error);
            CsvColumn column = new CsvColumn("Values", "Values", type, type == ECsvColumnType.Enum ? typeof(DayOfWeek) : null, true);
            Assert.That(CsvCodeGenerator.TryGenerate(table, new[] { column }, "Game.Data", "ItemData", out string source, out error), Is.False);
            Assert.That(source, Is.Null);
            Assert.That(error, Does.Contain("2행"));
        }
    }
}
