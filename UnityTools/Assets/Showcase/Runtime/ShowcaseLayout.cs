using UnityEngine;
using UnityEngine.UI;
using UnityTools.Ui;

namespace UnityTools.Showcase
{
    public class ShowcaseLayout : MonoBehaviour
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Viewport")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasScaler _scaler;
        [SerializeField] private UiSafeArea _safeArea;
        [SerializeField] private RectTransform _rtContent;
        [Header("Cards")]
        [SerializeField] private Text _txtEyebrow;
        [SerializeField] private Text _txtTitle;
        [SerializeField] private Text _txtIntro;
        [SerializeField] private RectTransform[] _rtCards;
        [SerializeField] private Text _txtStatus;
        [SerializeField] private Text _txtFooter;

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
            if (!TryApplyViewport(new Vector2(Screen.width, Screen.height), Screen.safeArea))
                return;
        }

        private void Update()
        {
            if (_screenSize.x == Screen.width && _screenSize.y == Screen.height && _area == Screen.safeArea)
                return;

            if (!TryApplyViewport(new Vector2(Screen.width, Screen.height), Screen.safeArea))
                return;
        }

        //============================================================
        // Logic
        //============================================================
        public bool TryApplyViewport(Vector2 screenSize, Rect area)
        {
            if (!_safeArea.TryApplyViewport(screenSize, area))
                return false;

            _screenSize = screenSize;
            _area = area;
            bool isPortrait = screenSize.y > screenSize.x;
            float referenceWidth = isPortrait ? 720.0f : 1440.0f;
            _scaler.referenceResolution = new Vector2(referenceWidth, 900.0f);
            _scaler.matchWidthOrHeight = 0.0f;
            _canvas.scaleFactor = _canvas.pixelRect.width / referenceWidth;
            float width = referenceWidth * area.width / screenSize.x - 96.0f;
            float y = 34.0f;
            y += PlaceText(_txtEyebrow, y, width, 32.0f) + 20.0f;
            _txtTitle.fontSize = isPortrait ? 32 : 42;
            y += PlaceText(_txtTitle, y, width, 64.0f) + 12.0f;
            y += PlaceText(_txtIntro, y, width, 60.0f) + 28.0f;
            int cols = isPortrait ? 1 : 3;
            float cardWidth = (width - (cols - 1) * 24.0f) / cols;
            for (int idx = 0; idx < _rtCards.Length; ++idx)
            {
                _rtCards[idx].anchoredPosition = new Vector2(48.0f + idx % cols * (cardWidth + 24.0f), -y - idx / cols * 462.0f);
                _rtCards[idx].sizeDelta = new Vector2(cardWidth, 438.0f);
            }

            y += Mathf.CeilToInt((float)_rtCards.Length / cols) * 462.0f + 16.0f;
            y += PlaceText(_txtStatus, y, width, 36.0f) + 24.0f;
            y += PlaceText(_txtFooter, y, width, 62.0f) + 28.0f;
            _rtContent.anchorMin = new Vector2(0.0f, 1.0f);
            _rtContent.anchorMax = Vector2.one;
            _rtContent.pivot = new Vector2(0.0f, 1.0f);
            _rtContent.sizeDelta = new Vector2(0.0f, Mathf.Max(y, referenceWidth * area.height / screenSize.x));
            return true;
        }

        //============================================================
        // Utilities
        //============================================================
        private static float PlaceText(Text txt, float y, float width, float minHeight)
        {
            RectTransform rt = txt.rectTransform;
            rt.anchoredPosition = new Vector2(48.0f, -y);
            rt.sizeDelta = new Vector2(width, minHeight);
            float height = Mathf.Max(minHeight, txt.preferredHeight + 4.0f);
            rt.sizeDelta = new Vector2(width, height);
            return height;
        }
    }
}
