namespace UnityTools.Util
{
    public class TaskTimerStates
    {
        //============================================================
        // Logic
        //============================================================
        public class BaseState : IState
        {
            protected readonly TaskTimer _timer;

            protected BaseState(TaskTimer timer)
            {
                _timer = timer;
            }

            public virtual void Enter() { }
            public virtual void Execute() { }
            public virtual void Exit() { }
        }

        public class NoneState : BaseState
        {
            public NoneState(TaskTimer timer) : base(timer) { }
        }

        public class ProcessingState : BaseState
        {
            public ProcessingState(TaskTimer timer) : base(timer) { }

            public override void Enter()
            {
                if(_timer.FSM.CurType != ETaskTimerType.Processing)
                    return;

                _timer.NotifyProcessingStarted();
            }

            public override void Execute()
            {
                if(_timer.IsPeriodExpired)
                {
                    _timer.UpdateCompletionTime();
                    return;
                }

                _timer.NotifyUpdate();
            }
        }

        public class CompletedState : BaseState
        {
            public CompletedState(TaskTimer timer) : base(timer) { }

            public override void Enter()
            {
                if(_timer.FSM.CurType != ETaskTimerType.Completed)
                    return;

                _timer.NotifyCompleted();
            }
        }
    }
}
