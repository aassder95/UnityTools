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
        private readonly Func<DateTime> _utcNow;

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
        public ETaskTimerType CurType => _fsm.CurType;
        public int RemainingSec => DateTimeUtils.GetRemainingSec(EndTime, GetUtcNow());
        public int DurationSec => (int)_durationSec;
        public float Progress => CalculateProgress();
        public DateTime EndTime => _startTime.AddSeconds(_durationSec);
        private bool IsPeriodExpired => DateTimeUtils.CompareWithoutMs(GetUtcNow(), EndTime) >= 0;
        private bool IsTampered => DateTimeUtils.CompareWithoutMs(GetUtcNow(), _updatedTime) < 0;

        //============================================================
        // Constructors
        //============================================================
        private TaskTimer(string normalizedId, MonoBehaviour runner, IStorage storage, Func<DateTime> utcNow)
        {
            _id = normalizedId;
            _runner = runner;
            _persistence = new TaskTimerPersistence(_id, storage);
            _utcNow = utcNow ?? GetSystemUtcNow;
            _fsm = new StateMachine<ETaskTimerType>();
            RegisterStates();
        }

        //============================================================
        // Init/Register
        //============================================================
        public static bool TryCreate(string id, MonoBehaviour runner, out TaskTimer timer, IStorage storage = null, Func<DateTime> utcNow = null)
        {
            timer = null;
            if(!StringTokenUtils.TryNormalizeNonEmpty(id, out string normalizedId) || runner == null)
                return false;

            timer = new TaskTimer(normalizedId, runner, storage, utcNow);
            return true;
        }

        public bool TryInit()
        {
            if(_isInit)
                return true;

            if(!TryLoad())
                return false;

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

        private void RegisterStates()
        {
            _fsm.Add(ETaskTimerType.None, new TaskTimerBaseState(this));
            _fsm.Add(ETaskTimerType.Processing, new TaskTimerProcessingState(this));
            _fsm.Add(ETaskTimerType.Completed, new TaskTimerCompletedState(this));
        }

        //============================================================
        // Persistence
        //============================================================
        private bool TryLoad()
        {
            if(!_persistence.TryLoad(out TaskTimerStorageSnapshot snapshot))
                return false;

            _startTime = snapshot.StartTime;
            _durationSec = snapshot.DurationSec;
            _updatedTime = snapshot.UpdatedTime;
            _savedStateType = snapshot.SavedStateType;
            return true;
        }

        private bool TrySaveSnapshot(DateTime startTime, double durationSec, ETaskTimerType stateType, DateTime updatedTime)
        {
            return _persistence.TrySave(startTime, durationSec, stateType, updatedTime);
        }

        private void Clear()
        {
            _startTime = DateTime.MinValue;
            _durationSec = 0.0d;
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
                    DateTime now = GetUtcNow();
                    double adjustedDurationSec = _durationSec + (_updatedTime - now).TotalSeconds;
                    if(!TrySaveSnapshot(_startTime, adjustedDurationSec, ETaskTimerType.Processing, now))
                        return false;

                    _durationSec = adjustedDurationSec;
                    _updatedTime = now;
                }

                if(IsPeriodExpired)
                    return TryUpdateCompletionTime();

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

            DateTime now = GetUtcNow();
            double nextDurationSec = durationSec;
            if(_updatedTime != DateTime.MinValue && DateTimeUtils.CompareWithoutMs(now, _updatedTime) < 0)
                nextDurationSec += (_updatedTime - now).TotalSeconds;

            if(!TrySaveSnapshot(now, nextDurationSec, ETaskTimerType.Processing, now))
                return false;

            _startTime = now;
            _durationSec = nextDurationSec;
            _updatedTime = now;
            _savedStateType = (int)ETaskTimerType.Processing;
            _fsm.Change(ETaskTimerType.Processing);
            StartUpdate();
            return true;
        }

        public bool TryReduce(double reduceSec)
        {
            if(!_isInit || _fsm.CurType != ETaskTimerType.Processing || !IsPositiveFinite(reduceSec))
                return false;

            DateTime now = GetUtcNow();
            double remainSec = (EndTime - now).TotalSeconds;
            double actualReduceSec = Math.Min(reduceSec, remainSec);
            if(actualReduceSec <= 0.0d)
                return false;

            DateTime nextStartTime = _startTime.AddSeconds(-actualReduceSec);
            if(!TrySaveSnapshot(nextStartTime, _durationSec, ETaskTimerType.Processing, now))
                return false;

            _startTime = nextStartTime;
            _updatedTime = now;
            _onRemainSecUpdated?.Invoke(RemainingSec);
            if(IsPeriodExpired)
                return TryUpdateCompletionTime();

            return true;
        }

        public bool TryComplete()
        {
            return _isInit && _fsm.CurType == ETaskTimerType.Processing && TryUpdateCompletionTime();
        }

        public bool TryClaim()
        {
            if(!_isInit || _fsm.CurType != ETaskTimerType.Completed || !_persistence.TryClearRuntimeData())
                return false;

            _fsm.Change(ETaskTimerType.None);
            Clear();
            _onClaimed?.Invoke();
            return true;
        }

        public bool TryGetClaimed(out bool isClaimed)
        {
            return _persistence.TryLoadClaimed(out isClaimed);
        }

        private bool TryUpdateCompletionTime()
        {
            DateTime updatedTime = GetUtcNow();
            if(!TrySaveSnapshot(_startTime, _durationSec, ETaskTimerType.Completed, updatedTime))
                return false;

            _updatedTime = updatedTime;
            _savedStateType = (int)ETaskTimerType.Completed;
            if(_fsm.CurType != ETaskTimerType.Completed)
                _fsm.Change(ETaskTimerType.Completed);

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

        private void NotifyUpdate()
        {
            if(_fsm.CurType == ETaskTimerType.Processing)
                _onRemainSecUpdated?.Invoke(RemainingSec);
        }

        private void NotifyProcessingStarted()
        {
            _onProgressStarted?.Invoke();
        }

        private void NotifyCompleted()
        {
            StopUpdate();
            _onCompleted?.Invoke();
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
            int elapsedSec = (int)Math.Round((GetUtcNow() - _startTime).TotalSeconds);
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

        private DateTime GetUtcNow()
        {
            return DateTimeUtils.RemoveMs(_utcNow.Invoke());
        }

        private static DateTime GetSystemUtcNow()
        {
            return DateTime.UtcNow;
        }

        private static bool IsPositiveFinite(double value)
        {
            return value > 0.0d && !double.IsNaN(value) && !double.IsInfinity(value);
        }

        //============================================================
        // Nested Types
        //============================================================
        private class TaskTimerBaseState : IState
        {
            //============================================================
            // Readonly
            //============================================================
            protected readonly TaskTimer _timer;

            //============================================================
            // Constructors
            //============================================================
            public TaskTimerBaseState(TaskTimer timer)
            {
                _timer = timer;
            }

            //============================================================
            // Logic
            //============================================================
            public virtual void Enter() { }
            public virtual void Execute() { }
            public virtual void Exit() { }
        }

        private class TaskTimerProcessingState : TaskTimerBaseState
        {
            //============================================================
            // Constructors
            //============================================================
            public TaskTimerProcessingState(TaskTimer timer) : base(timer) { }

            //============================================================
            // Logic
            //============================================================
            public override void Enter()
            {
                _timer.NotifyProcessingStarted();
            }

            public override void Execute()
            {
                if(_timer.IsPeriodExpired)
                {
                    _timer.TryUpdateCompletionTime();
                    return;
                }

                _timer.NotifyUpdate();
            }
        }

        private class TaskTimerCompletedState : TaskTimerBaseState
        {
            //============================================================
            // Constructors
            //============================================================
            public TaskTimerCompletedState(TaskTimer timer) : base(timer) { }

            //============================================================
            // Logic
            //============================================================
            public override void Enter()
            {
                _timer.NotifyCompleted();
            }
        }
    }
}
