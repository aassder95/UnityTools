using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace UnityTools.Persistence.Tests
{
    public class PersistenceTests
    {
        //============================================================
        // Fields
        //============================================================
        private string _directory;
        private string _path;

        //============================================================
        // Init/Register
        //============================================================
        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "UnityTools-Persistence-" + Guid.NewGuid().ToString("N"));
            _path = Path.Combine(_directory, "save.json");
            Directory.CreateDirectory(_directory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directory))
                Directory.Delete(_directory, true);
        }

        //============================================================
        // Logic
        //============================================================
        [Test]
        public void RecoversBackupWithoutReplacingItWithCorruptPrimary()
        {
            VersionedSaveStore<SaveV1> store = CreateV1Store();
            Assert.That(store.TrySave(new SaveV1(1)), Is.True);
            Assert.That(store.TrySave(new SaveV1(2)), Is.True);
            string backup = File.ReadAllText(_path + ".bak");
            string primary = File.ReadAllText(_path);
            File.WriteAllText(_path, primary.Replace("\"_hash\":\"", "\"_hash\":\"tampered"));

            Assert.That(store.TryLoad(out SaveV1 data, out bool wasRecovered, out bool wasMigrated), Is.True);
            Assert.That(data.Level, Is.EqualTo(1));
            Assert.That(wasRecovered, Is.True);
            Assert.That(wasMigrated, Is.False);
            Assert.That(store.TrySave(data), Is.True);
            Assert.That(File.ReadAllText(_path + ".bak"), Is.EqualTo(backup));
            Assert.That(store.TryLoad(out SaveV1 restored, out wasRecovered, out _), Is.True);
            Assert.That(restored.Level, Is.EqualTo(1));
            Assert.That(wasRecovered, Is.False);
        }

        [Test]
        public void MigratesV1AndPersistsV2()
        {
            Assert.That(CreateV1Store().TrySave(new SaveV1(3)), Is.True);
            VersionedSaveStore<SaveV2> store = CreateV2Store(new ISaveMigration[] { new V1ToV2Migration() });

            Assert.That(store.TryLoad(out SaveV2 data, out bool wasRecovered, out bool wasMigrated), Is.True);
            Assert.That(data.Level, Is.EqualTo(3));
            Assert.That(data.BranchCnt, Is.EqualTo(4));
            Assert.That(wasRecovered, Is.False);
            Assert.That(wasMigrated, Is.True);
            Assert.That(store.TrySave(data), Is.True);

            VersionedSaveStore<SaveV2> reloadedStore = CreateV2Store(null);
            Assert.That(reloadedStore.TryLoad(out SaveV2 reloaded, out _, out wasMigrated), Is.True);
            Assert.That(reloaded.BranchCnt, Is.EqualTo(4));
            Assert.That(wasMigrated, Is.False);
        }

        [Test]
        public void CorruptPrimaryWithoutBackupFails()
        {
            Assert.That(CreateV1Store().TrySave(new SaveV1(1)), Is.True);
            File.WriteAllText(_path, "{}");
            Assert.That(CreateV1Store().TryLoad(out SaveV1 data, out bool wasRecovered, out bool wasMigrated), Is.False);
            Assert.That(data, Is.Null);
            Assert.That(wasRecovered, Is.False);
            Assert.That(wasMigrated, Is.False);
        }

        [Test]
        public void FuturePrimaryDoesNotFallBackToOlderBackup()
        {
            VersionedSaveStore<SaveV1> firstStore = CreateV1Store();
            Assert.That(firstStore.TrySave(new SaveV1(1)), Is.True);
            Assert.That(firstStore.TrySave(new SaveV1(2)), Is.True);
            Assert.That(File.Exists(_path + ".bak"), Is.True);
            Assert.That(CreateV2Store(null).TrySave(new SaveV2(2, 3)), Is.True);
            VersionedSaveStore<SaveV1> oldStore = CreateV1Store();

            Assert.That(oldStore.TryLoad(out SaveV1 data, out bool wasRecovered, out _), Is.False);
            Assert.That(data, Is.Null);
            Assert.That(wasRecovered, Is.False);
            Assert.That(oldStore.TrySave(new SaveV1(9)), Is.False);
        }

        [Test]
        public void ValidationAndMissingMigrationPreventLoad()
        {
            VersionedSaveStore<SaveV1> store = CreateV1Store();
            Assert.That(store.TrySave(new SaveV1(-1)), Is.False);
            Assert.That(File.Exists(_path), Is.False);
            Assert.That(store.TrySave(new SaveV1(1)), Is.True);
            Assert.That(CreateV2Store(null).TryLoad(out SaveV2 data, out _, out _), Is.False);
            Assert.That(data, Is.Null);
        }

        [Test]
        public void DuplicateMigrationIsRejected()
        {
            ISaveMigration[] migrations = { new V1ToV2Migration(), new V1ToV2Migration() };
            Assert.That(VersionedSaveStore<SaveV2>.TryCreate(_path, 2, new UnityJsonSaveCodec<SaveV2>(), IsValid,
                migrations, out _), Is.False);
        }

        [TestCase(null, 1)]
        [TestCase("", 1)]
        [TestCase(" ", 1)]
        [TestCase("save.json", 0)]
        public void InvalidCreationReturnsNoStore(string path, int version)
        {
            Assert.That(VersionedSaveStore<SaveV1>.TryCreate(path, version, new UnityJsonSaveCodec<SaveV1>(), IsValid, null, out VersionedSaveStore<SaveV1> store), Is.False);
            Assert.That(store, Is.Null);
        }

        [Test]
        public void MissingFilesReturnEmptyOutputsWithoutCreatingFiles()
        {
            Assert.That(CreateV1Store().TryLoad(out SaveV1 data, out bool wasRecovered, out bool wasMigrated), Is.False);
            Assert.That(data, Is.Null);
            Assert.That(wasRecovered, Is.False);
            Assert.That(wasMigrated, Is.False);
            Assert.That(Directory.GetFiles(_directory), Is.Empty);
        }

        [Test]
        public void RejectedSavePreservesPrimaryAndBackup()
        {
            VersionedSaveStore<SaveV1> store = CreateV1Store();
            Assert.That(store.TrySave(new SaveV1(1)), Is.True);
            Assert.That(store.TrySave(new SaveV1(2)), Is.True);
            string primary = File.ReadAllText(_path);
            string backup = File.ReadAllText(_path + ".bak");
            Assert.That(store.TrySave(null), Is.False);
            Assert.That(store.TrySave(new SaveV1(-1)), Is.False);
            Assert.That(File.ReadAllText(_path), Is.EqualTo(primary));
            Assert.That(File.ReadAllText(_path + ".bak"), Is.EqualTo(backup));
            Assert.That(File.Exists(_path + ".tmp"), Is.False);
        }

        [Test]
        public void FailedTemporaryWritePreservesPrimaryAndBackup()
        {
            VersionedSaveStore<SaveV1> store = CreateV1Store();
            Assert.That(store.TrySave(new SaveV1(1)), Is.True);
            Assert.That(store.TrySave(new SaveV1(2)), Is.True);
            string primary = File.ReadAllText(_path);
            string backup = File.ReadAllText(_path + ".bak");
            Directory.CreateDirectory(_path + ".tmp");
            Assert.That(store.TrySave(new SaveV1(3)), Is.False);
            Assert.That(File.ReadAllText(_path), Is.EqualTo(primary));
            Assert.That(File.ReadAllText(_path + ".bak"), Is.EqualTo(backup));
            Assert.That(store.TryLoad(out SaveV1 data, out bool wasRecovered, out _), Is.True);
            Assert.That(data.Level, Is.EqualTo(2));
            Assert.That(wasRecovered, Is.False);
        }

        [Test]
        public void SaveBeforeLoadCannotOverwriteFuturePrimary()
        {
            VersionedSaveStore<SaveV1> firstStore = CreateV1Store();
            Assert.That(firstStore.TrySave(new SaveV1(1)), Is.True);
            Assert.That(firstStore.TrySave(new SaveV1(2)), Is.True);
            Assert.That(CreateV2Store(null).TrySave(new SaveV2(2, 3)), Is.True);
            string primary = File.ReadAllText(_path);
            string backup = File.ReadAllText(_path + ".bak");
            Assert.That(CreateV1Store().TrySave(new SaveV1(9)), Is.False);
            Assert.That(File.ReadAllText(_path), Is.EqualTo(primary));
            Assert.That(File.ReadAllText(_path + ".bak"), Is.EqualTo(backup));
        }

        [Test]
        public void CompatibleLoadClearsFutureVersionProtection()
        {
            Assert.That(CreateV1Store().TrySave(new SaveV1(1)), Is.True);
            string compatible = File.ReadAllText(_path);
            Assert.That(CreateV2Store(null).TrySave(new SaveV2(2, 3)), Is.True);
            VersionedSaveStore<SaveV1> oldStore = CreateV1Store();
            Assert.That(oldStore.TryLoad(out _, out _, out _), Is.False);
            File.WriteAllText(_path, compatible);
            Assert.That(oldStore.TrySave(new SaveV1(3)), Is.False);
            Assert.That(oldStore.TryLoad(out SaveV1 data, out _, out _), Is.True);
            Assert.That(data.Level, Is.EqualTo(1));
            Assert.That(oldStore.TrySave(new SaveV1(3)), Is.True);
        }

        [Test]
        public void StoreOwnsMigrationArraySnapshot()
        {
            Assert.That(CreateV1Store().TrySave(new SaveV1(3)), Is.True);
            ISaveMigration[] migrations = { new V1ToV2Migration() };
            VersionedSaveStore<SaveV2> store = CreateV2Store(migrations);
            migrations[0] = null;
            Assert.That(store.TryLoad(out SaveV2 data, out _, out bool wasMigrated), Is.True);
            Assert.That(data.BranchCnt, Is.EqualTo(4));
            Assert.That(wasMigrated, Is.True);
        }

        [Test]
        public void DevelopmentEnvelopeRemainsCompatibleWithoutMigration()
        {
            string envelope = "{\"_version\":1,\"_payload\":\"{\\\"_level\\\":7}\",\"_hash\":\"BTjsd/+D3D6UqPUkUR9WSEgUP1FH3bonSSn2pDwZsFY=\"}";
            File.WriteAllText(_path, envelope);
            VersionedSaveStore<SaveV1> store = CreateV1Store();
            Assert.That(store.TryLoad(out SaveV1 data, out bool wasRecovered, out bool wasMigrated), Is.True);
            Assert.That(data.Level, Is.EqualTo(7));
            Assert.That(wasRecovered, Is.False);
            Assert.That(wasMigrated, Is.False);
            Assert.That(File.ReadAllText(_path), Is.EqualTo(envelope));
            Assert.That(store.TrySave(data), Is.True);
            Assert.That(File.ReadAllText(_path), Is.EqualTo(envelope));
            Assert.That(File.ReadAllText(_path + ".bak"), Is.EqualTo(envelope));
        }

        [Test]
        public void OverflowingMigrationVersionIsRejected()
        {
            ISaveMigration[] migrations = { new OverflowMigration() };
            Assert.That(VersionedSaveStore<SaveV1>.TryCreate(_path, int.MaxValue, new UnityJsonSaveCodec<SaveV1>(), IsValid, migrations, out VersionedSaveStore<SaveV1> store), Is.False);
            Assert.That(store, Is.Null);
            Assert.That(Directory.GetFiles(_directory), Is.Empty);
        }

        [Test]
        public void DetailedLoadDistinguishesMissingCorruptAndFutureFiles()
        {
            VersionedSaveStore<SaveV1> store = CreateV1Store();
            Assert.That(store.TryLoad(out _, out _, out _, out ESaveFailure failure), Is.False);
            Assert.That(failure, Is.EqualTo(ESaveFailure.FileNotFound));
            File.WriteAllText(_path, "{}");
            Assert.That(store.TryLoad(out SaveV1 data, out bool wasRecovered, out bool wasMigrated, out failure), Is.False);
            Assert.That(failure, Is.EqualTo(ESaveFailure.CorruptData));
            Assert.That(data, Is.Null);
            Assert.That(wasRecovered || wasMigrated, Is.False);
            File.WriteAllText(_path, "{\"_version\":2}");
            string primary = File.ReadAllText(_path);
            Assert.That(store.TryLoad(out _, out _, out _, out failure), Is.False);
            Assert.That(failure, Is.EqualTo(ESaveFailure.FutureVersion));
            Assert.That(store.TrySave(new SaveV1(2), out failure), Is.False);
            Assert.That(failure, Is.EqualTo(ESaveFailure.FutureVersion));
            Assert.That(File.ReadAllText(_path), Is.EqualTo(primary));
        }

        [Test]
        public void DetailedRecoveryReportsSuccessAndPreservesBackup()
        {
            VersionedSaveStore<SaveV1> store = CreateV1Store();
            Assert.That(store.TrySave(new SaveV1(1), out ESaveFailure failure), Is.True);
            Assert.That(failure, Is.EqualTo(ESaveFailure.None));
            Assert.That(store.TrySave(new SaveV1(2)), Is.True);
            string backup = File.ReadAllText(_path + ".bak");
            File.WriteAllText(_path, "{}");
            Assert.That(store.TryLoad(out SaveV1 data, out bool wasRecovered, out bool wasMigrated, out failure), Is.True);
            Assert.That(failure, Is.EqualTo(ESaveFailure.None));
            Assert.That(data.Level, Is.EqualTo(1));
            Assert.That(wasRecovered, Is.True);
            Assert.That(wasMigrated, Is.False);
            Assert.That(store.TrySave(data, out failure), Is.True);
            Assert.That(failure, Is.EqualTo(ESaveFailure.None));
            Assert.That(File.ReadAllText(_path + ".bak"), Is.EqualTo(backup));
        }

        [Test]
        public void DetailedLoadReportsBackupFailureWhenPrimaryIsMissing()
        {
            File.WriteAllText(_path + ".bak", "{}");
            Assert.That(CreateV1Store().TryLoad(out _, out _, out _, out ESaveFailure failure), Is.False);
            Assert.That(failure, Is.EqualTo(ESaveFailure.CorruptData));
        }

        [Test]
        public void DetailedMigrationDistinguishesMissingAndRejectedSteps()
        {
            Assert.That(CreateV1Store().TrySave(new SaveV1(3)), Is.True);
            string primary = File.ReadAllText(_path);
            Assert.That(CreateV2Store(null).TryLoad(out _, out _, out _, out ESaveFailure failure), Is.False);
            Assert.That(failure, Is.EqualTo(ESaveFailure.MigrationMissing));
            Assert.That(CreateV2Store(new ISaveMigration[] { new RejectedMigration() }).TryLoad(out _, out _, out _, out failure), Is.False);
            Assert.That(failure, Is.EqualTo(ESaveFailure.MigrationFailed));
            Assert.That(CreateV2Store(new ISaveMigration[] { new V1ToV2Migration() }).TryLoad(out _, out _, out bool wasMigrated, out failure), Is.True);
            Assert.That(failure, Is.EqualTo(ESaveFailure.None));
            Assert.That(wasMigrated, Is.True);
            Assert.That(File.ReadAllText(_path), Is.EqualTo(primary));
        }

        [Test]
        public void DetailedSaveReportsInvalidDataAndWriteFailureWithoutChangingFiles()
        {
            VersionedSaveStore<SaveV1> store = CreateV1Store();
            Assert.That(store.TrySave(null, out ESaveFailure failure), Is.False);
            Assert.That(failure, Is.EqualTo(ESaveFailure.InvalidArgument));
            Assert.That(store.TrySave(new SaveV1(-1), out failure), Is.False);
            Assert.That(failure, Is.EqualTo(ESaveFailure.ValidationFailed));
            Assert.That(store.TrySave(new SaveV1(1)), Is.True);
            Assert.That(store.TrySave(new SaveV1(2)), Is.True);
            string primary = File.ReadAllText(_path);
            string backup = File.ReadAllText(_path + ".bak");
            Directory.CreateDirectory(_path + ".tmp");
            Assert.That(store.TrySave(new SaveV1(3), out failure), Is.False);
            Assert.That(failure, Is.EqualTo(ESaveFailure.AccessDenied).Or.EqualTo(ESaveFailure.IoError));
            Assert.That(File.ReadAllText(_path), Is.EqualTo(primary));
            Assert.That(File.ReadAllText(_path + ".bak"), Is.EqualTo(backup));
        }

        [Test]
        public void DetailedCodecAndValidationFailuresAreDistinct()
        {
            Assert.That(CreateV1Store().TrySave(new SaveV1(1)), Is.True);
            Assert.That(VersionedSaveStore<SaveV1>.TryCreate(_path, 1, new RejectedCodec(), IsValid, null, out VersionedSaveStore<SaveV1> store), Is.True);
            Assert.That(store.TrySave(new SaveV1(1), out ESaveFailure failure), Is.False);
            Assert.That(failure, Is.EqualTo(ESaveFailure.SerializationFailed));
            Assert.That(store.TryLoad(out _, out _, out _, out failure), Is.False);
            Assert.That(failure, Is.EqualTo(ESaveFailure.DeserializationFailed));
            Assert.That(VersionedSaveStore<SaveV1>.TryCreate(_path, 1, new UnityJsonSaveCodec<SaveV1>(), data => data.Level > 1, null, out store), Is.True);
            Assert.That(store.TryLoad(out _, out _, out _, out failure), Is.False);
            Assert.That(failure, Is.EqualTo(ESaveFailure.ValidationFailed));
        }

        [Test]
        public void DetailedFileReadDistinguishesMissingFromLockedFiles()
        {
            SaveFileStore files = new SaveFileStore(_path);
            Assert.That(files.TryRead(false, out _, out ESaveFailure failure), Is.False);
            Assert.That(failure, Is.EqualTo(ESaveFailure.FileNotFound));
            File.WriteAllText(_path, "{}");
            using (FileStream stream = new FileStream(_path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                Assert.That(files.TryRead(false, out string content, out failure), Is.False);
                Assert.That(content, Is.Null);
                Assert.That(failure, Is.EqualTo(ESaveFailure.IoError));
            }
        }

        //============================================================
        // Utilities
        //============================================================
        private VersionedSaveStore<SaveV1> CreateV1Store()
        {
            Assert.That(VersionedSaveStore<SaveV1>.TryCreate(_path, 1, new UnityJsonSaveCodec<SaveV1>(), IsValid,
                null, out VersionedSaveStore<SaveV1> store), Is.True);
            return store;
        }

        private VersionedSaveStore<SaveV2> CreateV2Store(ISaveMigration[] migrations)
        {
            Assert.That(VersionedSaveStore<SaveV2>.TryCreate(_path, 2, new UnityJsonSaveCodec<SaveV2>(), IsValid,
                migrations, out VersionedSaveStore<SaveV2> store), Is.True);
            return store;
        }

        private static bool IsValid(SaveV1 data) => data.Level >= 0;
        private static bool IsValid(SaveV2 data) => data.Level >= 0 && data.BranchCnt >= 0;

        //============================================================
        // Nested Types
        //============================================================
        [Serializable]
        private class SaveV1
        {
            [SerializeField] private int _level;
            public int Level => _level;

            public SaveV1(int level)
            {
                _level = level;
            }
        }

        [Serializable]
        private class SaveV2
        {
            [SerializeField] private int _level;
            [SerializeField] private int _branchCnt;
            public int Level => _level;
            public int BranchCnt => _branchCnt;

            public SaveV2(int level, int branchCnt)
            {
                _level = level;
                _branchCnt = branchCnt;
            }
        }

        private class V1ToV2Migration : ISaveMigration
        {
            public int FromVersion => 1;
            public int ToVersion => 2;

            public bool TryMigrate(string payload, out string migratedPayload)
            {
                migratedPayload = null;
                SaveV1 oldData = JsonUtility.FromJson<SaveV1>(payload);
                if (oldData == null)
                    return false;

                migratedPayload = JsonUtility.ToJson(new SaveV2(oldData.Level, oldData.Level + 1));
                return true;
            }
        }

        private class RejectedCodec : ISaveCodec<SaveV1>
        {
            public bool TrySerialize(SaveV1 data, out string payload)
            {
                payload = null;
                return false;
            }

            public bool TryDeserialize(string payload, out SaveV1 data)
            {
                data = null;
                return false;
            }
        }

        private class RejectedMigration : ISaveMigration
        {
            public int FromVersion => 1;
            public int ToVersion => 2;

            public bool TryMigrate(string payload, out string migratedPayload)
            {
                migratedPayload = null;
                return false;
            }
        }

        private class OverflowMigration : ISaveMigration
        {
            public int FromVersion => int.MaxValue;
            public int ToVersion => int.MinValue;

            public bool TryMigrate(string payload, out string migratedPayload)
            {
                migratedPayload = null;
                return false;
            }
        }
    }
}
