using UnityTools.Util;

namespace UnityTools.Manager
{
    public class GameManager : Singleton<GameManager>
    {
        void Awake()
        {
            InitSingletons();
        }

        void InitSingletons()
        {
            Singletons.GameManager = Instance;
            Singletons.UIManager = UIManager.Instance;
            Singletons.RankOSAManager = RankOSAManager.Instance;
        }
    }
}