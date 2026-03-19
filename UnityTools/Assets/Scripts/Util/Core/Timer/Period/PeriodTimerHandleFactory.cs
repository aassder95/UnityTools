using UnityEngine;

namespace UnityTools.Util
{
    public class PeriodTimerHandleFactory : ITimerHandleFactory<PeriodTimerHandle>
    {
        //============================================================
        //Logic
        //============================================================
        public PeriodTimerHandle Create(string normalizedId, MonoBehaviour runner)
        {
            return new PeriodTimerHandle(new PeriodTimer(normalizedId, runner));
        }
    }
}
