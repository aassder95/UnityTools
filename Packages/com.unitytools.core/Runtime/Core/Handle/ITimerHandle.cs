namespace UnityTools.Util
{
    public interface ITimerHandle
    {
        string Id { get; }
        void Release();
    }
}
