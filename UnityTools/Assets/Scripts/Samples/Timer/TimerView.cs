using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityTools.Util;
using UnityTools.Util.Constants;
using UnityTools.Util.Core.Collections;
using UnityTools.Util.Core.Events;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.Core.Persistence;
using UnityTools.Util.Core.Pooling;
using UnityTools.Util.Core.Singleton;
using UnityTools.Util.Core.State;
using UnityTools.Util.Core.Timer.Period;
using UnityTools.Util.Core.Timer.Shared;
using UnityTools.Util.Core.Timer.Task;
using UnityTools.Util.Coroutines;
using UnityTools.Util.Extensions;
using UnityTools.Util.UIFramework;
using UnityTools.Util.Utilities;

namespace UnityTools.Samples.Timer
{
    public class TimerView : BaseView<TimerModel>
    {
        //============================================================
        //Inspector Fields
        //============================================================
        [SerializeField] private TextMeshProUGUI _txtState;
        [SerializeField] private TextMeshProUGUI _txtSubState;
        [SerializeField] private TextMeshProUGUI _txtCur;
        [SerializeField] private TextMeshProUGUI _txtLoop;
        [SerializeField] private TextMeshProUGUI _txtOpenUpdated;
        [SerializeField] private TextMeshProUGUI _txtOpenEnd;
        [SerializeField] private TextMeshProUGUI _txtClosedEnd;

        //============================================================
        //Events
        //============================================================
        public event UnityAction OnForceOpen { add => _onForceOpen += value; remove => _onForceOpen -= value; }
        public event UnityAction OnForceClosed { add => _onForceClosed += value; remove => _onForceClosed -= value; }
        private event UnityAction _onForceOpen;
        private event UnityAction _onForceClosed;

        //============================================================
        //Unity Methods
        //============================================================
        private void Update()
        {
            DateTime currentUtcTime = DateTimeUtils.RemoveMilliseconds(DateTime.UtcNow);
            _txtCur.SetText("cur: {0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}",
                currentUtcTime.Year,
                currentUtcTime.Month,
                currentUtcTime.Day,
                currentUtcTime.Hour,
                currentUtcTime.Minute,
                currentUtcTime.Second);
        }

        //============================================================
        //Logic
        //============================================================
        protected override void OnRefresh(TimerModel model)
        {
            SetState(model.State, model.SubState);
            SetLoop(model.LoopMinutes, model.OpenUpdated);
            SetTimer(model.OpenUpdated, model.OpenEnd, model.ClosedEnd);
        }

        //============================================================
        //Callbacks
        //============================================================
        public void OnForceOpenInspector()
        {
            _onForceOpen?.Invoke();
        }

        public void OnForceClosedInspector()
        {
            _onForceClosed?.Invoke();
        }

        //============================================================
        //Utilities
        //============================================================
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

        private void SetTimer(DateTime openUpdated, DateTime openEnd, DateTime closedEnd)
        {
            _txtOpenUpdated.SetText($"updated: {openUpdated}");
            _txtOpenEnd.SetText($"open: {openEnd}");
            _txtClosedEnd.SetText($"closed: {closedEnd}");
        }
    }
}
