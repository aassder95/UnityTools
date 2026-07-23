using System.Collections.Generic;

namespace UnityTools.Util.Core.Timer
{
    public class TimerHandleRegistry<THandle> where THandle : ITimerHandle
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly Dictionary<string, THandle> _handles = new();

        //============================================================
        // Logic
        //============================================================
        public bool TryGet(string id, out THandle handle)
        {
            return _handles.TryGetValue(id, out handle);
        }

        public THandle SetOrReplace(string id, THandle handle)
        {
            THandle oldHandle = _handles.GetValueOrDefault(id);
            _handles[id] = handle;
            return oldHandle;
        }

        public bool TryRemove(string id, out THandle removedHandle)
        {
            return _handles.Remove(id, out removedHandle);
        }

        public void Clear()
        {
            foreach(THandle handle in _handles.Values)
            {
                handle.Release();
            }

            _handles.Clear();
        }
    }
}
