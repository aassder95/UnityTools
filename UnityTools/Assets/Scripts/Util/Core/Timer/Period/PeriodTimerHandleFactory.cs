using UnityEngine;

namespace UnityTools.Util.Core.Timer.Period
{
    public class PeriodTimerHandleFactory : ITimerHandleFactory<PeriodTimerHandle>
    {
        //============================================================
        // Logic
        //============================================================
        public PeriodTimerHandle Create(string normalizedId, MonoBehaviour runner)
        {
            return new PeriodTimerHandle(new PeriodTimer(normalizedId, runner));
        }
    }
}
