using UnityEngine;

namespace UnityTools.Util.Utilities
{
    // Exception: stateless utility is kept as a static helper.
    public static class IndexUtils
    {
        //============================================================
        // Constants
        //============================================================
        private const float POSITION_EPSILON = 0.0001f;

        //============================================================
        // Logic
        //============================================================
        public static int CalculateClampedIndexFromPosition(float position, float itemSize, int lastIndex)
        {
            if(itemSize <= 0.0f)
                return 0;

            int index = Mathf.FloorToInt((position / itemSize) + POSITION_EPSILON);
            int safeLastIndex = lastIndex < 0 ? 0 : lastIndex;
            if(index < 0)
                return 0;
            if(index > safeLastIndex)
                return safeLastIndex;

            return index;
        }
    }
}
