using UnityTools.Manager;

namespace UnityTools.Util
{
    public class Singletons
    {
        //============================================================
        //Fields
        //============================================================
        private static TimerManager _timerManager;

        //============================================================
        //Properties
        //============================================================
        public static TimerManager TimerManager => _timerManager;

        //============================================================
        //Logic
        //============================================================
        public static void RegisterTimerManager(TimerManager timerManager)
        {
            _timerManager = timerManager;
        }
    }
}
