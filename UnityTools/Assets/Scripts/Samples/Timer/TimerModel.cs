using System;
using UnityTools.Util;

namespace UnityTools.Samples.Timer
{
    public class TimerModel : BaseModel
    {
        //============================================================
        //Fields
        //============================================================
        private string _state;
        private string _subState;
        private int _loopMinutes;
        private DateTime _openUpdated;
        private DateTime _openEnd;
        private DateTime _closedEnd;

        //============================================================
        //Properties
        //============================================================
        public string State => _state;
        public string SubState => _subState;
        public int LoopMinutes => _loopMinutes;
        public DateTime OpenUpdated => _openUpdated;
        public DateTime OpenEnd => _openEnd;
        public DateTime ClosedEnd => _closedEnd;

        //============================================================
        //Logic
        //============================================================
        public void SetState(string state)
        {
            _state = state;
            NotifyUpdated();
        }

        public void SetSubState(string state)
        {
            _subState = state;
            NotifyUpdated();
        }

        public void SetLoop(int min, DateTime openUpdated)
        {
            _loopMinutes = min;
            _openUpdated = openUpdated;
            NotifyUpdated();
        }

        public void SetTimer(DateTime openUpdated, DateTime openEnd, DateTime closedEnd)
        {
            _openUpdated = openUpdated;
            _openEnd = openEnd;
            _closedEnd = closedEnd;
            NotifyUpdated();
        }
    }
}
