namespace UnityTools.Util.Core.Timer.Task
{
    public class TaskTimerCompletedState : TaskTimerBaseState
    {
        //============================================================
        // Constructors
        //============================================================
        public TaskTimerCompletedState(TaskTimer timer) : base(timer) { }

        //============================================================
        // Logic
        //============================================================
        public override void Enter()
        {
            _timer.NotifyCompleted();
        }
    }
}
