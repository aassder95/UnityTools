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

namespace UnityTools.Util.Core.Timer.Shared
{
    public class TimerHandleRegistry<THandle> where THandle : ITimerHandle
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly Dictionary<string, THandle> _handles = new();

        //============================================================
        //Logic
        //============================================================
        public bool TryGet(string id, out THandle handle)
        {
            return _handles.TryGetValue(id, out handle);
        }

        public THandle GetOrDefault(string id)
        {
            return _handles.GetValueOrDefault(id);
        }

        public THandle SetOrReplace(string id, THandle handle)
        {
            THandle oldHandle = _handles.GetValueOrDefault(id);
            _handles[id] = handle;
            return oldHandle;
        }

        public bool Remove(string id, out THandle removedHandle)
        {
            removedHandle = _handles.GetValueOrDefault(id);
            if(removedHandle == null)
                return false;

            return _handles.Remove(id);
        }

        public void ClearAll()
        {
            foreach (THandle handle in _handles.Values)
            {
                handle?.Release();
            }

            _handles.Clear();
        }
    }
}
