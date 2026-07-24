using UnityEngine;

namespace UnityTools.Manager
{
    public class UiManagerBootstrap : MonoBehaviour
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private UiManager _uiManager;

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
