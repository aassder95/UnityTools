using System.Collections;
using System.Threading.Tasks;
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
        private TaskCompletionSource<EUiTransitionResult> _completion;
        private bool _isVisible;
        private bool _isInteractionEnabled = true;

        //============================================================
        // Properties
        //============================================================
        public bool IsTransitioning => _completion != null;
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

        private void OnDisable()
        {
            StopFade();
            _isVisible = false;
            if (_cgTarget != null)
                _cgTarget.alpha = 0.0f;

            ApplyInteraction(false);
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
            BeginTransition(true);
        }

        public void Hide()
        {
            BeginTransition(false);
        }

        public Task<EUiTransitionResult> ShowAsync()
        {
            return BeginTransition(true);
        }

        public Task<EUiTransitionResult> HideAsync()
        {
            return BeginTransition(false);
        }

        public void CancelTransition()
        {
            if (!IsTransitioning)
                return;

            bool isVisible = _isVisible;
            StopFade();
            _cgTarget.alpha = isVisible ? 1.0f : 0.0f;
            ApplyInteraction(isVisible && _isInteractionEnabled);
            if (!isVisible)
                gameObject.SetActive(false);
        }

        private Task<EUiTransitionResult> BeginTransition(bool shouldShow)
        {
            StopFade();
            if (shouldShow)
                gameObject.SetActive(true);

            if (!isActiveAndEnabled)
            {
                _isVisible = false;
                if (_cgTarget != null)
                    _cgTarget.alpha = 0.0f;

                ApplyInteraction(false);
                if (!shouldShow)
                    gameObject.SetActive(false);

                return Task.FromResult(shouldShow ? EUiTransitionResult.Cancelled : EUiTransitionResult.Completed);
            }

            _isVisible = shouldShow;
            ApplyInteraction(false);
            _completion = new TaskCompletionSource<EUiTransitionResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            Task<EUiTransitionResult> task = _completion.Task;
            float durationSec = shouldShow ? _showDurationSec : _hideDurationSec;
            if (durationSec <= 0.0f)
            {
                CompleteTransition(shouldShow);
                return task;
            }

            _coFade = StartCoroutine(CoFade(shouldShow ? 1.0f : 0.0f, durationSec, !shouldShow));
            return task;
        }

        private void CompleteTransition(bool shouldShow)
        {
            _cgTarget.alpha = shouldShow ? 1.0f : 0.0f;
            _coFade = null;
            TaskCompletionSource<EUiTransitionResult> completion = _completion;
            _completion = null;
            ApplyInteraction(shouldShow && _isInteractionEnabled);
            if (!shouldShow)
                gameObject.SetActive(false);

            completion.TrySetResult(EUiTransitionResult.Completed);
        }

        public void SetInteractionEnabled(bool isEnabled)
        {
            _isInteractionEnabled = isEnabled;
            ApplyInteraction(_coFade == null && _isVisible && isEnabled);
        }

        private void StopFade()
        {
            if (_coFade != null)
            {
                StopCoroutine(_coFade);
                _coFade = null;
            }

            TaskCompletionSource<EUiTransitionResult> completion = _completion;
            _completion = null;
            completion?.TrySetResult(EUiTransitionResult.Cancelled);
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

            CompleteTransition(!shouldDeactivate);
        }
    }
}
