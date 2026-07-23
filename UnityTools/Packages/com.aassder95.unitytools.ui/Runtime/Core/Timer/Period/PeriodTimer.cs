using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
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
        public static PeriodTimer Create(string normalizedId, MonoBehaviour runner)
        {
            PeriodTimer timer = new(normalizedId, runner);
            timer._fsm.Add(EPeriodTimerType.Reset, new PeriodTimerResetState(timer));
            timer._fsm.Add(EPeriodTimerType.Open, new PeriodTimerOpenState(timer));
            timer._fsm.Add(EPeriodTimerType.Closed, new PeriodTimerClosedState(timer));
            return timer;
        }

        public static bool TryCreate(string id, MonoBehaviour runner, out PeriodTimer timer)
        {
            timer = null;
            if(!StringTokenUtils.TryNormalizeNonEmpty(id, out string normalizedId) || runner == null)
                return false;

            timer = Create(normalizedId, runner);
            return true;
        }

        public void Init(double openMin, double closedMin, Func<IEnumerator> initWaitFunc = null)
        {
            if(_isInit)
                return;

            StopInit();
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
                Refresh();
                StartUpdate();
                return;
            }

            IEnumerator initEnumerator = initWaitFunc();
            _coInit = _runner.StartCoroutine(CoInit(initEnumerator));
        }

        public bool TryInit(double openMin, double closedMin, Func<IEnumerator> initWaitFunc = null)
        {
            if(_isInit)
                return true;

            if(!IsPositiveFinite(openMin) || !IsPositiveFinite(closedMin))
                return false;

            Init(openMin, closedMin, initWaitFunc);
            return true;
        }

        public void Release()
        {
            StopInit();
            StopUpdate();
            if(_isStateEventRegistered)
            {
                _fsm.OnStateTransition -= OnStateTransitionCallback;
                _isStateEventRegistered = false;
            }

            _isInit = false;
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
        private void Refresh()
        {
            DateTime now = DateTimeUtils.RemoveMs(DateTime.UtcNow);
            if(_openEndTime == DateTime.MinValue)
            {
                _fsm.Change(EPeriodTimerType.Reset, true);
                return;
            }

            if(now < _openEndTime)
            {
                if(IsTampered)
                {
                    HandleTampered();
                    return;
                }

                _fsm.Change(EPeriodTimerType.Open);
                return;
            }

            if(now < _closedEndTime)
            {
                _fsm.Change(EPeriodTimerType.Closed);
                return;
            }

            _fsm.Change(EPeriodTimerType.Reset, true);
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

        public void HandleTampered()
        {
            _isTamperedFlag = true;
            _fsm.Change(EPeriodTimerType.Closed);
            _persistence.Save(_openEndTime, _closedEndTime, _openUpdatedTime, _isTamperedFlag);
        }

        public void ClearTampered()
        {
            _isTamperedFlag = false;
            SetClosedPeriodFromNow();
            _persistence.Save(_openEndTime, _closedEndTime, _openUpdatedTime, _isTamperedFlag);
        }

        public bool TrySetPeriods(double openMin, double closedMin)
        {
            if(!_isInit || !IsPositiveFinite(openMin) || !IsPositiveFinite(closedMin))
                return false;

            _nextOpenPeriodMin = openMin;
            _nextClosedPeriodMin = closedMin;
            _hasPendingPeriodChange = true;
            return true;
        }

        public void ForceOpen()
        {
            _isTamperedFlag = false;
            StopUpdate();
            _fsm.Change(EPeriodTimerType.Reset, true);
            StartUpdate();
        }

        public void ForceClosed()
        {
            _isTamperedFlag = false;
            SetClosedPeriodFromNow();
            _fsm.Change(EPeriodTimerType.Closed);
            _persistence.Save(_openEndTime, _closedEndTime, _openUpdatedTime, _isTamperedFlag);
        }

        public bool TryForceOpen()
        {
            if(!_isInit)
                return false;

            ForceOpen();
            return true;
        }

        public bool TryForceClosed()
        {
            if(!_isInit)
                return false;

            ForceClosed();
            return true;
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
            while(_isInit && _fsm.CurType != EPeriodTimerType.None)
            {
                _fsm.Tick();
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
