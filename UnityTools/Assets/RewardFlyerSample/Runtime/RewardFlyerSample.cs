using UnityEngine;
using UnityEngine.UI;

namespace UnityTools.Ui.Samples.Rewards
{
    public class RewardFlyerSample : MonoBehaviour
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Reward")]
        [SerializeField] private UiRewardFlyer _flyer;
        [SerializeField] private RectTransform _rtSource;
        [SerializeField] private RectTransform _rtTarget;
        [SerializeField, Min(1)] private int _iconCnt = 12;
        [Header("Controls")]
        [SerializeField] private Button _btnPlay;
        [SerializeField] private Button _btnCancel;
        [SerializeField] private Button _btnMove;
        [SerializeField] private Text _txtStatus;
        [Header("Target Motion")]
        [SerializeField] private Vector2 _targetOriginPos = new Vector2(330.0f, 150.0f);
        [SerializeField, Min(0.0f)] private float _travelDist = 120.0f;
        [SerializeField, Min(0.0f)] private float _speed = 1.5f;

        //============================================================
        // Fields
        //============================================================
        private bool _isTargetMoving;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnEnable()
        {
            _btnPlay.onClick.AddListener(PlayReward);
            _btnCancel.onClick.AddListener(CancelReward);
            _btnMove.onClick.AddListener(ToggleTargetMotion);
            _flyer.OnArrived += HandleArrived;
            _flyer.OnCompleted += HandleCompleted;
            _flyer.OnCancelled += HandleCancelled;
        }

        private void OnDisable()
        {
            _btnPlay.onClick.RemoveListener(PlayReward);
            _btnCancel.onClick.RemoveListener(CancelReward);
            _btnMove.onClick.RemoveListener(ToggleTargetMotion);
            _flyer.OnArrived -= HandleArrived;
            _flyer.OnCompleted -= HandleCompleted;
            _flyer.OnCancelled -= HandleCancelled;
            _flyer.Cancel();
        }

        private void Update()
        {
            if (!_isTargetMoving)
                return;

            _rtTarget.anchoredPosition = _targetOriginPos + Vector2.right * (Mathf.Sin(Time.unscaledTime * _speed) * _travelDist);
        }

        //============================================================
        // Callbacks
        //============================================================
        private void PlayReward()
        {
            _flyer.Prewarm();
            bool isStarted = _flyer.TryPlay(_rtSource, null, _rtTarget, null, _iconCnt);
            _txtStatus.text = isStarted ? "Flying: 0 / " + _iconCnt : "A reward is already flying.";
        }

        private void CancelReward()
        {
            _flyer.Cancel();
        }

        private void ToggleTargetMotion()
        {
            _isTargetMoving = !_isTargetMoving;
            if (!_isTargetMoving)
                _rtTarget.anchoredPosition = _targetOriginPos;
        }

        private void HandleArrived(int arrivedCnt)
        {
            _txtStatus.text = "Arrived: " + arrivedCnt + " / " + _flyer.RequestedCnt;
        }

        private void HandleCompleted()
        {
            _txtStatus.text = "Complete. Icons are ready for reuse.";
        }

        private void HandleCancelled()
        {
            _txtStatus.text = "Cancelled. No completion was reported.";
        }
    }
}
