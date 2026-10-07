using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityTools.Timer;
using UnityTools.Timer.Period;
using UnityTools.Timer.Persistence;
using UnityTools.Timer.Task;
using UnityTools.Ui;

namespace UnityTools.TimerDashboard
{
    public class TimerDashboardController : MonoBehaviour
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Dashboard View")]
        [SerializeField] private Text _txtTimers;
        [SerializeField] private Text _txtStatus;
        [SerializeField] private InputField _inputId;
        [SerializeField] private UiToastQueue _toast;
        [Header("Actions")]
        [SerializeField] private Button[] _btnActions;
        [Header("Timer Fixture")]
        [SerializeField, Min(1.0f)] private float _durationSec = 60.0f;
        [SerializeField] private string _periodId = "SHOP";
        [SerializeField, Min(0.01f)] private double _openPeriodMin = 1.0d;
        [SerializeField, Min(0.01f)] private double _closedPeriodMin = 2.0d;

        //============================================================
        // Fields
        //============================================================
        private TaskTimerService _taskTimers;
        private PeriodTimerService _periodTimers;
        private PeriodTimerHandle _period;
        private ActionBinding[] _bindings;
        private Coroutine _coRender;
        private bool _isInit;

        //============================================================
        // Properties
        //============================================================
        public bool IsInitialized => _isInit;
        public TaskTimerService TaskTimers => _taskTimers;
        public PeriodTimerService PeriodTimers => _periodTimers;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnEnable()
        {
            if (_bindings == null)
            {
                _bindings = new ActionBinding[_btnActions.Length];
                for (int idx = 0; idx < _bindings.Length; ++idx)
                {
                    _bindings[idx] = new ActionBinding(this, (ETimerDashboardAction)idx);
                }
            }

            for (int idx = 0; idx < _btnActions.Length; ++idx)
            {
                _btnActions[idx].onClick.AddListener(_bindings[idx].Invoke);
            }
        }

        private void OnDisable()
        {
            for (int idx = 0; idx < _btnActions.Length; ++idx)
            {
                _btnActions[idx].onClick.RemoveListener(_bindings[idx].Invoke);
            }

            Release();
        }

        //============================================================
        // Init/Register
        //============================================================
        public bool TryInit(IStorage storage, Func<DateTime> utcNow)
        {
            if (!isActiveAndEnabled || storage == null || utcNow == null)
                return false;

            Release();
            _taskTimers = new TaskTimerService(this, storage, utcNow);
            _periodTimers = new PeriodTimerService(this, storage, utcNow);
            if (!_periodTimers.TryCreate(_periodId, out _period) || !_periodTimers.TryInit(_period, _openPeriodMin, _closedPeriodMin))
            {
                _taskTimers.Release();
                _periodTimers.Release();
                return false;
            }

            _isInit = true;
            _taskTimers.OnTimersChanged += OnTimersChanged;
            _periodTimers.OnTimersChanged += OnTimersChanged;
            OnTimersChanged();
            return true;
        }

        public void Release()
        {
            if (_coRender != null)
            {
                StopCoroutine(_coRender);
                _coRender = null;
            }

            if (!_isInit)
                return;

            _taskTimers.OnTimersChanged -= OnTimersChanged;
            _periodTimers.OnTimersChanged -= OnTimersChanged;
            _taskTimers.Release();
            _periodTimers.Release();
            _isInit = false;
        }

        //============================================================
        // Logic
        //============================================================
        public void Run(ETimerDashboardAction action)
        {
            string id = _inputId.text.Trim();
            bool isSucceeded = false;
            if (_isInit)
            {
                switch (action)
                {
                    case ETimerDashboardAction.Register:
                        isSucceeded = !_taskTimers.TryGetHandle(id, out _) && _taskTimers.TryCreate(id, out TaskTimerHandle handle) && _taskTimers.TryInit(handle);
                        break;
                    case ETimerDashboardAction.Start: isSucceeded = _taskTimers.TryStart(id, _durationSec); break;
                    case ETimerDashboardAction.Pause: isSucceeded = _taskTimers.TryPause(id); break;
                    case ETimerDashboardAction.Resume: isSucceeded = _taskTimers.TryResume(id); break;
                    case ETimerDashboardAction.Cancel: isSucceeded = _taskTimers.TryCancel(id); break;
                    case ETimerDashboardAction.Complete: isSucceeded = _taskTimers.TryComplete(id); break;
                    case ETimerDashboardAction.Claim: isSucceeded = _taskTimers.TryClaim(id); break;
                    case ETimerDashboardAction.Delete: isSucceeded = _taskTimers.TryDelete(id); break;
                    case ETimerDashboardAction.Unregister: isSucceeded = _taskTimers.TryUnregister(id); break;
                    case ETimerDashboardAction.OpenPeriod: isSucceeded = _period.TryForceOpen(); break;
                    case ETimerDashboardAction.ClosePeriod: isSucceeded = _period.TryForceClosed(); break;
                }
            }

            string message = action + " / " + id + (isSucceeded ? " : OK" : " : REJECTED (check ID, state or storage)");
            if (_toast.TryEnqueue(action + ":" + id + ":" + isSucceeded, message, 2.0f))
                _txtStatus.text = isSucceeded ? "Action accepted" : "Action rejected";
            else
                _txtStatus.text = message;
        }

        //============================================================
        // Coroutines
        //============================================================
        private IEnumerator CoRender()
        {
            yield return null;
            TaskTimerData[] tasks = _taskTimers.GetSnapshots();
            Array.Sort(tasks, CompareIds);
            StringBuilder text = new StringBuilder();
            text.Append("TASKS / ").Append(tasks.Length).AppendLine();
            for (int idx = 0; idx < tasks.Length; ++idx)
            {
                TaskTimerData data = tasks[idx];
                text.Append(data.Id).Append(" | ").Append(data.CurType).Append(" | ").Append(data.RemainingSec).Append(" sec | ").Append((data.Progress * 100.0f).ToString("F0")).AppendLine(" %");
            }

            text.AppendLine().AppendLine("PERIODS");
            PeriodTimerData[] periods = _periodTimers.GetSnapshots();
            for (int idx = 0; idx < periods.Length; ++idx)
            {
                PeriodTimerData data = periods[idx];
                text.Append(data.Id).Append(" | ").Append(data.CurType).Append(" | ").Append(data.RemainingSec).Append(" sec | ready=").Append(data.IsReady).AppendLine();
            }

            _txtTimers.text = text.ToString();
            _coRender = null;
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OnTimersChanged()
        {
            if (_coRender == null && _isInit)
                _coRender = StartCoroutine(CoRender());
        }

        //============================================================
        // Utilities
        //============================================================
        private static int CompareIds(TaskTimerData first, TaskTimerData second) => string.CompareOrdinal(first.Id, second.Id);

        //============================================================
        // Nested Types
        //============================================================
        private class ActionBinding
        {
            private readonly TimerDashboardController _owner;
            private readonly ETimerDashboardAction _action;

            public ActionBinding(TimerDashboardController owner, ETimerDashboardAction action)
            {
                _owner = owner;
                _action = action;
            }

            public void Invoke()
            {
                _owner.Run(_action);
            }
        }
    }
}
