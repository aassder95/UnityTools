using UnityEngine;

namespace UnityTools.Util.Core.Timer.Task
{
    public class TaskTimerHandleFactory : ITimerHandleFactory<TaskTimerHandle>
    {
        //============================================================
        // Logic
        //============================================================
        public bool TryCreate(string normalizedId, MonoBehaviour runner, out TaskTimerHandle handle)
        {
            handle = null;
            if(!TaskTimer.TryCreate(normalizedId, runner, out TaskTimer timer))
                return false;

            handle = new TaskTimerHandle(timer);
            return true;
        }
    }
}
