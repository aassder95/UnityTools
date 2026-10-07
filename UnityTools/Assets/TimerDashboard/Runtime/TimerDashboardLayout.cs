using UnityEngine;
using UnityEngine.UI;
using UnityTools.Ui;

namespace UnityTools.TimerDashboard
{
    public class TimerDashboardLayout : MonoBehaviour
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Viewport")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasScaler _scaler;
        [SerializeField] private UiSafeArea _safeArea;
        [SerializeField] private RectTransform _rtContent;
        [Header("Controls")]
        [SerializeField] private Text _txtTitle;
        [SerializeField] private Text _txtHelp;
        [SerializeField] private RectTransform _rtInput;
        [SerializeField] private RectTransform[] _rtButtons;
        [SerializeField] private Text[] _txtButtonLabels;
        [SerializeField] private Text _txtStatus;
        [SerializeField] private RectTransform _rtTimers;
        [SerializeField] private Text _txtTimers;
        [SerializeField] private RectTransform _rtToast;
        [SerializeField] private Text _txtToast;

        //============================================================
        // Fields
        //============================================================
        private Vector2 _screenSize;
        private Rect _area;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnEnable()
        {
            Refresh();
        }

        private void Update()
        {
            if (_screenSize.x != Screen.width || _screenSize.y != Screen.height || _area != Screen.safeArea)
                Refresh();
        }

        //============================================================
        // Logic
        //============================================================
        public void Refresh()
        {
            if (!TryApplyViewport(new Vector2(Screen.width, Screen.height), Screen.safeArea))
                return;
        }

        public bool TryApplyViewport(Vector2 screenSize, Rect safeArea)
        {
            if (!_safeArea.TryApplyViewport(screenSize, safeArea))
                return false;

            _screenSize = screenSize;
            _area = safeArea;
            bool isPortrait = screenSize.y > screenSize.x;
            float referenceWidth = isPortrait ? 720.0f : 1440.0f;
            _scaler.referenceResolution = new Vector2(referenceWidth, 900.0f);
            _scaler.matchWidthOrHeight = 0.0f;
            _canvas.scaleFactor = _canvas.pixelRect.width / referenceWidth;
            float width = referenceWidth * safeArea.width / screenSize.x - 56.0f;
            float y = 22.0f;
            _txtTitle.fontSize = isPortrait ? 24 : 30;
            y += PlaceText(_txtTitle, 28.0f, y, width, 48.0f) + 8.0f;
            y += PlaceText(_txtHelp, 28.0f, y, width, 50.0f) + 16.0f;
            Place(_rtInput, 28.0f, y, width, 56.0f);
            y += 76.0f;
            int cols = isPortrait ? 2 : 4;
            float buttonWidth = (width - (cols - 1) * 16.0f) / cols;
            for (int idx = 0; idx < _rtButtons.Length; ++idx)
            {
                Place(_rtButtons[idx], 28.0f + idx % cols * (buttonWidth + 16.0f), y + idx / cols * 64.0f, buttonWidth, 54.0f);
                Place(_txtButtonLabels[idx].rectTransform, 0.0f, 0.0f, buttonWidth, 54.0f);
            }

            y += Mathf.CeilToInt((float)_rtButtons.Length / cols) * 64.0f + 12.0f;
            y += PlaceText(_txtStatus, 28.0f, y, width, 48.0f) + 16.0f;
            Place(_rtTimers, 28.0f, y, width, 310.0f);
            Place(_txtTimers.rectTransform, 0.0f, 0.0f, width, 310.0f);
            float listHeight = Mathf.Max(310.0f, _txtTimers.preferredHeight + 8.0f);
            _rtTimers.sizeDelta = new Vector2(width, listHeight);
            y += listHeight + 100.0f;
            _rtContent.anchorMin = new Vector2(0.0f, 1.0f);
            _rtContent.anchorMax = Vector2.one;
            _rtContent.pivot = new Vector2(0.0f, 1.0f);
            _rtContent.sizeDelta = new Vector2(0.0f, Mathf.Max(y, referenceWidth * safeArea.height / screenSize.x));
            _rtToast.anchorMin = _rtToast.anchorMax = Vector2.zero;
            _rtToast.pivot = Vector2.zero;
            _rtToast.anchoredPosition = new Vector2(28.0f, 16.0f);
            _rtToast.sizeDelta = new Vector2(width, 66.0f);
            Place(_txtToast.rectTransform, 12.0f, 8.0f, width - 24.0f, 50.0f);
            return true;
        }

        //============================================================
        // Utilities
        //============================================================
        private static float PlaceText(Text txt, float x, float y, float width, float minHeight)
        {
            Place(txt.rectTransform, x, y, width, minHeight);
            float height = Mathf.Max(minHeight, txt.preferredHeight + 4.0f);
            txt.rectTransform.sizeDelta = new Vector2(width, height);
            return height;
        }

        private static void Place(RectTransform rt, float x, float y, float width, float height)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.0f, 1.0f);
            rt.pivot = new Vector2(0.0f, 1.0f);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(width, height);
        }
    }
}
