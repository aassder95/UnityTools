using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.Core.Persistence;
using UnityTools.Util.Core.State;
using UnityTools.Util.Utilities;

namespace UnityTools.Util.Core.Timer.Task
{
    public class TaskTimer : ITaskTimer
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly string _id;
        private readonly StateMachine<ETaskTimerType> _fsm;
        private readonly MonoBehaviour _runner;
        private readonly TaskTimerPersistence _persistence;

        //============================================================
        // Fields
        //============================================================
        private bool _isInit;
        private double _durationSec;
        private DateTime _startTime;
        private DateTime _updatedTime;
        private int _savedStateType;
        private Coroutine _coUpdate;

        //============================================================
        // Events
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
        // Properties
        //============================================================
        public string Id => _id;
        public StateMachine<ETaskTimerType> Fsm => _fsm;
        public ETaskTimerType CurType => _fsm.CurType;
        public int RemainingSec => DateTimeUtils.GetRemainingSec(EndTime);
        public int DurationSec => (int)_durationSec;
        public float Progress => CalculateProgress();
        public bool IsClaimed => _persistence.IsClaimed();
        public bool IsPeriodExpired => DateTimeUtils.CompareWithoutMs(DateTime.UtcNow, EndTime) >= 0;
        public DateTime EndTime => _startTime.AddSeconds(_durationSec);
        private bool IsTampered => DateTimeUtils.CompareWithoutMs(DateTime.UtcNow, _updatedTime) < 0;

        //============================================================
        // Constructors
        //============================================================
        private TaskTimer(string normalizedId, MonoBehaviour runner)
        {
            _id = normalizedId;
            _runner = runner;
            _persistence = new TaskTimerPersistence(_id);
            _fsm = new StateMachine<ETaskTimerType>();
        }

        //============================================================
        // Init/Register
        //============================================================
        public static bool TryCreate(string id, MonoBehaviour runner, out TaskTimer timer)
        {
            timer = null;
            if(!StringTokenUtils.TryNormalizeNonEmpty(id, out string normalizedId))
            {
                DebugLogger.LogError("TaskTimer ID가 유효하지 않습니다. ID=" + StringTokenUtils.ToLogSafe(id));
                return false;
            }

            if(runner == null)
            {
                DebugLogger.LogError("TaskTimer Runner 참조가 비어 있습니다.");
                return false;
            }

            TaskTimer createdTimer = new(normalizedId, runner);
            if(!createdTimer._fsm.TryAdd(ETaskTimerType.None, new TaskTimerBaseState(createdTimer)) || !createdTimer._fsm.TryAdd(ETaskTimerType.Processing, new TaskTimerProcessingState(createdTimer)) || !createdTimer._fsm.TryAdd(ETaskTimerType.Completed, new TaskTimerCompletedState(createdTimer)))
                return false;

            timer = createdTimer;
            return true;
        }

        public bool TryInit()
        {
            if(_isInit)
                return true;

            Load();
            _isInit = true;
            if(TryRefresh())
                return true;

            _isInit = false;
            return false;
        }

        public bool TryRelease()
        {
            bool isSuccess = TryStopUpdate();
            _isInit = false;
            return isSuccess;
        }

