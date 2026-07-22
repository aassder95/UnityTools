
namespace UnityTools.Util.Core.Persistence
{
    public interface IStorage
    {
        //============================================================
        // Persistence
        //============================================================
        void Save(string key, string data);
        string Load(string key);
        bool HasKey(string key);
        void Delete(string key);
    }
}
