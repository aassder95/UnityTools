using UnityEngine;

namespace UnityTools.Util.Core.Timer
{
    public interface ITimerHandleFactory<THandle> where THandle : ITimerHandle
    {
        //============================================================
        // Logic
        //============================================================
        bool TryCreate(string normalizedId, MonoBehaviour runner, out THandle handle);
    }
}
