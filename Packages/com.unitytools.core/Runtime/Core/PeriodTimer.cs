using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace UnityTools.Util
{
    public enum EPeriodTimerType
    {
        None,
        Reset,
        Open,
        Closed
    }

    public class PeriodTimer
    {
        //============================================================
        //Constants
        //============================================================
        private const double DEFAULT_PERIOD_MIN = 1d;

        //============================================================
        //Readonly
        //============================================================
        private readonly string _id;
        private readonly StateMachine<EPeriodTimerType> _fsm;
        private readonly MonoBehaviour _runner;
        private readonly PeriodTimerPersistence _persistence;
        private readonly bool _isEnableLog;

        //============================================================
        //Fields
        //============================================================
        private bool _isInit;
        private bool _isTamperedFlag;
        private bool _hasPendingPeriodChange;
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
        //Events
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
        //Properties
        //============================================================
        public string Id => _id;
        public StateMachine<EPeriodTimerType> FSM => _fsm;
        public bool IsTamperedFlag => _isTamperedFlag;
        public bool IsOpenPeriod => DateTimeUtils.CompareWithoutMilliseconds(DateTime.UtcNow, _openEndTime) < 0;
        public bool IsClosedPeriod => DateTimeUtils.CompareWithoutMilliseconds(DateTime.UtcNow, _closedEndTime) < 0;
        public bool IsTampered => DateTimeUtils.CompareWithoutMilliseconds(DateTime.UtcNow, _openUpdatedTime) < 0;
        public bool IsReady => _isInit && _coInit == null;
        public DateTime OpenUpdatedTime => _openUpdatedTime;
        public DateTime OpenEndTime => _openEndTime;
        public DateTime ClosedEndTime => _closedEndTime;

        //============================================================
        //Constructors
        //============================================================
        public PeriodTimer(string id, MonoBehaviour runner, bool isEnableLog = false)
        {
            if(!PeriodTimerStorageKeys.TryNormalizeId(id, out _id))
                _id = string.Empty;

            _runner = runner;
            _persistence = new PeriodTimerPersistence(_id);
            _isEnableLog = isEnableLog;
            _fsm = new StateMachine<EPeriodTimerType>(false);
            _fsm.OnStateTransition += OnStateTransitionCallback;

            if(!_fsm.Add(EPeriodTimerType.Reset, new PeriodTimerStates.ResetState(this)))
                DebugLogger.LogWarning(_isEnableLog, nameof(PeriodTimer), "Ctor", $"상태 등록 실패: {EPeriodTimerType.Reset}");
            if(!_fsm.Add(EPeriodTimerType.Open, new PeriodTimerStates.OpenState(this)))
                DebugLogger.LogWarning(_isEnableLog, nameof(PeriodTimer), "Ctor", $"상태 등록 실패: {EPeriodTimerType.Open}");
            if(!_fsm.Add(EPeriodTimerType.Closed, new PeriodTimerStates.ClosedState(this)))
                DebugLogger.LogWarning(_isEnableLog, nameof(PeriodTimer), "Ctor", $"상태 등록 실패: {EPeriodTimerType.Closed}");
        }

        //============================================================
        //Init/Register
        //============================================================
        public void Init(double openMin, double closedMin, Func<IEnumerator> initWaitFunc = null)
        {
            if(string.IsNullOrEmpty(_id))
            {
                DebugLogger.LogWarning(_isEnableLog, nameof(PeriodTimer), nameof(Init), "유효하지 않은 ID로 초기화를 무시합니다.");
                return;
            }

            if(_runner == null)
            {
                DebugLogger.LogWarning(_isEnableLog, nameof(PeriodTimer), nameof(Init), "러너가 null이라 초기화를 무시합니다.");
                return;
            }

            if(_isInit)
                return;

            StopInit();
            _openPeriodMin = SanitizePeriod(openMin, nameof(openMin));
            _closedPeriodMin = SanitizePeriod(closedMin, nameof(closedMin));

            Load();
            if(initWaitFunc == null)
            {
                _isInit = true;
                Refresh();
                StartUpdate();
                return;
            }

            _coInit = _runner.StartCoroutine(CoInit(initWaitFunc));
        }

        public void Release()
        {
            StopInit();
            if(!_isInit)
                return;

            StopUpdate();
            _isInit = false;
        }

        //============================================================
        //Persistence
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
        //Logic
        //============================================================
        public void Refresh()
        {
            DateTime now = DateTimeUtils.RemoveMilliseconds(DateTime.UtcNow);
            if(_openEndTime == DateTime.MinValue)
            {
                TryChangeState(EPeriodTimerType.Reset, true, "Refresh");
                return;
            }

            if(now < _openEndTime)
            {
                if(IsTampered)
                {
                    HandleTampered();
                    return;
                }

                TryChangeState(EPeriodTimerType.Open, false, "Refresh");
                return;
            }

            if(now < _closedEndTime)
            {
                TryChangeState(EPeriodTimerType.Closed, false, "Refresh");
                return;
            }

            TryChangeState(EPeriodTimerType.Reset, true, "Refresh");
        }

        public void ApplyPeriodTime()
        {
            ApplyPendingPeriodsIfNeeded();

            DateTime now = DateTimeUtils.RemoveMilliseconds(DateTime.UtcNow);
            _openUpdatedTime = now;
            _openEndTime = DateTimeUtils.RemoveMilliseconds(now.AddMinutes(_openPeriodMin));
            _closedEndTime = DateTimeUtils.RemoveMilliseconds(now.AddMinutes(_openPeriodMin + _closedPeriodMin));
            _persistence.Save(_openEndTime, _closedEndTime, _openUpdatedTime, _isTamperedFlag);
        }

        public void HandleTampered()
        {
            _isTamperedFlag = true;
            TryChangeState(EPeriodTimerType.Closed, false, "HandleTampered");
            _persistence.Save(_openEndTime, _closedEndTime, _openUpdatedTime, _isTamperedFlag);
        }

        public void ClearTampered()
        {
            _isTamperedFlag = false;
            SetClosedPeriodFromNow();
            _persistence.Save(_openEndTime, _closedEndTime, _openUpdatedTime, _isTamperedFlag);
        }

        public void SetPeriods(double openMin, double closedMin)
        {
            if(!_isInit)
                return;

            _nextOpenPeriodMin = SanitizePeriod(openMin, nameof(openMin));
            _nextClosedPeriodMin = SanitizePeriod(closedMin, nameof(closedMin));
            _hasPendingPeriodChange = true;
        }

        public void ForceOpen()
        {
            if(!_isInit)
                return;

            _isTamperedFlag = false;
            StopUpdate();
            TryChangeState(EPeriodTimerType.Reset, true, "ForceOpen");
            StartUpdate();
        }

        public void ForceClosed()
        {
            if(!_isInit)
                return;

            _isTamperedFlag = false;
            SetClosedPeriodFromNow();
            TryChangeState(EPeriodTimerType.Closed, false, "ForceClosed");
            _persistence.Save(_openEndTime, _closedEndTime, _openUpdatedTime, _isTamperedFlag);
        }

        public bool TryChangeState(EPeriodTimerType type, bool isUpdate = false, string method = "TryChangeState")
        {
            if(!_fsm.HasState(type))
            {
                DebugLogger.LogWarning(_isEnableLog, nameof(PeriodTimer), method, $"등록되지 않은 상태 전이 요청: {type}");
                return false;
            }

            if(!_fsm.HasCurrentState)
            {
                if(_fsm.SetInitialState(type, isUpdate))
                    return true;

                DebugLogger.LogWarning(_isEnableLog, nameof(PeriodTimer), method, $"초기 상태 설정 실패: {type}");
                return false;
            }

            if(_fsm.Change(type, isUpdate))
                return true;

            DebugLogger.LogWarning(_isEnableLog, nameof(PeriodTimer), method, $"상태 전이 실패: {_fsm.CurType} -> {type}");
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
            DateTime now = DateTimeUtils.RemoveMilliseconds(DateTime.UtcNow);
            _openUpdatedTime = now;
            _openEndTime = now;
            _closedEndTime = DateTimeUtils.RemoveMilliseconds(now.AddMinutes(_closedPeriodMin));
        }

        //============================================================
        //Coroutines
        //============================================================
        private IEnumerator CoInit(Func<IEnumerator> initWaitFunc)
        {
            bool isCompleted = false;
            try
            {
                IEnumerator initEnumerator = initWaitFunc?.Invoke();
                if(initEnumerator != null)
                    yield return initEnumerator;

                _isInit = true;
                Refresh();
                StartUpdate();
                isCompleted = true;
            }
            finally
            {
                _coInit = null;
                if(!isCompleted)
                    _isInit = false;
            }
        }

        private void StopInit()
        {
            if(_coInit == null)
                return;

            _runner.StopCoroutine(_coInit);
            _coInit = null;
        }

        private IEnumerator CoUpdate()
        {
            while (_isInit && _fsm.CurType != EPeriodTimerType.None)
            {
                _fsm.Update();
                if(!_isInit || _fsm.CurType == EPeriodTimerType.None)
                    yield break;

                int waitSec = GetWaitSec();
                yield return new WaitForSecondsRealtime(waitSec);
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

        private int GetWaitSec()
        {
            switch (_fsm.CurType)
            {
                case EPeriodTimerType.Open:
                    return Mathf.Clamp(DateTimeUtils.GetRemainingSeconds(_openEndTime), 1, 60);
                case EPeriodTimerType.Closed:
                    return Mathf.Clamp(DateTimeUtils.GetRemainingSeconds(_closedEndTime), 1, 60);
                default:
                    return 1;
            }
        }

        //============================================================
        //Callbacks
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
            _openUpdatedTime = DateTimeUtils.RemoveMilliseconds(DateTime.UtcNow);
            _persistence.Save(_openEndTime, _closedEndTime, _openUpdatedTime, _isTamperedFlag);

            int remainMin = DateTimeUtils.GetRemainingMinutes(_openEndTime);
            _onRemainMinUpdated?.Invoke(remainMin);
        }

        public void NotifyClosedRemainMinUpdated()
        {
            _persistence.Save(_openEndTime, _closedEndTime, _openUpdatedTime, _isTamperedFlag);

            int remainMin = DateTimeUtils.GetRemainingMinutes(_closedEndTime);
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
        //Utilities
        //============================================================
        public int GetRemainingMin()
        {
            switch (_fsm.CurType)
            {
                case EPeriodTimerType.Open:
                    return DateTimeUtils.GetRemainingMinutes(_openEndTime);
                case EPeriodTimerType.Closed:
                    return DateTimeUtils.GetRemainingMinutes(_closedEndTime);
                default:
                    return 0;
            }
        }

        public int GetRemainingSec()
        {
            switch (_fsm.CurType)
            {
                case EPeriodTimerType.Open:
                    return DateTimeUtils.GetRemainingSeconds(_openEndTime);
                case EPeriodTimerType.Closed:
                    return DateTimeUtils.GetRemainingSeconds(_closedEndTime);
                default:
                    return 0;
            }
        }

        private double SanitizePeriod(double min, string name)
        {
            if(min > 0d && !double.IsNaN(min) && !double.IsInfinity(min))
                return min;

            DebugLogger.LogWarning(_isEnableLog, nameof(PeriodTimer), nameof(SanitizePeriod), $"유효하지 않은 값 {name}={min}, 기본값 {DEFAULT_PERIOD_MIN}분 적용");
            return DEFAULT_PERIOD_MIN;
        }
    }

}
