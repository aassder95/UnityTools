using UnityEngine;

namespace UnityTools.Manager
{
    public class UIManagerBootstrap : MonoBehaviour
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private UIManager _uiManager;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnEnable()
        {
            _uiManager.Init();
        }

        private void OnDisable()
        {
            _uiManager.Release();
        }
    }
}
