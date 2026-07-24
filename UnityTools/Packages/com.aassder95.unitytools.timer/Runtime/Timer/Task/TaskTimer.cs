using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityTools.Timer.Persistence;

namespace UnityTools.Timer.Task
{
    public class TaskTimer : ITaskTimer
    {
        //============================================================
        // Constants
        //============================================================
        private const string STORAGE_PREFIX = "TaskTimer_";
        private const string START_TIME_SUFFIX = "_START";
        private const string UPDATED_TIME_SUFFIX = "_UPDATED";
        private const string DURATION_SUFFIX = "_DURATION";
        private const string STATE_SUFFIX = "_STATE";

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
        private double _durationSec;
        private DateTime _startTime;
        private DateTime _updatedTime;
        private int _savedStateType;
        private ETaskTimerType _curType;
        private bool _hasCurState;
        private Coroutine _coUpdate;

        //============================================================
        // Events
        //============================================================
        public event UnityAction OnProgressStarted { add => _onProgressStarted += value; remove => _onProgressStarted -= value; }
        public event UnityAction<int> OnRemainSecUpdated { add => _onRemainSecUpdated += value; remove => _onRemainSecUpdated -= value; }
        public event UnityAction OnCompleted { add => _onCompleted += value; remove => _onCompleted -= value; }
        public event UnityAction OnClaimed { add => _onClaimed += value; remove => _onClaimed -= value; }
        public event UnityAction<ETaskTimerType, ETaskTimerType> OnStateTransition { add => _onStateTransition += value; remove => _onStateTransition -= value; }
        private event UnityAction _onProgressStarted;
        private event UnityAction<int> _onRemainSecUpdated;
        private event UnityAction _onCompleted;
        private event UnityAction _onClaimed;
        private event UnityAction<ETaskTimerType, ETaskTimerType> _onStateTransition;

        //============================================================
        // Properties
        //============================================================
        public string Id => _id;
        public ETaskTimerType CurType => _curType;
        public int RemainingSec => GetRemainingSec(EndTime, GetUtcNow());
        public int DurationSec => (int)_durationSec;
        public float Progress => CalculateProgress();
        public DateTime EndTime => _startTime.AddSeconds(_durationSec);
        private bool IsPeriodExpired => CompareWithoutMs(GetUtcNow(), EndTime) >= 0;
        private bool IsTampered => CompareWithoutMs(GetUtcNow(), _updatedTime) < 0;

        //============================================================
        // Constructors
        //============================================================
        private TaskTimer(string normalizedId, MonoBehaviour runner, IStorage storage, Func<DateTime> utcNow)
        {
            _id = normalizedId;
            _runner = runner;
            _storage = storage ?? new PlayerPrefsStorage();
            _utcNow = utcNow ?? GetSystemUtcNow;
        }

        //============================================================
        // Init/Register
        //============================================================
        public static bool TryCreate(string id, MonoBehaviour runner, out TaskTimer timer, IStorage storage = null, Func<DateTime> utcNow = null)
        {
            timer = null;
            string normalizedId = id?.Trim();
            if (string.IsNullOrEmpty(normalizedId) || runner == null)
                return false;

            timer = new TaskTimer(normalizedId, runner, storage, utcNow);
            return true;
        }

