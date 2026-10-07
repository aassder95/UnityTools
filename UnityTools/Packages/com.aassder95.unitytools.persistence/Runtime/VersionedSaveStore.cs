using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace UnityTools.Persistence
{
    public class VersionedSaveStore<T> where T : class
    {
        //============================================================
        // Fields
        //============================================================
        private readonly int _curVersion;
        private readonly ISaveCodec<T> _codec;
        private readonly Func<T, bool> _validate;
        private readonly ISaveMigration[] _migrations;
        private readonly SaveFileStore _files;
        private bool _shouldPreserveBackup;
        private bool _isFutureVersion;

        //============================================================
        // Constructors
        //============================================================
        private VersionedSaveStore(string path, int curVersion, ISaveCodec<T> codec, Func<T, bool> validate, ISaveMigration[] migrations)
        {
            _curVersion = curVersion;
            _codec = codec;
            _validate = validate;
            _migrations = migrations;
            _files = new SaveFileStore(path);
        }

        //============================================================
        // Logic
        //============================================================
        public static bool TryCreate(string path, int curVersion, ISaveCodec<T> codec, Func<T, bool> validate,
            ISaveMigration[] migrations, out VersionedSaveStore<T> store)
        {
            store = null;
            if (string.IsNullOrWhiteSpace(path) || curVersion < 1 || codec == null || validate == null)
                return false;

            try
            {
                path = Path.GetFullPath(path);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
            catch (PathTooLongException)
            {
                return false;
            }

            ISaveMigration[] migrationItems = migrations ?? Array.Empty<ISaveMigration>();
            for (int idx = 0; idx < migrationItems.Length; ++idx)
            {
                ISaveMigration migration = migrationItems[idx];
                if (migration == null || migration.FromVersion < 1 || migration.FromVersion >= curVersion || migration.ToVersion != migration.FromVersion + 1 || migration.ToVersion > curVersion)
                    return false;

                for (int prevIdx = 0; prevIdx < idx; ++prevIdx)
                {
                    if (migrationItems[prevIdx].FromVersion == migration.FromVersion)
                        return false;
                }
            }

            store = new VersionedSaveStore<T>(path, curVersion, codec, validate, (ISaveMigration[])migrationItems.Clone());
            return true;
        }

        public bool TrySave(T data)
        {
            return TrySave(data, out _);
        }

        public bool TrySave(T data, out ESaveFailure failure)
        {
            failure = ESaveFailure.None;
            if (_isFutureVersion)
            {
                failure = ESaveFailure.FutureVersion;
                return false;
            }

            if (data == null)
            {
                failure = ESaveFailure.InvalidArgument;
                return false;
            }

            if (!_validate(data))
            {
                failure = ESaveFailure.ValidationFailed;
                return false;
            }

            if (!_codec.TrySerialize(data, out string payload) || string.IsNullOrEmpty(payload))
            {
                failure = ESaveFailure.SerializationFailed;
                return false;
            }

            if (_files.TryRead(false, out string primary) && !TryDecode(primary, out _, out _, out ESaveFailure decodeFailure))
            {
                if (decodeFailure == ESaveFailure.FutureVersion)
                {
                    _isFutureVersion = true;
                    failure = decodeFailure;
                    return false;
                }

                _shouldPreserveBackup = true;
            }

            SaveEnvelope envelope = new SaveEnvelope(_curVersion, payload, CalculateHash(_curVersion, payload));
            if (!_files.TryWrite(JsonUtility.ToJson(envelope), _shouldPreserveBackup, out failure))
                return false;

            _shouldPreserveBackup = false;
            return true;
        }

        public bool TryLoad(out T data, out bool wasRecovered, out bool wasMigrated)
        {
            return TryLoad(out data, out wasRecovered, out wasMigrated, out _);
        }

        public bool TryLoad(out T data, out bool wasRecovered, out bool wasMigrated, out ESaveFailure failure)
        {
            data = null;
            wasRecovered = false;
            wasMigrated = false;
            failure = ESaveFailure.None;
            if (_files.TryRead(false, out string primary, out ESaveFailure primaryFailure) && TryDecode(primary, out data, out wasMigrated, out primaryFailure))
            {
                _shouldPreserveBackup = false;
                _isFutureVersion = false;
                return true;
            }

            if (primaryFailure == ESaveFailure.FutureVersion)
            {
                _isFutureVersion = true;
                failure = primaryFailure;
                return false;
            }

            if (!_files.TryRead(true, out string backup, out ESaveFailure backupFailure) || !TryDecode(backup, out data, out wasMigrated, out backupFailure))
            {
                failure = primaryFailure == ESaveFailure.FileNotFound ? backupFailure : primaryFailure;
                return false;
            }

            wasRecovered = true;
            _shouldPreserveBackup = true;
            _isFutureVersion = false;
            return true;
        }

        //============================================================
        // Utilities
        //============================================================
        private bool TryDecode(string content, out T data, out bool wasMigrated, out ESaveFailure failure)
        {
            data = null;
            wasMigrated = false;
            failure = ESaveFailure.None;
            SaveEnvelope envelope;
            try
            {
                envelope = JsonUtility.FromJson<SaveEnvelope>(content);
            }
            catch (ArgumentException)
            {
                failure = ESaveFailure.CorruptData;
                return false;
            }

            if (envelope != null && envelope.Version > _curVersion)
            {
                failure = ESaveFailure.FutureVersion;
                return false;
            }

            if (envelope == null || envelope.Version < 1 || string.IsNullOrEmpty(envelope.Payload) || envelope.Hash != CalculateHash(envelope.Version, envelope.Payload))
            {
                failure = ESaveFailure.CorruptData;
                return false;
            }

            int version = envelope.Version;
            string payload = envelope.Payload;
            while (version < _curVersion)
            {
                ISaveMigration migration = null;
                for (int idx = 0; idx < _migrations.Length; ++idx)
                {
                    if (_migrations[idx].FromVersion == version)
                    {
                        migration = _migrations[idx];
                        break;
                    }
                }

                if (migration == null)
                {
                    failure = ESaveFailure.MigrationMissing;
                    return false;
                }

                if (!migration.TryMigrate(payload, out string migratedPayload) || string.IsNullOrEmpty(migratedPayload))
                {
                    failure = ESaveFailure.MigrationFailed;
                    return false;
                }

                payload = migratedPayload;
                version = migration.ToVersion;
            }

            if (!_codec.TryDeserialize(payload, out T loadedData) || loadedData == null)
            {
                failure = ESaveFailure.DeserializationFailed;
                return false;
            }

            if (!_validate(loadedData))
            {
                failure = ESaveFailure.ValidationFailed;
                return false;
            }

            data = loadedData;
            wasMigrated = envelope.Version != _curVersion;
            return true;
        }

        private static string CalculateHash(int version, string payload)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(version.ToString(CultureInfo.InvariantCulture) + "\n" + payload);
            using (SHA256 hash = SHA256.Create())
            {
                return Convert.ToBase64String(hash.ComputeHash(bytes));
            }
        }

        //============================================================
        // Nested Types
        //============================================================
        [Serializable]
        private class SaveEnvelope
        {
            [SerializeField] private int _version;
            [SerializeField] private string _payload;
            [SerializeField] private string _hash;

            public int Version => _version;
            public string Payload => _payload;
            public string Hash => _hash;

            public SaveEnvelope(int version, string payload, string hash)
            {
                _version = version;
                _payload = payload;
                _hash = hash;
            }
        }
    }
}
