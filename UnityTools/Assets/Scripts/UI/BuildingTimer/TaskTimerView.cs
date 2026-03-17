using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UnityTools.UI
{
    public class TaskTimerView : MonoBehaviour
    {
        //============================================================
        //Inspector Fields
        //============================================================
        [SerializeField] private double _duration;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI _txtId;
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
            _btnStart.onClick.AddListener(() => _onStartClicked?.Invoke());
            _btnComplete.onClick.AddListener(() => _onCompleteClicked?.Invoke());
            _btnClaim.onClick.AddListener(() => _onClaimClicked?.Invoke());
            _btnReduce1min.onClick.AddListener(() => _onReduce1MinClicked?.Invoke());
        }

        //============================================================
        //Logic
        //============================================================
        public void SetState(string state)
        {
            _txtState.text = $"State: {state}";
            _txtState.color = state switch
            {
                "Processing" => Color.yellow,
                "Completed" => Color.green,
                _ => Color.white
            };
        }

        public void SetBtnActive(bool start, bool complete, bool claim, bool reduce)
        {
            _btnStart.gameObject.SetActive(start);
            _btnComplete.gameObject.SetActive(complete);
            _btnClaim.gameObject.SetActive(claim);
            _btnReduce1min.gameObject.SetActive(reduce);
        }

        public void SetId(string id)
        {
            _txtId.text = $"Task: {id}";
        }

        public void SetTimer(string time)
        {
            _txtTimer.text = time;
        }
    }
}
