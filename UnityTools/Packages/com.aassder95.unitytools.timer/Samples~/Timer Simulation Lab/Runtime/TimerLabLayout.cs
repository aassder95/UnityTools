using UnityEngine;
using UnityEngine.UI;

namespace UnityTools.Timer.Samples
{
    public class TimerLabLayout : MonoBehaviour
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Viewport")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasScaler _scaler;
        [SerializeField] private RectTransform _rtSafeArea;
        [SerializeField] private RectTransform _rtContent;
        [Header("Content")]
        [SerializeField] private Text _txtTitle;
        [SerializeField] private Text _txtHelp;
        [SerializeField] private RectTransform[] _rtButtons;
        [SerializeField] private Text[] _txtButtonLabels;
        [SerializeField] private Text _txtBeforeTitle;
        [SerializeField] private Text _txtBefore;
        [SerializeField] private Text _txtAfterTitle;
        [SerializeField] private Text _txtAfter;
        [SerializeField] private Text _txtResultTitle;
        [SerializeField] private Text _txtResult;

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
            Refresh();
        }

        private void Update()
        {
            if (_screenSize.x != Screen.width || _screenSize.y != Screen.height || _safeArea != Screen.safeArea)
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
            if (!(screenSize.x > 0.0f && screenSize.y > 0.0f && safeArea.width > 0.0f && safeArea.height > 0.0f && safeArea.xMin >= 0.0f && safeArea.yMin >= 0.0f && safeArea.xMax <= screenSize.x && safeArea.yMax <= screenSize.y) || float.IsInfinity(screenSize.x) || float.IsInfinity(screenSize.y))
                return false;

            _screenSize = screenSize;
            _safeArea = safeArea;
            bool isPortrait = screenSize.y > screenSize.x;
            float referenceWidth = isPortrait ? 720.0f : 1440.0f;
            _scaler.referenceResolution = new Vector2(referenceWidth, 900.0f);
            _scaler.matchWidthOrHeight = 0.0f;
            _canvas.scaleFactor = _canvas.pixelRect.width / referenceWidth;
            _rtSafeArea.anchorMin = new Vector2(safeArea.xMin / screenSize.x, safeArea.yMin / screenSize.y);
            _rtSafeArea.anchorMax = new Vector2(safeArea.xMax / screenSize.x, safeArea.yMax / screenSize.y);
            _rtSafeArea.offsetMin = _rtSafeArea.offsetMax = Vector2.zero;
            float width = referenceWidth * safeArea.width / screenSize.x;
            float innerWidth = width - 56.0f;
            float y = 22.0f;
            _txtTitle.fontSize = isPortrait ? 24 : 30;
            y += PlaceText(_txtTitle, 28.0f, y, innerWidth, 46.0f) + 10.0f;
            y += PlaceText(_txtHelp, 28.0f, y, innerWidth, 30.0f) + 18.0f;
            int cols = isPortrait ? 2 : 5;
            float buttonWidth = (innerWidth - (cols - 1) * 16.0f) / cols;
            for (int idx = 0; idx < _rtButtons.Length; ++idx)
            {
                Place(_rtButtons[idx], 28.0f + idx % cols * (buttonWidth + 16.0f), y + idx / cols * 64.0f, buttonWidth, 54.0f);
                _txtButtonLabels[idx].fontSize = isPortrait ? 18 : 15;
                Place(_txtButtonLabels[idx].rectTransform, 0.0f, 0.0f, buttonWidth, 54.0f);
            }

            y += Mathf.CeilToInt((float)_rtButtons.Length / cols) * 64.0f + 24.0f;
            float panelWidth = isPortrait ? innerWidth : (innerWidth - 44.0f) * 0.5f;
            PlaceText(_txtBeforeTitle, 28.0f, y, panelWidth, 32.0f);
            float beforeHeight = PlaceText(_txtBefore, 28.0f, y + 40.0f, panelWidth, 270.0f);
            float afterY = isPortrait ? y + 40.0f + beforeHeight + 24.0f : y;
            float afterX = isPortrait ? 28.0f : 28.0f + panelWidth + 44.0f;
            PlaceText(_txtAfterTitle, afterX, afterY, panelWidth, 32.0f);
            float afterHeight = PlaceText(_txtAfter, afterX, afterY + 40.0f, panelWidth, 270.0f);
            y = Mathf.Max(y + 40.0f + beforeHeight, afterY + 40.0f + afterHeight) + 26.0f;
            PlaceText(_txtResultTitle, 28.0f, y, innerWidth, 32.0f);
            y += 40.0f + PlaceText(_txtResult, 28.0f, y + 40.0f, innerWidth, 240.0f) + 28.0f;
            _rtContent.anchorMin = new Vector2(0.0f, 1.0f);
            _rtContent.anchorMax = Vector2.one;
            _rtContent.pivot = new Vector2(0.0f, 1.0f);
            _rtContent.anchoredPosition = Vector2.zero;
            _rtContent.sizeDelta = new Vector2(0.0f, Mathf.Max(y, referenceWidth * safeArea.height / screenSize.x));
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
