using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace UnityTools.UI
{
    public class TimerView : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _txtState;
        [SerializeField] TextMeshProUGUI _txtSubState;
        [SerializeField] TextMeshProUGUI _txtCur;
        [SerializeField] TextMeshProUGUI _txtLoop;
        [SerializeField] TextMeshProUGUI _txtOpen;
        [SerializeField] TextMeshProUGUI _txtClosed;
        [SerializeField] TextMeshProUGUI _txtOpenStart;
        [SerializeField] TextMeshProUGUI _txtOpenUpdated;

        public event UnityAction OnForceOpen;
        public event UnityAction OnForceClosed;

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

        public void SetTimer(DateTime cur, DateTime openStart, DateTime openEnd, DateTime closedEnd)
        {
            _txtCur.SetText($"cur: {cur}");
            _txtOpen.SetText($"open: {openEnd}");
            _txtClosed.SetText($"closed: {closedEnd}");
            _txtOpenStart.SetText($"start: {openStart}");
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