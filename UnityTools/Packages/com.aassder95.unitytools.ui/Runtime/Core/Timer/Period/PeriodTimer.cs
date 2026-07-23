using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.Core.State;
using UnityTools.Util.Utilities;

namespace UnityTools.Util.Core.Timer.Period
{
    public class PeriodTimer : IPeriodTimer
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly string _id;
        private readonly StateMachine<EPeriodTimerType> _fsm;
        private readonly MonoBehaviour _runner;
        private readonly PeriodTimerPersistence _persistence;

        //============================================================
        // Fields
        //============================================================
        private bool _isInit;
        private bool _isTamperedFlag;
        private bool _hasPendingPeriodChange;
        private bool _isStateEventRegistered;
        private double _openPeriodMin;
        private double _closedPeriodMin;
        private double _nextOpenPeriodMin;
        private double _nextClosedPeriodMin;
        private DateTime _openUpdatedTime;
        private DateTime _openEndTime;
        private DateTime _closedEndTime;
        private Coroutine _coInit;
        private Coroutine _coUpdate;

        //============================================================
        // Events
        //============================================================
        public event UnityAction OnOpenPeriodPreparing { add => _onOpenPeriodPreparing += value; remove => _onOpenPeriodPreparing -= value; }
        public event UnityAction OnOpenPeriodStarted { add => _onOpenPeriodStarted += value; remove => _onOpenPeriodStarted -= value; }
        public event UnityAction<int> OnRemainMinUpdated { add => _onRemainMinUpdated += value; remove => _onRemainMinUpdated -= value; }
        public event UnityAction OnClosedPeriodStarted { add => _onClosedPeriodStarted += value; remove => _onClosedPeriodStarted -= value; }
        public event UnityAction<EPeriodTimerType, EPeriodTimerType> OnPeriodStateTransition { add => _onPeriodStateTransition += value; remove => _onPeriodStateTransition -= value; }
        private event UnityAction _onOpenPeriodPreparing;
        private event UnityAction _onOpenPeriodStarted;
        private event UnityAction<int> _onRemainMinUpdated;
        private event UnityAction _onClosedPeriodStarted;
        private event UnityAction<EPeriodTimerType, EPeriodTimerType> _onPeriodStateTransition;

        //============================================================
        // Properties
        //============================================================
        public string Id => _id;
        public StateMachine<EPeriodTimerType> Fsm => _fsm;
        public EPeriodTimerType CurType => _fsm.CurType;
        public bool IsTamperedFlag => _isTamperedFlag;
        public bool IsOpenPeriod => DateTimeUtils.CompareWithoutMs(DateTime.UtcNow, _openEndTime) < 0;
        public bool IsClosedPeriod => DateTimeUtils.CompareWithoutMs(DateTime.UtcNow, _closedEndTime) < 0;
        public bool IsTampered => DateTimeUtils.CompareWithoutMs(DateTime.UtcNow, _openUpdatedTime) < 0;
        public bool IsReady => _isInit && _coInit == null;
        public int RemainingMin => CalculateRemainingMin();
        public int RemainingSec => CalculateRemainingSec();
        public DateTime OpenUpdatedTime => _openUpdatedTime;
        public DateTime OpenEndTime => _openEndTime;
        public DateTime ClosedEndTime => _closedEndTime;

        //============================================================
        // Constructors
        //============================================================
        private PeriodTimer(string normalizedId, MonoBehaviour runner)
        {
            _id = normalizedId;
            _runner = runner;
            _persistence = new PeriodTimerPersistence(_id);
            _fsm = new StateMachine<EPeriodTimerType>();
        }

