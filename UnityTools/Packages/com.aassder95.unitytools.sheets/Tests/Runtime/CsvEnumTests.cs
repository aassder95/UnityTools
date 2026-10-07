using System;
using NUnit.Framework;

namespace UnityTools.Sheets.Tests
{
    public class CsvEnumTests
    {
        [TestCase("Monday", true)]
        [TestCase("Sunday", true)]
        [TestCase("monday", false)]
        [TestCase(" Monday ", false)]
        [TestCase("1", false)]
        [TestCase("999", false)]
        [TestCase("Monday,Tuesday", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void OnlyExactDeclaredNamesAreAccepted(string text, bool expected)
        {
            Assert.That(CsvEnum<DayOfWeek>.TryParse(text, out DayOfWeek value), Is.EqualTo(expected));
            if (expected)
            {
                Assert.That(value.ToString(), Is.EqualTo(text));
            }
            else
            {
                Assert.That(value, Is.EqualTo(default(DayOfWeek)));
            }
        }

        [Test]
        public void FlagsAcceptDeclaredNamesAndRejectCombinations()
        {
            Assert.That(CsvEnum<AttributeTargets>.TryParse("All", out AttributeTargets value), Is.True);
            Assert.That(value, Is.EqualTo(AttributeTargets.All));
            Assert.That(CsvEnum<AttributeTargets>.TryParse("Class,Method", out value), Is.False);
        }
    }
}
