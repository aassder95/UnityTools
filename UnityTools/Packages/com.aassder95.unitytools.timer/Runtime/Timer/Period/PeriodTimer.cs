using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityTools.Timer.Persistence;

namespace UnityTools.Timer.Period
{
    public class PeriodTimer : IPeriodTimer
    {
        //============================================================
        // Constants
        //============================================================
        private const string STORAGE_PREFIX = "PeriodTimer_";
        private const string OPEN_END_TIME_SUFFIX = "_OPEN_END";
        private const string CLOSED_END_TIME_SUFFIX = "_CLOSED_END";
        private const string OPEN_UPDATED_TIME_SUFFIX = "_OPEN_UPDATED";
        private const string TAMPERED_SUFFIX = "_TAMPERED";

        //============================================================
        // Readonly
        //============================================================
        private readonly string _id;
        private readonly MonoBehaviour _runner;
        private readonly IStorage _storage;
        private readonly Func<DateTime> _utcNow;

        //============================================================
        // Fields
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
        private EPeriodTimerType _curType;
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
        public EPeriodTimerType CurType => _curType;
        public bool IsOpenPeriod => CompareWithoutMs(GetUtcNow(), _openEndTime) < 0;
        public bool IsClosedPeriod => CompareWithoutMs(GetUtcNow(), _closedEndTime) < 0;
        public bool IsReady => _isInit && _coInit == null;
        public int RemainingMin => CalculateRemainingMin();
        public int RemainingSec => CalculateRemainingSec();
        public DateTime OpenUpdatedTime => _openUpdatedTime;
        public DateTime OpenEndTime => _openEndTime;
        public DateTime ClosedEndTime => _closedEndTime;
        private bool IsTampered => CompareWithoutMs(GetUtcNow(), _openUpdatedTime) < 0;

        //============================================================
        // Constructors
        //============================================================
        private PeriodTimer(string normalizedId, MonoBehaviour runner, IStorage storage, Func<DateTime> utcNow)
        {
            _id = normalizedId;
            _runner = runner;
            _storage = storage ?? new PlayerPrefsStorage();
            _utcNow = utcNow ?? GetSystemUtcNow;
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
            _openPeriodMin = openMin;
            _closedPeriodMin = closedMin;
            if (!TryLoad())
                return false;

            IEnumerator initEnumerator = initWaitFunc?.Invoke();
            if (initEnumerator == null)
            {
                _isInit = true;
                if (TryRefresh())
                {
                    StartUpdate();
                    return true;
                }

                _isInit = false;
                return false;
            }

            _coInit = _runner.StartCoroutine(CoInit(initEnumerator));
            return true;
        }

        public void Release()
        {
            StopInit();
            StopUpdate();
            _isInit = false;
        }

        //============================================================
        // Persistence
        //============================================================
        private bool TryLoad()
        {
            bool isOpenEndLoaded = TryLoadDate(GetOpenEndKey(_id), out _openEndTime);
            bool isClosedEndLoaded = TryLoadDate(GetClosedEndKey(_id), out _closedEndTime);
            bool isOpenUpdatedLoaded = TryLoadDate(GetOpenUpdatedKey(_id), out _openUpdatedTime);
            bool isTamperedLoaded = TryLoadString(GetTamperedKey(_id), out string rawTampered);
            _isTamperedFlag = rawTampered == "1";
            return isOpenEndLoaded && isClosedEndLoaded && isOpenUpdatedLoaded && isTamperedLoaded;
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

                return TryChangeState(EPeriodTimerType.Open);
            }

            if (now < _closedEndTime)
                return TryChangeState(EPeriodTimerType.Closed);

            return TryOpenNewPeriod();
        }

        private bool TryOpenNewPeriod()
        {
            NotifyOpenPeriodPreparing();
            if (!TryApplyPeriodTime())
                return false;

            if (!TryChangeState(EPeriodTimerType.Reset, true))
                return false;

            NotifyOpenPeriodStarted();
            return TryChangeState(EPeriodTimerType.Open, true);
        }

        private bool TryApplyPeriodTime()
        {
            double openPeriodMin = _hasPendingPeriodChange ? _nextOpenPeriodMin : _openPeriodMin;
            double closedPeriodMin = _hasPendingPeriodChange ? _nextClosedPeriodMin : _closedPeriodMin;
            DateTime now = GetUtcNow();
            DateTime openEndTime = RemoveMs(now.AddMinutes(openPeriodMin));
            DateTime closedEndTime = RemoveMs(now.AddMinutes(openPeriodMin + closedPeriodMin));
            if (!TrySavePeriod(openEndTime, closedEndTime, now, false))
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
            if (!TrySavePeriod(_openEndTime, _closedEndTime, _openUpdatedTime, true))
                return false;

            _isTamperedFlag = true;
            return TryChangeState(EPeriodTimerType.Closed);
        }

