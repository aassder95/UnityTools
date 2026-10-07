using NUnit.Framework;

namespace UnityTools.Ui.Tests
{
    public class EnumDisplayTests
    {
        //============================================================
        // Type Declarations
        //============================================================
        private enum EChoice
        {
            [EnumDisplayName("시작")] Start = -3,
            [EnumDisplayName("종료")] End = 19,
            Plain = 20,
            [EnumDisplayName("중복")] First = 30,
            [EnumDisplayName("중복")] Second = 31,
            [EnumDisplayName("별칭")] Alias = 19
        }

        //============================================================
        // Logic
        //============================================================
        [Test]
        public void PreservesValuesAndRejectsAmbiguousNames()
        {
            Assert.That(EnumDisplay<EChoice>.TryParseName("종료", out var value), Is.True);
            Assert.That((int)value, Is.EqualTo(19));
            Assert.That(EnumDisplay<EChoice>.TryGetName(EChoice.Alias, out string name), Is.True);
            Assert.That(name, Is.EqualTo("종료"));
            Assert.That(EnumDisplay<EChoice>.TryParseName("별칭", out value), Is.True);
            Assert.That(value, Is.EqualTo(EChoice.End));
            Assert.That(EnumDisplay<EChoice>.TryGetName(EChoice.Plain, out name), Is.True);
            Assert.That(name, Is.EqualTo("Plain"));
            Assert.That(EnumDisplay<EChoice>.TryParseName("중복", out _), Is.False);
            Assert.That(EnumDisplay<EChoice>.TryParseName(null, out _), Is.False);
            Assert.That(EnumDisplay<EChoice>.TryGetName((EChoice)200, out _), Is.False);
        }
    }
}
