using UnityEngine;

namespace UnityTools.Util.Utilities
{
    public static class IdxUtils
    {
        //============================================================
        // Logic
        //============================================================
        public static int GetClampedIdx(float pos, float itemSize, int lastIdx)
        {
            int idx = Mathf.FloorToInt((pos / itemSize) + 0.0001f);
            return Mathf.Clamp(idx, 0, lastIdx);
        }
    }
}
