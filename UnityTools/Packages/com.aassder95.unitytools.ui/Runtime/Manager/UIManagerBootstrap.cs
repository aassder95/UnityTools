using UnityEngine;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Manager
{
    public class UIManagerBootstrap : MonoBehaviour
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private UIManager _uiManager;

        //============================================================
        // Fields
        //============================================================
        private bool _isManagerInit;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnEnable()
        {
            _isManagerInit = _uiManager.TryInit();
            if(!_isManagerInit)
                enabled = false;
        }

        private void OnDisable()
        {
            if(!_isManagerInit)
                return;

            if(!_uiManager.TryRelease())
                DebugLogger.LogError("UIManagerBootstrap이 UIManager를 정상적으로 해제하지 못했습니다.", this);

            _isManagerInit = false;
        }
    }
}