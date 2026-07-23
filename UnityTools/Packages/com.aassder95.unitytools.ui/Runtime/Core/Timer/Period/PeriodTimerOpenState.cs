using UnityTools.Util.Core.Logging;

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
                if(!_timer.TryHandleTampered())
                    if(!_timer.TryRelease())
                        DebugLogger.LogError("PeriodTimer Open 상태 실패 후 정리를 완료하지 못했습니다. ID=" + _timer.Id);

                return;
            }

            if(!_timer.IsOpenPeriod)
            {
                if(!_timer.TryChangeState(EPeriodTimerType.Closed, false, "PeriodTimerOpenState.Execute"))
                    if(!_timer.TryRelease())
                        DebugLogger.LogError("PeriodTimer Open 상태 실패 후 정리를 완료하지 못했습니다. ID=" + _timer.Id);

                return;
            }

            _timer.NotifyOpenRemainMinUpdated();
        }
    }
}
