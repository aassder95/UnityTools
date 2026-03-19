using UnityEngine;
using UnityTools.Util.Constants;
using UnityTools.Util.Core.Collections;
using UnityTools.Util.Core.Events;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.Core.Persistence;
using UnityTools.Util.Core.Pooling;
using UnityTools.Util.Core.Singleton;
using UnityTools.Util.Core.State;
using UnityTools.Util.Core.Timer.Period;
using UnityTools.Util.Core.Timer.Shared;
using UnityTools.Util.Core.Timer.Task;
using UnityTools.Util.Coroutines;
using UnityTools.Util.Extensions;
using UnityTools.Util.UIFramework;
using UnityTools.Util.Utilities;

namespace UnityTools.Util.Utilities
{
    public static class IndexUtils
    {
        //============================================================
        //Constants
        //============================================================
        private const float POSITION_EPSILON = 0.0001f;

        //============================================================
        //Logic
        //============================================================
        public static int CalculateClampedIndexFromPosition(float position, float itemSize, int lastIndex)
        {
            if(itemSize <= 0f)
                return 0;

            int index = Mathf.FloorToInt(position / itemSize + POSITION_EPSILON);
            int safeLastIndex = lastIndex < 0 ? 0 : lastIndex;
            if(index < 0)
                return 0;
            if(index > safeLastIndex)
                return safeLastIndex;

            return index;
        }
    }
}
