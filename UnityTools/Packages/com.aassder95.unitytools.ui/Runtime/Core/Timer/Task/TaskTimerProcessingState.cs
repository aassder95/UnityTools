namespace UnityTools.Util.Core.Timer.Task
{
    public class TaskTimerProcessingState : TaskTimerBaseState
    {
        //============================================================
        // Constructors
        //============================================================
        public TaskTimerProcessingState(TaskTimer timer) : base(timer) { }

        //============================================================
        // Logic
        //============================================================
        public override void Enter()
        {
            _timer.NotifyProcessingStarted();
        }

        public override void Execute()
        {
            if(_timer.IsPeriodExpired)
            {
                if(!_timer.UpdateCompletionTime())
                    _timer.Release();

                return;
            }

            _timer.NotifyUpdate();
        }
    }
}
