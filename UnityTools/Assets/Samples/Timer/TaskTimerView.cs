using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UnityTools.Samples.Timer
{
    public class TaskTimerView : MonoBehaviour
    {
        //============================================================
        //Inspector Fields
        //============================================================
        [SerializeField] private double _duration;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI _txtState;
        [SerializeField] private TextMeshProUGUI _txtTimer;

        [Header("Buttons")]
        [SerializeField] private Button _btnStart;
        [SerializeField] private Button _btnReduce1min;
        [SerializeField] private Button _btnComplete;
        [SerializeField] private Button _btnClaim;

        //============================================================
        //Events
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
        //Properties
        //============================================================
        public double Duration => _duration;

        //============================================================
        //Unity Methods
        //============================================================
        private void Awake()
        {
            _btnStart.onClick.AddListener(OnStartButtonClicked);
            _btnComplete.onClick.AddListener(OnCompleteButtonClicked);
            _btnClaim.onClick.AddListener(OnClaimButtonClicked);
            _btnReduce1min.onClick.AddListener(OnReduceButtonClicked);
        }

        private void OnDestroy()
        {
            _btnStart.onClick.RemoveListener(OnStartButtonClicked);
            _btnComplete.onClick.RemoveListener(OnCompleteButtonClicked);
            _btnClaim.onClick.RemoveListener(OnClaimButtonClicked);
            _btnReduce1min.onClick.RemoveListener(OnReduceButtonClicked);
        }

        //============================================================
        //Logic
        //============================================================
        public void SetState(string state)
        {
            if(string.IsNullOrWhiteSpace(state))
                return;

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
            if(string.IsNullOrWhiteSpace(time))
                return;

            _txtTimer.text = time;
        }

        //============================================================
        //Callbacks
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
