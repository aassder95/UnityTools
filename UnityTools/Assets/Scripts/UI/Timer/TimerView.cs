using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityTools.Model;
using UnityTools.Util;

namespace UnityTools.UI
{
    public class TimerView : BaseView<TimerModel>
    {
        [SerializeField] private TextMeshProUGUI _txtState;
        [SerializeField] private TextMeshProUGUI _txtSubState;
        [SerializeField] private TextMeshProUGUI _txtCur;
        [SerializeField] private TextMeshProUGUI _txtLoop;
        [SerializeField] private TextMeshProUGUI _txtOpenStart;
        [SerializeField] private TextMeshProUGUI _txtOpenUpdated;
        [SerializeField] private TextMeshProUGUI _txtOpenEnd;
        [SerializeField] private TextMeshProUGUI _txtClosedEnd;

        public event UnityAction OnForceOpen;
        public event UnityAction OnForceClosed;
        
        public override void Refresh(TimerModel model)
        {
            SetState(model.State, model.SubState);
            SetLoop(model.LoopMinutes, model.OpenUpdated);
            SetTimer(model.OpenStart, model.OpenUpdated, model.OpenEnd, model.ClosedEnd);
        }

        private void Update()
        {
            _txtCur.SetText($"cur: {DateTimeUtils.RemoveMilliseconds(DateTime.UtcNow)}");
        }

        private void SetState(string state, string subState)
        {
            _txtState.SetText(state);
            _txtSubState.SetText(subState);
        }

        private void SetLoop(int min, DateTime openUpdated)
        {
            _txtLoop.SetText("({0})", min);
            _txtOpenUpdated.SetText($"updated: {openUpdated}");
        }

        private void SetTimer(DateTime openStart, DateTime openUpdated, DateTime openEnd, DateTime closedEnd)
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