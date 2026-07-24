using System;
using UnityTools.Ui;

namespace UnityTools.Samples.Timer
{
    public class TimerModel : BaseModel
    {
        //============================================================
        // Fields
        //============================================================
        private string _state;
        private string _subState;
        private int _loopMin;
        private DateTime _openUpdatedTime;
        private DateTime _openEndTime;
        private DateTime _closedEndTime;

        //============================================================
        // Properties
        //============================================================
        public string State => _state;
        public string SubState => _subState;
        public int LoopMin => _loopMin;
        public DateTime OpenUpdatedTime => _openUpdatedTime;
        public DateTime OpenEndTime => _openEndTime;
        public DateTime ClosedEndTime => _closedEndTime;

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

        public void SetLoop(int remainingMin, DateTime openUpdatedTime)
        {
            RunBatchUpdate(() =>
            {
                SetField(ref _loopMin, remainingMin);
                SetField(ref _openUpdatedTime, openUpdatedTime);
            });
        }

        public void SetTimer(DateTime openUpdatedTime, DateTime openEndTime, DateTime closedEndTime)
        {
            RunBatchUpdate(() =>
            {
                SetField(ref _openUpdatedTime, openUpdatedTime);
                SetField(ref _openEndTime, openEndTime);
                SetField(ref _closedEndTime, closedEndTime);
            });
        }

        public void SetSnapshot(DateTime openUpdatedTime, DateTime openEndTime, DateTime closedEndTime, string state, string subState, bool shouldUpdateSubState)
        {
            RunBatchUpdate(() =>
            {
                SetField(ref _openUpdatedTime, openUpdatedTime);
                SetField(ref _openEndTime, openEndTime);
                SetField(ref _closedEndTime, closedEndTime);
                SetField(ref _state, state);

                if (shouldUpdateSubState)
                    SetField(ref _subState, subState);
            });
        }
    }
}
