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
