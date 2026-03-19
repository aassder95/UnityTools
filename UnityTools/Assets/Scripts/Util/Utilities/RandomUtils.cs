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
    // Exception: stateless utility is kept as a static helper.
    public static class RandomUtils
    {
        //============================================================
        //Constants
        //============================================================
        private const float HALF = 0.5f;

        //============================================================
        //Logic
        //============================================================
        public static Vector2 GetRandomPositionInRange(Vector2 center, Vector2 range)
        {
            float x = UnityEngine.Random.Range(center.x - range.x * HALF, center.x + range.x * HALF);
            float y = UnityEngine.Random.Range(center.y - range.y * HALF, center.y + range.y * HALF);
            return new Vector2(x, y);
        }

        public static Color GetRandomColor()
        {
            return new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
        }
    }
}
