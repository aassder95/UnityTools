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
        private readonly EnumStateMachine<EPeriodTimerType> _fsm;
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
        private DateTime _openStartTime;
        private DateTime _openUpdatedTime;
        private DateTime _openEndTime;
        private DateTime _closedEndTime;
        private Coroutine _coUpdate;

        //============================================================
        //Events
        //============================================================
        public event UnityAction OnOpenStarted { add => _onOpenStarted += value; remove => _onOpenStarted -= value; }
        public event UnityAction<int> OnUpdated { add => _onUpdated += value; remove => _onUpdated -= value; }
        public event UnityAction OnClosedStarted { add => _onClosedStarted += value; remove => _onClosedStarted -= value; }
        public event UnityAction<EPeriodTimerType> OnStateChanged { add => _fsm.OnStateChanged += value; remove => _fsm.OnStateChanged -= value; }
        private event UnityAction _onOpenStarted;
        private event UnityAction<int> _onUpdated;
        private event UnityAction _onClosedStarted;

        //============================================================
        //Properties
        //============================================================
        public string Id => _id;
        public EnumStateMachine<EPeriodTimerType> FSM => _fsm;
        public bool IsTamperedFlag => _isTamperedFlag;
        public bool IsOpenPeriod => DateTimeUtils.CompareWithoutMilliseconds(DateTime.UtcNow, _openEndTime) < 0;
        public bool IsClosedPeriod => DateTimeUtils.CompareWithoutMilliseconds(DateTime.UtcNow, _closedEndTime) < 0;
        public bool IsTampered => DateTimeUtils.CompareWithoutMilliseconds(DateTime.UtcNow, _openUpdatedTime) < 0;
        public DateTime OpenStartTime => _openStartTime;
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
            _fsm = new EnumStateMachine<EPeriodTimerType>(false, ESameStateTransitionPolicy.ReEnter);

            if(!_fsm.Add(EPeriodTimerType.Reset, new PeriodTimerStates.ResetState(this)))
                LogTest("Ctor", $"상태 등록 실패: {EPeriodTimerType.Reset}");
            if(!_fsm.Add(EPeriodTimerType.Open, new PeriodTimerStates.OpenState(this)))
                LogTest("Ctor", $"상태 등록 실패: {EPeriodTimerType.Open}");
            if(!_fsm.Add(EPeriodTimerType.Closed, new PeriodTimerStates.ClosedState(this)))
                LogTest("Ctor", $"상태 등록 실패: {EPeriodTimerType.Closed}");
        }

        //============================================================
        //Init/Register
        //============================================================
        public void Init(double openMin, double closedMin)
        {
            if(string.IsNullOrEmpty(_id))
            {
                LogTest("Init", "유효하지 않은 ID라 초기화를 무시합니다.");
                return;
            }

            if(_runner == null)
            {
                LogTest("Init", "러너가 null이라 초기화를 무시합니다.");
                return;
            }

            if(_isInit)
                return;

            _openPeriodMin = SanitizePeriod(openMin, nameof(openMin));
            _closedPeriodMin = SanitizePeriod(closedMin, nameof(closedMin));

            Load();
            _isInit = true;
            Refresh();
            StartUpdate();
        }

        public void Release()
        {
            if(!_isInit)
                return;

            StopUpdate();
            _isInit = false;
            _onOpenStarted = null;
            _onUpdated = null;
            _onClosedStarted = null;
        }

        //============================================================
        //Persistence
        //============================================================
        private void Load()
        {
            PeriodTimerStorageSnapshot snapshot = _persistence.Load();
            _openStartTime = snapshot.OpenStartTime;
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
            _openStartTime = now;
            _openUpdatedTime = now;
            _openEndTime = DateTimeUtils.RemoveMilliseconds(now.AddMinutes(_openPeriodMin));
            _closedEndTime = DateTimeUtils.RemoveMilliseconds(now.AddMinutes(_openPeriodMin + _closedPeriodMin));
            _persistence.Save(_openStartTime, _openEndTime, _closedEndTime, _openUpdatedTime, _isTamperedFlag);
        }

        public void HandleTampered()
        {
            _isTamperedFlag = true;
            TryChangeState(EPeriodTimerType.Closed, false, "HandleTampered");
            _persistence.Save(_openStartTime, _openEndTime, _closedEndTime, _openUpdatedTime, _isTamperedFlag);
        }

        public void ClearTampered()
        {
            _isTamperedFlag = false;
            SetClosedPeriodFromNow();
            _persistence.Save(_openStartTime, _openEndTime, _closedEndTime, _openUpdatedTime, _isTamperedFlag);
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
            _persistence.Save(_openStartTime, _openEndTime, _closedEndTime, _openUpdatedTime, _isTamperedFlag);
        }

        public bool TryChangeState(EPeriodTimerType type, bool isUpdate = false, string method = "TryChangeState")
        {
            if(!_fsm.HasState(type))
            {
                LogTest(method, $"등록되지 않은 상태 전이 요청: {type}");
                return false;
            }

            if(!_fsm.HasCurrentState)
            {
                if(_fsm.SetInitialState(type, isUpdate))
                    return true;

                LogTest(method, $"초기 상태 설정 실패: {type}");
                return false;
            }

            if(_fsm.Change(type, out EStateTransitionFailReason reason, isUpdate))
                return true;

            LogTest(method, $"상태 전이 실패: {_fsm.CurType} -> {type}, reason={reason}");
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
            _openStartTime = now;
            _openUpdatedTime = now;
            _openEndTime = now;
            _closedEndTime = DateTimeUtils.RemoveMilliseconds(now.AddMinutes(_closedPeriodMin));
        }

        //============================================================
        //Coroutines
        //============================================================
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
        public void NotifyOpenStarted()
        {
            _onOpenStarted?.Invoke();
        }

        public void NotifyUpdateOpen()
        {
            _openUpdatedTime = DateTimeUtils.RemoveMilliseconds(DateTime.UtcNow);
            _persistence.Save(_openStartTime, _openEndTime, _closedEndTime, _openUpdatedTime, _isTamperedFlag);

            int remainMin = DateTimeUtils.GetRemainingMinutes(_openEndTime);
            _onUpdated?.Invoke(remainMin);
        }

        public void NotifyUpdateClosed()
        {
            _persistence.Save(_openStartTime, _openEndTime, _closedEndTime, _openUpdatedTime, _isTamperedFlag);

            int remainMin = DateTimeUtils.GetRemainingMinutes(_closedEndTime);
            _onUpdated?.Invoke(remainMin);
        }

        public void NotifyClosedStarted()
        {
            _onClosedStarted?.Invoke();
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

        private void LogTest(string method, string msg)
        {
            if(_isEnableLog)
                Debug.LogWarning($"[PeriodTimer:{method}] {msg}");
        }

        private double SanitizePeriod(double min, string name)
        {
            if(min > 0d && !double.IsNaN(min) && !double.IsInfinity(min))
                return min;

            LogTest("SanitizePeriod", $"유효하지 않은 {name}={min}, 기본값 {DEFAULT_PERIOD_MIN}분을 사용합니다.");
            return DEFAULT_PERIOD_MIN;
        }
    }

    public class PeriodTimerStorageSnapshot
    {
        private readonly DateTime _openStartTime;
        private readonly DateTime _openEndTime;
        private readonly DateTime _closedEndTime;
        private readonly DateTime _openUpdatedTime;
        private readonly bool _isTamperedFlag;

        public DateTime OpenStartTime => _openStartTime;
        public DateTime OpenEndTime => _openEndTime;
        public DateTime ClosedEndTime => _closedEndTime;
        public DateTime OpenUpdatedTime => _openUpdatedTime;
        public bool IsTamperedFlag => _isTamperedFlag;

        public PeriodTimerStorageSnapshot(
            DateTime openStartTime,
            DateTime openEndTime,
            DateTime closedEndTime,
            DateTime openUpdatedTime,
            bool isTamperedFlag)
        {
            _openStartTime = openStartTime;
            _openEndTime = openEndTime;
            _closedEndTime = closedEndTime;
            _openUpdatedTime = openUpdatedTime;
            _isTamperedFlag = isTamperedFlag;
        }
    }

    public class PeriodTimerPersistence
    {
        private readonly string _id;
        private readonly IStorage _storage;

        public PeriodTimerPersistence(string id)
        {
            if(!PeriodTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
                normalizedId = string.Empty;

            _id = normalizedId;
            _storage = new PlayerPrefsStorage();
        }

        public void Save(
            DateTime openStartTime,
            DateTime openEndTime,
            DateTime closedEndTime,
            DateTime openUpdatedTime,
            bool isTamperedFlag)
        {
            SaveString(PeriodTimerStorageKeys.OpenStart(_id), openStartTime.Ticks.ToString());
            SaveString(PeriodTimerStorageKeys.OpenEnd(_id), openEndTime.Ticks.ToString());
            SaveString(PeriodTimerStorageKeys.ClosedEnd(_id), closedEndTime.Ticks.ToString());
            SaveString(PeriodTimerStorageKeys.OpenUpdated(_id), openUpdatedTime.Ticks.ToString());
            SaveString(PeriodTimerStorageKeys.Tampered(_id), isTamperedFlag ? "1" : "0");
        }

        public PeriodTimerStorageSnapshot Load()
        {
            DateTime openStartTime = TryLoadDate(PeriodTimerStorageKeys.OpenStart(_id));
            DateTime openEndTime = TryLoadDate(PeriodTimerStorageKeys.OpenEnd(_id));
            DateTime closedEndTime = TryLoadDate(PeriodTimerStorageKeys.ClosedEnd(_id));
            DateTime openUpdatedTime = TryLoadDate(PeriodTimerStorageKeys.OpenUpdated(_id));
            bool isTamperedFlag = LoadString(PeriodTimerStorageKeys.Tampered(_id)) == "1";
            return new PeriodTimerStorageSnapshot(openStartTime, openEndTime, closedEndTime, openUpdatedTime, isTamperedFlag);
        }

        private void SaveString(string key, string value)
        {
            if(string.IsNullOrEmpty(key))
                return;

            _storage.Save(key, value ?? string.Empty);
        }

        private string LoadString(string key)
        {
            if(string.IsNullOrEmpty(key) || !_storage.HasKey(key))
                return string.Empty;

            return _storage.Load(key);
        }

        private DateTime TryLoadDate(string key)
        {
            string raw = LoadString(key);
            if(!long.TryParse(raw, out long ticks))
                return DateTime.MinValue;

            if(ticks == DateTime.MinValue.Ticks)
                return DateTime.MinValue;
            if(ticks < DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Ticks)
                return DateTime.MinValue;

            return new DateTime(ticks, DateTimeKind.Utc);
        }
    }
}
