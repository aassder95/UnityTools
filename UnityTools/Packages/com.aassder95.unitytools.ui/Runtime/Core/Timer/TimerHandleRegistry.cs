using System.Collections.Generic;
using UnityTools.Util.Core.Logging;

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
            handle = default;
            return !string.IsNullOrWhiteSpace(id) && _handles.TryGetValue(id, out handle);
        }

        public bool TrySetOrReplace(string id, THandle handle, out THandle oldHandle)
        {
            oldHandle = default;
            if(string.IsNullOrWhiteSpace(id) || handle == null)
            {
                DebugLogger.LogError("Timer Handle 등록 인자가 유효하지 않습니다.");
                return false;
            }

            if(_handles.TryGetValue(id, out THandle registeredHandle))
                oldHandle = registeredHandle;
            _handles[id] = handle;
            return true;
        }

        public bool TryRemove(string id, out THandle removedHandle)
        {
            removedHandle = default;
            if(string.IsNullOrWhiteSpace(id))
            {
                DebugLogger.LogError("제거할 Timer Handle ID가 비어 있습니다.");
                return false;
            }

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
