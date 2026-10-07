using System.Collections.Generic;
using NUnit.Framework;

namespace UnityTools.Sheets.Editor.Tests
{
    public class CsvDiffTests
    {
        //============================================================
        // Logic
        //============================================================
        [Test]
        public void RowAndHeaderOrderDoNotProduceChanges()
        {
            Assert.That(CsvParser.TryParse("Id,Name\n1,피자\n2,소스", out CsvTable before, out string error), Is.True, error);
            Assert.That(CsvParser.TryParse("Name,Id\n소스,2\n피자,1", out CsvTable after, out error), Is.True, error);
            Assert.That(CsvTableDiff.TryCompare(before, after, "Id", out IReadOnlyList<CsvChange> changes, out error), Is.True, error);
            Assert.That(changes, Is.Empty);
        }

        [Test]
        public void AddedRemovedAndMultilineValuesAreReported()
        {
            Assert.That(CsvParser.TryParse("Id,Name,Old\n1,\"첫\n줄\",a\n2,삭제,b", out CsvTable before, out string error), Is.True, error);
            Assert.That(CsvParser.TryParse("Id,Name,New\n1,\"둘째\n줄\",x\n3,추가,y", out CsvTable after, out error), Is.True, error);
            Assert.That(CsvTableDiff.TryCompare(before, after, "Id", out IReadOnlyList<CsvChange> changes, out error), Is.True, error);
            Assert.That(changes.Count, Is.EqualTo(11));
            Assert.That(changes, Has.Some.Matches<CsvChange>(item => item.Kind == ECsvChange.Changed && item.Key == "1" && item.Header == "Name" && item.Before == "첫\n줄" && item.After == "둘째\n줄"));
            Assert.That(changes, Has.Some.Matches<CsvChange>(item => item.Kind == ECsvChange.HeaderRemoved && item.Header == "Old"));
            Assert.That(changes, Has.Some.Matches<CsvChange>(item => item.Kind == ECsvChange.HeaderAdded && item.Header == "New"));
        }

        [TestCase("Id,Name\n1,a\n1,b")]
        [TestCase("Id,Name\n,a")]
        [TestCase("Name\na")]
        public void AmbiguousKeysReturnNoPartialComparison(string csv)
        {
            Assert.That(CsvParser.TryParse("Id,Name\n1,a", out CsvTable before, out string error), Is.True, error);
            Assert.That(CsvParser.TryParse(csv, out CsvTable after, out error), Is.True, error);
            Assert.That(CsvTableDiff.TryCompare(before, after, "Id", out IReadOnlyList<CsvChange> changes, out error), Is.False);
            Assert.That(changes, Is.Null);
            Assert.That(error, Is.Not.Empty);
        }
    }
}
