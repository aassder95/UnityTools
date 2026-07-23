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
                if(!_timer.TryChangeState(EPeriodTimerType.Reset, true, "PeriodTimerClosedState.Execute"))
                    _timer.Release();

                return;
            }

            _timer.NotifyClosedRemainMinUpdated();
        }
    }
}
