using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityTools.Util.Core.Logging;
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
        public bool IsClaimed => _persistence.IsClaimed;
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
        public static TaskTimer Create(string normalizedId, MonoBehaviour runner)
        {
            TaskTimer timer = new(normalizedId, runner);
            timer._fsm.Add(ETaskTimerType.None, new TaskTimerBaseState(timer));
            timer._fsm.Add(ETaskTimerType.Processing, new TaskTimerProcessingState(timer));
            timer._fsm.Add(ETaskTimerType.Completed, new TaskTimerCompletedState(timer));
            return timer;
        }

        public static bool TryCreate(string id, MonoBehaviour runner, out TaskTimer timer)
        {
            timer = null;
            if(!StringTokenUtils.TryNormalizeNonEmpty(id, out string normalizedId) || runner == null)
                return false;

            timer = Create(normalizedId, runner);
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

        public void Release()
        {
            StopUpdate();
            _isInit = false;
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
        private bool TryRefresh()
        {
            if(!TryLoadStateType(out ETaskTimerType type))
                return false;

            if(_startTime == DateTime.MinValue || type == ETaskTimerType.None)
            {
                StopUpdate();
                if(!_fsm.HasCurState || _fsm.CurType != ETaskTimerType.None)
                    _fsm.Change(ETaskTimerType.None);

                return true;
            }

            if(type == ETaskTimerType.Processing)
            {
                if(IsTampered)
                {
                    double adjustSec = (_updatedTime - DateTime.UtcNow).TotalSeconds;
                    _durationSec += adjustSec;
                    _persistence.SaveDuration(_durationSec);
                }

                if(IsPeriodExpired)
                {
                    UpdateCompletionTime();
                    return true;
                }

                if(!_fsm.HasCurState || _fsm.CurType != ETaskTimerType.Processing)
                    _fsm.Change(ETaskTimerType.Processing);

                StartUpdate();
                return true;
            }

            StopUpdate();
            if(!_fsm.HasCurState || _fsm.CurType != ETaskTimerType.Completed)
                _fsm.Change(ETaskTimerType.Completed);

            return true;
        }

        public bool TryStart(double durationSec)
        {
            if(!_isInit || _fsm.CurType == ETaskTimerType.Processing || !IsPositiveFinite(durationSec))
                return false;

            _startTime = DateTimeUtils.RemoveMs(DateTime.UtcNow);
            _durationSec = durationSec;
            if(_updatedTime != DateTime.MinValue && IsTampered)
                _durationSec += (_updatedTime - DateTime.UtcNow).TotalSeconds;

            _fsm.Change(ETaskTimerType.Processing);
            Save();
            StartUpdate();
            return true;
        }

        public bool TryReduce(double reduceSec)
        {
            if(!_isInit || _fsm.CurType != ETaskTimerType.Processing || !IsPositiveFinite(reduceSec))
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

        public bool TryComplete()
        {
            if(!_isInit || _fsm.CurType != ETaskTimerType.Processing)
                return false;

            UpdateCompletionTime();
            return true;
        }

        public bool TryClaim()
        {
            if(!_isInit || _fsm.CurType != ETaskTimerType.Completed)
                return false;

            _fsm.Change(ETaskTimerType.None);
            Clear();
            _onClaimed?.Invoke();
            return true;
        }

        public void UpdateCompletionTime()
        {
            _updatedTime = DateTimeUtils.RemoveMs(DateTime.UtcNow);
            _persistence.SaveUpdated(_updatedTime);
            if(_fsm.CurType != ETaskTimerType.Completed)
                _fsm.Change(ETaskTimerType.Completed);

            _persistence.SaveState(ETaskTimerType.Completed);
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
            StopUpdate();
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
    }
}
