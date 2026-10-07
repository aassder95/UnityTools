using NUnit.Framework;

namespace UnityTools.Ui.Localization.Tests
{
    public class TmpIntegerTextTests
    {
        //============================================================
        // Logic
        //============================================================
        [TestCase(long.MinValue)]
        [TestCase(long.MaxValue)]
        [TestCase(0L)]
        [TestCase(-1L)]
        public void FormatsAllSignedValuesWithoutSharedState(long value)
        {
            var buffer = new char[100];
            Assert.That(TmpIntegerText.TryFormat(value, buffer, "HP ", " pt", out int length), Is.True);
            Assert.That(new string(buffer, 0, length), Is.EqualTo("HP " + value.ToString(System.Globalization.CultureInfo.InvariantCulture) + " pt"));
        }

        [Test]
        public void InsufficientBufferRemainsUnchanged()
        {
            char[] buffer = { 'x', 'y' };
            Assert.That(TmpIntegerText.TryFormat(100, buffer, "", "", out int length), Is.False);
            Assert.That(length, Is.Zero);
            Assert.That(buffer, Is.EqualTo(new[] { 'x', 'y' }));
        }
    }
}
