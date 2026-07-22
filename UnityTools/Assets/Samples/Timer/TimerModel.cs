using System;
using UnityTools.Util.UIFramework;

namespace UnityTools.Samples.Timer
{
    public class TimerModel : BaseModel
    {
        //============================================================
        // Fields
        //============================================================
        private string _state;
        private string _subState;
        private int _loopMinutes;
        private DateTime _openUpdated;
        private DateTime _openEnd;
        private DateTime _closedEnd;

        //============================================================
        // Properties
        //============================================================
        public string State => _state;
        public string SubState => _subState;
        public int LoopMinutes => _loopMinutes;
        public DateTime OpenUpdated => _openUpdated;
        public DateTime OpenEnd => _openEnd;
        public DateTime ClosedEnd => _closedEnd;

        //============================================================
        // Logic
        //============================================================
        public void SetState(string state)
        {
            SetField(ref _state, state);
        }

        public void SetSubState(string subState)
        {
            SetField(ref _subState, subState);
        }

        public void SetStateAndSubState(string state, string subState)
        {
            RunBatchUpdate(() =>
            {
                SetField(ref _state, state);
                SetField(ref _subState, subState);
            });
        }

        public void SetLoop(int min, DateTime openUpdated)
        {
            RunBatchUpdate(() =>
            {
                SetField(ref _loopMinutes, min);
                SetField(ref _openUpdated, openUpdated);
            });
        }

        public void SetTimer(DateTime openUpdated, DateTime openEnd, DateTime closedEnd)
        {
            RunBatchUpdate(() =>
            {
                SetField(ref _openUpdated, openUpdated);
                SetField(ref _openEnd, openEnd);
                SetField(ref _closedEnd, closedEnd);
            });
        }

        public void SetSnapshot(DateTime openUpdated, DateTime openEnd, DateTime closedEnd, string state, string subState, bool shouldUpdateSubState)
        {
            RunBatchUpdate(() =>
            {
                SetField(ref _openUpdated, openUpdated);
                SetField(ref _openEnd, openEnd);
                SetField(ref _closedEnd, closedEnd);
                SetField(ref _state, state);

                if (shouldUpdateSubState)
                    SetField(ref _subState, subState);
            });
        }
    }
}
