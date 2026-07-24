using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTools.Ui;

namespace UnityTools.Samples.Timer
{
    public class TaskTimerView : MonoBehaviour
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Timer")]
        [SerializeField, Min(0.0001f)] private double _durationSec;
        [Header("Info")]
        [SerializeField] private TextMeshProUGUI _txtState;
        [SerializeField] private TextMeshProUGUI _txtTimer;
        [Header("Buttons")]
        [SerializeField] private Button _btnStart;
        [SerializeField] private Button _btnReduce1Min;
        [SerializeField] private Button _btnComplete;
        [SerializeField] private Button _btnClaim;

        //============================================================
        // Events
        //============================================================
        public event Action OnStartClicked { add => _onStartClicked += value; remove => _onStartClicked -= value; }
        public event Action OnCompleteClicked { add => _onCompleteClicked += value; remove => _onCompleteClicked -= value; }
        public event Action OnClaimClicked { add => _onClaimClicked += value; remove => _onClaimClicked -= value; }
        public event Action OnReduce1MinClicked { add => _onReduce1MinClicked += value; remove => _onReduce1MinClicked -= value; }
        private event Action _onStartClicked;
        private event Action _onCompleteClicked;
        private event Action _onClaimClicked;
        private event Action _onReduce1MinClicked;

        //============================================================
        // Properties
        //============================================================
        public double DurationSec => _durationSec;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnEnable()
        {
            _btnStart.onClick.AddListener(OnStartButtonClicked);
            _btnComplete.onClick.AddListener(OnCompleteButtonClicked);
            _btnClaim.onClick.AddListener(OnClaimButtonClicked);
            _btnReduce1Min.onClick.AddListener(OnReduceButtonClicked);
        }

        private void OnDisable()
        {
            _btnStart.onClick.RemoveListener(OnStartButtonClicked);
            _btnComplete.onClick.RemoveListener(OnCompleteButtonClicked);
            _btnClaim.onClick.RemoveListener(OnClaimButtonClicked);
            _btnReduce1Min.onClick.RemoveListener(OnReduceButtonClicked);
        }

        //============================================================
        // Logic
        //============================================================
        public void SetState(string state)
        {
            if (string.IsNullOrWhiteSpace(state))
            {
                Debug.LogError("TaskTimerView에 표시할 상태 문자열이 비어 있습니다.", this);
                return;
            }

            _txtState.text = $"State: {state}";
            _txtState.color = state switch
            {
                "Processing" => Color.yellow,
                "Completed" => Color.green,
                _ => Color.white
            };
        }

        public void SetTimer(string time)
        {
            if (string.IsNullOrWhiteSpace(time))
            {
                Debug.LogError("TaskTimerView에 표시할 시간 문자열이 비어 있습니다.", this);
                return;
            }

            _txtTimer.text = time;
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OnStartButtonClicked()
        {
            _onStartClicked?.Invoke();
        }

        private void OnCompleteButtonClicked()
        {
            _onCompleteClicked?.Invoke();
        }

        private void OnClaimButtonClicked()
        {
            _onClaimClicked?.Invoke();
        }

        private void OnReduceButtonClicked()
        {
            _onReduce1MinClicked?.Invoke();
        }
    }
}
