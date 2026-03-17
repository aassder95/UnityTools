using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;

namespace UnityTools.Util
{
    public enum ETaskTimerType
    {
        None,
        Processing,
        Completed
    }

    public class TaskTimer
    {
        //============================================================
        //Constants
        //============================================================
        private const double DEFAULT_DURATION_SEC = 1d;
        private const double SEC_PER_MIN = 60d;

        //============================================================
        //Readonly
        //============================================================
        private readonly string _id;
        private readonly EnumStateMachine<ETaskTimerType> _fsm;
        private readonly MonoBehaviour _runner;
        private readonly TaskTimerPersistence _persistence;
        private readonly bool _isEnableLog;

        //============================================================
        //Fields
        //============================================================
        private bool _isInit;
        private double _durationSec;
        private DateTime _startTime;
        private DateTime _updatedTime;
        private int _savedStateType;
        private Coroutine _coUpdate;

        //============================================================
        //Events
        //============================================================
        public event UnityAction OnProgressStarted { add => _onProgressStarted += value; remove => _onProgressStarted -= value; }
        public event UnityAction<int> OnUpdated { add => _onUpdated += value; remove => _onUpdated -= value; }
        public event UnityAction OnCompleted { add => _onCompleted += value; remove => _onCompleted -= value; }
        public event UnityAction OnClaimed { add => _onClaimed += value; remove => _onClaimed -= value; }
        public event UnityAction<ETaskTimerType> OnStateChanged { add => _fsm.OnStateChanged += value; remove => _fsm.OnStateChanged -= value; }
        private event UnityAction _onProgressStarted;
        private event UnityAction<int> _onUpdated;
        private event UnityAction _onCompleted;
        private event UnityAction _onClaimed;

        //============================================================
        //Properties
        //============================================================
        public string Id => _id;
        public EnumStateMachine<ETaskTimerType> FSM => _fsm;
        public int RemainingSec => DateTimeUtils.GetRemainingSeconds(EndTime);
        public int DurationSec => (int)_durationSec;
        public bool IsClaimed => _persistence.IsClaimed();
        public bool IsPeriodExpired => DateTimeUtils.CompareWithoutMilliseconds(DateTime.UtcNow, EndTime) >= 0;
        public DateTime EndTime => _startTime.AddSeconds(_durationSec);
        private bool IsTampered => DateTimeUtils.CompareWithoutMilliseconds(DateTime.UtcNow, _updatedTime) < 0;

        //============================================================
        //Constructors
        //============================================================
        public TaskTimer(string id, MonoBehaviour runner, bool isEnableLog = false)
        {
            if(!TaskTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
                normalizedId = string.Empty;

            _id = normalizedId;
            _runner = runner;
            _persistence = new TaskTimerPersistence(_id);
            _isEnableLog = isEnableLog;
            _fsm = new EnumStateMachine<ETaskTimerType>(false, ESameStateTransitionPolicy.Ignore);

            if(!_fsm.Add(ETaskTimerType.None, new TaskTimerStates.NoneState(this)))
                LogTest("Ctor", $"상태 등록 실패: {ETaskTimerType.None}");
            if(!_fsm.Add(ETaskTimerType.Processing, new TaskTimerStates.ProcessingState(this)))
                LogTest("Ctor", $"상태 등록 실패: {ETaskTimerType.Processing}");
            if(!_fsm.Add(ETaskTimerType.Completed, new TaskTimerStates.CompletedState(this)))
                LogTest("Ctor", $"상태 등록 실패: {ETaskTimerType.Completed}");
        }

        //============================================================
        //Init/Register
        //============================================================
        public void Init()
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

            Load();
            _isInit = true;
            Refresh();
        }

        public void Release()
        {
            if(!_isInit)
                return;

            StopUpdate();
            _isInit = false;
            _onProgressStarted = null;
            _onUpdated = null;
            _onCompleted = null;
            _onClaimed = null;
        }

