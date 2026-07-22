using UnityEngine;

namespace UnityTools.Util.Core.Timer
{
    public interface ITimerHandleFactory<THandle> where THandle : ITimerHandle
    {
        //============================================================
        // Logic
        //============================================================
        THandle Create(string normalizedId, MonoBehaviour runner);
    }
}