        public bool TryInit()
        {
            if (_isInit)
                return true;

            if (!TryLoad())
                return false;

            _isInit = true;
            if (TryRefresh())
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
        private bool TryLoad()
        {
            bool isStartLoaded = TryLoadDate(GetStartKey(_id), out _startTime);
            bool isDurationLoaded = TryLoadDouble(GetDurationKey(_id), out _durationSec);
            bool isUpdatedLoaded = TryLoadDate(GetUpdatedKey(_id), out _updatedTime);
            bool isStateLoaded = TryLoadInt(GetStateKey(_id), out _savedStateType);
            return isStartLoaded && isDurationLoaded && isUpdatedLoaded && isStateLoaded;
        }

        private bool TrySaveSnapshot(DateTime startTime, double durationSec, ETaskTimerType stateType, DateTime updatedTime)
        {
            bool isStartSaved = TrySaveString(GetStartKey(_id), startTime.Ticks.ToString(CultureInfo.InvariantCulture));
            bool isDurationSaved = TrySaveString(GetDurationKey(_id), durationSec.ToString(CultureInfo.InvariantCulture));
            bool isStateSaved = TrySaveString(GetStateKey(_id), ((int)stateType).ToString(CultureInfo.InvariantCulture));
            bool isUpdatedSaved = TrySaveString(GetUpdatedKey(_id), updatedTime.Ticks.ToString(CultureInfo.InvariantCulture));
            return isStartSaved && isDurationSaved && isStateSaved && isUpdatedSaved;
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
            if (!TryLoadStateType(out ETaskTimerType type))
                return false;

            if (_startTime == DateTime.MinValue || type == ETaskTimerType.None)
            {
                StopUpdate();
                if (!_hasCurState || _curType != ETaskTimerType.None)
                    ChangeState(ETaskTimerType.None);

                return true;
            }

            if (type == ETaskTimerType.Processing)
            {
                if (IsTampered)
                {
                    DateTime now = GetUtcNow();
                    double adjustedDurationSec = _durationSec + (_updatedTime - now).TotalSeconds;
                    if (!TrySaveSnapshot(_startTime, adjustedDurationSec, ETaskTimerType.Processing, now))
                        return false;

                    _durationSec = adjustedDurationSec;
                    _updatedTime = now;
                }

                if (IsPeriodExpired)
                    return TryUpdateCompletionTime();

                if (!_hasCurState || _curType != ETaskTimerType.Processing)
                    ChangeState(ETaskTimerType.Processing);

                StartUpdate();
                return true;
            }

            StopUpdate();
            if (!_hasCurState || _curType != ETaskTimerType.Completed)
                ChangeState(ETaskTimerType.Completed);

            return true;
        }

        public bool TryStart(double durationSec)
        {
            if (!_isInit || _curType == ETaskTimerType.Processing || !IsPositiveFinite(durationSec))
                return false;

            DateTime now = GetUtcNow();
            double nextDurationSec = durationSec;
            if (_updatedTime != DateTime.MinValue && CompareWithoutMs(now, _updatedTime) < 0)
                nextDurationSec += (_updatedTime - now).TotalSeconds;

            if (!TrySaveSnapshot(now, nextDurationSec, ETaskTimerType.Processing, now))
                return false;

            _startTime = now;
            _durationSec = nextDurationSec;
            _updatedTime = now;
            _savedStateType = (int)ETaskTimerType.Processing;
            ChangeState(ETaskTimerType.Processing);
            StartUpdate();
            return true;
        }

        public bool TryReduce(double reduceSec)
        {
            if (!_isInit || _curType != ETaskTimerType.Processing || !IsPositiveFinite(reduceSec))
                return false;

            DateTime now = GetUtcNow();
            double remainSec = (EndTime - now).TotalSeconds;
            double actualReduceSec = Math.Min(reduceSec, remainSec);
            if (actualReduceSec <= 0.0d)
                return false;

            DateTime nextStartTime = _startTime.AddSeconds(-actualReduceSec);
            if (!TrySaveSnapshot(nextStartTime, _durationSec, ETaskTimerType.Processing, now))
                return false;

            _startTime = nextStartTime;
            _updatedTime = now;
            _onRemainSecUpdated?.Invoke(RemainingSec);
            if (IsPeriodExpired)
                return TryUpdateCompletionTime();

            return true;
        }

        public bool TryComplete()
        {
            return _isInit && _curType == ETaskTimerType.Processing && TryUpdateCompletionTime();
        }

        public bool TryClaim()
        {
            if (!_isInit || _curType != ETaskTimerType.Completed || !TryClearRuntimeData())
                return false;

            ChangeState(ETaskTimerType.None);
            Clear();
            _onClaimed?.Invoke();
            return true;
        }

        public bool TryGetClaimed(out bool isClaimed)
        {
            return TryLoadClaimed(_id, _storage, out isClaimed);
        }

        private bool TryUpdateCompletionTime()
        {
            DateTime updatedTime = GetUtcNow();
            if (!TrySaveSnapshot(_startTime, _durationSec, ETaskTimerType.Completed, updatedTime))
                return false;

            _updatedTime = updatedTime;
            _savedStateType = (int)ETaskTimerType.Completed;
            if (_curType != ETaskTimerType.Completed)
                ChangeState(ETaskTimerType.Completed);

            return true;
        }

        //============================================================
        // Coroutines
        //============================================================
        private IEnumerator CoUpdate()
        {
            try
            {
                while (_isInit && _curType == ETaskTimerType.Processing)
                {
                    if (TryTickState() && (!_isInit || _curType != ETaskTimerType.Processing))
                        yield break;

                    yield return new WaitForSecondsRealtime(1.0f);
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

        //============================================================
        // Callbacks
        //============================================================
        public void NotifyCurType()
        {
            switch (_curType)
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
            if (_curType == ETaskTimerType.Processing)
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
            if (_startTime == DateTime.MinValue)
                return 0.0f;

            if (_curType == ETaskTimerType.Completed || IsPeriodExpired)
                return 1.0f;

            int totalSec = (int)Math.Round(_durationSec);
            int elapsedSec = (int)Math.Round((GetUtcNow() - _startTime).TotalSeconds);
            return totalSec <= 0 ? 0.0f : Mathf.Clamp01((float)elapsedSec / totalSec);
        }

        private bool TryLoadStateType(out ETaskTimerType type)
        {
            type = (ETaskTimerType)_savedStateType;
            if (IsKnownState(type))
                return true;

            Debug.LogError("유효하지 않은 TaskTimer 저장 상태값입니다. 값=" + _savedStateType + ", ID=" + _id);
            return false;
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

        private void ChangeState(ETaskTimerType type)
        {
            if (!IsKnownState(type) || _hasCurState && _curType == type)
                return;

            ETaskTimerType prevType = _curType;
            _curType = type;
            _hasCurState = true;

            if (type == ETaskTimerType.Processing)
                NotifyProcessingStarted();
            else if (type == ETaskTimerType.Completed)
                NotifyCompleted();

            _onStateTransition?.Invoke(prevType, type);
        }

        private bool TryTickState()
        {
            if (_curType != ETaskTimerType.Processing)
                return false;

            if (IsPeriodExpired)
                return TryUpdateCompletionTime();

            NotifyUpdate();
            return true;
        }

        private bool TryClearRuntimeData()
        {
            bool isStartDeleted = _storage.TryDelete(GetStartKey(_id));
            bool isDurationDeleted = _storage.TryDelete(GetDurationKey(_id));
            bool isStateDeleted = _storage.TryDelete(GetStateKey(_id));
            return isStartDeleted && isDurationDeleted && isStateDeleted;
        }

        private bool TrySaveString(string key, string value)
        {
            return !string.IsNullOrWhiteSpace(key) && value != null && _storage.TrySave(key, value);
        }

        private bool TryLoadDate(string key, out DateTime value)
        {
            value = DateTime.MinValue;
            if (!TryLoadRaw(key, out string raw))
                return false;
            if (string.IsNullOrEmpty(raw))
                return true;

            if (!long.TryParse(raw, out long ticks) || ticks < DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Ticks)
                return false;

            value = new DateTime(ticks, DateTimeKind.Utc);
            return true;
        }

        private bool TryLoadDouble(string key, out double value)
        {
            value = 0.0d;
            if (!TryLoadRaw(key, out string raw))
                return false;
            if (string.IsNullOrEmpty(raw))
                return true;

            bool isParsed = double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsedValue) || double.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out parsedValue);
            if (!isParsed || parsedValue < 0.0d || double.IsNaN(parsedValue) || double.IsInfinity(parsedValue))
                return false;

            value = parsedValue;
            return true;
        }

        private bool TryLoadInt(string key, out int value)
        {
            value = 0;
            if (!TryLoadRaw(key, out string raw))
                return false;
            if (string.IsNullOrEmpty(raw))
                return true;

            return int.TryParse(raw, out value);
        }

        private bool TryLoadRaw(string key, out string value)
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

        private static bool TryLoadClaimed(string id, IStorage storage, out bool isClaimed)
        {
            isClaimed = false;
            bool isStartChecked = storage.TryHasKey(GetStartKey(id), out bool hasStart);
            bool isDurationChecked = storage.TryHasKey(GetDurationKey(id), out bool hasDuration);
            bool isStateChecked = storage.TryHasKey(GetStateKey(id), out bool hasState);
            bool isUpdatedChecked = storage.TryHasKey(GetUpdatedKey(id), out bool hasUpdated);
            if (!isStartChecked || !isDurationChecked || !isStateChecked || !isUpdatedChecked)
                return false;

            isClaimed = !hasStart && !hasDuration && !hasState && hasUpdated;
            return true;
        }

        private static bool IsKnownState(ETaskTimerType type)
        {
            return type == ETaskTimerType.None || type == ETaskTimerType.Processing || type == ETaskTimerType.Completed;
        }

        private static DateTime RemoveMs(DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, time.Kind);
        }

        private static int CompareWithoutMs(DateTime first, DateTime second)
        {
            return RemoveMs(first).CompareTo(RemoveMs(second));
        }

        private static int GetRemainingSec(DateTime targetTime, DateTime utcNow)
        {
            TimeSpan remainingTime = targetTime - RemoveMs(utcNow);
            return remainingTime.TotalSeconds <= 0.0d ? 0 : Mathf.CeilToInt((float)remainingTime.TotalSeconds);
        }

        private static string GetStartKey(string id)
        {
            return $"{STORAGE_PREFIX}{id}{START_TIME_SUFFIX}";
        }

        private static string GetUpdatedKey(string id)
        {
            return $"{STORAGE_PREFIX}{id}{UPDATED_TIME_SUFFIX}";
        }

        private static string GetDurationKey(string id)
        {
            return $"{STORAGE_PREFIX}{id}{DURATION_SUFFIX}";
        }

        private static string GetStateKey(string id)
        {
            return $"{STORAGE_PREFIX}{id}{STATE_SUFFIX}";
        }
    }
}