        //============================================================
        //Persistence
        //============================================================
        private void Save()
        {
            _updatedTime = DateTimeUtils.RemoveMilliseconds(DateTime.UtcNow);
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
        //Logic
        //============================================================
        public void Refresh()
        {
            ETaskTimerType type = LoadStateType();
            if(_startTime == DateTime.MinValue || type == ETaskTimerType.None)
            {
                StopUpdate();
                if(!_fsm.HasCurrentState || _fsm.CurType != ETaskTimerType.None)
                    TryChangeState(ETaskTimerType.None, false, "Refresh");

                return;
            }

            switch (type)
            {
                case ETaskTimerType.Processing:
                    if(IsTampered)
                    {
                        double adjustSec = (_updatedTime - DateTime.UtcNow).TotalSeconds;
                        _durationSec += adjustSec;
                        _persistence.SaveDuration(_durationSec);
                    }

                    if(IsPeriodExpired)
                    {
                        UpdateCompletionTime();
                        return;
                    }

                    if(!_fsm.HasCurrentState || _fsm.CurType != ETaskTimerType.Processing)
                        TryChangeState(ETaskTimerType.Processing, false, "Refresh");

                    StartUpdate();
                    break;

                case ETaskTimerType.Completed:
                    StopUpdate();
                    if(!_fsm.HasCurrentState || _fsm.CurType != ETaskTimerType.Completed)
                        TryChangeState(ETaskTimerType.Completed, false, "Refresh");
                    break;

                default:
                    StopUpdate();
                    if(!_fsm.HasCurrentState || _fsm.CurType != ETaskTimerType.None)
                        TryChangeState(ETaskTimerType.None, false, "Refresh");
                    break;
            }
        }

        public bool Start(double durationSec)
        {
            if(!_isInit || _fsm.CurType == ETaskTimerType.Processing)
                return false;

            _startTime = DateTimeUtils.RemoveMilliseconds(DateTime.UtcNow);
            double safeDurationSec = SanitizeDuration(durationSec);
            if(_updatedTime != DateTime.MinValue && IsTampered)
            {
                double adjustSec = (_updatedTime - DateTime.UtcNow).TotalSeconds;
                _durationSec = safeDurationSec + adjustSec;
            }
            else
            {
                _durationSec = safeDurationSec;
            }

            if(!TryChangeState(ETaskTimerType.Processing, false, "Start"))
                return false;

            Save();
            StartUpdate();
            return true;
        }

        public bool StartMinutes(double durationMin)
        {
            if(double.IsNaN(durationMin) || double.IsInfinity(durationMin))
                return false;

            return Start(durationMin * SEC_PER_MIN);
        }

        public bool Reduce(double reduceSec)
        {
            if(!_isInit || _fsm.CurType != ETaskTimerType.Processing)
                return false;

            if(reduceSec <= 0d || double.IsNaN(reduceSec) || double.IsInfinity(reduceSec))
                return false;

            double remainSec = (EndTime - DateTime.UtcNow).TotalSeconds;
            double actualReduceSec = Math.Min(reduceSec, remainSec);
            if(actualReduceSec <= 0d)
                return false;

            _startTime = _startTime.AddSeconds(-actualReduceSec);
            Save();

            _onUpdated?.Invoke(RemainingSec);

            if(IsPeriodExpired)
                UpdateCompletionTime();

            return true;
        }

        public bool ReduceMinutes(double reduceMin)
        {
            if(double.IsNaN(reduceMin) || double.IsInfinity(reduceMin))
                return false;

            return Reduce(reduceMin * SEC_PER_MIN);
        }

        public bool CompleteImmediately()
        {
            if(!_isInit || _fsm.CurType != ETaskTimerType.Processing)
                return false;

            UpdateCompletionTime();
            return true;
        }

        public bool Claim()
        {
            if(!_isInit || _fsm.CurType != ETaskTimerType.Completed)
                return false;

            Clear();
            if(!TryChangeState(ETaskTimerType.None, false, "Claim"))
                return false;

            _onClaimed?.Invoke();
            return true;
        }

        public void UpdateCompletionTime()
        {
            _updatedTime = DateTimeUtils.RemoveMilliseconds(DateTime.UtcNow);
            _persistence.SaveUpdated(_updatedTime);

            if(_fsm.CurType != ETaskTimerType.Completed)
            {
                if(!TryChangeState(ETaskTimerType.Completed, false, "UpdateCompletion"))
                    return;
            }

            _persistence.SaveState(ETaskTimerType.Completed);
        }

        //============================================================
        //Coroutines
        //============================================================
        private IEnumerator CoUpdate()
        {
            while (_isInit && _fsm.CurType == ETaskTimerType.Processing)
            {
                _fsm.Update();
                if(!_isInit || _fsm.CurType != ETaskTimerType.Processing)
                    yield break;

                yield return new WaitForSecondsRealtime(1f);
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
        //Callbacks
        //============================================================
        public void NotifyUpdate()
        {
            if(_fsm.CurType == ETaskTimerType.Processing)
                _onUpdated?.Invoke(RemainingSec);
        }

        public void NotifyProcessingStarted()
        {
            if(_fsm.CurType == ETaskTimerType.Processing)
                _onProgressStarted?.Invoke();
        }

        public void NotifyCompleted()
        {
            if(_fsm.CurType != ETaskTimerType.Completed)
                return;

            StopUpdate();
            _onCompleted?.Invoke();
        }

        public void NotifyCurType()
        {
            switch (_fsm.CurType)
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
        //Utilities
        //============================================================
        public float GetProgress()
        {
            if(_startTime == DateTime.MinValue)
                return 0f;

            if(_fsm.CurType == ETaskTimerType.Completed || IsPeriodExpired)
                return 1f;

            int totalSec = (int)Math.Round(_durationSec);
            int elapsedSec = (int)Math.Round((DateTime.UtcNow - _startTime).TotalSeconds);
            return totalSec <= 0 ? 0f : Mathf.Clamp01((float)elapsedSec / totalSec);
        }

        public static bool IsClaimedStatic(string id)
        {
            if(!TaskTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
                return false;

            IStorage storage = new PlayerPrefsStorage();
            bool hasStart = storage.HasKey(TaskTimerStorageKeys.Start(normalizedId));
            bool hasDuration = storage.HasKey(TaskTimerStorageKeys.Duration(normalizedId));
            bool hasState = storage.HasKey(TaskTimerStorageKeys.State(normalizedId));
            bool hasUpdated = storage.HasKey(TaskTimerStorageKeys.Updated(normalizedId));
            return !hasStart && !hasDuration && !hasState && hasUpdated;
        }

        private ETaskTimerType LoadStateType()
        {
            ETaskTimerType type = (ETaskTimerType)_savedStateType;
            if(_fsm.HasState(type))
                return type;

            if(_savedStateType != 0)
                LogTest("LoadStateType", $"유효하지 않은 저장 상태값: {_savedStateType}");
            return ETaskTimerType.None;
        }

        private double SanitizeDuration(double durationSec)
        {
            if(durationSec > 0d && !double.IsNaN(durationSec) && !double.IsInfinity(durationSec))
                return durationSec;

            LogTest("SanitizeDuration", $"유효하지 않은 duration={durationSec}, 기본값 {DEFAULT_DURATION_SEC}초를 사용합니다.");
            return DEFAULT_DURATION_SEC;
        }

        private bool TryChangeState(ETaskTimerType type, bool isUpdate = false, string method = "TryChangeState")
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

        private void LogTest(string method, string msg)
        {
            if(_isEnableLog)
                Debug.LogWarning($"[TaskTimer:{method}] {msg}");
        }
    }

    // Exception: storage snapshot/persistence types are grouped in Types section.
    //============================================================
    //Types
    //============================================================
    public class TaskTimerStorageSnapshot
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly DateTime _startTime;
        private readonly double _durationSec;
        private readonly DateTime _updatedTime;
        private readonly int _savedStateType;

        //============================================================
        //Properties
        //============================================================
        public DateTime StartTime => _startTime;
        public double DurationSec => _durationSec;
        public DateTime UpdatedTime => _updatedTime;
        public int SavedStateType => _savedStateType;

        //============================================================
        //Constructors
        //============================================================
        public TaskTimerStorageSnapshot(DateTime startTime, double durationSec, DateTime updatedTime, int savedStateType)
        {
            _startTime = startTime;
            _durationSec = durationSec;
            _updatedTime = updatedTime;
            _savedStateType = savedStateType;
        }
    }

    public class TaskTimerPersistence
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly string _id;
        private readonly IStorage _storage;

        //============================================================
        //Constructors
        //============================================================
        public TaskTimerPersistence(string id)
        {
            if(!TaskTimerStorageKeys.TryNormalizeId(id, out string normalizedId))
                normalizedId = string.Empty;

            _id = normalizedId;
            _storage = new PlayerPrefsStorage();
        }

        //============================================================
        //Persistence
        //============================================================
        public void Save(DateTime startTime, double durationSec, ETaskTimerType stateType, DateTime updatedTime)
        {
            SaveString(TaskTimerStorageKeys.Start(_id), startTime.Ticks.ToString());
            SaveString(TaskTimerStorageKeys.Duration(_id), durationSec.ToString(CultureInfo.InvariantCulture));
            SaveString(TaskTimerStorageKeys.State(_id), ((int)stateType).ToString());
            SaveString(TaskTimerStorageKeys.Updated(_id), updatedTime.Ticks.ToString());
        }

        public void SaveDuration(double durationSec)
        {
            string key = TaskTimerStorageKeys.Duration(_id);
            if(string.IsNullOrEmpty(key))
                return;

            _storage.Save(key, durationSec.ToString(CultureInfo.InvariantCulture));
        }

        public void SaveState(ETaskTimerType stateType)
        {
            string key = TaskTimerStorageKeys.State(_id);
            if(string.IsNullOrEmpty(key))
                return;

            _storage.Save(key, ((int)stateType).ToString());
        }

        public void SaveUpdated(DateTime updatedTime)
        {
            string key = TaskTimerStorageKeys.Updated(_id);
            if(string.IsNullOrEmpty(key))
                return;

            _storage.Save(key, updatedTime.Ticks.ToString());
        }

        public TaskTimerStorageSnapshot Load()
        {
            DateTime startTime = TryLoadDate(TaskTimerStorageKeys.Start(_id));
            double durationSec = TryLoadDouble(TaskTimerStorageKeys.Duration(_id));
            DateTime updatedTime = TryLoadDate(TaskTimerStorageKeys.Updated(_id));
            int savedStateType = TryLoadInt(TaskTimerStorageKeys.State(_id));
            return new TaskTimerStorageSnapshot(startTime, durationSec, updatedTime, savedStateType);
        }

        public void ClearRuntimeData()
        {
            _storage.Delete(TaskTimerStorageKeys.Start(_id));
            _storage.Delete(TaskTimerStorageKeys.Duration(_id));
            _storage.Delete(TaskTimerStorageKeys.State(_id));
        }

        public bool IsClaimed()
        {
            bool hasStart = HasKey(TaskTimerStorageKeys.Start(_id));
            bool hasDuration = HasKey(TaskTimerStorageKeys.Duration(_id));
            bool hasState = HasKey(TaskTimerStorageKeys.State(_id));
            bool hasUpdated = HasKey(TaskTimerStorageKeys.Updated(_id));
            return !hasStart && !hasDuration && !hasState && hasUpdated;
        }

        //============================================================
        //Utilities
        //============================================================
        private void SaveString(string key, string value)
        {
            if(string.IsNullOrEmpty(key))
                return;

            _storage.Save(key, value ?? string.Empty);
        }

        private string LoadString(string key)
        {
            if(!HasKey(key))
                return string.Empty;

            return _storage.Load(key);
        }

        private bool HasKey(string key)
        {
            if(string.IsNullOrEmpty(key))
                return false;

            return _storage.HasKey(key);
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

        private double TryLoadDouble(string key)
        {
            string raw = LoadString(key);
            bool isSuccess = double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) ||
                             double.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
            if(!isSuccess || double.IsNaN(value) || double.IsInfinity(value))
                return 0d;

            return Math.Max(0d, value);
        }

        private int TryLoadInt(string key)
        {
            string raw = LoadString(key);
            if(!int.TryParse(raw, out int value))
                return 0;

            return value;
        }
    }
}
