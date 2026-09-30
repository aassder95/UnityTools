using System;
using System.IO;
using UnityEngine;

namespace UnityTools.Persistence.Samples
{
    public class SaveLabExperiment
    {
        //============================================================
        // Logic
        //============================================================
        public bool TryRun(ESaveLabScenario scenario, out SaveLabReport report)
        {
            report = null;
            if (scenario < ESaveLabScenario.Migration || scenario > ESaveLabScenario.RejectedMigration)
                return false;

            string directory = Path.Combine(Application.temporaryCachePath, "UnityTools-SaveLab", Guid.NewGuid().ToString("N"));
            string path = Path.Combine(directory, "fixture.json");
            try
            {
                Directory.CreateDirectory(directory);
                if (!VersionedSaveStore<SaveLabV1>.TryCreate(path, 1, new UnityJsonSaveCodec<SaveLabV1>(), data => data.Level >= 0, null, out VersionedSaveStore<SaveLabV1> v1))
                    return false;

                ISaveMigration[] migrations = { new LabMigration(scenario == ESaveLabScenario.RejectedMigration) };
                if (!VersionedSaveStore<SaveLabV2>.TryCreate(path, 2, new UnityJsonSaveCodec<SaveLabV2>(), data => data.Level >= 0 && data.BranchCnt >= 0, migrations, out VersionedSaveStore<SaveLabV2> v2))
                    return false;

                if (scenario != ESaveLabScenario.MissingFile && !v1.TrySave(new SaveLabV1(3)))
                    return false;

                if (scenario == ESaveLabScenario.BackupRecovery)
                {
                    if (!v1.TrySave(new SaveLabV1(9)))
                        return false;

                    File.WriteAllText(path, "{}");
                }
                else if (scenario == ESaveLabScenario.CorruptFile)
                {
                    File.WriteAllText(path, "{}");
                }
                else if (scenario == ESaveLabScenario.FutureVersion && !v2.TrySave(new SaveLabV2(12, 4)))
                {
                    return false;
                }

                string primary = ReadSnapshot(path);
                string backup = ReadSnapshot(path + ".bak");
                string before = FormatFiles(primary, backup);
                bool isLoaded;
                bool wasRecovered;
                bool wasMigrated;
                bool isPassed;
                string result;
                if (scenario == ESaveLabScenario.Migration)
                {
                    isLoaded = v2.TryLoad(out SaveLabV2 data, out wasRecovered, out wasMigrated);
                    bool isSaved = isLoaded && v2.TrySave(data);
                    bool isReloaded = v2.TryLoad(out SaveLabV2 persisted, out bool recoveredAgain, out bool migratedAgain);
                    isPassed = isLoaded && wasMigrated && !wasRecovered && data.Level == 3 && data.BranchCnt == 4 && isSaved && isReloaded && !recoveredAgain && !migratedAgain && persisted.Level == 3 && persisted.BranchCnt == 4;
                    result = $"V1 -> V2 migration\nLoad={isLoaded}, Recovered={wasRecovered}, Migrated={wasMigrated}\nData: level={data?.Level}, branchCnt={data?.BranchCnt}\nSave V2={isSaved}, reload={isReloaded}, migrated again={migratedAgain}";
                }
                else if (scenario == ESaveLabScenario.BackupRecovery)
                {
                    isLoaded = v1.TryLoad(out SaveLabV1 data, out wasRecovered, out wasMigrated);
                    bool isSaved = isLoaded && v1.TrySave(data);
                    bool isReloaded = v1.TryLoad(out SaveLabV1 persisted, out bool recoveredAgain, out bool migratedAgain);
                    bool isBackupPreserved = backup == ReadSnapshot(path + ".bak");
                    isPassed = isLoaded && wasRecovered && !wasMigrated && data.Level == 3 && isSaved && isReloaded && !recoveredAgain && !migratedAgain && persisted.Level == 3 && isBackupPreserved;
                    result = $"Corrupt primary -> valid backup -> repaired primary\nLoad={isLoaded}, Recovered={wasRecovered}, Migrated={wasMigrated}\nData: level={data?.Level}\nRepair save={isSaved}, reload={isReloaded}, backup unchanged={isBackupPreserved}";
                }
                else if (scenario == ESaveLabScenario.FutureVersion)
                {
                    isLoaded = v1.TryLoad(out SaveLabV1 data, out wasRecovered, out wasMigrated);
                    bool isSaved = v1.TrySave(new SaveLabV1(1));
                    bool isPrimaryPreserved = primary == ReadSnapshot(path);
                    bool isBackupPreserved = backup == ReadSnapshot(path + ".bak");
                    isPassed = !isLoaded && data == null && !wasRecovered && !wasMigrated && !isSaved && isPrimaryPreserved && isBackupPreserved;
                    result = $"V1 reader sees V2 primary + V1 backup\nLoad={isLoaded}, Recovered={wasRecovered}, Migrated={wasMigrated}\nOld-version save={isSaved}\nPrimary unchanged={isPrimaryPreserved}, backup unchanged={isBackupPreserved}";
                }
                else
                {
                    isLoaded = v2.TryLoad(out SaveLabV2 data, out wasRecovered, out wasMigrated);
                    bool areFilesPreserved = primary == ReadSnapshot(path) && backup == ReadSnapshot(path + ".bak");
                    isPassed = !isLoaded && data == null && !wasRecovered && !wasMigrated && areFilesPreserved;
                    string condition = scenario == ESaveLabScenario.MissingFile ? "No primary or backup exists" : scenario == ESaveLabScenario.CorruptFile ? "Invalid envelope, no backup" : "Valid V1 envelope, migration deliberately returns false";
                    result = $"Fixture: {condition}\nLoad={isLoaded}, Recovered={wasRecovered}, Migrated={wasMigrated}\nData is null={data == null}, files unchanged={areFilesPreserved}\nFailure cause is known from fixture setup; API returns false.";
                }

                report = new SaveLabReport(isPassed, before, FormatFiles(ReadSnapshot(path), ReadSnapshot(path + ".bak")), result);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
            finally
            {
                try
                {
                    if (File.Exists(path))
                        File.Delete(path);

                    if (File.Exists(path + ".bak"))
                        File.Delete(path + ".bak");

                    if (File.Exists(path + ".tmp"))
                        File.Delete(path + ".tmp");

                    if (Directory.Exists(directory))
                        Directory.Delete(directory, false);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }

        //============================================================
        // Utilities
        //============================================================
        private static string ReadSnapshot(string path)
        {
            return File.Exists(path) ? File.ReadAllText(path) : "<missing>";
        }

        private static string FormatFiles(string primary, string backup)
        {
            return "PRIMARY\n" + primary + "\n\nBACKUP\n" + backup;
        }

        //============================================================
        // Nested Types
        //============================================================
        private class LabMigration : ISaveMigration
        {
            private readonly bool _shouldReject;
            public int FromVersion => 1;
            public int ToVersion => 2;

            public LabMigration(bool shouldReject)
            {
                _shouldReject = shouldReject;
            }

            public bool TryMigrate(string payload, out string migratedPayload)
            {
                migratedPayload = null;
                if (_shouldReject || !new UnityJsonSaveCodec<SaveLabV1>().TryDeserialize(payload, out SaveLabV1 data))
                    return false;

                return new UnityJsonSaveCodec<SaveLabV2>().TrySerialize(new SaveLabV2(data.Level, 4), out migratedPayload);
            }
        }
    }
}