        //============================================================
        // Init/Register
        //============================================================
        public static bool TryCreate(string id, MonoBehaviour runner, out PeriodTimer timer)
        {
            timer = null;
            if(!StringTokenUtils.TryNormalizeNonEmpty(id, out string normalizedId))
            {
                DebugLogger.LogError("PeriodTimer ID가 유효하지 않습니다. ID=" + StringTokenUtils.ToLogSafe(id));
                return false;
            }

            if(runner == null)
            {
                DebugLogger.LogError("PeriodTimer Runner 참조가 비어 있습니다.");
                return false;
            }

            PeriodTimer createdTimer = new(normalizedId, runner);
            if(!createdTimer._fsm.TryAdd(EPeriodTimerType.Reset, new PeriodTimerResetState(createdTimer)) || !createdTimer._fsm.TryAdd(EPeriodTimerType.Open, new PeriodTimerOpenState(createdTimer)) || !createdTimer._fsm.TryAdd(EPeriodTimerType.Closed, new PeriodTimerClosedState(createdTimer)))
                return false;

            timer = createdTimer;
            return true;
        }

        public bool TryInit(double openMin, double closedMin, Func<IEnumerator> initWaitFunc = null)
        {
            if(_isInit)
                return true;

            if(!IsPositiveFinite(openMin) || !IsPositiveFinite(closedMin))
            {
                DebugLogger.LogError("PeriodTimer 주기는 0분보다 큰 유한값이어야 합니다. Open=" + openMin + ", Closed=" + closedMin);
                return false;
            }

            if(!TryStopInit())
                return false;

            if(!_isStateEventRegistered)
            {
                _fsm.OnStateTransition += OnStateTransitionCallback;
                _isStateEventRegistered = true;
            }

            _openPeriodMin = openMin;
            _closedPeriodMin = closedMin;
            Load();
            if(initWaitFunc == null)
            {
                _isInit = true;
                if(TryRefresh() && TryStartUpdate())
                    return true;

                if(!TryRelease())
                    DebugLogger.LogError("PeriodTimer 초기화 실패 후 정리를 완료하지 못했습니다. ID=" + _id);

                return false;
            }

            try
            {
                IEnumerator initEnumerator = initWaitFunc();
                _coInit = _runner.StartCoroutine(CoInit(initEnumerator));
                return true;
            }
            catch(Exception exception)
            {
                DebugLogger.LogError("PeriodTimer 초기화 Coroutine 시작에 실패했습니다. ID=" + _id + ", 원인=" + exception.Message);
                if(!TryRelease())
                    DebugLogger.LogError("PeriodTimer 초기화 실패 후 정리를 완료하지 못했습니다. ID=" + _id);

                return false;
            }
        }

        public bool TryRelease()
        {
            bool isSuccess = TryStopInit();
            isSuccess &= TryStopUpdate();
            if(_isStateEventRegistered)
            {
                _fsm.OnStateTransition -= OnStateTransitionCallback;
                _isStateEventRegistered = false;
            }

            _isInit = false;
            return isSuccess;
        }

        //============================================================
        // Persistence
        //============================================================
        private void Load()
        {
            PeriodTimerStorageSnapshot snapshot = _persistence.Load();
            _openEndTime = snapshot.OpenEndTime;
            _closedEndTime = snapshot.ClosedEndTime;
            _openUpdatedTime = snapshot.OpenUpdatedTime;
            _isTamperedFlag = snapshot.IsTamperedFlag;
        }

        //============================================================
        // Logic
        //============================================================
        public bool TryRefresh()
        {
            if(!_isInit)
            {
                DebugLogger.LogError("초기화되지 않은 PeriodTimer를 갱신할 수 없습니다. ID=" + _id);
                return false;
            }

            DateTime now = DateTimeUtils.RemoveMs(DateTime.UtcNow);
            if(_openEndTime == DateTime.MinValue)
                return TryChangeState(EPeriodTimerType.Reset, true, "TryRefresh");

            if(now < _openEndTime)
            {
                if(IsTampered)
                    return TryHandleTampered();

                return TryChangeState(EPeriodTimerType.Open, false, "TryRefresh");
            }

            if(now < _closedEndTime)
                return TryChangeState(EPeriodTimerType.Closed, false, "TryRefresh");

            return TryChangeState(EPeriodTimerType.Reset, true, "TryRefresh");
        }

