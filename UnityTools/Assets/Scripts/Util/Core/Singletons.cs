using UnityTools.Manager;

namespace UnityTools.Util
{
    public class Singletons
    {
        static GameManager _gameManager;
        static UIManager _uiManager;
        static RankOSAManager _rankOSAManager;

        public static GameManager GameManager { get => _gameManager; set => _gameManager = value; }
        public static UIManager UIManager { get => _uiManager; set => _uiManager = value; }
        public static RankOSAManager RankOSAManager { get => _rankOSAManager; set => _rankOSAManager = value; }
    }
}