        //============================================================
        // Persistence
        //============================================================
        private void Save()
        {
            _updatedTime = DateTimeUtils.RemoveMs(DateTime.UtcNow);
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
        // Logic
        //============================================================
        public bool TryRefresh()
        {
            if(!_isInit)
            {
                DebugLogger.LogError("초기화되지 않은 TaskTimer를 갱신할 수 없습니다. ID=" + _id);
                return false;
            }

            if(!TryLoadStateType(out ETaskTimerType type))
                return false;

            if(_startTime == DateTime.MinValue || type == ETaskTimerType.None)
            {
                if(!TryStopUpdate())
                    return false;

                return _fsm.HasCurState && _fsm.CurType == ETaskTimerType.None || TryChangeState(ETaskTimerType.None, false, "TryRefresh");
            }

            switch(type)
            {
                case ETaskTimerType.Processing:
                    if(IsTampered)
                    {
                        double adjustSec = (_updatedTime - DateTime.UtcNow).TotalSeconds;
                        _durationSec += adjustSec;
                        _persistence.SaveDuration(_durationSec);
                    }

                    if(IsPeriodExpired)
                        return TryUpdateCompletionTime();

                    if((!_fsm.HasCurState || _fsm.CurType != ETaskTimerType.Processing) && !TryChangeState(ETaskTimerType.Processing, false, "TryRefresh"))
                        return false;

                    return TryStartUpdate();

                case ETaskTimerType.Completed:
                    if(!TryStopUpdate())
                        return false;

                    return _fsm.HasCurState && _fsm.CurType == ETaskTimerType.Completed || TryChangeState(ETaskTimerType.Completed, false, "TryRefresh");

                default:
                    DebugLogger.LogError("지원하지 않는 TaskTimer 상태입니다. 상태=" + type);
                    return false;
            }
        }

        public bool TryStart(double durationSec)
        {
            if(!_isInit || _fsm.CurType == ETaskTimerType.Processing)
            {
                DebugLogger.LogError("TaskTimer를 시작할 수 없는 상태입니다. ID=" + _id + ", 상태=" + _fsm.CurType);
                return false;
            }

            if(!IsPositiveFinite(durationSec))
            {
                DebugLogger.LogError("TaskTimer 지속시간은 0초보다 큰 유한값이어야 합니다. 값=" + durationSec);
                return false;
            }

            _startTime = DateTimeUtils.RemoveMs(DateTime.UtcNow);
            _durationSec = durationSec;
            if(_updatedTime != DateTime.MinValue && IsTampered)
                _durationSec += (_updatedTime - DateTime.UtcNow).TotalSeconds;

            if(!TryChangeState(ETaskTimerType.Processing, false, "TryStart"))
                return false;

            Save();
            return TryStartUpdate();
        }

        public bool TryReduce(double reduceSec)
        {
            if(!_isInit || _fsm.CurType != ETaskTimerType.Processing)
            {
                DebugLogger.LogError("TaskTimer 시간을 단축할 수 없는 상태입니다. ID=" + _id + ", 상태=" + _fsm.CurType);
                return false;
            }

            if(!IsPositiveFinite(reduceSec))
            {
                DebugLogger.LogError("TaskTimer 단축 시간은 0초보다 큰 유한값이어야 합니다. 값=" + reduceSec);
                return false;
            }

            double remainSec = (EndTime - DateTime.UtcNow).TotalSeconds;
            double actualReduceSec = Math.Min(reduceSec, remainSec);
            if(actualReduceSec <= 0d)
            {
                DebugLogger.LogError("TaskTimer에 단축할 남은 시간이 없습니다. ID=" + _id);
                return false;
            }

            _startTime = _startTime.AddSeconds(-actualReduceSec);
            Save();
            _onRemainSecUpdated?.Invoke(RemainingSec);
            return !IsPeriodExpired || TryUpdateCompletionTime();
        }

        public bool TryComplete()
        {
            if(!_isInit || _fsm.CurType != ETaskTimerType.Processing)
            {
                DebugLogger.LogError("TaskTimer를 즉시 완료할 수 없는 상태입니다. ID=" + _id + ", 상태=" + _fsm.CurType);
                return false;
            }

            return TryUpdateCompletionTime();
        }

        public bool TryClaim()
        {
            if(!_isInit || _fsm.CurType != ETaskTimerType.Completed)
            {
                DebugLogger.LogError("TaskTimer 보상을 수령할 수 없는 상태입니다. ID=" + _id + ", 상태=" + _fsm.CurType);
                return false;
            }

            if(!TryChangeState(ETaskTimerType.None, false, "TryClaim"))
                return false;

            Clear();
            _onClaimed?.Invoke();
            return true;
        }

        public bool TryUpdateCompletionTime()
        {
            _updatedTime = DateTimeUtils.RemoveMs(DateTime.UtcNow);
            _persistence.SaveUpdated(_updatedTime);
            if(_fsm.CurType != ETaskTimerType.Completed && !TryChangeState(ETaskTimerType.Completed, false, "TryUpdateCompletionTime"))
                return false;

            _persistence.SaveState(ETaskTimerType.Completed);
            return true;
        }

        //============================================================
        // Coroutines
        //============================================================
        private IEnumerator CoUpdate()
        {
            while(_isInit && _fsm.CurType == ETaskTimerType.Processing)
            {
                _fsm.Tick();
                if(!_isInit || _fsm.CurType != ETaskTimerType.Processing)
                    yield break;

                yield return new WaitForSecondsRealtime(1.0f);
            }
        }

        private bool TryStartUpdate()
        {
            if(!TryStopUpdate())
                return false;

            try
            {
                _coUpdate = _runner.StartCoroutine(CoUpdate());
                return true;
            }
            catch(Exception exception)
            {
                DebugLogger.LogError("TaskTimer Coroutine 시작에 실패했습니다. ID=" + _id + ", 원인=" + exception.Message);
                _coUpdate = null;
                return false;
            }
        }

        private bool TryStopUpdate()
        {
            if(_coUpdate == null)
                return true;

            try
            {
                _runner.StopCoroutine(_coUpdate);
                _coUpdate = null;
                return true;
            }
            catch(Exception exception)
            {
                DebugLogger.LogError("TaskTimer Coroutine 중단에 실패했습니다. ID=" + _id + ", 원인=" + exception.Message);
                _coUpdate = null;
                return false;
            }
        }

        //============================================================
        // Callbacks
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
            if(TryStopUpdate())
                _onCompleted?.Invoke();
        }