        public void ApplyPeriodTime()
        {
            ApplyPendingPeriodsIfNeeded();
            DateTime now = DateTimeUtils.RemoveMs(DateTime.UtcNow);
            _openUpdatedTime = now;
            _openEndTime = DateTimeUtils.RemoveMs(now.AddMinutes(_openPeriodMin));
            _closedEndTime = DateTimeUtils.RemoveMs(now.AddMinutes(_openPeriodMin + _closedPeriodMin));
            _persistence.Save(_openEndTime, _closedEndTime, _openUpdatedTime, _isTamperedFlag);
        }

        public bool TryHandleTampered()
        {
            _isTamperedFlag = true;
            if(!TryChangeState(EPeriodTimerType.Closed, false, "TryHandleTampered"))
                return false;

            _persistence.Save(_openEndTime, _closedEndTime, _openUpdatedTime, _isTamperedFlag);
            return true;
        }

        public void ClearTampered()
        {
            _isTamperedFlag = false;
            SetClosedPeriodFromNow();
            _persistence.Save(_openEndTime, _closedEndTime, _openUpdatedTime, _isTamperedFlag);
        }

        public bool TrySetPeriods(double openMin, double closedMin)
        {
            if(!_isInit)
            {
                DebugLogger.LogError("초기화되지 않은 PeriodTimer의 주기를 변경할 수 없습니다. ID=" + _id);
                return false;
            }

            if(!IsPositiveFinite(openMin) || !IsPositiveFinite(closedMin))
            {
                DebugLogger.LogError("PeriodTimer 주기는 0분보다 큰 유한값이어야 합니다. Open=" + openMin + ", Closed=" + closedMin);
                return false;
            }

            _nextOpenPeriodMin = openMin;
            _nextClosedPeriodMin = closedMin;
            _hasPendingPeriodChange = true;
            return true;
        }

        public bool TryForceOpen()
        {
            if(!_isInit)
            {
                DebugLogger.LogError("초기화되지 않은 PeriodTimer를 강제 Open할 수 없습니다. ID=" + _id);
                return false;
            }

            _isTamperedFlag = false;
            return TryStopUpdate() && TryChangeState(EPeriodTimerType.Reset, true, "TryForceOpen") && TryStartUpdate();
        }

        public bool TryForceClosed()
        {
            if(!_isInit)
            {
                DebugLogger.LogError("초기화되지 않은 PeriodTimer를 강제 Closed할 수 없습니다. ID=" + _id);
                return false;
            }

            _isTamperedFlag = false;
            SetClosedPeriodFromNow();
            if(!TryChangeState(EPeriodTimerType.Closed, false, "TryForceClosed"))
                return false;

            _persistence.Save(_openEndTime, _closedEndTime, _openUpdatedTime, _isTamperedFlag);
            return true;
        }

        public bool TryChangeState(EPeriodTimerType type, bool isUpdate = false, string method = "TryChangeState")
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

        private void ApplyPendingPeriodsIfNeeded()
        {
            if(!_hasPendingPeriodChange)
                return;

            _openPeriodMin = _nextOpenPeriodMin;
            _closedPeriodMin = _nextClosedPeriodMin;
            _hasPendingPeriodChange = false;
        }

        private void SetClosedPeriodFromNow()
        {
            DateTime now = DateTimeUtils.RemoveMs(DateTime.UtcNow);
            _openUpdatedTime = now;
            _openEndTime = now;
            _closedEndTime = DateTimeUtils.RemoveMs(now.AddMinutes(_closedPeriodMin));
        }

        //============================================================
        // Coroutines
        //============================================================
        private IEnumerator CoInit(IEnumerator initEnumerator)
        {
            bool isCompleted = false;
            try
            {
                if(initEnumerator != null)
                    yield return initEnumerator;

                _isInit = true;
                isCompleted = TryRefresh() && TryStartUpdate();
            }
            finally
            {
                _coInit = null;
                if(!isCompleted)
                    _isInit = false;
            }
        }

