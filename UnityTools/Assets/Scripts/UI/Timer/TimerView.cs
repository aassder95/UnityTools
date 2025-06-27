using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityTools.Model;
using UnityTools.Util;

namespace UnityTools.UI
{
    public class TimerView : MonoBehaviour, IView<TimerModel>
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

        public void InitView(TimerModel model)
        {
            UpdateView(model);
        }

        public void UpdateView(TimerModel model)
        {
            SetState(model.State, model.SubState);
            SetLoop(model.LoopMinutes, model.OpenUpdated);
            SetTimer(model.OpenStart, model.OpenUpdated, model.OpenEnd, model.ClosedEnd);
        }

        void Update()
        {
            _txtCur.SetText($"cur: {Utils.TrimMilliseconds(DateTime.UtcNow)}");
        }

        void SetState(string state, string subState)
        {
            _txtState.SetText(state);
            _txtSubState.SetText(subState);
        }

        void SetLoop(int min, DateTime openUpdated)
        {
            _txtLoop.SetText("({0})", min);
            _txtOpenUpdated.SetText($"updated: {openUpdated}");
        }

        void SetTimer(DateTime openStart, DateTime openUpdated, DateTime openEnd, DateTime closedEnd)
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