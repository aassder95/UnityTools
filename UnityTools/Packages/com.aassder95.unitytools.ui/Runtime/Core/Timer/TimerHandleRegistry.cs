using System.Collections.Generic;

namespace UnityTools.Util.Core.Timer
{
    public class TimerHandleRegistry<THandle> where THandle : class, ITimerHandle
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
            handle = null;
            if(string.IsNullOrWhiteSpace(id))
                return false;

            return _handles.TryGetValue(id, out handle);
        }

        public bool TrySetOrReplace(string id, THandle handle, out THandle oldHandle)
        {
            oldHandle = null;
            if(string.IsNullOrWhiteSpace(id) || handle == null)
                return false;

            _handles.TryGetValue(id, out oldHandle);
            _handles[id] = handle;
            return true;
        }

        public bool TryRemove(string id, out THandle removedHandle)
        {
            removedHandle = null;
            if(string.IsNullOrWhiteSpace(id))
                return false;

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
