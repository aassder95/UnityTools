using UnityEngine;

namespace UnityTools.Util
{
    public abstract class UIView : MonoBehaviour, IPoolable
    {
        public abstract void OnGet();
        public abstract void OnReturn();
    }
}