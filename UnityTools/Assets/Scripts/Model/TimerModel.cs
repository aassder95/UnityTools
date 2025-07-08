using System;
using UnityTools.Util;

namespace UnityTools.Model
{
    public class TimerModel : BaseModel
    {
        public string State { get; private set; }
        public string SubState { get; private set; }
        public int LoopMinutes { get; private set; }
        public DateTime OpenStart { get; private set; }
        public DateTime OpenUpdated { get; private set; }
        public DateTime OpenEnd { get; private set; }
        public DateTime ClosedEnd { get; private set; }

        public void SetState(string state)
        {
            State = state;
            NotifyUpdated();
        }

        public void SetSubState(string state)
        {
            SubState = state;
            NotifyUpdated();
        }

        public void SetLoop(int min, DateTime openUpdated)
        {
            LoopMinutes = min;
            OpenUpdated = openUpdated;
            NotifyUpdated();
        }

        public void SetTimer(DateTime openStart, DateTime openUpdated, DateTime openEnd, DateTime closedEnd)
        {
            OpenStart = openStart;
            OpenUpdated = openUpdated;
            OpenEnd = openEnd;
            ClosedEnd = closedEnd;
            NotifyUpdated();
        }
    }
}
