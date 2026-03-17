using UnityTools.Util;

namespace UnityTools.Manager
{
    //============================================================
    //Logic
    //============================================================
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
