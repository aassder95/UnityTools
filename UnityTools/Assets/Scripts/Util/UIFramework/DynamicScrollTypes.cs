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

namespace UnityTools.Util.UIFramework
{
    // Exception: enum-only file uses Types section.
    //============================================================
    //Types
    //============================================================
    public enum EDynamicScrollAxisType
    {
        Vertical = 0,
        Horizontal
    }

    public enum EDynamicScrollMovementType
    {
        Unrestricted = 0,
        Elastic,
        Clamped
    }

    public enum EDynamicScrollLayoutMode
    {
        Single = 0,
        FixedCount,
        AutoFit
    }

    public enum EDynamicScrollContentAlignment
    {
        TopLeft = 0,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }
}
