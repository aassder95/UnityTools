using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityTools.Util.UiFramework
{
    public class UiBackInput : MonoBehaviour
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private InputActionReference _backAction;

        //============================================================
        // Fields
        //============================================================
        private UiNavigator _navigator;
        private bool _isRegistered;

        //============================================================
        // Properties
        //============================================================
        public bool IsInit => _navigator != null;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnEnable()
        {
            if (_navigator != null)
                RegisterInput();
        }

        private void OnDisable()
        {
            UnregisterInput();
        }

        private void OnDestroy()
        {
            Release();
        }

        //============================================================
        // Init/Register
        //============================================================
        public void Init(UiNavigator navigator)
        {
            if (navigator == null)
                return;

            if (ReferenceEquals(_navigator, navigator))
                return;

            Release();
            _navigator = navigator;
            if (isActiveAndEnabled)
                RegisterInput();
        }

        public void Release()
        {
            UnregisterInput();
            _navigator = null;
        }

        private void RegisterInput()
        {
            if (_isRegistered)
                return;

            _backAction.action.performed += OnBackPerformed;
            _isRegistered = true;
        }

        private void UnregisterInput()
        {
            if (!_isRegistered)
                return;

            _backAction.action.performed -= OnBackPerformed;
            _isRegistered = false;
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OnBackPerformed(InputAction.CallbackContext context)
        {
            _navigator.HandleBack();
        }
    }
}
