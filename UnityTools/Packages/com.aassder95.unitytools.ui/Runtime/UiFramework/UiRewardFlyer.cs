using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Ui
{
    [RequireComponent(typeof(RectTransform))]
    public class UiRewardFlyer : MonoBehaviour
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly List<IconState> _icons = new List<IconState>();

        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Presentation")]
        [SerializeField] private RectTransform _rtIconPrefab;
        [SerializeField] private UiRewardMotion _motion;
        [Tooltip("이 연출 레이어의 Canvas 카메라. Screen Space Overlay에서는 비워 둡니다.")]
        [SerializeField] private Camera _camLayer;
        [Header("Capacity")]
        [SerializeField, Min(1)] private int _maxIconCnt = 32;

        //============================================================
        // Fields
        //============================================================
        private RectTransform _rtLayer;
        private Transform _trTarget;
        private Camera _camTarget;
        private Vector2 _startPos;
        private int _requestedCnt;
        private int _arrivedCnt;
        private int _generation;
        private bool _isPlaying;
        private bool _hasTargetCamera;
        private bool _hasLayerCamera;

        //============================================================
        // Events
        //============================================================
        private event Action<int> _onArrived;
        private event Action _onCompleted;
        private event Action _onCancelled;
        public event Action<int> OnArrived { add => _onArrived += value; remove => _onArrived -= value; }
        public event Action OnCompleted { add => _onCompleted += value; remove => _onCompleted -= value; }
        public event Action OnCancelled { add => _onCancelled += value; remove => _onCancelled -= value; }

        //============================================================
        // Properties
        //============================================================
        public bool IsPlaying => _isPlaying;
        public int RequestedCnt => _requestedCnt;
        public int ArrivedCnt => _arrivedCnt;

        //============================================================
        // Unity Methods
        //============================================================
        private void Awake()
        {
            _rtLayer = GetComponent<RectTransform>();
        }

        private void OnDisable()
        {
            Cancel();
        }

        private void OnDestroy()
        {
            Cancel();
            for (int idx = 0; idx < _icons.Count; idx++)
            {
                Destroy(_icons[idx].Rect.gameObject);
            }

            _icons.Clear();
        }

        private void Update()
        {
            if (!_isPlaying)
                return;

            if (_trTarget == null || !_trTarget.gameObject.activeInHierarchy || (_hasTargetCamera && _camTarget == null) || (_hasLayerCamera && _camLayer == null))
            {
                Cancel();
                return;
            }

            if (!TryProject(_trTarget.position, _camTarget, out Vector2 targetPos))
            {
                Cancel();
                return;
            }

            float deltaSec = _motion.IsUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            int prevArrivedCnt = _arrivedCnt;
            float flyStartSec = _motion.SpreadDurationSec + _motion.WaitDurationSec;
            float endSec = flyStartSec + _motion.FlyDurationSec;
            for (int idx = 0; idx < _requestedCnt; idx++)
            {
                IconState icon = _icons[idx];
                if (!icon.IsActive)
                    continue;

                icon.ElapsedSec += deltaSec;
                float elapsedSec = icon.ElapsedSec - idx * _motion.StaggerSec;
                if (elapsedSec < 0.0f)
                    continue;

                icon.Rect.gameObject.SetActive(true);
                if (elapsedSec >= endSec)
                {
                    icon.Rect.localPosition = targetPos;
                    icon.Rect.localScale = Vector3.one * _motion.EndScale;
                    icon.IsActive = false;
                    icon.Rect.gameObject.SetActive(false);
                    _arrivedCnt++;
                }
                else if (elapsedSec < _motion.SpreadDurationSec)
                {
                    float ratio = elapsedSec / _motion.SpreadDurationSec;
                    float smoothRatio = ratio * ratio * (3.0f - 2.0f * ratio);
                    icon.Rect.localPosition = Vector2.Lerp(_startPos, icon.SpreadPos, smoothRatio);
                    icon.Rect.localScale = Vector3.one * Mathf.Lerp(_motion.StartScale, _motion.SpreadScale, smoothRatio);
                }
                else if (elapsedSec < flyStartSec)
                {
                    icon.Rect.localPosition = icon.SpreadPos;
                    icon.Rect.localScale = Vector3.one * _motion.SpreadScale;
                }
                else
                {
                    float ratio = (elapsedSec - flyStartSec) / _motion.FlyDurationSec;
                    float smoothRatio = ratio * ratio * (3.0f - 2.0f * ratio);
                    icon.Rect.localPosition = Vector2.Lerp(icon.SpreadPos, targetPos, smoothRatio);
                    icon.Rect.localScale = Vector3.one * Mathf.Lerp(_motion.SpreadScale, _motion.EndScale, smoothRatio);
                }
            }

            int generation = _generation;
            if (_arrivedCnt != prevArrivedCnt)
                _onArrived?.Invoke(_arrivedCnt);

            // 도착 알림에서 취소하거나 새 연출을 시작했다면 이전 완료를 통지하지 않습니다.
            if (_generation != generation || !_isPlaying || _arrivedCnt != _requestedCnt)
                return;

            _isPlaying = false;
            _trTarget = null;
            _camTarget = null;
            _onCompleted?.Invoke();
        }

        //============================================================
        // Logic
        //============================================================
        public bool TryPlay(Transform trSource, Camera camSource, Transform trTarget, Camera camTarget, int iconCnt)
        {
            if (!isActiveAndEnabled || _isPlaying || trSource == null || trTarget == null || !trTarget.gameObject.activeInHierarchy || iconCnt <= 0 || iconCnt > _maxIconCnt)
                return false;

            if (!TryProject(trSource.position, camSource, out Vector2 startPos) || !TryProject(trTarget.position, camTarget, out _))
                return false;

            EnsureIcons(iconCnt);
            _generation++;
            _requestedCnt = iconCnt;
            _arrivedCnt = 0;
            _startPos = startPos;
            _trTarget = trTarget;
            _camTarget = camTarget;
            _hasTargetCamera = camTarget != null;
            _hasLayerCamera = _camLayer != null;
            _isPlaying = true;
            for (int idx = 0; idx < iconCnt; idx++)
            {
                IconState icon = _icons[idx];
                icon.SpreadPos = startPos + UnityEngine.Random.insideUnitCircle * _motion.SpreadRadius;
                icon.ElapsedSec = 0.0f;
                icon.IsActive = true;
                icon.Rect.localPosition = startPos;
                icon.Rect.localRotation = Quaternion.identity;
                icon.Rect.localScale = Vector3.one * _motion.StartScale;
                icon.Rect.gameObject.SetActive(idx == 0 || _motion.StaggerSec == 0.0f);
            }

            return true;
        }

        public void Prewarm()
        {
            EnsureIcons(_maxIconCnt);
        }

        public void Cancel()
        {
            if (!_isPlaying)
                return;

            _generation++;
            _isPlaying = false;
            _trTarget = null;
            _camTarget = null;
            for (int idx = 0; idx < _requestedCnt; idx++)
            {
                _icons[idx].IsActive = false;
                _icons[idx].Rect.gameObject.SetActive(false);
            }

            _onCancelled?.Invoke();
        }

        //============================================================
        // Utilities
        //============================================================
        private bool TryProject(Vector3 worldPos, Camera camSource, out Vector2 localPos)
        {
            localPos = default;
            Vector3 screenPos = camSource == null ? worldPos : camSource.WorldToScreenPoint(worldPos);
            if ((camSource != null && screenPos.z <= 0.0f) || float.IsNaN(screenPos.x) || float.IsInfinity(screenPos.x) || float.IsNaN(screenPos.y) || float.IsInfinity(screenPos.y))
                return false;

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(_rtLayer, screenPos, _camLayer, out localPos);
        }

        private void EnsureIcons(int iconCnt)
        {
            for (int idx = _icons.Count; idx < iconCnt; idx++)
            {
                RectTransform rtIcon = Instantiate(_rtIconPrefab, _rtLayer, false);
                rtIcon.gameObject.SetActive(false);
                _icons.Add(new IconState(rtIcon));
            }
        }

        //============================================================
        // Nested Types
        //============================================================
        private class IconState
        {
            private readonly RectTransform _rtIcon;
            public RectTransform Rect => _rtIcon;
            public Vector2 SpreadPos { get; set; }
            public float ElapsedSec { get; set; }
            public bool IsActive { get; set; }

            public IconState(RectTransform rtIcon)
            {
                _rtIcon = rtIcon;
            }
        }
    }
}
