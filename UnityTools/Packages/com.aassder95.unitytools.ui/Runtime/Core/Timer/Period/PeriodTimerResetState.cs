namespace UnityTools.Util.Core.Timer.Period
{
    public class PeriodTimerResetState : PeriodTimerBaseState
    {
        //============================================================
        // Constructors
        //============================================================
        public PeriodTimerResetState(PeriodTimer timer) : base(timer) { }

        //============================================================
        // Logic
        //============================================================
        public override void Enter()
        {
            _timer.NotifyOpenPeriodPreparing();
            _timer.ApplyPeriodTime();
            _timer.NotifyOpenPeriodStarted();
        }

        public override void Execute()
        {
            _timer.TryChangeState(EPeriodTimerType.Open, false, "PeriodTimerResetState.Execute");
        }
    }
}
