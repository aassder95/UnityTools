namespace UnityTools.Util.Core.Timer.Period
{
    public class PeriodTimerOpenState : PeriodTimerBaseState
    {
        //============================================================
        // Constructors
        //============================================================
        public PeriodTimerOpenState(PeriodTimer timer) : base(timer) { }

        //============================================================
        // Logic
        //============================================================
        public override void Enter()
        {
            _timer.NotifyOpenRemainMinUpdated();
        }

        public override void Execute()
        {
            if(_timer.IsTampered)
            {
                if(!_timer.HandleTampered())
                    _timer.Release();

                return;
            }

            if(!_timer.IsOpenPeriod)
            {
                if(!_timer.TryChangeState(EPeriodTimerType.Closed, false, "PeriodTimerOpenState.Execute"))
                    _timer.Release();

                return;
            }

            _timer.NotifyOpenRemainMinUpdated();
        }
    }
}
