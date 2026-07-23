namespace UnityTools.Util.Core.Timer
{
    public interface ITimerLifecycle
    {
        //============================================================
        // Properties
        //============================================================
        string Id { get; }

        //============================================================
        // Logic
        //============================================================
        bool TryRefresh();
        bool TryRelease();
    }
}
