using System.Collections;
using UnityEngine;

namespace UnityTools.Ui
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UiCanvasTransition : MonoBehaviour, IUiInteractionControl
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField, Min(0.0f)] private float _showDurationSec = 0.2f;
        [SerializeField, Min(0.0f)] private float _hideDurationSec = 0.15f;

        //============================================================
        // Fields
        //============================================================
        private CanvasGroup _cgTarget;
        private Coroutine _coFade;
        private bool _isVisible;
        private bool _isInteractionEnabled = true;

        //============================================================
        // Properties
        //============================================================
        public bool IsVisible => _isVisible;
        public bool IsInteractionEnabled => _isInteractionEnabled;

        //============================================================
        // Unity Methods
        //============================================================
        private void Awake()
        {
            _cgTarget = GetComponent<CanvasGroup>();
            _isVisible = gameObject.activeSelf && _cgTarget.alpha > 0.0f;
            ApplyInteraction(_isVisible && _isInteractionEnabled);
        }

        private void OnDestroy()
        {
            StopFade();
        }

        //============================================================
        // Logic
        //============================================================
        public void Show()
        {
            StopFade();
            gameObject.SetActive(true);
            Prepare();
            _isVisible = true;
            ApplyInteraction(false);

            if (_showDurationSec <= 0.0f)
            {
                _cgTarget.alpha = 1.0f;
                ApplyInteraction(_isInteractionEnabled);
                return;
            }

            _coFade = StartCoroutine(CoFade(1.0f, _showDurationSec, false));
        }

        public void Hide()
        {
            if (!gameObject.activeSelf)
            {
                _isVisible = false;
                return;
            }

            StopFade();
            Prepare();
            _isVisible = false;
            ApplyInteraction(false);

            if (_hideDurationSec <= 0.0f)
            {
                _cgTarget.alpha = 0.0f;
                gameObject.SetActive(false);
                return;
            }

            _coFade = StartCoroutine(CoFade(0.0f, _hideDurationSec, true));
        }

        public void SetInteractionEnabled(bool isEnabled)
        {
            _isInteractionEnabled = isEnabled;
            ApplyInteraction(_coFade == null && _isVisible && isEnabled);
        }

        private void Prepare()
        {
            if (_cgTarget == null)
                _cgTarget = GetComponent<CanvasGroup>();
        }

        private void StopFade()
        {
            if (_coFade == null)
                return;

            StopCoroutine(_coFade);
            _coFade = null;
        }

        private void ApplyInteraction(bool isEnabled)
        {
            if (_cgTarget == null)
                return;

            _cgTarget.interactable = isEnabled;
            _cgTarget.blocksRaycasts = isEnabled;
        }

        //============================================================
        // Coroutines
        //============================================================
        private IEnumerator CoFade(float targetAlpha, float durationSec, bool shouldDeactivate)
        {
            float startAlpha = _cgTarget.alpha;
            float elapsedSec = 0.0f;

            while (elapsedSec < durationSec)
            {
                elapsedSec += Time.unscaledDeltaTime;
                float ratio = Mathf.Clamp01(elapsedSec / durationSec);
                float smoothRatio = ratio * ratio * (3.0f - (2.0f * ratio));
                _cgTarget.alpha = Mathf.Lerp(startAlpha, targetAlpha, smoothRatio);
                yield return null;
            }

            _cgTarget.alpha = targetAlpha;
            _coFade = null;
            if (shouldDeactivate)
            {
                gameObject.SetActive(false);
                yield break;
            }

            ApplyInteraction(_isInteractionEnabled);
        }
    }
}
