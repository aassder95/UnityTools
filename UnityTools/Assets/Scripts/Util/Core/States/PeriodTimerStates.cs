namespace UnityTools.Util
{
    public class PeriodTimerStates
    {
        //============================================================
        // Logic
        //============================================================
        public class BaseState : IState
        {
            protected readonly PeriodTimer _timer;

            protected BaseState(PeriodTimer timer)
            {
                _timer = timer;
            }

            public virtual void Enter() { }
            public virtual void Execute() { }
            public virtual void Exit() { }
        }

        public class ResetState : BaseState
        {
            public ResetState(PeriodTimer timer) : base(timer) { }

            public override void Enter()
            {
                _timer.ApplyPeriodTime();
                _timer.NotifyOpenStarted();
            }

            public override void Execute()
            {
                _timer.TryChangeState(EPeriodTimerType.Open, false, "ResetState.Execute");
            }
        }

        public class OpenState : BaseState
        {
            public OpenState(PeriodTimer timer) : base(timer) { }

            public override void Enter()
            {
                _timer.NotifyUpdateOpen();
            }

            public override void Execute()
            {
                if(_timer.IsTampered)
                {
                    _timer.HandleTampered();
                    return;
                }

                if(!_timer.IsOpenPeriod)
                {
                    _timer.TryChangeState(EPeriodTimerType.Closed, false, "OpenState.Execute");
                    return;
                }

                _timer.NotifyUpdateOpen();
            }
        }

        public class ClosedState : BaseState
        {
            public ClosedState(PeriodTimer timer) : base(timer) { }

            public override void Enter()
            {
                if(_timer.IsTamperedFlag)
                    _timer.ClearTampered();

                _timer.NotifyClosedStarted();
                _timer.NotifyUpdateClosed();
            }

            public override void Execute()
            {
                if(!_timer.IsClosedPeriod)
                {
                    _timer.TryChangeState(EPeriodTimerType.Reset, true, "ClosedState.Execute");
                    return;
                }

                _timer.NotifyUpdateClosed();
            }
        }
    }
}
