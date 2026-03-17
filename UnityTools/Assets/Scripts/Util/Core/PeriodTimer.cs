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
        // Constants
        //============================================================
        private const double DEFAULT_PERIOD_MIN = 1d;

        //============================================================
        // Readonly
        //============================================================
        private readonly string _id;
        private readonly EnumStateMachine<EPeriodTimerType> _fsm;
        private readonly MonoBehaviour _runner;
        private readonly IStorage _storage;
        private readonly bool _isEnableLog;

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
        private DateTime _openStartTime;
        private DateTime _openUpdatedTime;
        private DateTime _openEndTime;
        private DateTime _closedEndTime;
        private Coroutine _coUpdate;

        //============================================================
        // Events
        //============================================================
        public event UnityAction OnOpenStarted { add => _onOpenStarted += value; remove => _onOpenStarted -= value; }
        public event UnityAction<int> OnUpdated { add => _onUpdated += value; remove => _onUpdated -= value; }
        public event UnityAction OnClosedStarted { add => _onClosedStarted += value; remove => _onClosedStarted -= value; }
        public event UnityAction<EPeriodTimerType> OnStateChanged { add => _fsm.OnStateChanged += value; remove => _fsm.OnStateChanged -= value; }
        private event UnityAction _onOpenStarted;
        private event UnityAction<int> _onUpdated;
        private event UnityAction _onClosedStarted;

        //============================================================
        // Properties
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
        // Constructors
        //============================================================
        public PeriodTimer(string id, MonoBehaviour runner, bool isEnableLog = false)
        {
            if(!PeriodTimerStorageKeys.TryNormalizeId(id, out _id))
                _id = string.Empty;

            _runner = runner;
            _storage = new PlayerPrefsStorage();
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
        // Init/Register
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
        // Persistence
        //============================================================
        private void Save()
        {
            SaveData(PeriodTimerStorageKeys.OpenStart(_id), _openStartTime.Ticks.ToString());
            SaveData(PeriodTimerStorageKeys.OpenEnd(_id), _openEndTime.Ticks.ToString());
            SaveData(PeriodTimerStorageKeys.ClosedEnd(_id), _closedEndTime.Ticks.ToString());
            SaveData(PeriodTimerStorageKeys.OpenUpdated(_id), _openUpdatedTime.Ticks.ToString());
            SaveData(PeriodTimerStorageKeys.Tampered(_id), _isTamperedFlag ? "1" : "0");
        }

        private void Load()
        {
            _openStartTime = TryLoadDate(PeriodTimerStorageKeys.OpenStart(_id));
            _openEndTime = TryLoadDate(PeriodTimerStorageKeys.OpenEnd(_id));
            _closedEndTime = TryLoadDate(PeriodTimerStorageKeys.ClosedEnd(_id));
            _openUpdatedTime = TryLoadDate(PeriodTimerStorageKeys.OpenUpdated(_id));
            _isTamperedFlag = LoadData(PeriodTimerStorageKeys.Tampered(_id)) == "1";
        }

        private void SaveData(string key, string value)
        {
            _storage.Save(key, value);
        }

        private string LoadData(string key)
        {
            return _storage.HasKey(key) ? _storage.Load(key) : "0";
        }

        private DateTime TryLoadDate(string key)
        {
            string raw = LoadData(key);
            if(!string.IsNullOrEmpty(raw) && long.TryParse(raw, out long ticks))
            {
                if(ticks == DateTime.MinValue.Ticks)
                    return DateTime.MinValue;

                if(ticks < DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Ticks)
                {
                    LogTest("TryLoadDate", $"유효하지 않은 ticks={ticks}, key={key}");
                    return DateTime.MinValue;
                }

                return new DateTime(ticks, DateTimeKind.Utc);
            }

            return DateTime.MinValue;
        }

        //============================================================
        // Logic
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
            Save();
        }

        public void HandleTampered()
        {
            _isTamperedFlag = true;
            TryChangeState(EPeriodTimerType.Closed, false, "HandleTampered");
            Save();
        }

        public void ClearTampered()
        {
            _isTamperedFlag = false;
            SetClosedPeriodFromNow();
            Save();
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
            Save();
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
        // Coroutines
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
                    return Mathf.Clamp(GetRemainingSec(_openEndTime), 1, 60);
                case EPeriodTimerType.Closed:
                    return Mathf.Clamp(GetRemainingSec(_closedEndTime), 1, 60);
                default:
                    return 1;
            }
        }

        //============================================================
        // Callbacks
        //============================================================
        public void NotifyOpenStarted()
        {
            _onOpenStarted?.Invoke();
        }

        public void NotifyUpdateOpen()
        {
            _openUpdatedTime = DateTimeUtils.RemoveMilliseconds(DateTime.UtcNow);
            Save();

            int remainMin = GetRemainingMin(_openEndTime);
            _onUpdated?.Invoke(remainMin);
        }

        public void NotifyUpdateClosed()
        {
            Save();

            int remainMin = GetRemainingMin(_closedEndTime);
            _onUpdated?.Invoke(remainMin);
        }

        public void NotifyClosedStarted()
        {
            _onClosedStarted?.Invoke();
        }

        //============================================================
        // Utilities
        //============================================================
        public int GetRemainingMin()
        {
            switch (_fsm.CurType)
            {
                case EPeriodTimerType.Open:
                    return GetRemainingMin(_openEndTime);
                case EPeriodTimerType.Closed:
                    return GetRemainingMin(_closedEndTime);
                default:
                    return 0;
            }
        }

        public int GetRemainingSec()
        {
            switch (_fsm.CurType)
            {
                case EPeriodTimerType.Open:
                    return GetRemainingSec(_openEndTime);
                case EPeriodTimerType.Closed:
                    return GetRemainingSec(_closedEndTime);
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

        private static int GetRemainingMin(DateTime targetTime)
        {
            return DateTimeUtils.GetRemainingMinutes(targetTime);
        }

        private static int GetRemainingSec(DateTime targetTime)
        {
            return DateTimeUtils.GetRemainingSeconds(targetTime);
        }
    }
}
