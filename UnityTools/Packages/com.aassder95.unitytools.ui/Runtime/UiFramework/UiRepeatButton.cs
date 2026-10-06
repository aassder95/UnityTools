using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityTools.Ui
{
    public class UiRepeatButton : Button, ICancelHandler
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Repeat Timing")]
        [SerializeField, Min(0.0f)] private float _initialDelaySec = 0.6f;
        [SerializeField, Min(0.01f)] private float _intervalSec = 0.3f;
        [SerializeField, Min(0.01f)] private float _minIntervalSec = 0.05f;
        [SerializeField, Range(0.1f, 1.0f)] private float _acceleration = 0.85f;

        //============================================================
        // Fields
        //============================================================
        private int _pointerId;
        private int _repeatCnt;
        private double _nextRepeatSec;
        private float _curIntervalSec;
        private bool _isHeld;
        private bool _hasRepeated;
        private bool _canClick;

        //============================================================
        // Properties
        //============================================================
        public bool IsHeld => _isHeld;
        public int RepeatCnt => _repeatCnt;

        //============================================================
        // Unity Methods
        //============================================================
        protected override void OnDisable()
        {
            CancelPress();
            base.OnDisable();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
                CancelPress();
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
                CancelPress();
        }

        private void Update()
        {
            if (!_isHeld)
                return;

            if (!IsActive() || !IsInteractable())
            {
                CancelPress();
                return;
            }

            double nowSec = Time.unscaledTimeAsDouble;
            if (nowSec < _nextRepeatSec)
                return;

            _hasRepeated = true;
            _repeatCnt++;
            _nextRepeatSec = nowSec + _curIntervalSec;
            _curIntervalSec = Mathf.Max(_minIntervalSec, _curIntervalSec * _acceleration);
            onClick.Invoke();
        }

        //============================================================
        // Logic
        //============================================================
        public bool TryConfigure(float initialDelaySec, float intervalSec, float minIntervalSec, float acceleration)
        {
            if (float.IsNaN(initialDelaySec) || float.IsInfinity(initialDelaySec) || initialDelaySec < 0.0f || float.IsNaN(intervalSec) || float.IsInfinity(intervalSec) || intervalSec < 0.01f || float.IsNaN(minIntervalSec) || float.IsInfinity(minIntervalSec) || minIntervalSec < 0.01f || minIntervalSec > intervalSec || float.IsNaN(acceleration) || float.IsInfinity(acceleration) || acceleration < 0.1f || acceleration > 1.0f)
                return false;

            CancelPress();
            _initialDelaySec = initialDelaySec;
            _intervalSec = intervalSec;
            _minIntervalSec = minIntervalSec;
            _acceleration = acceleration;
            return true;
        }

        public void CancelPress()
        {
            _isHeld = false;
            _canClick = false;
        }

        //============================================================
        // Callbacks
        //============================================================
        public override void OnPointerDown(PointerEventData eventData)
        {
            if (eventData == null || eventData.button != PointerEventData.InputButton.Left || _isHeld || !IsActive() || !IsInteractable())
                return;

            base.OnPointerDown(eventData);
            _pointerId = eventData.pointerId;
            _isHeld = true;
            _hasRepeated = false;
            _canClick = true;
            _repeatCnt = 0;
            _curIntervalSec = _intervalSec;
            _nextRepeatSec = Time.unscaledTimeAsDouble + _initialDelaySec;
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (eventData == null || eventData.button != PointerEventData.InputButton.Left || eventData.pointerId != _pointerId)
                return;

            base.OnPointerUp(eventData);
            _isHeld = false;
            if (!IsActive() || !IsInteractable())
                _canClick = false;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData == null || eventData.button != PointerEventData.InputButton.Left || eventData.pointerId != _pointerId || !_canClick)
                return;

            _canClick = false;
            if (!_hasRepeated)
                base.OnPointerClick(eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (eventData == null)
                return;

            if (eventData.pointerId == _pointerId)
                CancelPress();

            base.OnPointerExit(eventData);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            CancelPress();
            base.OnDeselect(eventData);
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            CancelPress();
            base.OnSubmit(eventData);
        }

        public void OnCancel(BaseEventData eventData)
        {
            CancelPress();
        }
    }
}
