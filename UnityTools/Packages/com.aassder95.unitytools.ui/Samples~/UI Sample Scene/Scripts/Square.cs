using UnityEngine;
using UnityTools.Util.Core.Pooling;

namespace UnityTools.Samples
{
    public class Square : MonoBehaviour, IPoolable
    {
        //============================================================
        // Callbacks
        //============================================================
        void IPoolable.OnGet() { }
        void IPoolable.OnReturn() { }
    }
}
