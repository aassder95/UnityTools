using System;
using System.IO;
using NUnit.Framework;
using UnityTools.Util.Core.Persistence;

namespace UnityTools.Util.Tests.Persistence
{
    public class FileStorageTests
    {
        //============================================================
        // Fields
        //============================================================
        private string _rootPath;
        private string _outsidePath;
        private FileStorage _storage;

        //============================================================
        // Init/Register
        //============================================================
        [SetUp]
        public void SetUp()
        {
            string testId = Guid.NewGuid().ToString("N");
            _rootPath = Path.Combine(Path.GetTempPath(), "UnityTools_FileStorage_" + testId);
            _outsidePath = Path.Combine(Path.GetDirectoryName(_rootPath), "UnityTools_Outside_" + testId + ".txt");
            Assert.That(FileStorage.TryCreate(_rootPath, out _storage), Is.True);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_rootPath))
                Directory.Delete(_rootPath, true);
            if (File.Exists(_outsidePath))
                File.Delete(_outsidePath);
        }

        //============================================================
        // Logic
        //============================================================
        [Test]
        public void TrySaveAndLoadUsesRootFile()
        {
            Assert.That(_storage.TrySave("value.json", "payload"), Is.True);
            Assert.That(_storage.TryLoad("value.json", out string data), Is.True);
            Assert.That(data, Is.EqualTo("payload"));
            Assert.That(File.ReadAllText(Path.Combine(_rootPath, "value.json")), Is.EqualTo("payload"));
            Assert.That(_storage.TryDelete("value.json"), Is.True);
            Assert.That(_storage.TryHasKey("value.json", out bool hasKey), Is.True);
            Assert.That(hasKey, Is.False);
        }

        [Test]
        public void TryHasKeyDistinguishesMissingKey()
        {
            Assert.That(_storage.TryHasKey("missing.json", out bool hasKey), Is.True);
            Assert.That(hasKey, Is.False);
            Assert.That(_storage.TryLoad("missing.json", out string data), Is.False);
            Assert.That(data, Is.Null);
        }

        [Test]
        public void TrySaveRejectsParentTraversal()
        {
            string key = ".." + Path.DirectorySeparatorChar + Path.GetFileName(_outsidePath);
            Assert.That(_storage.TrySave(key, "escape"), Is.False);
            Assert.That(_storage.TryLoad(key, out _), Is.False);
            Assert.That(_storage.TryHasKey(key, out _), Is.False);
            Assert.That(_storage.TryDelete(key), Is.False);
            Assert.That(File.Exists(_outsidePath), Is.False);
        }

        [Test]
        public void TrySaveRejectsRootedPath()
        {
            Assert.That(_storage.TrySave(_outsidePath, "escape"), Is.False);
            Assert.That(_storage.TryLoad(_outsidePath, out _), Is.False);
            Assert.That(_storage.TryHasKey(_outsidePath, out _), Is.False);
            Assert.That(_storage.TryDelete(_outsidePath), Is.False);
            Assert.That(File.Exists(_outsidePath), Is.False);
        }
    }
}
