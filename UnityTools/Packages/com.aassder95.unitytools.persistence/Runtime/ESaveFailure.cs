namespace UnityTools.Persistence
{
    public enum ESaveFailure
    {
        None,
        InvalidArgument,
        FileNotFound,
        IoError,
        AccessDenied,
        InvalidPath,
        UnsupportedOperation,
        CorruptData,
        FutureVersion,
        MigrationMissing,
        MigrationFailed,
        SerializationFailed,
        DeserializationFailed,
        ValidationFailed
    }
}
