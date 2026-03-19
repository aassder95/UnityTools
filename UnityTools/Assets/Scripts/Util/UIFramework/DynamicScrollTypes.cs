namespace UnityTools.Util
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
