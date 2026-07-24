namespace UnityTools.Util.UiFramework
{
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
        FixedCnt,
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
