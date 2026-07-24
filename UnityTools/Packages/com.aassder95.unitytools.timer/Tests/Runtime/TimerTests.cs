using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityTools.Timer;
using UnityTools.Timer.Persistence;
using UnityTools.Timer.Period;
using UnityTools.Timer.Task;

namespace UnityTools.Timer.Tests.Timer
{
    public class TimerTests
    {
        //============================================================
        // Fields
        //============================================================
        private GameObject _goRunner;
        private TestRunner _runner;
        private DateTime _utcNow;

        //============================================================
        // Init/Register
        //============================================================
        [SetUp]
        public void SetUp()
        {
            _goRunner = new GameObject("TimerTestRunner");
            _runner = _goRunner.AddComponent<TestRunner>();
            _utcNow = new DateTime(2026, 7, 24, 0, 0, 0, DateTimeKind.Utc);
        }

        [TearDown]
        public void TearDown()
        {
            if (_goRunner != null)
                UnityEngine.Object.DestroyImmediate(_goRunner);
        }

        //============================================================
        // Logic
        //============================================================
        [Test]
        public void TaskTimerUsesInjectedUtcClock()
        {
            MemoryStorage storage = new();
            Assert.That(TaskTimer.TryCreate("Task", _runner, out TaskTimer timer, storage, GetUtcNow), Is.True);
            Assert.That(timer.TryInit(), Is.True);
            Assert.That(timer.CurType, Is.EqualTo(ETaskTimerType.None));
            Assert.That(timer.TryStart(120.0d), Is.True);
            Assert.That(timer.RemainingSec, Is.EqualTo(120));

            _utcNow = _utcNow.AddSeconds(30.0d);
            Assert.That(timer.RemainingSec, Is.EqualTo(90));
            Assert.That(timer.Progress, Is.EqualTo(0.25f).Within(0.001f));

            Assert.That(timer.TryComplete(), Is.True);
            Assert.That(timer.CurType, Is.EqualTo(ETaskTimerType.Completed));
            timer.Release();
        }

        [Test]
        public void TaskTimerPreservesStateWhenSaveFails()
        {
            MemoryStorage storage = new();
            Assert.That(TaskTimer.TryCreate("Task", _runner, out TaskTimer timer, storage, GetUtcNow), Is.True);
            Assert.That(timer.TryInit(), Is.True);
            storage.DisableSave();

            Assert.That(timer.TryStart(60.0d), Is.False);
            Assert.That(timer.CurType, Is.EqualTo(ETaskTimerType.None));
            Assert.That(timer.EndTime, Is.EqualTo(DateTime.MinValue));
            timer.Release();
        }

        [Test]
        public void TaskTimerPreservesProcessingStateWhenCompletionSaveFails()
        {
            MemoryStorage storage = new();
            Assert.That(TaskTimer.TryCreate("Task", _runner, out TaskTimer timer, storage, GetUtcNow), Is.True);
            Assert.That(timer.TryInit(), Is.True);
            Assert.That(timer.TryStart(60.0d), Is.True);
            storage.DisableSave();

            Assert.That(timer.TryComplete(), Is.False);
            Assert.That(timer.CurType, Is.EqualTo(ETaskTimerType.Processing));
            timer.Release();
        }

        [Test]
        public void TaskTimerReportsClaimedReadFailure()
        {
            MemoryStorage storage = new();
            Assert.That(TaskTimer.TryCreate("Task", _runner, out TaskTimer timer, storage, GetUtcNow), Is.True);
            Assert.That(timer.TryInit(), Is.True);
            storage.DisableRead();

            Assert.That(timer.TryGetClaimed(out bool isClaimed), Is.False);
            Assert.That(isClaimed, Is.False);
            timer.Release();
        }

