using UnityTools.Util;

namespace UnityTools.Manager
{
    //============================================================
    //Logic
    //============================================================
    public class GameManager : MonoSingleton<GameManager>
    {
        private void Awake()
        {
            InitSingletons();
        }

        private void InitSingletons()
        {
            Singletons.RegisterGameManager(Instance);
            Singletons.RegisterUIManager(UIManager.Instance);
        }
    }
}
