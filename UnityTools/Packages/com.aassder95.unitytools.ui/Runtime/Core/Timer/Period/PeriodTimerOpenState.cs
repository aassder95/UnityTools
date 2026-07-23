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
                _timer.HandleTampered();
                return;
            }

            if(!_timer.IsOpenPeriod)
            {
                _timer.Fsm.Change(EPeriodTimerType.Closed);
                return;
            }

            _timer.NotifyOpenRemainMinUpdated();
        }
    }
}