        [Test]
        public void TaskTimerAppliesClockRollbackOnlyOnce()
        {
            MemoryStorage storage = new();
            Assert.That(TaskTimer.TryCreate("Task", _runner, out TaskTimer timer, storage, GetUtcNow), Is.True);
            Assert.That(timer.TryInit(), Is.True);
            Assert.That(timer.TryStart(60.0d), Is.True);
            timer.Release();

            _utcNow = _utcNow.AddSeconds(-30.0d);
            Assert.That(timer.TryInit(), Is.True);
            Assert.That(timer.DurationSec, Is.EqualTo(90));
            timer.Release();

            Assert.That(timer.TryInit(), Is.True);
            Assert.That(timer.DurationSec, Is.EqualTo(90));
            timer.Release();
        }

        [Test]
        public void PeriodTimerUsesInjectedUtcClockAndPendingPeriods()
        {
            MemoryStorage storage = new();
            Assert.That(PeriodTimer.TryCreate("Period", _runner, out PeriodTimer timer, storage, GetUtcNow), Is.True);
            Assert.That(timer.TryInit(1.0d, 2.0d), Is.True);
            Assert.That(timer.CurType, Is.EqualTo(EPeriodTimerType.Open));
            Assert.That(timer.RemainingSec, Is.EqualTo(60));

            Assert.That(timer.TrySetPeriods(2.0d, 3.0d), Is.True);
            Assert.That(timer.TryForceOpen(), Is.True);
            Assert.That(timer.OpenEndTime, Is.EqualTo(_utcNow.AddMinutes(2.0d)));
            Assert.That(timer.ClosedEndTime, Is.EqualTo(_utcNow.AddMinutes(5.0d)));
            timer.Release();
        }

        [Test]
        public void PeriodTimerPreservesStateWhenForceSaveFails()
        {
            MemoryStorage storage = new();
            Assert.That(PeriodTimer.TryCreate("Period", _runner, out PeriodTimer timer, storage, GetUtcNow), Is.True);
            Assert.That(timer.TryInit(1.0d, 2.0d), Is.True);
            EPeriodTimerType prevType = timer.CurType;
            DateTime prevOpenEndTime = timer.OpenEndTime;
            DateTime prevClosedEndTime = timer.ClosedEndTime;
            storage.DisableSave();

            Assert.That(timer.TryForceClosed(), Is.False);
            Assert.That(timer.CurType, Is.EqualTo(prevType));
            Assert.That(timer.OpenEndTime, Is.EqualTo(prevOpenEndTime));
            Assert.That(timer.ClosedEndTime, Is.EqualTo(prevClosedEndTime));
            timer.Release();
        }

        [Test]
        public void PeriodTimerReportsRefreshSaveFailure()
        {
            MemoryStorage storage = new();
            Assert.That(PeriodTimer.TryCreate("Period", _runner, out PeriodTimer timer, storage, GetUtcNow), Is.True);
            Assert.That(timer.TryInit(1.0d, 2.0d), Is.True);
            Assert.That(timer.IsReady, Is.True);
            timer.Release();
            _utcNow = _utcNow.AddSeconds(90.0d);
            storage.DisableSave();

            Assert.That(timer.TryInit(1.0d, 2.0d), Is.False);
            Assert.That(timer.CurType, Is.EqualTo(EPeriodTimerType.Open));
            Assert.That(timer.IsReady, Is.False);
            timer.Release();
        }

        [Test]
        public void TaskTimerRestoresPersistedProcessingState()
        {
            MemoryStorage storage = new();
            Assert.That(TaskTimer.TryCreate("Task", _runner, out TaskTimer firstTimer, storage, GetUtcNow), Is.True);
            Assert.That(firstTimer.TryInit(), Is.True);
            Assert.That(firstTimer.TryStart(120.0d), Is.True);
            firstTimer.Release();

            _utcNow = _utcNow.AddSeconds(30.0d);
            Assert.That(TaskTimer.TryCreate("Task", _runner, out TaskTimer restoredTimer, storage, GetUtcNow), Is.True);
            Assert.That(restoredTimer.TryInit(), Is.True);
            Assert.That(restoredTimer.CurType, Is.EqualTo(ETaskTimerType.Processing));
            Assert.That(restoredTimer.RemainingSec, Is.EqualTo(90));
            restoredTimer.Release();
        }

