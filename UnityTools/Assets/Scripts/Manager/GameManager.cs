using UnityTools.Util;

namespace UnityTools.Manager
{
    public class GameManager : MonoSingleton<GameManager>
    {
        private void Awake()
        {
            InitSingletons();
        }

        private void InitSingletons()
        {
            Singletons.GameManager = Instance;
            Singletons.UIManager = UIManager.Instance;
        }
    }
}
