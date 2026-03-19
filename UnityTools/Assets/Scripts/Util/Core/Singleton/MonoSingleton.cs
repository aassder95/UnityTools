using UnityEngine;
using UnityTools.Util.Constants;
using UnityTools.Util.Core.Collections;
using UnityTools.Util.Core.Events;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.Core.Persistence;
using UnityTools.Util.Core.Pooling;
using UnityTools.Util.Core.Singleton;
using UnityTools.Util.Core.State;
using UnityTools.Util.Core.Timer.Period;
using UnityTools.Util.Core.Timer.Shared;
using UnityTools.Util.Core.Timer.Task;
using UnityTools.Util.Coroutines;
using UnityTools.Util.Extensions;
using UnityTools.Util.UIFramework;
using UnityTools.Util.Utilities;

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
                    DebugLogger.LogError($"{typeof(T).Name} 인스턴스를 찾을 수 없습니다.");

                return _instance;
            }
        }
    }
}
