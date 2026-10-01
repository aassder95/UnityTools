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
            if (_isFutureVersion || data == null || !_validate(data) || !_codec.TrySerialize(data, out string payload)
                || string.IsNullOrEmpty(payload))
                return false;

            if (_files.TryRead(false, out string primary) && !TryDecode(primary, out _, out _, out bool isFutureVersion))
            {
                if (isFutureVersion)
                {
                    _isFutureVersion = true;
                    return false;
                }

                _shouldPreserveBackup = true;
            }

            SaveEnvelope envelope = new SaveEnvelope(_curVersion, payload, CalculateHash(_curVersion, payload));
            if (!_files.TryWrite(JsonUtility.ToJson(envelope), _shouldPreserveBackup))
                return false;

            _shouldPreserveBackup = false;
            return true;
        }

        public bool TryLoad(out T data, out bool wasRecovered, out bool wasMigrated)
        {
            data = null;
            wasRecovered = false;
            wasMigrated = false;
            bool isFutureVersion = false;
            if (_files.TryRead(false, out string primary) && TryDecode(primary, out data, out wasMigrated, out isFutureVersion))
            {
                _shouldPreserveBackup = false;
                _isFutureVersion = false;
                return true;
            }

            if (isFutureVersion)
            {
                _isFutureVersion = true;
                return false;
            }

            if (!_files.TryRead(true, out string backup) || !TryDecode(backup, out data, out wasMigrated, out _))
                return false;

            wasRecovered = true;
            _shouldPreserveBackup = true;
            _isFutureVersion = false;
            return true;
        }

        //============================================================
        // Utilities
        //============================================================
        private bool TryDecode(string content, out T data, out bool wasMigrated, out bool isFutureVersion)
        {
            data = null;
            wasMigrated = false;
            isFutureVersion = false;
            SaveEnvelope envelope;
            try
            {
                envelope = JsonUtility.FromJson<SaveEnvelope>(content);
            }
            catch (ArgumentException)
            {
                return false;
            }

            if (envelope != null && envelope.Version > _curVersion)
            {
                isFutureVersion = true;
                return false;
            }

            if (envelope == null || envelope.Version < 1
                || string.IsNullOrEmpty(envelope.Payload) || envelope.Hash != CalculateHash(envelope.Version, envelope.Payload))
                return false;

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

                if (migration == null || !migration.TryMigrate(payload, out string migratedPayload)
                    || string.IsNullOrEmpty(migratedPayload))
                    return false;

                payload = migratedPayload;
                version = migration.ToVersion;
            }

            if (!_codec.TryDeserialize(payload, out T loadedData) || loadedData == null || !_validate(loadedData))
                return false;

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
