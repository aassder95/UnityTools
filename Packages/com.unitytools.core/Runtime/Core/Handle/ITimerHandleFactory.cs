using UnityEngine;

namespace UnityTools.Util
{
    public interface ITimerHandleFactory<THandle> where THandle : ITimerHandle
    {
        THandle Create(string normalizedId, MonoBehaviour runner);
    }
}
