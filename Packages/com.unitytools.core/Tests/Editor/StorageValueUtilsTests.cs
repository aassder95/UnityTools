using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityTools.Util;

namespace UnityTools.Tests.Editor
{
    public class StorageValueUtilsTests
    {
        //============================================================
        //Logic
        //============================================================
        [Test]
        public void SaveString_NullValue_StoresEmpty()
        {
            MemoryStorage storage = new();

            StorageValueUtils.SaveString(storage, "key", null);

            Assert.That(storage.HasKey("key"), Is.True);
            Assert.That(storage.Load("key"), Is.EqualTo(string.Empty));
        }

        [Test]
        public void SaveString_NullStorage_DoesNothing()
        {
            Assert.DoesNotThrow(() => StorageValueUtils.SaveString(null, "key", "value"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void SaveString_InvalidKey_DoesNothing(string key)
        {
            MemoryStorage storage = new();

            StorageValueUtils.SaveString(storage, key, "value");

            Assert.That(storage.Count, Is.EqualTo(0));
        }

        [Test]
        public void LoadString_MissingKey_ReturnsEmpty()
        {
            MemoryStorage storage = new();

            string result = StorageValueUtils.LoadString(storage, "missing");

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void LoadString_NullValueInStorage_ReturnsEmpty()
        {
            MemoryStorage storage = new();
            storage.SetRaw("key", null);

            string result = StorageValueUtils.LoadString(storage, "key");

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void TryLoadDate_ValidTicks_ReturnsUtcDate()
        {
            MemoryStorage storage = new();
            DateTime expected = new DateTime(2026, 3, 18, 11, 22, 33, DateTimeKind.Utc);
            StorageValueUtils.SaveString(storage, "key", expected.Ticks.ToString());

            DateTime result = StorageValueUtils.TryLoadDate(storage, "key");

            Assert.That(result, Is.EqualTo(expected));
            Assert.That(result.Kind, Is.EqualTo(DateTimeKind.Utc));
        }

        [Test]
        public void TryLoadDate_InvalidTicks_ReturnsMinValue()
        {
            MemoryStorage storage = new();
            StorageValueUtils.SaveString(storage, "key", "invalid");

            DateTime result = StorageValueUtils.TryLoadDate(storage, "key");

            Assert.That(result, Is.EqualTo(DateTime.MinValue));
        }

        [Test]
        public void TryLoadDate_MinTicks_ReturnsMinValue()
        {
            MemoryStorage storage = new();
            StorageValueUtils.SaveString(storage, "key", DateTime.MinValue.Ticks.ToString());

            DateTime result = StorageValueUtils.TryLoadDate(storage, "key");

            Assert.That(result, Is.EqualTo(DateTime.MinValue));
        }

        [Test]
        public void TryLoadDate_OutOfRangeTicks_ReturnsMinValue()
        {
            MemoryStorage storage = new();
            long outOfRange = DateTime.MaxValue.Ticks + 1;
            StorageValueUtils.SaveString(storage, "key", outOfRange.ToString());

            DateTime result = StorageValueUtils.TryLoadDate(storage, "key");

            Assert.That(result, Is.EqualTo(DateTime.MinValue));
        }

        [Test]
        public void TryLoadDouble_ValidValue_ReturnsParsedValue()
        {
            MemoryStorage storage = new();
            StorageValueUtils.SaveString(storage, "key", "12.5");

            double result = StorageValueUtils.TryLoadDouble(storage, "key");

            Assert.That(result, Is.EqualTo(12.5d));
        }

        [Test]
        public void TryLoadDouble_NegativeValue_ClampsToZero()
        {
            MemoryStorage storage = new();
            StorageValueUtils.SaveString(storage, "key", "-1.5");

            double result = StorageValueUtils.TryLoadDouble(storage, "key");

            Assert.That(result, Is.EqualTo(0d));
        }

        [TestCase("NaN")]
        [TestCase("Infinity")]
        [TestCase("not-a-number")]
        public void TryLoadDouble_InvalidValue_ReturnsZero(string raw)
        {
            MemoryStorage storage = new();
            StorageValueUtils.SaveString(storage, "key", raw);

            double result = StorageValueUtils.TryLoadDouble(storage, "key");

            Assert.That(result, Is.EqualTo(0d));
        }

        [Test]
        public void TryLoadInt_ValidValue_ReturnsParsedValue()
        {
            MemoryStorage storage = new();
            StorageValueUtils.SaveString(storage, "key", "123");

            int result = StorageValueUtils.TryLoadInt(storage, "key");

            Assert.That(result, Is.EqualTo(123));
        }

        [Test]
        public void TryLoadInt_InvalidValue_ReturnsZero()
        {
            MemoryStorage storage = new();
            StorageValueUtils.SaveString(storage, "key", "invalid");

            int result = StorageValueUtils.TryLoadInt(storage, "key");

            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void HasKey_NullStorageOrInvalidKey_ReturnsFalse()
        {
            Assert.That(StorageValueUtils.HasKey(null, "key"), Is.False);
            Assert.That(StorageValueUtils.HasKey(new MemoryStorage(), null), Is.False);
            Assert.That(StorageValueUtils.HasKey(new MemoryStorage(), string.Empty), Is.False);
        }

        //============================================================
        //Types
        //============================================================
        private class MemoryStorage : IStorage
        {
            //============================================================
            //Readonly
            //============================================================
            private readonly Dictionary<string, string> _values = new();

            //============================================================
            //Properties
            //============================================================
            public int Count => _values.Count;

            //============================================================
            //Persistence
            //============================================================
            public void Save(string key, string data)
            {
                _values[key] = data;
            }

            public string Load(string key)
            {
                if(!_values.TryGetValue(key, out string value))
                    return string.Empty;

                return value;
            }

            public bool HasKey(string key)
            {
                return !string.IsNullOrEmpty(key) && _values.ContainsKey(key);
            }

            public void Delete(string key)
            {
                _values.Remove(key);
            }

            //============================================================
            //Utilities
            //============================================================
            public void SetRaw(string key, string value)
            {
                _values[key] = value;
            }
        }
    }
}
