using UnityEngine;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Util.Core.Singleton
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : Component
    {
        //============================================================
        // Fields
        //============================================================
        private static T _instance;
        private static bool _hasMissingLog;

        //============================================================
        // Properties
        //============================================================
        public static T Instance
        {
            get
            {
                if(_instance != null)
                {
                    _hasMissingLog = false;
                    return _instance;
                }

                if(!_hasMissingLog)
                {
                    DebugLogger.LogError(typeof(T).Name + " 인스턴스가 씬에 배치되어 있지 않습니다.");
                    _hasMissingLog = true;
                }

                return null;
            }
        }

        //============================================================
        // Unity Methods
        //============================================================
        protected virtual void Awake()
        {
            if(this is not T instance)
            {
                DebugLogger.LogError(typeof(T).Name + " MonoSingleton 타입 선언이 실제 Component 타입과 일치하지 않습니다.", this);
                enabled = false;
                return;
            }

            if(_instance == null)
            {
                _instance = instance;
                _hasMissingLog = false;
                return;
            }

            if(_instance == instance)
                return;

            DebugLogger.LogError(typeof(T).Name + " MonoSingleton 인스턴스가 중복 배치되어 있습니다.", this);
            enabled = false;
        }

        protected virtual void OnDestroy()
        {
            if(_instance == this)
            {
                _instance = null;
                _hasMissingLog = false;
            }
        }
    }
}
