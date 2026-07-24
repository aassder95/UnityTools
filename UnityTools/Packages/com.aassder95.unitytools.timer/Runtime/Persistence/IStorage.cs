namespace UnityTools.Timer.Persistence
{
    public interface IStorage
    {
        //============================================================
        // Persistence
        //============================================================
        bool TrySave(string key, string data);
        bool TryLoad(string key, out string data);
        bool TryHasKey(string key, out bool hasKey);
        bool TryDelete(string key);
    }
}
