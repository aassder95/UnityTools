using UnityEngine;

namespace UnityTools.Util.Core.Persistence
{
    public class JsonSerializer : ISerializer
    {
        //============================================================
        //Logic
        //============================================================
        public string Serialize<T>(T data)
        {
            return JsonUtility.ToJson(data);
        }

        public T Deserialize<T>(string data)
        {
            return JsonUtility.FromJson<T>(data);
        }
    }
}
