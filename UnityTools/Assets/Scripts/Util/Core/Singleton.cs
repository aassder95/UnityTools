using UnityEngine;

namespace UnityTools.Util
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindFirstObjectByType<T>();

                if (_instance == null)
                    Debug.LogWarning($"[Singleton:Instance] {typeof(T).Name} is null");

                return _instance;
            }
        }
    }
}
