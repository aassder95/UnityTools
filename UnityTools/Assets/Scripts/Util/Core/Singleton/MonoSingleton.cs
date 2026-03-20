using UnityEngine;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Util.Core.Singleton
{
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
                    DebugLogger.LogError($"{typeof(T).Name} ?몄뒪?댁뒪瑜?李얠쓣 ???놁뒿?덈떎.");

                return _instance;
            }
        }
    }
}
