namespace UnityTools.Util.Core.Timer.Period
{
    public class PeriodTimerClosedState : PeriodTimerBaseState
    {
        //============================================================
        // Constructors
        //============================================================
        public PeriodTimerClosedState(PeriodTimer timer) : base(timer) { }

        //============================================================
        // Logic
        //============================================================
        public override void Enter()
        {
            if(_timer.IsTamperedFlag)
                _timer.ClearTampered();

            _timer.NotifyClosedPeriodStarted();
            _timer.NotifyClosedRemainMinUpdated();
        }

        public override void Execute()
        {
            if(!_timer.IsClosedPeriod)
            {
                _timer.Fsm.Change(EPeriodTimerType.Reset, true);
                return;
            }

            _timer.NotifyClosedRemainMinUpdated();
        }
    }
}
