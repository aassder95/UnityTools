using UnityEngine;

namespace UnityTools.Util
{
    public class TaskTimerHandleFactory : ITimerHandleFactory<TaskTimerHandle>
    {
        //============================================================
        //Logic
        //============================================================
        public TaskTimerHandle Create(string normalizedId, MonoBehaviour runner)
        {
            return new TaskTimerHandle(new TaskTimer(normalizedId, runner));
        }
    }
}
