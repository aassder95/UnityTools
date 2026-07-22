using UnityTools.Util.Core.State;

namespace UnityTools.Util.Core.Timer.Period
{
    public class PeriodTimerBaseState : IState
    {
        //============================================================
        // Readonly
        //============================================================
        protected readonly PeriodTimer _timer;

        //============================================================
        // Constructors
        //============================================================
        protected PeriodTimerBaseState(PeriodTimer timer)
        {
            _timer = timer;
        }

        //============================================================
        // Logic
        //============================================================
        public virtual void Enter() { }
        public virtual void Execute() { }
        public virtual void Exit() { }
    }
}
