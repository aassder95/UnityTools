using System.Collections.Generic;
using NUnit.Framework;

namespace UnityTools.Sheets.Tests
{
    public class CsvDataSetTests
    {
        //============================================================
        // Logic
        //============================================================
        [Test]
        public void ReadsInSourceOrderAndPreservesExactKeys()
        {
            Assert.That(CsvParser.TryParse("Id,Name\n01,첫번째\n1,두번째\nA,대문자\na,소문자\n A ,공백", out CsvTable table, out string error), Is.True, error);
            Assert.That(CsvDataSet<string>.TryRead(table, "Id", ReadName, out CsvDataSet<string> dataSet, out error), Is.True, error);
            Assert.That(dataSet.Items, Is.EqualTo(new[] { "첫번째", "두번째", "대문자", "소문자", "공백" }));
            Assert.That(dataSet.TryGet("01", out string first), Is.True);
            Assert.That(first, Is.EqualTo("첫번째"));
            Assert.That(dataSet.TryGet("1", out string second), Is.True);
            Assert.That(second, Is.EqualTo("두번째"));
            Assert.That(dataSet.TryGet("A", out string upper), Is.True);
            Assert.That(upper, Is.EqualTo("대문자"));
            Assert.That(dataSet.TryGet("a", out string lower), Is.True);
            Assert.That(lower, Is.EqualTo("소문자"));
            Assert.That(dataSet.TryGet(" A ", out string spaced), Is.True);
            Assert.That(spaced, Is.EqualTo("공백"));
            Assert.That(dataSet.TryGet(null, out string missing), Is.False);
            Assert.That(missing, Is.Null);
            Assert.That(dataSet.TryGet("absent", out missing), Is.False);
            Assert.That(((ICollection<string>)dataSet.Items).IsReadOnly, Is.True);
        }

        [TestCase("Id,Name\n1,first\n1,second", "3행")]
        [TestCase("Id,Name\n1,first\n,second", "3행")]
        [TestCase("Id,Name\n1,first\n   ,second", "3행")]
        [TestCase("Id,Name\n1,first\n2,invalid", "3행")]
        [TestCase("Id,Name\n1,first\n2,null", "3행")]
        [TestCase("Id,Name\n1,\"multi\nline\"\n1,last", "4행")]
        public void FailureDoesNotPublishPartialData(string csv, string expected)
        {
            Assert.That(CsvParser.TryParse(csv, out CsvTable table, out string error), Is.True, error);
            Assert.That(CsvDataSet<string>.TryRead(table, "Id", ReadName, out CsvDataSet<string> dataSet, out error), Is.False);
            Assert.That(dataSet, Is.Null);
            Assert.That(error, Does.Contain(expected));
        }

        [Test]
        public void EmptyTableStillRequiresKeyHeader()
        {
            Assert.That(CsvParser.TryParse("Id,Name", out CsvTable table, out string error), Is.True, error);
            Assert.That(CsvDataSet<string>.TryRead(table, "Id", ReadName, out CsvDataSet<string> dataSet, out error), Is.True, error);
            Assert.That(dataSet.Items, Is.Empty);
            Assert.That(CsvDataSet<string>.TryRead(table, "id", ReadName, out dataSet, out error), Is.False);
            Assert.That(dataSet, Is.Null);
            Assert.That(error, Does.Contain("키 헤더"));
            Assert.That(CsvDataSet<string>.TryRead(null, "Id", ReadName, out dataSet, out error), Is.False);
            Assert.That(CsvDataSet<string>.TryRead(table, "Id", null, out dataSet, out error), Is.False);
        }

        [Test]
        public void DuplicateKeyDoesNotInvokeReaderAgain()
        {
            Assert.That(CsvParser.TryParse("Id,Name\n1,first\n1,second", out CsvTable table, out string error), Is.True, error);
            int readCnt = 0;
            CsvRowReader<string> reader = (CsvRow row, out string item, out string rowError) =>
            {
                readCnt++;
                return ReadName(row, out item, out rowError);
            };
            Assert.That(CsvDataSet<string>.TryRead(table, "Id", reader, out CsvDataSet<string> dataSet, out error), Is.False);
            Assert.That(readCnt, Is.EqualTo(1));
            Assert.That(dataSet, Is.Null);
        }

        //============================================================
        // Utilities
        //============================================================
        private static bool ReadName(CsvRow row, out string item, out string error)
        {
            error = string.Empty;
            if (!row.TryGetCell("Name", out item))
                return false;

            if (item == "invalid")
            {
                error = "이름 변환 오류";
                return false;
            }

            if (item == "null")
                item = null;

            return true;
        }
    }
}