        public void NotifyCurType()
        {
            switch(_fsm.CurType)
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
        // Utilities
        //============================================================
        private float CalculateProgress()
        {
            if(_startTime == DateTime.MinValue)
                return 0.0f;

            if(_fsm.CurType == ETaskTimerType.Completed || IsPeriodExpired)
                return 1.0f;

            int totalSec = (int)Math.Round(_durationSec);
            int elapsedSec = (int)Math.Round((DateTime.UtcNow - _startTime).TotalSeconds);
            return totalSec <= 0 ? 0.0f : Mathf.Clamp01((float)elapsedSec / totalSec);
        }

        public static bool TryGetClaimed(string id, out bool isClaimed)
        {
            isClaimed = false;
            if(!StringTokenUtils.TryNormalizeNonEmpty(id, out string normalizedId))
            {
                DebugLogger.LogError("TaskTimer 수령 상태 조회 ID가 유효하지 않습니다. ID=" + StringTokenUtils.ToLogSafe(id));
                return false;
            }

            IStorage storage = new PlayerPrefsStorage();
            bool hasStart = storage.HasKey(TaskTimerStorageKeys.Start(normalizedId));
            bool hasDuration = storage.HasKey(TaskTimerStorageKeys.Duration(normalizedId));
            bool hasState = storage.HasKey(TaskTimerStorageKeys.State(normalizedId));
            bool hasUpdated = storage.HasKey(TaskTimerStorageKeys.Updated(normalizedId));
            isClaimed = !hasStart && !hasDuration && !hasState && hasUpdated;
            return true;
        }

        private bool TryLoadStateType(out ETaskTimerType type)
        {
            type = (ETaskTimerType)_savedStateType;
            if(_fsm.HasState(type))
                return true;

            DebugLogger.LogError("유효하지 않은 TaskTimer 저장 상태값입니다. 값=" + _savedStateType + ", ID=" + _id);
            return false;
        }

        private static bool IsPositiveFinite(double value)
        {
            return value > 0d && !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private bool TryChangeState(ETaskTimerType type, bool isUpdate = false, string method = "TryChangeState")
        {
            if(!_fsm.HasState(type))
            {
                StateTransitionLogUtils.LogMissingState(method, type);
                return false;
            }

            if(!_fsm.HasCurState)
            {
                if(_fsm.TrySetInitialState(type, isUpdate))
                    return true;

                StateTransitionLogUtils.LogInitialSetFailed(method, type);
                return false;
            }

            if(_fsm.TryChange(type, isUpdate))
                return true;

            StateTransitionLogUtils.LogTransitionFailed(method, _fsm.CurType, type);
            return false;
        }
    }
}
