using UnityEngine;

namespace UnityTools.Util
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindFirstObjectByType<T>();

                if (_instance == null)
                    Debug.LogError($"[Singleton:Instance] {typeof(T).Name} is not found");

                return _instance;
            }
        }
    }
}
