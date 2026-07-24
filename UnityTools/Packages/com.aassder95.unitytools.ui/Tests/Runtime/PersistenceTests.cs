using NUnit.Framework;
using UnityTools.Util.Core.Persistence;
using UnityTools.Util.Core.Timer.Period;
using UnityTools.Util.Core.Timer.Task;
using PersistenceService = UnityTools.Util.Core.Persistence.Persistence;

namespace UnityTools.Util.Tests.Persistence
{
    public class PersistenceTests
    {
        //============================================================
        // Logic
        //============================================================
        [Test]
        public void TrySaveReturnsFalseWhenStorageFails()
        {
            PersistenceService persistence = new("Test", storage: new StubStorage(false, false, null));
            Assert.That(persistence.TrySave("Value", 10), Is.False);
        }

        [Test]
        public void TryLoadReturnsDefaultWhenKeyIsMissing()
        {
            PersistenceService persistence = new("Test", storage: new StubStorage(true, false, null));
            Assert.That(persistence.TryLoad("Value", out int value, 7), Is.True);
            Assert.That(value, Is.EqualTo(7));
        }

        [Test]
        public void TryLoadReturnsFalseWhenStorageCheckFails()
        {
            PersistenceService persistence = new("Test", storage: new StubStorage(false, false, null));
            Assert.That(persistence.TryLoad("Value", out int value, 7), Is.False);
            Assert.That(value, Is.EqualTo(7));
        }

        [Test]
        public void TryLoadReturnsFalseForInvalidPrimitive()
        {
            PersistenceService persistence = new("Test", storage: new StubStorage(true, true, "invalid"));
            Assert.That(persistence.TryLoad("Value", out int value, 7), Is.False);
            Assert.That(value, Is.EqualTo(7));
        }

        [Test]
        public void TryHasKeyReturnsFalseWhenStorageFails()
        {
            PersistenceService persistence = new("Test", storage: new StubStorage(false, false, null));
            Assert.That(persistence.TryHasKey("Value", out bool hasKey), Is.False);
            Assert.That(hasKey, Is.False);
        }

        [Test]
        public void TryDeleteReturnsFalseWhenStorageFails()
        {
            PersistenceService persistence = new("Test", storage: new StubStorage(false, false, null));
            Assert.That(persistence.TryDelete("Value"), Is.False);
        }

        [Test]
        public void TryLoadClaimedReturnsFalseWhenStorageFails()
        {
            Assert.That(TaskTimerPersistence.TryLoadClaimed("Timer", out bool isClaimed, new StubStorage(false, false, null)), Is.False);
            Assert.That(isClaimed, Is.False);
        }

        [Test]
        public void TryDeleteAllReturnsFalseWhenStorageFails()
        {
            Assert.That(PeriodTimerPersistence.TryDeleteAll("Timer", new StubStorage(false, false, null)), Is.False);
        }

        //============================================================
        // Nested Types
        //============================================================
        private class StubStorage : IStorage
        {
            //============================================================
            // Readonly
            //============================================================
            private readonly bool _canAccess;
            private readonly bool _hasValue;
            private readonly string _value;

            //============================================================
            // Constructors
            //============================================================
            public StubStorage(bool canAccess, bool hasValue, string value)
            {
                _canAccess = canAccess;
                _hasValue = hasValue;
                _value = value;
            }

            //============================================================
            // Persistence
            //============================================================
            public bool TrySave(string key, string data)
            {
                return !string.IsNullOrWhiteSpace(key) && data != null && _canAccess;
            }

            public bool TryLoad(string key, out string data)
            {
                data = _value;
                return !string.IsNullOrWhiteSpace(key) && _canAccess && _hasValue;
            }

            public bool TryHasKey(string key, out bool hasKey)
            {
                hasKey = _hasValue;
                return !string.IsNullOrWhiteSpace(key) && _canAccess;
            }

            public bool TryDelete(string key)
            {
                return !string.IsNullOrWhiteSpace(key) && _canAccess;
            }
        }
    }
}
