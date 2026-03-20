using UnityEngine;

namespace UnityTools.Util.Core.Timer
{
    // Exception: interface-centric file uses Types section.
    //============================================================
    //Types
    //============================================================
    public interface ITimerHandleFactory<THandle> where THandle : ITimerHandle
    {
        //============================================================
        //Logic
        //============================================================
        THandle Create(string normalizedId, MonoBehaviour runner);
    }
}
