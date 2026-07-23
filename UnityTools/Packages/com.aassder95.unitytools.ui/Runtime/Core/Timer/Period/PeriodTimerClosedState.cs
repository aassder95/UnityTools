using UnityTools.Util.Core.Logging;

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
                    if(!_timer.TryRelease())
                        DebugLogger.LogError("PeriodTimer Closed 전환 실패 후 정리를 완료하지 못했습니다. ID=" + _timer.Id);

                return;
            }

            _timer.NotifyClosedRemainMinUpdated();
        }
    }
}
