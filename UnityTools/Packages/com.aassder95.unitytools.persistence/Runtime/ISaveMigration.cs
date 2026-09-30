namespace UnityTools.Persistence
{
    public interface ISaveMigration
    {
        int FromVersion { get; }
        int ToVersion { get; }
        bool TryMigrate(string payload, out string migratedPayload);
    }
}
