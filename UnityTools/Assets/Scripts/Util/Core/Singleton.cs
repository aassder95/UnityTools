using UnityEngine;

namespace UnityTools.Util
{
    public class Singleton<T> where T : Singleton<T>, new()
    {
        //============================================================
        //Fields
        //============================================================
        private static T _instance;

        //============================================================
        //Properties
        //============================================================
        public static T Instance => _instance ??= new T();

        //============================================================
        //Constructors
        //============================================================
        protected Singleton()
        {
        }
    }

    public class MonoSingleton<T> : MonoBehaviour where T : Component
    {
        //============================================================
        //Fields
        //============================================================
        private static T _instance;

        //============================================================
        //Properties
        //============================================================
        public static T Instance
        {
            get
            {
                if(_instance == null)
                    _instance = FindFirstObjectByType<T>();

                if(_instance == null)
                    Debug.LogError($"[MonoSingleton:Instance] {typeof(T).Name} is not found");

                return _instance;
            }
        }
    }
}


