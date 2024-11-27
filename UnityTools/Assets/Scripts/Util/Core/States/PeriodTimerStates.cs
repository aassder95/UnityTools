using System;

namespace UnityTools.Util
{
    public class PeriodTimerStates
    {
        public class BaseState : IState
        {
            protected PeriodTimer _timer;
            public BaseState(PeriodTimer timer) { _timer = timer; }
            public virtual void Enter() { }
            public virtual void Execute() { }
            public virtual void Exit() { }
        }

        public class ResetState : BaseState
        {
            public ResetState(PeriodTimer timer) : base(timer) { }
            public override void Enter()
            {
                DateTime now = DateTime.UtcNow;
                _timer.SetPeriodTime(PeriodTimer.OPEN_START_KEY, now);
                _timer.SetPeriodTime(PeriodTimer.OPEN_END_KEY, now.AddMinutes(_timer.OPEN_PERIOD_MINUTES));
                _timer.SetPeriodTime(PeriodTimer.CLOSED_END_KEY, now.AddMinutes(_timer.OPEN_PERIOD_MINUTES + _timer.CLOSED_PERIOD_MINUTES));
            }

            public override void Execute()
            {
                _timer.FSM.Change(EPeriodTimerState.Open);
            }
        }

        public class OpenState : BaseState
        {
            public OpenState(PeriodTimer timer) : base(timer) { }
            public override void Execute()
            {
                if (_timer.IsTimeTampered)
                {
                    _timer.MarkTimeTampered();
                    _timer.FSM.Change(EPeriodTimerState.Closed);
                    return;
                }
                else if (!_timer.IsOpenPeriod)
                {
                    _timer.FSM.Change(EPeriodTimerState.Closed);
                    return;
                }

                _timer.SetPeriodTime(PeriodTimer.OPEN_UPDATED_KEY, DateTime.UtcNow);
                _timer.InvokeLoopUpdated(PeriodTimer.OPEN_END_KEY);
            }
        }

        public class ClosedState : BaseState
        {
            public ClosedState(PeriodTimer timer) : base(timer) { }
            public override void Enter()
            {
                if (_timer.IsTimeTamperedFlag)
                    _timer.ClearTimeTampered();

                _timer.SetPeriodTime(PeriodTimer.OPEN_START_KEY, DateTime.MinValue);
                _timer.SetPeriodTime(PeriodTimer.OPEN_UPDATED_KEY, DateTime.MinValue);
                _timer.SetPeriodTime(PeriodTimer.OPEN_END_KEY, DateTime.MinValue);
            }

            public override void Execute()
            {
                if (!_timer.IsClosedPeriod)
                {
                    _timer.FSM.Change(EPeriodTimerState.Reset, true);
                    return;
                }

                _timer.InvokeLoopUpdated(PeriodTimer.CLOSED_END_KEY);
            }
        }
    }
}