        [Test]
        public void TaskTimerServiceReplacesDuplicateIdAndReleasesOldHandle()
        {
            TaskTimerService service = new(_runner, new MemoryStorage(), GetUtcNow);
            Assert.That(service.TryCreate("Task", out TaskTimerHandle firstHandle), Is.True);
            Assert.That(service.TryInit(firstHandle), Is.True);
            Assert.That(service.TryStart("Task", 60.0d), Is.True);
            Assert.That(service.TryCreate("Task", out TaskTimerHandle secondHandle), Is.True);

            Assert.That(service.TryInit(secondHandle), Is.True);

            Assert.That(service.TryGetHandle("Task", out TaskTimerHandle currentHandle), Is.True);
            Assert.That(currentHandle, Is.SameAs(secondHandle));
            Assert.That(firstHandle.TryComplete(), Is.False);

            service.Release();
            Assert.That(secondHandle.TryComplete(), Is.False);
        }

        [Test]
        public void PeriodTimerServiceReplacesDuplicateIdAndReleasesOldHandle()
        {
            PeriodTimerService service = new(_runner, new MemoryStorage(), GetUtcNow);
            Assert.That(service.TryCreate("Period", out PeriodTimerHandle firstHandle), Is.True);
            Assert.That(service.TryInit(firstHandle, 1.0d, 2.0d), Is.True);
            Assert.That(service.TryCreate("Period", out PeriodTimerHandle secondHandle), Is.True);

            Assert.That(service.TryInit(secondHandle, 1.0d, 2.0d), Is.True);

            Assert.That(service.TryGetHandle("Period", out PeriodTimerHandle currentHandle), Is.True);
            Assert.That(currentHandle, Is.SameAs(secondHandle));
            Assert.That(firstHandle.TryForceClosed(), Is.False);

            service.Release();
            Assert.That(secondHandle.TryForceOpen(), Is.False);
        }

        [Test]
        public void TimerHostOwnsAndReleasesBothServices()
        {
            TimerHost host = _goRunner.AddComponent<TimerHost>();

            host.Init(new MemoryStorage(), GetUtcNow);

            Assert.That(host.TaskTimers, Is.Not.Null);
            Assert.That(host.PeriodTimers, Is.Not.Null);

            host.Release();

            Assert.That(host.TaskTimers, Is.Null);
            Assert.That(host.PeriodTimers, Is.Null);
        }

        //============================================================
        // Utilities
        //============================================================
        private DateTime GetUtcNow()
        {
            return _utcNow;
        }

        //============================================================
        // Nested Types
        //============================================================
        private class TestRunner : MonoBehaviour { }

        private class MemoryStorage : IStorage
        {
            //============================================================
            // Readonly
            //============================================================
            private readonly Dictionary<string, string> _values = new();
            private bool _canSave = true;
            private bool _canRead = true;

            //============================================================
            // Logic
            //============================================================
            public void DisableSave()
            {
                _canSave = false;
            }

            public void DisableRead()
            {
                _canRead = false;
            }

            //============================================================
            // Persistence
            //============================================================
            public bool TrySave(string key, string data)
            {
                if (!_canSave || string.IsNullOrWhiteSpace(key) || data == null)
                    return false;

                _values[key] = data;
                return true;
            }

            public bool TryLoad(string key, out string data)
            {
                data = null;
                return _canRead && !string.IsNullOrWhiteSpace(key) && _values.TryGetValue(key, out data);
            }

            public bool TryHasKey(string key, out bool hasKey)
            {
                hasKey = _canRead && !string.IsNullOrWhiteSpace(key) && _values.ContainsKey(key);
                return _canRead && !string.IsNullOrWhiteSpace(key);
            }

            public bool TryDelete(string key)
            {
                if (string.IsNullOrWhiteSpace(key))
                    return false;

                _values.Remove(key);
                return true;
            }
        }
    }

}
