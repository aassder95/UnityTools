using UnityEngine;

namespace UnityTools.Ui
{
    public class UiSafeAreaFitter : MonoBehaviour
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private RectTransform _rtTarget;
        [SerializeField] private bool _shouldFitHorizontal = true;
        [SerializeField] private bool _shouldFitVertical = true;

        //============================================================
        // Fields
        //============================================================
        private Rect _safeArea;
        private Vector2 _screenSize;
        private bool _isApplied;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnEnable()
        {
            Refresh();
        }

        private void Update()
        {
            Rect safeArea = Screen.safeArea;
            Vector2 screenSize = new(Screen.width, Screen.height);
            if (_isApplied && _safeArea == safeArea && _screenSize == screenSize)
                return;

            Apply(safeArea, screenSize);
        }

        //============================================================
        // Logic
        //============================================================
        public void Refresh()
        {
            Apply(Screen.safeArea, new Vector2(Screen.width, Screen.height));
        }

        public void Apply(Rect safeArea, Vector2 screenSize)
        {
            if (screenSize.x <= 0.0f || screenSize.y <= 0.0f)
            {
                Debug.LogError("Safe Area를 적용할 화면 크기가 유효하지 않습니다. 화면 크기=" + screenSize, this);
                return;
            }

            Vector2 anchorMin = _rtTarget.anchorMin;
            Vector2 anchorMax = _rtTarget.anchorMax;
            Vector2 offsetMin = _rtTarget.offsetMin;
            Vector2 offsetMax = _rtTarget.offsetMax;

            if (_shouldFitHorizontal)
            {
                anchorMin.x = safeArea.xMin / screenSize.x;
                anchorMax.x = safeArea.xMax / screenSize.x;
                offsetMin.x = 0.0f;
                offsetMax.x = 0.0f;
            }

            if (_shouldFitVertical)
            {
                anchorMin.y = safeArea.yMin / screenSize.y;
                anchorMax.y = safeArea.yMax / screenSize.y;
                offsetMin.y = 0.0f;
                offsetMax.y = 0.0f;
            }

            _rtTarget.anchorMin = anchorMin;
            _rtTarget.anchorMax = anchorMax;
            _rtTarget.offsetMin = offsetMin;
            _rtTarget.offsetMax = offsetMax;
            _safeArea = safeArea;
            _screenSize = screenSize;
            _isApplied = true;
        }
    }
}
