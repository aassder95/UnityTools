using UnityTools.Manager;

namespace UnityTools.Util
{
    public class Singletons
    {
        private static GameManager _gameManager;
        private static UIManager _uiManager;

        public static GameManager GameManager { get => _gameManager; set => _gameManager = value; }
        public static UIManager UIManager { get => _uiManager; set => _uiManager = value; }
    }
}