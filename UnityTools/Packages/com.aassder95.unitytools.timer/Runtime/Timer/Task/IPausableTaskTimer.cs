namespace UnityTools.Timer.Task
{
    public interface IPausableTaskTimer : ITaskTimer
    {
        bool TryPause();
        bool TryResume();
    }
}
