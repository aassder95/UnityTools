namespace UnityTools.Util
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
