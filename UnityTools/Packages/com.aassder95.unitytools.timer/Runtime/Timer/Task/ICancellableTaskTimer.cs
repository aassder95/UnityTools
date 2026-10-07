namespace UnityTools.Timer.Task
{
    public interface ICancellableTaskTimer : ITaskTimer
    {
        bool TryCancel();
    }
}
