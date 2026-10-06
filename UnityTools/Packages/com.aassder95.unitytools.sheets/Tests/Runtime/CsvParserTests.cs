using System.Globalization;
using NUnit.Framework;

namespace UnityTools.Sheets.Tests
{
    public class CsvParserTests
    {
        [Test]
        public void QuotedCellsPreserveDelimitersWhitespaceAndNewlines()
        {
            string csv = "\uFEFFId,설명,Empty\r\n1,\"  한글,\"\"따옴표\"\"\r\n다음 줄  \",\r\n\r\n2,#문자열,\"\"\r\n";
            Assert.That(CsvParser.TryParse(csv, out CsvTable table, out string error), Is.True, error);
            Assert.That(table.Rows.Count, Is.EqualTo(2));
            Assert.That(table.Rows[0].Cells["설명"], Is.EqualTo("  한글,\"따옴표\"\r\n다음 줄  "));
            Assert.That(table.Rows[0].Cells["Empty"], Is.Empty);
            Assert.That(table.Rows[1].Cells["설명"], Is.EqualTo("#문자열"));
            Assert.That(table.Rows[1].LineNum, Is.EqualTo(5));
        }

        [TestCase("Id,Name\n1, 피자 ", " 피자 ")]
        [TestCase("Id,Name\r1,Pizza", "Pizza")]
        [TestCase("Id,Name\r\n1,Pizza\r\n", "Pizza")]
        public void LineEndingsAndUnquotedSpacesArePreserved(string text, string expected)
        {
            Assert.That(CsvParser.TryParse(text, out CsvTable table, out string error), Is.True, error);
            Assert.That(table.Rows.Count, Is.EqualTo(1));
            Assert.That(table.Rows[0].Cells["Name"], Is.EqualTo(expected));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("\n\r\n")]
        [TestCase("Id,Id\n1,2")]
        [TestCase("Id,\n1,2")]
        [TestCase("Id,Name\n1")]
        [TestCase("Id\n1,2")]
        [TestCase("Id\n\"unfinished")]
        [TestCase("Id\nabc\"def")]
        [TestCase("Id\n\"ok\"extra")]
        [TestCase("Id\n\"ok\" ")]
        public void InvalidInputReturnsNoPartialTable(string text)
        {
            Assert.That(CsvParser.TryParse(text, out CsvTable table, out string error), Is.False);
            Assert.That(table, Is.Null);
            Assert.That(error, Is.Not.Empty);
        }

        [Test]
        public void HeaderOnlyAndCaseSensitiveNamesAreSupported()
        {
            Assert.That(CsvParser.TryParse("Id,id", out CsvTable table, out string error), Is.True, error);
            Assert.That(table.Rows, Is.Empty);
            Assert.That(CsvParser.TryParse("Id,id\n1,2", out table, out error), Is.True, error);
            Assert.That(table.Rows[0].TryGetCell("Id", out string value), Is.True);
            Assert.That(value, Is.EqualTo("1"));
            Assert.That(table.Rows[0].TryGetCell("ID", out value), Is.False);
            Assert.That(table.Rows[0].TryGetCell(null, out value), Is.False);
        }

        [Test]
        public void NumericParsingDoesNotDependOnMachineCulture()
        {
            CultureInfo prev = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                Assert.That(CsvValue.TryParse("1.25", out float value), Is.True);
                Assert.That(value, Is.EqualTo(1.25f));
                Assert.That(CsvValue.TryParse("1,25", out value), Is.False);
                Assert.That(CsvValue.TryParse("9e2", out double large), Is.True);
                Assert.That(large, Is.EqualTo(900.0));
                Assert.That(CsvValue.TryParse("2147483648", out int overflow), Is.False);
                Assert.That(CsvValue.TryParse("9223372036854775807", out long max), Is.True);
                Assert.That(max, Is.EqualTo(long.MaxValue));
            }
            finally
            {
                CultureInfo.CurrentCulture = prev;
            }
        }

        [TestCase("NaN")]
        [TestCase("Infinity")]
        [TestCase("-Infinity")]
        [TestCase("1e999")]
        public void NonFiniteNumbersAreRejected(string text)
        {
            Assert.That(CsvValue.TryParse(text, out float single), Is.False);
            Assert.That(CsvValue.TryParse(text, out double number), Is.False);
        }

        [TestCase("true", true)]
        [TestCase("FALSE", false)]
        [TestCase("1", true)]
        [TestCase(" 0 ", false)]
        public void BooleansSupportTextAndBinaryValues(string text, bool expected)
        {
            Assert.That(CsvValue.TryParse(text, out bool value), Is.True);
            Assert.That(value, Is.EqualTo(expected));
            Assert.That(CsvValue.TryParse("yes", out value), Is.False);
        }
    }
}
