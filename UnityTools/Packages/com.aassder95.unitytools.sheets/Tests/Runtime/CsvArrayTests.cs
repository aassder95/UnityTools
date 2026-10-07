using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace UnityTools.Sheets.Tests
{
    public class CsvArrayTests
    {
        [Test]
        public void ValuesKeepOrderAndCannotBeMutated()
        {
            Assert.That(CsvArray<int>.TryParse("3|1|2", CsvValue.TryParse, out IReadOnlyList<int> values), Is.True);
            Assert.That(values, Is.EqualTo(new[] { 3, 1, 2 }));
            Assert.That(((ICollection<int>)values).IsReadOnly, Is.True);
            Assert.That(CsvArray<int>.TryParse("", CsvValue.TryParse, out values), Is.True);
            Assert.That(values, Is.Empty);
            Assert.That(CsvArray<string>.TryParse("한글| a,b |line\nbreak", CsvValue.TryParse, out IReadOnlyList<string> texts), Is.True);
            Assert.That(texts, Is.EqualTo(new[] { "한글", " a,b ", "line\nbreak" }));
        }

        [TestCase(null)]
        [TestCase("1||2")]
        [TestCase("|1")]
        [TestCase("1|")]
        [TestCase("1|bad|2")]
        [TestCase("1|2147483648")]
        public void InvalidElementsDoNotPublishPartialResults(string text)
        {
            Assert.That(CsvArray<int>.TryParse(text, CsvValue.TryParse, out IReadOnlyList<int> values), Is.False);
            Assert.That(values, Is.Null);
        }

        [Test]
        public void AllSupportedReadersCanBeUsed()
        {
            Assert.That(CsvArray<long>.TryParse("9223372036854775807|-1", CsvValue.TryParse, out IReadOnlyList<long> longs), Is.True);
            Assert.That(longs[0], Is.EqualTo(long.MaxValue));
            Assert.That(CsvArray<float>.TryParse("1.25|2e2", CsvValue.TryParse, out IReadOnlyList<float> floats), Is.True);
            Assert.That(floats[1], Is.EqualTo(200.0f));
            Assert.That(CsvArray<double>.TryParse("1.25|NaN", CsvValue.TryParse, out IReadOnlyList<double> doubles), Is.False);
            Assert.That(doubles, Is.Null);
            Assert.That(CsvArray<bool>.TryParse("true|0|FALSE|1", CsvValue.TryParse, out IReadOnlyList<bool> bools), Is.True);
            Assert.That(bools, Is.EqualTo(new[] { true, false, false, true }));
            Assert.That(CsvArray<DayOfWeek>.TryParse("Monday|Sunday", CsvEnum<DayOfWeek>.TryParse, out IReadOnlyList<DayOfWeek> days), Is.True);
            Assert.That(days[1], Is.EqualTo(DayOfWeek.Sunday));
            Assert.That(CsvArray<DayOfWeek>.TryParse("Monday|1", CsvEnum<DayOfWeek>.TryParse, out days), Is.False);
            Assert.That(days, Is.Null);
            Assert.That(CsvArray<int>.TryParse("", null, out IReadOnlyList<int> invalid), Is.False);
        }
    }
}
