using UnityTools.Util;

namespace UnityTools.Manager
{
    public class GameManager : MonoSingleton<GameManager>
    {
        //============================================================
        //Unity Methods
        //============================================================
        private void Awake()
        {
            Singletons.RegisterGameManager(Instance);
            Singletons.RegisterUIManager(UIManager.Instance);
        }
    }
}
