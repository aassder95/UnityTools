using UnityTools.Manager;

namespace UnityTools.Util
{
    public class Singletons
    {
        //============================================================
        // Fields
        //============================================================
        private static GameManager _gameManager;
        private static UIManager _uiManager;
        private static TaskTimerManager _taskTimerManager;
        private static PeriodTimerManager _periodTimerManager;

        //============================================================
        // Properties
        //============================================================
        public static GameManager GameManager { get => _gameManager; set => _gameManager = value; }
        public static UIManager UIManager { get => _uiManager; set => _uiManager = value; }
        public static TaskTimerManager TaskTimerManager { get => _taskTimerManager; set => _taskTimerManager = value; }
        public static PeriodTimerManager PeriodTimerManager { get => _periodTimerManager; set => _periodTimerManager = value; }
    }
}