        private bool TryClearTampered()
        {
            DateTime now = GetUtcNow();
            DateTime closedEndTime = RemoveMs(now.AddMinutes(_closedPeriodMin));
            if (!TrySavePeriod(now, closedEndTime, now, false))
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

            return TryChangeState(EPeriodTimerType.Closed, true);
        }

        private bool TrySetClosedPeriodFromNow()
        {
            DateTime now = GetUtcNow();
            DateTime closedEndTime = RemoveMs(now.AddMinutes(_closedPeriodMin));
            if (!TrySavePeriod(now, closedEndTime, now, false))
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
                    _isInit = false;
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
            try
            {
                while (_isInit && _curType != EPeriodTimerType.None)
                {
                    if (TryTickState() && (!_isInit || _curType == EPeriodTimerType.None))
                        yield break;

                    yield return new WaitForSecondsRealtime(GetWaitSec());
                }
            }
            finally
            {
                _coUpdate = null;
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
            switch (_curType)
            {
                case EPeriodTimerType.Open:
                    return Mathf.Clamp(GetRemainingSec(_openEndTime, now), 1, 60);
                case EPeriodTimerType.Closed:
                    return Mathf.Clamp(GetRemainingSec(_closedEndTime, now), 1, 60);
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
            if (!TryUpdateOpenRemainMin(out int remainingMin))
                return false;

            _onRemainMinUpdated?.Invoke(remainingMin);
            return true;
        }

        private bool TryNotifyClosedRemainMinUpdated()
        {
            if (!TryUpdateClosedRemainMin(out int remainingMin))
                return false;

            _onRemainMinUpdated?.Invoke(remainingMin);
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
            switch (_curType)
            {
                case EPeriodTimerType.Open:
                    return GetRemainingMin(_openEndTime, now);
                case EPeriodTimerType.Closed:
                    return GetRemainingMin(_closedEndTime, now);
                default:
                    return 0;
            }
        }

        private int CalculateRemainingSec()
        {
            DateTime now = GetUtcNow();
            switch (_curType)
            {
                case EPeriodTimerType.Open:
                    return GetRemainingSec(_openEndTime, now);
                case EPeriodTimerType.Closed:
                    return GetRemainingSec(_closedEndTime, now);
                default:
                    return 0;
            }
        }

        private DateTime GetUtcNow()
        {
            return RemoveMs(_utcNow.Invoke());
        }

        private static DateTime GetSystemUtcNow()
        {
            return DateTime.UtcNow;
        }

        private static bool IsPositiveFinite(double value)
        {
            return value > 0.0d && !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private bool TryChangeState(EPeriodTimerType type, bool hasSavedTime = false)
        {
            if (!IsKnownState(type))
                return false;
            if (_curType == type)
                return true;

            int remainingMin = 0;
            if (type == EPeriodTimerType.Open)
            {
                if (hasSavedTime)
                    remainingMin = GetRemainingMin(_openEndTime, GetUtcNow());
                else if (!TryUpdateOpenRemainMin(out remainingMin))
                    return false;
            }
            else if (type == EPeriodTimerType.Closed)
            {
                if (_isTamperedFlag)
                {
                    if (!TryClearTampered())
                        return false;

                    remainingMin = GetRemainingMin(_closedEndTime, GetUtcNow());
                }
                else if (hasSavedTime)
                {
                    remainingMin = GetRemainingMin(_closedEndTime, GetUtcNow());
                }
                else if (!TryUpdateClosedRemainMin(out remainingMin))
                {
                    return false;
                }
            }

            EPeriodTimerType prevType = _curType;
            _curType = type;

            if (type == EPeriodTimerType.Open)
            {
                _onRemainMinUpdated?.Invoke(remainingMin);
            }
            else if (type == EPeriodTimerType.Closed)
            {
                NotifyClosedPeriodStarted();
                _onRemainMinUpdated?.Invoke(remainingMin);
            }

            OnStateTransitionCallback(prevType, type);
            return true;
        }

        private bool TryTickState()
        {
            if (_curType == EPeriodTimerType.Open)
            {
                if (IsTampered)
                    return TryHandleTampered();

                if (!IsOpenPeriod)
                    return TryChangeState(EPeriodTimerType.Closed, true);

                return TryNotifyOpenRemainMinUpdated();
            }

            if (_curType != EPeriodTimerType.Closed)
                return false;

            if (_isTamperedFlag)
            {
                if (!TryClearTampered())
                    return false;

                NotifyClosedPeriodStarted();
            }

            if (!IsClosedPeriod)
                return TryOpenNewPeriod();

            return TryNotifyClosedRemainMinUpdated();
        }

        private bool TryUpdateOpenRemainMin(out int remainingMin)
        {
            remainingMin = 0;
            DateTime updatedTime = GetUtcNow();
            if (!TrySavePeriod(_openEndTime, _closedEndTime, updatedTime, _isTamperedFlag))
                return false;

            _openUpdatedTime = updatedTime;
            remainingMin = GetRemainingMin(_openEndTime, updatedTime);
            return true;
        }

        private bool TryUpdateClosedRemainMin(out int remainingMin)
        {
            remainingMin = 0;
            if (!TrySavePeriod(_openEndTime, _closedEndTime, _openUpdatedTime, _isTamperedFlag))
                return false;

            remainingMin = GetRemainingMin(_closedEndTime, GetUtcNow());
            return true;
        }

        private bool TrySavePeriod(DateTime openEndTime, DateTime closedEndTime, DateTime openUpdatedTime, bool isTamperedFlag)
        {
            bool isOpenEndSaved = TrySaveString(GetOpenEndKey(_id), openEndTime.Ticks.ToString(CultureInfo.InvariantCulture));
            bool isClosedEndSaved = TrySaveString(GetClosedEndKey(_id), closedEndTime.Ticks.ToString(CultureInfo.InvariantCulture));
            bool isOpenUpdatedSaved = TrySaveString(GetOpenUpdatedKey(_id), openUpdatedTime.Ticks.ToString(CultureInfo.InvariantCulture));
            bool isTamperedSaved = TrySaveString(GetTamperedKey(_id), isTamperedFlag ? "1" : "0");
            return isOpenEndSaved && isClosedEndSaved && isOpenUpdatedSaved && isTamperedSaved;
        }

        private bool TrySaveString(string key, string value)
        {
            return !string.IsNullOrWhiteSpace(key) && value != null && _storage.TrySave(key, value);
        }

        private bool TryLoadDate(string key, out DateTime value)
        {
            value = DateTime.MinValue;
            if (!TryLoadString(key, out string raw))
                return false;
            if (string.IsNullOrEmpty(raw))
                return true;
            if (!long.TryParse(raw, out long ticks) || ticks < DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Ticks)
                return false;

            value = new DateTime(ticks, DateTimeKind.Utc);
            return true;
        }

        private bool TryLoadString(string key, out string value)
        {
            value = string.Empty;
            if (!_storage.TryHasKey(key, out bool hasKey))
                return false;
            if (!hasKey)
                return true;
            if (!_storage.TryLoad(key, out string raw))
                return false;

            value = raw ?? string.Empty;
            return true;
        }

        private static bool IsKnownState(EPeriodTimerType type)
        {
            return type == EPeriodTimerType.Reset || type == EPeriodTimerType.Open || type == EPeriodTimerType.Closed;
        }

        private static DateTime RemoveMs(DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, time.Kind);
        }

        private static int CompareWithoutMs(DateTime first, DateTime second)
        {
            return RemoveMs(first).CompareTo(RemoveMs(second));
        }

        private static int GetRemainingMin(DateTime targetTime, DateTime utcNow)
        {
            TimeSpan remainingTime = targetTime - RemoveMs(utcNow);
            return remainingTime.TotalMinutes <= 0.0d ? 0 : Mathf.CeilToInt((float)remainingTime.TotalMinutes);
        }

        private static int GetRemainingSec(DateTime targetTime, DateTime utcNow)
        {
            TimeSpan remainingTime = targetTime - RemoveMs(utcNow);
            return remainingTime.TotalSeconds <= 0.0d ? 0 : Mathf.CeilToInt((float)remainingTime.TotalSeconds);
        }

        private static string GetOpenEndKey(string id)
        {
            return $"{STORAGE_PREFIX}{id}{OPEN_END_TIME_SUFFIX}";
        }

        private static string GetClosedEndKey(string id)
        {
            return $"{STORAGE_PREFIX}{id}{CLOSED_END_TIME_SUFFIX}";
        }

        private static string GetOpenUpdatedKey(string id)
        {
            return $"{STORAGE_PREFIX}{id}{OPEN_UPDATED_TIME_SUFFIX}";
        }

        private static string GetTamperedKey(string id)
        {
            return $"{STORAGE_PREFIX}{id}{TAMPERED_SUFFIX}";
        }
    }
}
