using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UnityTools.UI
{
    public class TaskTimerView : MonoBehaviour
    {
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

        public double Duration => _duration;

        public event Action OnStartClicked;
        public event Action OnCompleteClicked;
        public event Action OnClaimClicked;
        public event Action OnReduce1MinClicked;

        private void Awake()
        {
            _btnStart.onClick.AddListener(() => OnStartClicked?.Invoke());
            _btnComplete.onClick.AddListener(() => OnCompleteClicked?.Invoke());
            _btnClaim.onClick.AddListener(() => OnClaimClicked?.Invoke());
            _btnReduce1min.onClick.AddListener(() => OnReduce1MinClicked?.Invoke());
        }

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
