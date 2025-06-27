using System;

namespace UnityTools.Model
{
    public class TimerModel
    {
        string _state;
        string _subState;
        int _loopMinutes;
        DateTime _openStart;
        DateTime _openUpdated;
        DateTime _openEnd;
        DateTime _closedEnd;

        public string State => _state;
        public string SubState => _subState;
        public int LoopMinutes => _loopMinutes;
        public DateTime OpenStart => _openStart;
        public DateTime OpenUpdated => _openUpdated;
        public DateTime OpenEnd => _openEnd;
        public DateTime ClosedEnd => _closedEnd;

        public void SetState(string state)
        {
            _state = state;
        }

        public void SetSubState(string state)
        {
            _subState = state;
        }

        public void SetLoop(int min, DateTime openUpdated)
        {
            _loopMinutes = min;
            _openUpdated = openUpdated;
        }

        public void SetTimer(DateTime openStart, DateTime openUpdated, DateTime openEnd, DateTime closedEnd)
        {
            _openStart = openStart;
            _openUpdated = openUpdated;
            _openEnd = openEnd;
            _closedEnd = closedEnd;
        }
    }
}
