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
}