        private bool TryStopInit()
        {
            if(_coInit == null)
                return true;

            try
            {
                _runner.StopCoroutine(_coInit);
                _coInit = null;
                return true;
            }
            catch(Exception exception)
            {
                DebugLogger.LogError("PeriodTimer 초기화 Coroutine 중단에 실패했습니다. ID=" + _id + ", 원인=" + exception.Message);
                _coInit = null;
                return false;
            }
        }

        private IEnumerator CoUpdate()
        {
            while(_isInit && _fsm.CurType != EPeriodTimerType.None)
            {
                _fsm.Tick();
                if(!_isInit || _fsm.CurType == EPeriodTimerType.None)
                    yield break;

                int waitSec = GetWaitSec();
                yield return new WaitForSecondsRealtime(waitSec);
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
                DebugLogger.LogError("PeriodTimer Update Coroutine 시작에 실패했습니다. ID=" + _id + ", 원인=" + exception.Message);
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
                DebugLogger.LogError("PeriodTimer Update Coroutine 중단에 실패했습니다. ID=" + _id + ", 원인=" + exception.Message);
                _coUpdate = null;
                return false;
            }
        }

        private int GetWaitSec()
        {
            switch(_fsm.CurType)
            {
                case EPeriodTimerType.Open:
                    return Mathf.Clamp(DateTimeUtils.GetRemainingSec(_openEndTime), 1, 60);
                case EPeriodTimerType.Closed:
                    return Mathf.Clamp(DateTimeUtils.GetRemainingSec(_closedEndTime), 1, 60);
                default:
                    return 1;
            }
        }

        //============================================================
        // Callbacks
        //============================================================
        public void NotifyOpenPeriodPreparing()
        {
            _onOpenPeriodPreparing?.Invoke();
        }

        public void NotifyOpenPeriodStarted()
        {
            _onOpenPeriodStarted?.Invoke();
        }

        public void NotifyOpenRemainMinUpdated()
        {
            _openUpdatedTime = DateTimeUtils.RemoveMs(DateTime.UtcNow);
            _persistence.Save(_openEndTime, _closedEndTime, _openUpdatedTime, _isTamperedFlag);
            int remainMin = DateTimeUtils.GetRemainingMin(_openEndTime);
            _onRemainMinUpdated?.Invoke(remainMin);
        }

        public void NotifyClosedRemainMinUpdated()
        {
            _persistence.Save(_openEndTime, _closedEndTime, _openUpdatedTime, _isTamperedFlag);
            int remainMin = DateTimeUtils.GetRemainingMin(_closedEndTime);
            _onRemainMinUpdated?.Invoke(remainMin);
        }

        public void NotifyClosedPeriodStarted()
        {
            _onClosedPeriodStarted?.Invoke();
        }

        private void OnStateTransitionCallback(EPeriodTimerType prevType, EPeriodTimerType nextType)
        {
            _onPeriodStateTransition?.Invoke(prevType, nextType);
        }

        //============================================================
        // Utilities
        //============================================================
        private int CalculateRemainingMin()
        {
            switch(_fsm.CurType)
            {
                case EPeriodTimerType.Open:
                    return DateTimeUtils.GetRemainingMin(_openEndTime);
                case EPeriodTimerType.Closed:
                    return DateTimeUtils.GetRemainingMin(_closedEndTime);
                default:
                    return 0;
            }
        }

        private int CalculateRemainingSec()
        {
            switch(_fsm.CurType)
            {
                case EPeriodTimerType.Open:
                    return DateTimeUtils.GetRemainingSec(_openEndTime);
                case EPeriodTimerType.Closed:
                    return DateTimeUtils.GetRemainingSec(_closedEndTime);
                default:
                    return 0;
            }
        }

        private static bool IsPositiveFinite(double value)
        {
            return value > 0d && !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}