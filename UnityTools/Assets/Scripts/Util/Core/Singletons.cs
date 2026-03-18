using UnityTools.Manager;

namespace UnityTools.Util
{
    public class Singletons
    {
        //============================================================
        //Fields
        //============================================================
        private static GameManager _gameManager;
        private static UIManager _uiManager;
        private static TimerManager _timerManager;

        //============================================================
        //Properties
        //============================================================
        public static GameManager GameManager => _gameManager;
        public static UIManager UIManager => _uiManager;
        public static TimerManager TimerManager => _timerManager;

        //============================================================
        //Logic
        //============================================================
        public static void RegisterGameManager(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public static void RegisterUIManager(UIManager uiManager)
        {
            _uiManager = uiManager;
        }

        public static void RegisterTimerManager(TimerManager timerManager)
        {
            _timerManager = timerManager;
        }
    }
}
