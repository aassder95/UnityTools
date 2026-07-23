using UnityEngine;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Util.Utilities
{
    public static class IdxUtils
    {
        //============================================================
        // Logic
        //============================================================
        public static int GetClampedIdx(float pos, float itemSize, int lastIdx)
        {
            if(float.IsNaN(pos) || float.IsInfinity(pos) || float.IsNaN(itemSize) || float.IsInfinity(itemSize) || itemSize <= 0.0f || lastIdx < 0)
            {
                DebugLogger.LogError("인덱스 계산 입력이 유효하지 않습니다. 위치=" + pos + ", Item 크기=" + itemSize + ", 마지막 인덱스=" + lastIdx);
                return 0;
            }

            int idx = Mathf.FloorToInt((pos / itemSize) + 0.0001f);
            return Mathf.Clamp(idx, 0, Mathf.Max(0, lastIdx));
        }
    }
}
