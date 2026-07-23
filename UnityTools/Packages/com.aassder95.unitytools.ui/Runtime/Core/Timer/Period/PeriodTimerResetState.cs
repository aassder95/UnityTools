using UnityTools.Util.Core.Logging;

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
            if(!_timer.TryChangeState(EPeriodTimerType.Open, false, "PeriodTimerResetState.Execute"))
                if(!_timer.TryRelease())
                    DebugLogger.LogError("PeriodTimer Reset 전환 실패 후 정리를 완료하지 못했습니다. ID=" + _timer.Id);
        }
    }
}
