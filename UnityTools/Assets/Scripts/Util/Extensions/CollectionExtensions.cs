using System.Collections.Generic;
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

namespace UnityTools.Util.Extensions
{
    //============================================================
    //Logic
    //============================================================
    public static class CollectionExtensions
    {
        //============================================================
        //Logic
        //============================================================
        public static bool IsValidIndex<T>(this List<T> list, int index)
        {
            if(list == null)
            {
                DebugLogger.LogWarning("리스트가 비어 있습니다.");
                return false;
            }

            if(0 > index || index >= list.Count)
            {
                DebugLogger.LogWarning("인덱스가 범위를 벗어났습니다.");
                return false;
            }

            return true;
        }
    }
}
