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
        public static GameManager GameManager => _gameManager;
        public static UIManager UIManager => _uiManager;
        public static TaskTimerManager TaskTimerManager => _taskTimerManager;
        public static PeriodTimerManager PeriodTimerManager => _periodTimerManager;

        //============================================================
        // Logic
        //============================================================
        public static void RegisterGameManager(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public static void RegisterUIManager(UIManager uiManager)
        {
            _uiManager = uiManager;
        }

        public static void RegisterTaskTimerManager(TaskTimerManager taskTimerManager)
        {
            _taskTimerManager = taskTimerManager;
        }

        public static void RegisterPeriodTimerManager(PeriodTimerManager periodTimerManager)
        {
            _periodTimerManager = periodTimerManager;
        }
    }
}