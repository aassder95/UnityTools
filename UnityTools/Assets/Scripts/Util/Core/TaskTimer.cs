using System;
using System.Collections;
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
        private const string START_TIME_SUFFIX = "_START";
        private const string UPDATED_TIME_SUFFIX = "_UPDATED";
        private const string DURATION_SUFFIX = "_DURATION";
        private const string STATE_SUFFIX = "_STATE";

        private readonly string _id;
        private readonly Persistence _ps;
        private readonly StateMachine<ETaskTimerType> _fsm;
        private readonly MonoBehaviour _runner;

        private bool _init;
        private double _durationMin;
        private DateTime _startTime;
        private DateTime _updatedTime;
        private Coroutine _coUpdate;

        public StateMachine<ETaskTimerType> FSM => _fsm;
        public ETaskTimerType CurType => _fsm.CurType;
        public int RemainingSec => DateTimeUtils.GetRemainingSeconds(EndTime);
        private DateTime EndTime => _startTime.AddMinutes(_durationMin);

        private bool IsTimeTampered => DateTimeUtils.CompareWithoutMilliseconds(DateTime.UtcNow, _updatedTime) < 0;
        public bool IsPeriodExpired => DateTimeUtils.CompareWithoutMilliseconds(DateTime.UtcNow, EndTime) >= 0;

        public event UnityAction<int> OnUpdated;
        public event UnityAction OnCompleted;
        public event UnityAction OnClaimed;
        public event UnityAction<ETaskTimerType> OnStateChanged
        {
            add => _fsm.OnStateChanged += value;
            remove => _fsm.OnStateChanged -= value;
        }

        public TaskTimer(string rootKey, string id, MonoBehaviour runner)
        {
            _id = id;
            _ps = new Persistence($"{rootKey}_{id}");
            _fsm = new StateMachine<ETaskTimerType>();
            _runner = runner;

            _fsm.Add(ETaskTimerType.None, new TaskTimerStates.NoneState(this));
            _fsm.Add(ETaskTimerType.Processing, new TaskTimerStates.ProcessingState(this));
            _fsm.Add(ETaskTimerType.Completed, new TaskTimerStates.CompletedState(this));
        }

        private void Save()
        {
            _ps.Save(START_TIME_SUFFIX, _startTime.Ticks.ToString());
            _ps.Save(DURATION_SUFFIX, _durationMin.ToString());
            _ps.Save(STATE_SUFFIX, ((int)_fsm.CurType).ToString());
            _ps.Save(UPDATED_TIME_SUFFIX, DateTime.UtcNow.Ticks.ToString());
        }

        private void Load()
        {
            string startStr = _ps.Load(START_TIME_SUFFIX, "0");
            string durationStr = _ps.Load(DURATION_SUFFIX, "0");
            string updatedStr = _ps.Load(UPDATED_TIME_SUFFIX, "0");

            _startTime = !string.IsNullOrEmpty(startStr) && long.TryParse(startStr, out long startTicks) ? new DateTime(startTicks, DateTimeKind.Utc) : DateTime.MinValue;
            _durationMin = !string.IsNullOrEmpty(durationStr) && double.TryParse(durationStr, out double duration) ? duration : 0;
            _updatedTime = !string.IsNullOrEmpty(updatedStr) && long.TryParse(updatedStr, out long updatedTicks) ? new DateTime(updatedTicks, DateTimeKind.Utc) : DateTime.MinValue;
        }

        private void Clear()
        {
            _ps.Delete(START_TIME_SUFFIX);
            _ps.Delete(DURATION_SUFFIX);
            _ps.Delete(STATE_SUFFIX);

            _startTime = DateTime.MinValue;
            _durationMin = 0;
        }

        public void Init()
        {
            if(_init)
                return;

            Load();
            _init = true;

            if(!int.TryParse(_ps.Load(STATE_SUFFIX, "0"), out int savedType))
                savedType = 0;

            ETaskTimerType type = (ETaskTimerType)savedType;
            if(_startTime == DateTime.MinValue || type == ETaskTimerType.None)
            {
                _fsm.Change(ETaskTimerType.None);
                return;
            }

            if(type == ETaskTimerType.Processing)
            {
                if(IsTimeTampered)
                    _durationMin += (_updatedTime - DateTime.UtcNow).TotalMinutes;

                if(IsPeriodExpired)
                {
                    _updatedTime = DateTimeUtils.RemoveMilliseconds(DateTime.UtcNow);
                    _ps.Save(UPDATED_TIME_SUFFIX, _updatedTime.Ticks.ToString());
                    _fsm.Change(ETaskTimerType.Completed);
                    _ps.Save(STATE_SUFFIX, ((int)ETaskTimerType.Completed).ToString());
                }
                else
                {
                    _fsm.Change(ETaskTimerType.Processing);
                    StartUpdate();
                }
            }
            else if(type == ETaskTimerType.Completed)
            {
                _fsm.Change(ETaskTimerType.Completed);
            }
        }

        public void Release()
        {
            if(!_init)
                return;

            StopUpdate();
            _init = false;

            OnUpdated = null;
            OnCompleted = null;
            OnClaimed = null;
        }

        private IEnumerator CoUpdate()
        {
            while (_fsm.CurType == ETaskTimerType.Processing)
            {
                _fsm.Update();
                yield return new WaitForSecondsRealtime(1.0f);
            }
        }

        private void StartUpdate()
        {
            StopUpdate();

            if (_runner != null)
                _coUpdate = _runner.StartCoroutine(CoUpdate());
        }

        private void StopUpdate()
        {
            if (_coUpdate != null && _runner != null)
            {
                _runner.StopCoroutine(_coUpdate);
                _coUpdate = null;
            }
        }

        public void Start(double durationMin)
        {
            if(!_init || _fsm.CurType == ETaskTimerType.Processing)
                return;

            _startTime = DateTimeUtils.RemoveMilliseconds(DateTime.UtcNow);

            if(_updatedTime != DateTime.MinValue && IsTimeTampered)
                _durationMin = durationMin + (_updatedTime - DateTime.UtcNow).TotalMinutes;
            else
                _durationMin = durationMin;

            _fsm.Change(ETaskTimerType.Processing);
            Save();
            StartUpdate();
        }

        public void Reduce(double reduceMin)
        {
            if(!_init || _fsm.CurType != ETaskTimerType.Processing)
                return;

            double remainingMin = (EndTime - DateTime.UtcNow).TotalMinutes;
            double actualReduction = Math.Min(reduceMin, remainingMin);

            if(actualReduction <= 0)
                return;

            _startTime = _startTime.AddMinutes(-actualReduction);
            Save();

            OnUpdated?.Invoke(RemainingSec);

            if(IsPeriodExpired)
                _fsm.Change(ETaskTimerType.Completed);
        }

        public void CompleteImmediately()
        {
            if(!_init || _fsm.CurType != ETaskTimerType.Processing)
                return;

            StopUpdate();

            _updatedTime = DateTimeUtils.RemoveMilliseconds(DateTime.UtcNow);
            _ps.Save(UPDATED_TIME_SUFFIX, _updatedTime.Ticks.ToString());
            _ps.Save(STATE_SUFFIX, ((int)ETaskTimerType.Completed).ToString());

            _fsm.Change(ETaskTimerType.Completed);
        }

        public void Claim()
        {
            if(!_init || _fsm.CurType != ETaskTimerType.Completed)
                return;

            Clear();
            _fsm.Change(ETaskTimerType.None);
            OnClaimed?.Invoke();
        }

        public void UpdateCompletionTime()
        {
            _updatedTime = DateTimeUtils.RemoveMilliseconds(DateTime.UtcNow);
            _ps.Save(UPDATED_TIME_SUFFIX, _updatedTime.Ticks.ToString());
            _fsm.Change(ETaskTimerType.Completed);
        }

        public void NotifyUpdate()
        {
            OnUpdated?.Invoke(RemainingSec);
        }

        public void NotifyCompleted()
        {
            StopUpdate();
            OnCompleted?.Invoke();
        }
    }
}
