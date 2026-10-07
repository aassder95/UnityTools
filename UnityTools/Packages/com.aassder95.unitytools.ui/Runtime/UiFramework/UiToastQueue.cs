using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityTools.Ui
{
    public class UiToastQueue : MonoBehaviour
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly Queue<ToastRequest> _requests = new();

        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Toast View")]
        [SerializeField] private CanvasGroup _cgToast;
        [SerializeField] private Text _txtToast;
        [Header("Pending Messages")]
        [SerializeField, Min(1)] private int _maxPendingCnt = 8;

        //============================================================
        // Fields
        //============================================================
        private string _curKey;
        private double _hideAtSec;

        //============================================================
        // Events
        //============================================================
        private event Action<string> _onShown;
        private event Action<string> _onDismissed;
        public event Action<string> OnShown { add => _onShown += value; remove => _onShown -= value; }
        public event Action<string> OnDismissed { add => _onDismissed += value; remove => _onDismissed -= value; }

        //============================================================
        // Properties
        //============================================================
        public bool IsShowing => _curKey != null;
        public string CurrentKey => _curKey;
        public int PendingCnt => _requests.Count;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnEnable()
        {
            _cgToast.alpha = 0.0f;
            _cgToast.interactable = false;
            _cgToast.blocksRaycasts = false;
            _txtToast.text = string.Empty;
        }

        private void Update()
        {
            if (_curKey != null && Time.unscaledTimeAsDouble >= _hideAtSec)
                TryDismiss();
        }

        private void OnDisable()
        {
            Clear();
        }

        //============================================================
        // Logic
        //============================================================
        public bool TryEnqueue(string key, string message, float durationSec)
        {
            string normalizedKey = key?.Trim();
            if (!isActiveAndEnabled || string.IsNullOrEmpty(normalizedKey) || string.IsNullOrWhiteSpace(message) || float.IsNaN(durationSec) || float.IsInfinity(durationSec) || durationSec <= 0.0f || normalizedKey == _curKey || _requests.Count >= _maxPendingCnt)
                return false;

            foreach (ToastRequest request in _requests)
            {
                if (request.Key == normalizedKey)
                    return false;
            }

            _requests.Enqueue(new ToastRequest(normalizedKey, message, durationSec));
            if (_curKey == null)
                ShowNext();

            return true;
        }

        public bool TryDismiss()
        {
            if (_curKey == null)
                return false;

            string key = _curKey;
            _curKey = null;
            _cgToast.alpha = 0.0f;
            _txtToast.text = string.Empty;
            _onDismissed?.Invoke(key);
            if (_curKey == null && isActiveAndEnabled && _requests.Count > 0)
                ShowNext();

            return true;
        }

        public void Clear()
        {
            _requests.Clear();
            string key = _curKey;
            _curKey = null;
            _cgToast.alpha = 0.0f;
            _txtToast.text = string.Empty;
            if (key != null)
                _onDismissed?.Invoke(key);
        }

        private void ShowNext()
        {
            ToastRequest request = _requests.Dequeue();
            _curKey = request.Key;
            _hideAtSec = Time.unscaledTimeAsDouble + request.DurationSec;
            _txtToast.text = request.Message;
            _cgToast.alpha = 1.0f;
            _onShown?.Invoke(request.Key);
        }

        //============================================================
        // Nested Types
        //============================================================
        private class ToastRequest
        {
            public string Key { get; }
            public string Message { get; }
            public float DurationSec { get; }

            public ToastRequest(string key, string message, float durationSec)
            {
                Key = key;
                Message = message;
                DurationSec = durationSec;
            }
        }
    }
}
