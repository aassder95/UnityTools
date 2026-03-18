using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace UnityTools.Util
{
    public enum ETaskTimerType
    {
        None,
        Processing,
        Completed
    }

    public class TaskTimer
    {
        //============================================================
        //Constants
        //============================================================
        private const double DEFAULT_DURATION_SEC = 1d;

        //============================================================
        //Readonly
        //============================================================
        private readonly string _id;
        private readonly StateMachine<ETaskTimerType> _fsm;
        private readonly MonoBehaviour _runner;
        private readonly TaskTimerPersistence _persistence;

        //============================================================
        //Fields
        //============================================================
        private bool _isInit;
        private double _durationSec;
        private DateTime _startTime;
        private DateTime _updatedTime;
        private int _savedStateType;
        private Coroutine _coUpdate;

        //============================================================
        //Events
        //============================================================
        public event UnityAction OnProgressStarted { add => _onProgressStarted += value; remove => _onProgressStarted -= value; }
        public event UnityAction<int> OnRemainSecUpdated { add => _onRemainSecUpdated += value; remove => _onRemainSecUpdated -= value; }
        public event UnityAction OnCompleted { add => _onCompleted += value; remove => _onCompleted -= value; }
        public event UnityAction OnClaimed { add => _onClaimed += value; remove => _onClaimed -= value; }
        public event UnityAction<ETaskTimerType, ETaskTimerType> OnStateTransition { add => _fsm.OnStateTransition += value; remove => _fsm.OnStateTransition -= value; }
        private event UnityAction _onProgressStarted;
        private event UnityAction<int> _onRemainSecUpdated;
        private event UnityAction _onCompleted;
        private event UnityAction _onClaimed;

        //============================================================
        //Properties
        //============================================================
        public string Id => _id;
        public StateMachine<ETaskTimerType> FSM => _fsm;
        public int RemainingSec => DateTimeUtils.GetRemainingSeconds(EndTime);
        public int DurationSec => (int)_durationSec;
        public bool IsClaimed => _persistence.IsClaimed();
        public bool IsPeriodExpired => DateTimeUtils.CompareWithoutMilliseconds(DateTime.UtcNow, EndTime) >= 0;
        public DateTime EndTime => _startTime.AddSeconds(_durationSec);
        private bool IsTampered => DateTimeUtils.CompareWithoutMilliseconds(DateTime.UtcNow, _updatedTime) < 0;

        //============================================================
        //Constructors
        //============================================================
        public TaskTimer(string id, MonoBehaviour runner)
        {
            if(!TaskTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
                normalizedId = string.Empty;

            _id = normalizedId;
            _runner = runner;
            _persistence = new TaskTimerPersistence(_id);
            _fsm = new StateMachine<ETaskTimerType>();

            if(!_fsm.Add(ETaskTimerType.None, new TaskTimerStates.NoneState(this)))
                DebugLogger.LogWarning($"상태 등록 실패: {ETaskTimerType.None}");
            if(!_fsm.Add(ETaskTimerType.Processing, new TaskTimerStates.ProcessingState(this)))
                DebugLogger.LogWarning($"상태 등록 실패: {ETaskTimerType.Processing}");
            if(!_fsm.Add(ETaskTimerType.Completed, new TaskTimerStates.CompletedState(this)))
                DebugLogger.LogWarning($"상태 등록 실패: {ETaskTimerType.Completed}");
        }

        //============================================================
        //Init/Register
        //============================================================
        public void Init()
        {
            if(string.IsNullOrEmpty(_id))
            {
                DebugLogger.LogWarning("유효하지 않은 ID로 초기화를 무시합니다.");
                return;
            }

            if(_runner == null)
            {
                DebugLogger.LogWarning("러너 참조가 비어 있어 초기화를 무시합니다.");
                return;
            }

            if(_isInit)
                return;

            Load();
            _isInit = true;
            Refresh();
        }

        public void Release()
        {
            if(!_isInit)
                return;

            StopUpdate();
            _isInit = false;
        }

        //============================================================
        //Persistence
        //============================================================
        private void Save()
        {
            _updatedTime = DateTimeUtils.RemoveMilliseconds(DateTime.UtcNow);
            _persistence.Save(_startTime, _durationSec, _fsm.CurType, _updatedTime);
        }

        private void Load()
        {
            TaskTimerStorageSnapshot snapshot = _persistence.Load();
            _startTime = snapshot.StartTime;
            _durationSec = snapshot.DurationSec;
            _updatedTime = snapshot.UpdatedTime;
            _savedStateType = snapshot.SavedStateType;
        }

        private void Clear()
        {
            _persistence.ClearRuntimeData();
            _startTime = DateTime.MinValue;
            _durationSec = 0d;
            _savedStateType = 0;
        }

        //============================================================
        //Logic
        //============================================================
        public void Refresh()
        {
            ETaskTimerType type = LoadStateType();
            if(_startTime == DateTime.MinValue || type == ETaskTimerType.None)
            {
                StopUpdate();
                if(!_fsm.HasCurrentState || _fsm.CurType != ETaskTimerType.None)
                    TryChangeState(ETaskTimerType.None, false, "Refresh");

                return;
            }

            switch (type)
            {
                case ETaskTimerType.Processing:
                    if(IsTampered)
                    {
                        double adjustSec = (_updatedTime - DateTime.UtcNow).TotalSeconds;
                        _durationSec += adjustSec;
                        _persistence.SaveDuration(_durationSec);
                    }

                    if(IsPeriodExpired)
                    {
                        UpdateCompletionTime();
                        return;
                    }

                    if(!_fsm.HasCurrentState || _fsm.CurType != ETaskTimerType.Processing)
                        TryChangeState(ETaskTimerType.Processing, false, "Refresh");

                    StartUpdate();
                    break;

                case ETaskTimerType.Completed:
                    StopUpdate();
                    if(!_fsm.HasCurrentState || _fsm.CurType != ETaskTimerType.Completed)
                        TryChangeState(ETaskTimerType.Completed, false, "Refresh");
                    break;

                default:
                    StopUpdate();
                    if(!_fsm.HasCurrentState || _fsm.CurType != ETaskTimerType.None)
                        TryChangeState(ETaskTimerType.None, false, "Refresh");
                    break;
            }
        }

        public bool Start(double durationSec)
        {
            if(!_isInit || _fsm.CurType == ETaskTimerType.Processing)
                return false;

            _startTime = DateTimeUtils.RemoveMilliseconds(DateTime.UtcNow);
            double safeDurationSec = SanitizeDuration(durationSec);
            if(_updatedTime != DateTime.MinValue && IsTampered)
            {
                double adjustSec = (_updatedTime - DateTime.UtcNow).TotalSeconds;
                _durationSec = safeDurationSec + adjustSec;
            }
            else
            {
                _durationSec = safeDurationSec;
            }

            if(!TryChangeState(ETaskTimerType.Processing, false, "Start"))
                return false;

            Save();
            StartUpdate();
            return true;
        }

        public bool Reduce(double reduceSec)
        {
            if(!_isInit || _fsm.CurType != ETaskTimerType.Processing)
                return false;

            if(reduceSec <= 0d || double.IsNaN(reduceSec) || double.IsInfinity(reduceSec))
                return false;

            double remainSec = (EndTime - DateTime.UtcNow).TotalSeconds;
            double actualReduceSec = Math.Min(reduceSec, remainSec);
            if(actualReduceSec <= 0d)
                return false;

            _startTime = _startTime.AddSeconds(-actualReduceSec);
            Save();

            _onRemainSecUpdated?.Invoke(RemainingSec);

            if(IsPeriodExpired)
                UpdateCompletionTime();

            return true;
        }

        public bool CompleteImmediately()
        {
            if(!_isInit || _fsm.CurType != ETaskTimerType.Processing)
                return false;

            UpdateCompletionTime();
            return true;
        }

        public bool Claim()
        {
            if(!_isInit || _fsm.CurType != ETaskTimerType.Completed)
                return false;

            Clear();
            if(!TryChangeState(ETaskTimerType.None, false, "Claim"))
                return false;

            _onClaimed?.Invoke();
            return true;
        }

        public void UpdateCompletionTime()
        {
            _updatedTime = DateTimeUtils.RemoveMilliseconds(DateTime.UtcNow);
            _persistence.SaveUpdated(_updatedTime);

            if(_fsm.CurType != ETaskTimerType.Completed)
            {
                if(!TryChangeState(ETaskTimerType.Completed, false, "UpdateCompletion"))
                    return;
            }

            _persistence.SaveState(ETaskTimerType.Completed);
        }

        //============================================================
        //Coroutines
        //============================================================
        private IEnumerator CoUpdate()
        {
            while (_isInit && _fsm.CurType == ETaskTimerType.Processing)
            {
                _fsm.Update();
                if(!_isInit || _fsm.CurType != ETaskTimerType.Processing)
                    yield break;

                yield return new WaitForSecondsRealtime(1f);
            }
        }

        private void StartUpdate()
        {
            StopUpdate();
            _coUpdate = _runner.StartCoroutine(CoUpdate());
        }

        private void StopUpdate()
        {
            if(_coUpdate == null)
                return;

            _runner.StopCoroutine(_coUpdate);
            _coUpdate = null;
        }

        //============================================================
        //Callbacks
        //============================================================
        public void NotifyUpdate()
        {
            if(_fsm.CurType == ETaskTimerType.Processing)
                _onRemainSecUpdated?.Invoke(RemainingSec);
        }

        public void NotifyProcessingStarted()
        {
            _onProgressStarted?.Invoke();
        }

        public void NotifyCompleted()
        {
            StopUpdate();
            _onCompleted?.Invoke();
        }

        public void NotifyCurType()
        {
            switch (_fsm.CurType)
            {
                case ETaskTimerType.Processing:
                    NotifyProcessingStarted();
                    NotifyUpdate();
                    break;
                case ETaskTimerType.Completed:
                    NotifyCompleted();
                    break;
            }
        }

        //============================================================
        //Utilities
        //============================================================
        public float GetProgress()
        {
            if(_startTime == DateTime.MinValue)
                return 0f;

            if(_fsm.CurType == ETaskTimerType.Completed || IsPeriodExpired)
                return 1f;

            int totalSec = (int)Math.Round(_durationSec);
            int elapsedSec = (int)Math.Round((DateTime.UtcNow - _startTime).TotalSeconds);
            return totalSec <= 0 ? 0f : Mathf.Clamp01((float)elapsedSec / totalSec);
        }

        public static bool IsClaimedStatic(string id)
        {
            if(!TaskTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
                return false;

            IStorage storage = new PlayerPrefsStorage();
            bool hasStart = storage.HasKey(TaskTimerStorageKeys.Start(normalizedId));
            bool hasDuration = storage.HasKey(TaskTimerStorageKeys.Duration(normalizedId));
            bool hasState = storage.HasKey(TaskTimerStorageKeys.State(normalizedId));
            bool hasUpdated = storage.HasKey(TaskTimerStorageKeys.Updated(normalizedId));
            return !hasStart && !hasDuration && !hasState && hasUpdated;
        }

        private ETaskTimerType LoadStateType()
        {
            ETaskTimerType type = (ETaskTimerType)_savedStateType;
            if(_fsm.HasState(type))
                return type;

            if(_savedStateType != 0)
                DebugLogger.LogWarning($"유효하지 않은 저장 상태값입니다: {_savedStateType}");
            return ETaskTimerType.None;
        }

        private double SanitizeDuration(double durationSec)
        {
            if(durationSec > 0d && !double.IsNaN(durationSec) && !double.IsInfinity(durationSec))
                return durationSec;

            DebugLogger.LogWarning($"유효하지 않은 지속시간 값입니다: durationSec={durationSec}, 기본값 {DEFAULT_DURATION_SEC}초를 적용합니다.");
            return DEFAULT_DURATION_SEC;
        }

        private bool TryChangeState(ETaskTimerType type, bool isUpdate = false, string method = "TryChangeState")
        {
            if(!_fsm.HasState(type))
            {
                StateTransitionLogUtils.LogMissingState(method, type);
                return false;
            }

            if(!_fsm.HasCurrentState)
            {
                if(_fsm.SetInitialState(type, isUpdate))
                    return true;

                StateTransitionLogUtils.LogInitialSetFailed(method, type);
                return false;
            }

            if(_fsm.Change(type, isUpdate))
                return true;

            StateTransitionLogUtils.LogTransitionFailed(method, _fsm.CurType, type);
            return false;
        }
    }

}
