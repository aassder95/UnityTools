using UnityTools.Util.Core.State;

namespace UnityTools.Util.Core.Timer.Task
{
    public class TaskTimerBaseState : IState
    {
        //============================================================
        // Readonly
        //============================================================
        protected readonly TaskTimer _timer;

        //============================================================
        // Constructors
        //============================================================
        public TaskTimerBaseState(TaskTimer timer)
        {
            _timer = timer;
        }

        //============================================================
        // Logic
        //============================================================
        public virtual void Enter() { }
        public virtual void Execute() { }
        public virtual void Exit() { }
    }
}
