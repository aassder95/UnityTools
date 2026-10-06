using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityTools.Ui
{
    [RequireComponent(typeof(Button), typeof(RectTransform))]
    public class UiButtonPressScale : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, ICancelHandler, IDeselectHandler
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField, Range(0.1f, 1.0f)] private float _pressedScale = 0.9f;
        [SerializeField, Min(0.0f)] private float _durationSec = 0.08f;

        //============================================================
        // Fields
        //============================================================
        private Button _btn;
        private RectTransform _rt;
        private Vector3 _originScale;
        private Vector3 _startScale;
        private Vector3 _targetScale;
        private float _elapsedSec;
        private int _pointerId;
        private bool _hasOrigin;
        private bool _isHeld;
        private bool _isAnimating;

        //============================================================
        // Unity Methods
        //============================================================
        private void Awake()
        {
            _btn = GetComponent<Button>();
            _rt = GetComponent<RectTransform>();
        }

        private void OnDisable()
        {
            ResetImmediate();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
                ResetImmediate();
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
                ResetImmediate();
        }

        private void Update()
        {
            if (!_hasOrigin)
                return;

            if (!_btn.IsActive() || !_btn.IsInteractable())
            {
                ResetImmediate();
                return;
            }

            if (!_isAnimating)
                return;

            _elapsedSec += Time.unscaledDeltaTime;
            float ratio = _durationSec == 0.0f ? 1.0f : Mathf.Clamp01(_elapsedSec / _durationSec);
            _rt.localScale = Vector3.LerpUnclamped(_startScale, _targetScale, ratio * ratio * (3.0f - 2.0f * ratio));
            if (ratio >= 1.0f)
            {
                _isAnimating = false;
                if (!_isHeld)
                    _hasOrigin = false;
            }
        }

        //============================================================
        // Logic
        //============================================================
        public bool TryConfigure(float pressedScale, float durationSec)
        {
            if (float.IsNaN(pressedScale) || float.IsInfinity(pressedScale) || pressedScale < 0.1f || pressedScale > 1.0f || float.IsNaN(durationSec) || float.IsInfinity(durationSec) || durationSec < 0.0f)
                return false;

            ResetImmediate();
            _pressedScale = pressedScale;
            _durationSec = durationSec;
            return true;
        }

        public void ResetImmediate()
        {
            if (_hasOrigin)
                _rt.localScale = _originScale;

            _hasOrigin = false;
            _isHeld = false;
            _isAnimating = false;
        }

        private void StartMotion(Vector3 targetScale)
        {
            _startScale = _rt.localScale;
            _targetScale = targetScale;
            _elapsedSec = 0.0f;
            _isAnimating = true;
        }

        //============================================================
        // Callbacks
        //============================================================
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData == null || eventData.button != PointerEventData.InputButton.Left || _isHeld || !isActiveAndEnabled || !_btn.IsActive() || !_btn.IsInteractable())
                return;

            if (!_hasOrigin)
            {
                _originScale = _rt.localScale;
                _hasOrigin = true;
            }

            _pointerId = eventData.pointerId;
            _isHeld = true;
            StartMotion(_originScale * _pressedScale);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData == null || eventData.button != PointerEventData.InputButton.Left || eventData.pointerId != _pointerId || !_isHeld)
                return;

            _isHeld = false;
            StartMotion(_originScale);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventData == null || eventData.pointerId != _pointerId || !_isHeld)
                return;

            _isHeld = false;
            StartMotion(_originScale);
        }

        public void OnCancel(BaseEventData eventData)
        {
            ResetImmediate();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            ResetImmediate();
        }
    }
}
