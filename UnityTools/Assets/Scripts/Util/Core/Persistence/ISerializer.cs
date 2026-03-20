
namespace UnityTools.Util.Core.Persistence
{
    public interface ISerializer
    {
        //============================================================
        //Logic
        //============================================================
        string Serialize<T>(T data);
        T Deserialize<T>(string data);
    }
}
