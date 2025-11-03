using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace UnityTools.Util
{
    public enum ETaskTimerState
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
        private readonly StateMachine<ETaskTimerState> _fsm;

        private bool _init;
        private double _durationMin;
        private DateTime _startTime;
        private DateTime _updatedTime;
        private Coroutine _coUpdate;

        public StateMachine<ETaskTimerState> FSM => _fsm;
        public ETaskTimerState CurrentState => _fsm.CurType;
        public int RemainingSec => DateTimeUtils.GetRemainingSeconds(EndTime);
        private DateTime EndTime => _startTime.AddMinutes(_durationMin);

        private bool IsTimeTampered => DateTimeUtils.CompareWithoutMilliseconds(DateTime.UtcNow, _updatedTime) < 0;
        public bool IsPeriodExpired => DateTimeUtils.CompareWithoutMilliseconds(DateTime.UtcNow, EndTime) >= 0;

        public event UnityAction<int> OnUpdated;
        public event UnityAction OnCompleted;
        public event UnityAction OnClaimed;
        public event UnityAction<ETaskTimerState> OnStateChanged
        {
            add => _fsm.OnStateChanged += value;
            remove => _fsm.OnStateChanged -= value;
        }

        public TaskTimer(string rootKey, string id)
        {
            _id = id;
            _ps = new Persistence($"{rootKey}_{id}");
            _fsm = new StateMachine<ETaskTimerState>();

            _fsm.Add(ETaskTimerState.None, new TaskTimerStates.NoneState(this));
            _fsm.Add(ETaskTimerState.Processing, new TaskTimerStates.ProcessingState(this));
            _fsm.Add(ETaskTimerState.Completed, new TaskTimerStates.CompletedState(this));
        }

        private void Save()
        {
            _ps.Save(START_TIME_SUFFIX, _startTime.Ticks.ToString());
            _ps.Save(DURATION_SUFFIX, _durationMin.ToString());
            _ps.Save(STATE_SUFFIX, ((int)_fsm.CurType).ToString());
            _ps.Save(UPDATED_TIME_SUFFIX, DateTime.UtcNow.Ticks.ToString());
        }

        public void SaveStateOnly()
        {
            _ps.Save(STATE_SUFFIX, ((int)_fsm.CurType).ToString());
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

            if(!int.TryParse(_ps.Load(STATE_SUFFIX, "0"), out int savedState))
                savedState = 0;

            ETaskTimerState state = (ETaskTimerState)savedState;
            if(_startTime == DateTime.MinValue || state == ETaskTimerState.None)
            {
                _fsm.Change(ETaskTimerState.None);
                return;
            }

            if(state == ETaskTimerState.Processing)
            {
                if(IsTimeTampered)
                {
                    double penaltyMin = (_updatedTime - DateTime.UtcNow).TotalMinutes;
                    _durationMin += penaltyMin;
                }

                if(IsPeriodExpired)
                {
                    _updatedTime = DateTimeUtils.RemoveMilliseconds(DateTime.UtcNow);
                    _ps.Save(UPDATED_TIME_SUFFIX, _updatedTime.Ticks.ToString());
                    _fsm.Change(ETaskTimerState.Completed);
                    _ps.Save(STATE_SUFFIX, ((int)ETaskTimerState.Completed).ToString());
                }
                else
                {
                    _fsm.Change(ETaskTimerState.Processing);
                    StartUpdate();
                }
            }
            else if(state == ETaskTimerState.Completed)
            {
                _fsm.Change(ETaskTimerState.Completed);
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
            while (_fsm.CurType == ETaskTimerState.Processing)
            {
                _fsm.Update();
                yield return new WaitForSecondsRealtime(1.0f);
            }
        }

        private void StartUpdate()
        {
            CoroutineHelper.Replace(ref _coUpdate, CoUpdate());
        }

        private void StopUpdate()
        {
            CoroutineHelper.Dispose(ref _coUpdate);
        }

        public void Start(double durationMin)
        {
            if(!_init || _fsm.CurType == ETaskTimerState.Processing)
                return;

            _startTime = DateTimeUtils.RemoveMilliseconds(DateTime.UtcNow);

            if(_updatedTime != DateTime.MinValue && IsTimeTampered)
                _durationMin = durationMin + (_updatedTime - DateTime.UtcNow).TotalMinutes;
            else
                _durationMin = durationMin;

            _fsm.Change(ETaskTimerState.Processing);
            Save();
            StartUpdate();
        }

        public void Reduce(double reduceMin)
        {
            if(!_init || _fsm.CurType != ETaskTimerState.Processing)
                return;

            double remainingMin = (EndTime - DateTime.UtcNow).TotalMinutes;
            double actualReduction = Math.Min(reduceMin, remainingMin);

            if(actualReduction <= 0)
                return;

            _startTime = _startTime.AddMinutes(-actualReduction);
            Save();

            if(IsPeriodExpired)
                _fsm.Change(ETaskTimerState.Completed);
        }

        public void CompleteImmediately()
        {
            if(!_init || _fsm.CurType != ETaskTimerState.Processing)
                return;

            StopUpdate();

            _updatedTime = DateTimeUtils.RemoveMilliseconds(DateTime.UtcNow);
            _ps.Save(UPDATED_TIME_SUFFIX, _updatedTime.Ticks.ToString());
            _ps.Save(STATE_SUFFIX, ((int)ETaskTimerState.Completed).ToString());

            _fsm.Change(ETaskTimerState.Completed);
        }

        public void Claim()
        {
            if(!_init || _fsm.CurType != ETaskTimerState.Completed)
                return;

            Clear();
            _fsm.Change(ETaskTimerState.None);
            OnClaimed?.Invoke();
        }

        public void UpdateCompletionTime()
        {
            _updatedTime = DateTimeUtils.RemoveMilliseconds(DateTime.UtcNow);
            _ps.Save(UPDATED_TIME_SUFFIX, _updatedTime.Ticks.ToString());
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
