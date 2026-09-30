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
        public void TaskSaveFailurePreservesState()
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
        public void TaskCompletionSaveFailurePreservesState()
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
        public void TaskSnapshotFailureRestoresPreviousState()
        {
            MemoryStorage storage = new();
            Assert.That(TaskTimer.TryCreate("Task", _runner, out TaskTimer timer, storage, GetUtcNow), Is.True);
            Assert.That(timer.TryInit(), Is.True);
            Assert.That(timer.TryStart(60.0d), Is.True);
            Assert.That(storage.SaveCnt, Is.EqualTo(1));
            storage.DisableSave();

            Assert.That(timer.TryComplete(), Is.False);
            timer.Release();

            Assert.That(TaskTimer.TryCreate("Task", _runner, out TaskTimer restored, storage, GetUtcNow), Is.True);
            Assert.That(restored.TryInit(), Is.True);
            Assert.That(restored.CurType, Is.EqualTo(ETaskTimerType.Processing));
            Assert.That(restored.RemainingSec, Is.EqualTo(60));
            restored.Release();
        }

        [Test]
        public void TaskLegacyDataLoadsAndClaimUsesSnapshot()
        {
            MemoryStorage storage = new();
            storage.Seed("TaskTimer_Task_START", _utcNow.Ticks.ToString());
            storage.Seed("TaskTimer_Task_DURATION", "60");
            storage.Seed("TaskTimer_Task_STATE", ((int)ETaskTimerType.Completed).ToString());
            storage.Seed("TaskTimer_Task_UPDATED", _utcNow.Ticks.ToString());
            Assert.That(TaskTimer.TryCreate("Task", _runner, out TaskTimer timer, storage, GetUtcNow), Is.True);
            Assert.That(timer.TryInit(), Is.True);
            Assert.That(timer.CurType, Is.EqualTo(ETaskTimerType.Completed));

            Assert.That(timer.TryClaim(), Is.True);
            Assert.That(timer.TryGetClaimed(out bool isClaimed), Is.True);
            Assert.That(isClaimed, Is.True);
            timer.Release();

            TaskTimerService service = new(_runner, storage, GetUtcNow);
            Assert.That(service.TryGetClaimed("Task", out bool isServiceClaimed), Is.True);
            Assert.That(isServiceClaimed, Is.True);
            Assert.That(service.TryCreate("Task", out TaskTimerHandle handle), Is.True);
            Assert.That(service.TryInit(handle), Is.True);
            Assert.That(handle.CurType, Is.EqualTo(ETaskTimerType.None));
            service.Release();
        }

        [Test]
        public void InvalidClaimedSnapshotDoesNotReportClaimed()
        {
            MemoryStorage storage = new();
            storage.Seed("TaskTimer_Task_SNAPSHOT", $"1|{_utcNow.Ticks}|60|{(int)ETaskTimerType.Completed}|{_utcNow.Ticks}|1");

            Assert.That(TaskTimer.TryReadClaimed("Task", storage, out bool isClaimed), Is.False);
            Assert.That(isClaimed, Is.False);
            Assert.That(TaskTimer.TryCreate("Task", _runner, out TaskTimer timer, storage, GetUtcNow), Is.True);
            Assert.That(timer.TryInit(), Is.False);
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
        public void TaskAppliesClockRollbackOnce()
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
        public void PeriodUsesClockAndPendingPeriods()
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
        public void PeriodForceSaveFailurePreservesState()
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
        public void PeriodSnapshotFailureRestoresPreviousState()
        {
            MemoryStorage storage = new();
            Assert.That(PeriodTimer.TryCreate("Period", _runner, out PeriodTimer timer, storage, GetUtcNow), Is.True);
            Assert.That(timer.TryInit(1.0d, 2.0d), Is.True);
            Assert.That(storage.SaveCnt, Is.EqualTo(2));
            storage.DisableSave();

            Assert.That(timer.TryForceClosed(), Is.False);
            timer.Release();
            storage.EnableSave();

            Assert.That(PeriodTimer.TryCreate("Period", _runner, out PeriodTimer restored, storage, GetUtcNow), Is.True);
            Assert.That(restored.TryInit(1.0d, 2.0d), Is.True);
            Assert.That(restored.CurType, Is.EqualTo(EPeriodTimerType.Open));
            Assert.That(restored.OpenEndTime, Is.EqualTo(_utcNow.AddMinutes(1.0d)));
            restored.Release();
        }

        [Test]
        public void PeriodDeleteDoesNotRestoreLegacyData()
        {
            MemoryStorage storage = new();
            storage.Seed("PeriodTimer_Period_OPEN_END", _utcNow.AddMinutes(10.0d).Ticks.ToString());
            storage.Seed("PeriodTimer_Period_CLOSED_END", _utcNow.AddMinutes(20.0d).Ticks.ToString());
            storage.Seed("PeriodTimer_Period_OPEN_UPDATED", _utcNow.Ticks.ToString());
            storage.Seed("PeriodTimer_Period_TAMPERED", "0");
            PeriodTimerService service = new(_runner, storage, GetUtcNow);
            Assert.That(service.TryDelete("Period"), Is.True);

            Assert.That(service.TryCreate("Period", out PeriodTimerHandle handle), Is.True);
            Assert.That(service.TryInit(handle, 1.0d, 2.0d), Is.True);
            Assert.That(handle.RemainingSec, Is.EqualTo(60));
            service.Release();
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
        public void TaskServiceReplacesAndReleasesHandle()
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
        public void PeriodServiceReplacesAndReleasesHandle()
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
        public void TimerHostOwnsAndReleasesServices()
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

            public int SaveCnt { get; private set; }

            //============================================================
            // Persistence
            //============================================================
            public bool TrySave(string key, string data)
            {
                if (!_canSave || string.IsNullOrWhiteSpace(key) || data == null)
                    return false;

                _values[key] = data;
                SaveCnt++;
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

            //============================================================
            // Logic
            //============================================================
            public void DisableSave()
            {
                _canSave = false;
            }

            public void EnableSave()
            {
                _canSave = true;
            }

            public void DisableRead()
            {
                _canRead = false;
            }

            public void Seed(string key, string value)
            {
                _values[key] = value;
            }
        }
    }

}
