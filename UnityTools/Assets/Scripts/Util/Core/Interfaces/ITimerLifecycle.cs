namespace UnityTools.Util
{
    public interface ITimerLifecycle
    {
        string Id { get; }
        void Refresh();
        void Release();
    }
}
