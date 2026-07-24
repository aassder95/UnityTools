using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityTools.Timer.Persistence;
using UnityTools.Timer;

namespace UnityTools.Timer.Period
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
        private readonly Func<DateTime> _utcNow;

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
        public EPeriodTimerType CurType => _fsm.CurType;
        public bool IsOpenPeriod => DateTimeUtils.CompareWithoutMs(GetUtcNow(), _openEndTime) < 0;
        public bool IsClosedPeriod => DateTimeUtils.CompareWithoutMs(GetUtcNow(), _closedEndTime) < 0;
        public bool IsReady => _isInit && _coInit == null;
        public int RemainingMin => CalculateRemainingMin();
        public int RemainingSec => CalculateRemainingSec();
        public DateTime OpenUpdatedTime => _openUpdatedTime;
        public DateTime OpenEndTime => _openEndTime;
        public DateTime ClosedEndTime => _closedEndTime;
        private bool IsTampered => DateTimeUtils.CompareWithoutMs(GetUtcNow(), _openUpdatedTime) < 0;

        //============================================================
        // Constructors
        //============================================================
        private PeriodTimer(string normalizedId, MonoBehaviour runner, IStorage storage, Func<DateTime> utcNow)
        {
            _id = normalizedId;
            _runner = runner;
            _persistence = new PeriodTimerPersistence(_id, storage);
            _utcNow = utcNow ?? GetSystemUtcNow;
            _fsm = new StateMachine<EPeriodTimerType>();
            RegisterStates();
        }

        //============================================================
        // Init/Register
        //============================================================
        public static bool TryCreate(string id, MonoBehaviour runner, out PeriodTimer timer, IStorage storage = null, Func<DateTime> utcNow = null)
        {
            timer = null;
            string normalizedId = id?.Trim();
            if (string.IsNullOrEmpty(normalizedId) || runner == null)
                return false;

            timer = new PeriodTimer(normalizedId, runner, storage, utcNow);
            return true;
        }

        public bool TryInit(double openMin, double closedMin, Func<IEnumerator> initWaitFunc = null)
        {
            if (_isInit)
                return true;

            if (!IsPositiveFinite(openMin) || !IsPositiveFinite(closedMin))
                return false;

            StopInit();
            RegisterStateEvent();
            _openPeriodMin = openMin;
            _closedPeriodMin = closedMin;
            if (!TryLoad())
            {
                UnregisterStateEvent();
                return false;
            }

            if (initWaitFunc == null)
            {
                _isInit = true;
                if (TryRefresh())
                {
                    StartUpdate();
                    return true;
                }

                _isInit = false;
                UnregisterStateEvent();
                return false;
            }

            IEnumerator initEnumerator = initWaitFunc.Invoke();
            _coInit = _runner.StartCoroutine(CoInit(initEnumerator));
            return true;
        }

        public void Release()
        {
            StopInit();
            StopUpdate();
            UnregisterStateEvent();
            _isInit = false;
        }

        private void RegisterStates()
        {
            _fsm.Add(EPeriodTimerType.Reset, new PeriodTimerBaseState(this));
            _fsm.Add(EPeriodTimerType.Open, new PeriodTimerOpenState(this));
            _fsm.Add(EPeriodTimerType.Closed, new PeriodTimerClosedState(this));
        }

        private void RegisterStateEvent()
        {
            if (_isStateEventRegistered)
                return;

            _fsm.OnStateTransition += OnStateTransitionCallback;
            _isStateEventRegistered = true;
        }

        private void UnregisterStateEvent()
        {
            if (!_isStateEventRegistered)
                return;

            _fsm.OnStateTransition -= OnStateTransitionCallback;
            _isStateEventRegistered = false;
        }

        //============================================================
        // Persistence
        //============================================================
        private bool TryLoad()
        {
            if (!_persistence.TryLoad(out PeriodTimerStorageSnapshot snapshot))
                return false;

            _openEndTime = snapshot.OpenEndTime;
            _closedEndTime = snapshot.ClosedEndTime;
            _openUpdatedTime = snapshot.OpenUpdatedTime;
            _isTamperedFlag = snapshot.IsTamperedFlag;
            return true;
        }

        //============================================================
        // Logic
        //============================================================
        private bool TryRefresh()
        {
            DateTime now = GetUtcNow();
            if (_openEndTime == DateTime.MinValue)
                return TryOpenNewPeriod();

            if (now < _openEndTime)
            {
                if (IsTampered)
                    return TryHandleTampered();

                _fsm.Change(EPeriodTimerType.Open);
                return true;
            }

            if (now < _closedEndTime)
            {
                _fsm.Change(EPeriodTimerType.Closed);
                return true;
            }

            return TryOpenNewPeriod();
        }

        private bool TryOpenNewPeriod()
        {
            NotifyOpenPeriodPreparing();
            if (!TryApplyPeriodTime())
                return false;

            _fsm.Change(EPeriodTimerType.Reset);
            NotifyOpenPeriodStarted();
            _fsm.Change(EPeriodTimerType.Open);
            return true;
        }

        private bool TryApplyPeriodTime()
        {
            double openPeriodMin = _hasPendingPeriodChange ? _nextOpenPeriodMin : _openPeriodMin;
            double closedPeriodMin = _hasPendingPeriodChange ? _nextClosedPeriodMin : _closedPeriodMin;
            DateTime now = GetUtcNow();
            DateTime openEndTime = DateTimeUtils.RemoveMs(now.AddMinutes(openPeriodMin));
            DateTime closedEndTime = DateTimeUtils.RemoveMs(now.AddMinutes(openPeriodMin + closedPeriodMin));
            if (!_persistence.TrySave(openEndTime, closedEndTime, now, false))
                return false;

            _openPeriodMin = openPeriodMin;
            _closedPeriodMin = closedPeriodMin;
            _hasPendingPeriodChange = false;
            _isTamperedFlag = false;
            _openUpdatedTime = now;
            _openEndTime = openEndTime;
            _closedEndTime = closedEndTime;
            return true;
        }

        private bool TryHandleTampered()
        {
            if (!_persistence.TrySave(_openEndTime, _closedEndTime, _openUpdatedTime, true))
                return false;

            _isTamperedFlag = true;
            _fsm.Change(EPeriodTimerType.Closed);
            return true;
        }

        private bool TryClearTampered()
        {
            DateTime now = GetUtcNow();
            DateTime closedEndTime = DateTimeUtils.RemoveMs(now.AddMinutes(_closedPeriodMin));
            if (!_persistence.TrySave(now, closedEndTime, now, false))
                return false;

            _isTamperedFlag = false;
            _openUpdatedTime = now;
            _openEndTime = now;
            _closedEndTime = closedEndTime;
            return true;
        }

        public bool TrySetPeriods(double openMin, double closedMin)
        {
            if (!_isInit || !IsPositiveFinite(openMin) || !IsPositiveFinite(closedMin))
                return false;

            _nextOpenPeriodMin = openMin;
            _nextClosedPeriodMin = closedMin;
            _hasPendingPeriodChange = true;
            return true;
        }

        public bool TryForceOpen()
        {
            if (!_isInit)
                return false;

            StopUpdate();
            bool isOpened = TryOpenNewPeriod();
            StartUpdate();
            return isOpened;
        }

        public bool TryForceClosed()
        {
            if (!_isInit || !TrySetClosedPeriodFromNow())
                return false;

            _fsm.Change(EPeriodTimerType.Closed);
            return true;
        }

        private bool TrySetClosedPeriodFromNow()
        {
            DateTime now = GetUtcNow();
            DateTime closedEndTime = DateTimeUtils.RemoveMs(now.AddMinutes(_closedPeriodMin));
            if (!_persistence.TrySave(now, closedEndTime, now, false))
                return false;

            _isTamperedFlag = false;
            _openUpdatedTime = now;
            _openEndTime = now;
            _closedEndTime = closedEndTime;
            return true;
        }

        //============================================================
        // Coroutines
        //============================================================
        private IEnumerator CoInit(IEnumerator initEnumerator)
        {
            bool isCompleted = false;
            try
            {
                if (initEnumerator != null)
                    yield return initEnumerator;

                _isInit = true;
                if (!TryRefresh())
                    yield break;

                StartUpdate();
                isCompleted = true;
            }
            finally
            {
                _coInit = null;
                if (!isCompleted)
                {
                    _isInit = false;
                    UnregisterStateEvent();
                }
            }
        }

        private void StopInit()
        {
            if (_coInit == null)
                return;

            _runner.StopCoroutine(_coInit);
            _coInit = null;
        }

        private IEnumerator CoUpdate()
        {
            while (_isInit && _fsm.CurType != EPeriodTimerType.None)
            {
                _fsm.Tick();
                if (!_isInit || _fsm.CurType == EPeriodTimerType.None)
                    yield break;

                yield return new WaitForSecondsRealtime(GetWaitSec());
            }
        }

        private void StartUpdate()
        {
            StopUpdate();
            _coUpdate = _runner.StartCoroutine(CoUpdate());
        }

        private void StopUpdate()
        {
            if (_coUpdate == null)
                return;

            _runner.StopCoroutine(_coUpdate);
            _coUpdate = null;
        }

        private int GetWaitSec()
        {
            DateTime now = GetUtcNow();
            switch (_fsm.CurType)
            {
                case EPeriodTimerType.Open:
                    return Mathf.Clamp(DateTimeUtils.GetRemainingSec(_openEndTime, now), 1, 60);
                case EPeriodTimerType.Closed:
                    return Mathf.Clamp(DateTimeUtils.GetRemainingSec(_closedEndTime, now), 1, 60);
                default:
                    return 1;
            }
        }

        //============================================================
        // Callbacks
        //============================================================
        private void NotifyOpenPeriodPreparing()
        {
            _onOpenPeriodPreparing?.Invoke();
        }

        private void NotifyOpenPeriodStarted()
        {
            _onOpenPeriodStarted?.Invoke();
        }

        private bool TryNotifyOpenRemainMinUpdated()
        {
            DateTime updatedTime = GetUtcNow();
            if (!_persistence.TrySave(_openEndTime, _closedEndTime, updatedTime, _isTamperedFlag))
                return false;

            _openUpdatedTime = updatedTime;
            _onRemainMinUpdated?.Invoke(DateTimeUtils.GetRemainingMin(_openEndTime, updatedTime));
            return true;
        }

        private bool TryNotifyClosedRemainMinUpdated()
        {
            if (!_persistence.TrySave(_openEndTime, _closedEndTime, _openUpdatedTime, _isTamperedFlag))
                return false;

            _onRemainMinUpdated?.Invoke(DateTimeUtils.GetRemainingMin(_closedEndTime, GetUtcNow()));
            return true;
        }

        private void NotifyClosedPeriodStarted()
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
            DateTime now = GetUtcNow();
            switch (_fsm.CurType)
            {
                case EPeriodTimerType.Open:
                    return DateTimeUtils.GetRemainingMin(_openEndTime, now);
                case EPeriodTimerType.Closed:
                    return DateTimeUtils.GetRemainingMin(_closedEndTime, now);
                default:
                    return 0;
            }
        }

        private int CalculateRemainingSec()
        {
            DateTime now = GetUtcNow();
            switch (_fsm.CurType)
            {
                case EPeriodTimerType.Open:
                    return DateTimeUtils.GetRemainingSec(_openEndTime, now);
                case EPeriodTimerType.Closed:
                    return DateTimeUtils.GetRemainingSec(_closedEndTime, now);
                default:
                    return 0;
            }
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
        private class PeriodTimerBaseState : IState
        {
            //============================================================
            // Readonly
            //============================================================
            protected readonly PeriodTimer _timer;

            //============================================================
            // Constructors
            //============================================================
            public PeriodTimerBaseState(PeriodTimer timer)
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

        private class PeriodTimerOpenState : PeriodTimerBaseState
        {
            //============================================================
            // Constructors
            //============================================================
            public PeriodTimerOpenState(PeriodTimer timer) : base(timer) { }

            //============================================================
            // Logic
            //============================================================
            public override void Enter()
            {
                _timer.TryNotifyOpenRemainMinUpdated();
            }

            public override void Execute()
            {
                if (_timer.IsTampered)
                {
                    _timer.TryHandleTampered();
                    return;
                }

                if (!_timer.IsOpenPeriod)
                {
                    _timer._fsm.Change(EPeriodTimerType.Closed);
                    return;
                }

                _timer.TryNotifyOpenRemainMinUpdated();
            }
        }

        private class PeriodTimerClosedState : PeriodTimerBaseState
        {
            //============================================================
            // Constructors
            //============================================================
            public PeriodTimerClosedState(PeriodTimer timer) : base(timer) { }

            //============================================================
            // Logic
            //============================================================
            public override void Enter()
            {
                if (_timer._isTamperedFlag && !_timer.TryClearTampered())
                    return;

                _timer.NotifyClosedPeriodStarted();
                _timer.TryNotifyClosedRemainMinUpdated();
            }

            public override void Execute()
            {
                if (_timer._isTamperedFlag)
                {
                    if (!_timer.TryClearTampered())
                        return;

                    _timer.NotifyClosedPeriodStarted();
                }

                if (!_timer.IsClosedPeriod)
                {
                    _timer.TryOpenNewPeriod();
                    return;
                }

                _timer.TryNotifyClosedRemainMinUpdated();
            }
        }
    }
}
