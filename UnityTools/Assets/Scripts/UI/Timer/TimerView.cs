using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityTools.Util;

namespace UnityTools.UI
{
    public class TimerView : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _txtState;
        [SerializeField] TextMeshProUGUI _txtSubState;
        [SerializeField] TextMeshProUGUI _txtCur;
        [SerializeField] TextMeshProUGUI _txtLoop;
        [SerializeField] TextMeshProUGUI _txtOpenStart;
        [SerializeField] TextMeshProUGUI _txtOpenUpdated;
        [SerializeField] TextMeshProUGUI _txtOpenEnd;
        [SerializeField] TextMeshProUGUI _txtClosedEnd;

        public event UnityAction OnForceOpen;
        public event UnityAction OnForceClosed;

        void Update()
        {
            _txtCur.SetText($"cur: {Utils.TrimMilliseconds(DateTime.UtcNow)}");
        }

        public void SetState(string state)
        {
            _txtState.SetText(state);
        }

        public void SetSubState(string state)
        {
            _txtSubState.SetText(state);
        }

        public void SetLoop(int min, DateTime updated)
        {
            _txtLoop.SetText("({0})", min);
            _txtOpenUpdated.SetText($"updated: {updated}");
        }

        public void SetTimer(DateTime openStart, DateTime openUpdated, DateTime openEnd, DateTime closedEnd)
        {
            _txtOpenStart.SetText($"start: {openStart}");
            _txtOpenUpdated.SetText($"updated: {openUpdated}");
            _txtOpenEnd.SetText($"open: {openEnd}");
            _txtClosedEnd.SetText($"closed: {closedEnd}");
        }

        public void OnForceOpenInspector()
        {
            OnForceOpen?.Invoke();
        }

        public void OnForceClosedInspector()
        {
            OnForceClosed?.Invoke();
        }
    }
}