using UnityEngine;

namespace UnityTools.Ui
{
    public class UiSafeArea : MonoBehaviour
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private RectTransform _rtSafeArea;

        //============================================================
        // Fields
        //============================================================
        private Vector2 _screenSize;
        private Rect _safeArea;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnEnable()
        {
            if (!TryApplyViewport(new Vector2(Screen.width, Screen.height), Screen.safeArea))
                return;
        }

        private void Update()
        {
            if (_screenSize.x == Screen.width && _screenSize.y == Screen.height && _safeArea == Screen.safeArea)
                return;

            if (!TryApplyViewport(new Vector2(Screen.width, Screen.height), Screen.safeArea))
                return;
        }

        //============================================================
        // Logic
        //============================================================
        public bool TryApplyViewport(Vector2 screenSize, Rect safeArea)
        {
            if (!(screenSize.x > 0.0f && screenSize.y > 0.0f && safeArea.width > 0.0f && safeArea.height > 0.0f && safeArea.xMin >= 0.0f && safeArea.yMin >= 0.0f && safeArea.xMax <= screenSize.x && safeArea.yMax <= screenSize.y) || float.IsInfinity(screenSize.x) || float.IsInfinity(screenSize.y))
                return false;

            _screenSize = screenSize;
            _safeArea = safeArea;
            _rtSafeArea.anchorMin = new Vector2(safeArea.xMin / screenSize.x, safeArea.yMin / screenSize.y);
            _rtSafeArea.anchorMax = new Vector2(safeArea.xMax / screenSize.x, safeArea.yMax / screenSize.y);
            _rtSafeArea.offsetMin = _rtSafeArea.offsetMax = Vector2.zero;
            return true;
        }
    }
}
