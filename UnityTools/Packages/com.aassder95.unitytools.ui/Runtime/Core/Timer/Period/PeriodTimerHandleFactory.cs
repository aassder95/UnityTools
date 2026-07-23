using UnityEngine;

namespace UnityTools.Util.Core.Timer.Period
{
    public class PeriodTimerHandleFactory : ITimerHandleFactory<PeriodTimerHandle>
    {
        //============================================================
        // Logic
        //============================================================
        public bool TryCreate(string normalizedId, MonoBehaviour runner, out PeriodTimerHandle handle)
        {
            handle = null;
            if(!PeriodTimer.TryCreate(normalizedId, runner, out PeriodTimer timer))
                return false;

            handle = new PeriodTimerHandle(timer);
            return true;
        }
    }
}
