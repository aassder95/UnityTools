using UnityEngine;

namespace UnityTools.Util
{
    public class Singleton<T> where T : Singleton<T>, new()
    {
        private static T _instance;

        public static T Instance => _instance ??= new T();

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
                    Debug.LogError($"[MonoSingleton:Instance] {typeof(T).Name} 인스턴스를 찾을 수 없습니다.");

                return _instance;
            }
        }
    }
}


