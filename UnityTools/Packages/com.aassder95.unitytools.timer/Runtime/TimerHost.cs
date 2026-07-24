using System;
using UnityEngine;
using UnityTools.Timer.Persistence;

namespace UnityTools.Timer
{
    public class TimerHost : MonoBehaviour
    {
        //============================================================
        // Fields
        //============================================================
        private TaskTimerService _taskTimers;
        private PeriodTimerService _periodTimers;

        //============================================================
        // Properties
        //============================================================
        public TaskTimerService TaskTimers => _taskTimers;
        public PeriodTimerService PeriodTimers => _periodTimers;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnDestroy()
        {
            Release();
        }

        //============================================================
        // Init/Register
        //============================================================
        public void Init(IStorage storage = null, Func<DateTime> utcNow = null)
        {
            Release();
            IStorage targetStorage = storage ?? new PlayerPrefsStorage();
            _taskTimers = new TaskTimerService(this, targetStorage, utcNow);
            _periodTimers = new PeriodTimerService(this, targetStorage, utcNow);
        }

        public void Release()
        {
            _taskTimers?.Release();
            _periodTimers?.Release();
            _taskTimers = null;
            _periodTimers = null;
        }
    }
}
