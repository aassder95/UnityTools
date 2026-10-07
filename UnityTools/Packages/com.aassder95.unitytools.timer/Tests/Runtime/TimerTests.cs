using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
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
        private int _changedCnt;
        private int _completedCnt;
        private int _claimedCnt;

        //============================================================
        // Init/Register
        //============================================================
        [SetUp]
        public void SetUp()
        {
            _goRunner = new GameObject("TimerTestRunner");
            _runner = _goRunner.AddComponent<TestRunner>();
            _utcNow = new DateTime(2026, 7, 24, 0, 0, 0, DateTimeKind.Utc);
            _changedCnt = 0;
            _completedCnt = 0;
            _claimedCnt = 0;
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

        [Test]
        public void TaskSnapshotsOwnTheirDataAndReflectRegisteredTimers()
        {
            TaskTimerService service = new(_runner, new MemoryStorage(), GetUtcNow);
            Assert.That(service.TryCreate("A", out TaskTimerHandle first), Is.True);
            Assert.That(service.TimerCnt, Is.Zero);
            Assert.That(service.TryInit(first), Is.True);
            Assert.That(service.TryCreate("B", out TaskTimerHandle second), Is.True);
            Assert.That(service.TryInit(second), Is.True);
            Assert.That(service.TryStart("A", 120.0d), Is.True);
            TaskTimerData[] snapshots = service.GetSnapshots();
            Assert.That(snapshots.Length, Is.EqualTo(2));
            TaskTimerData initial = Array.Find(snapshots, data => data.Id == "A");
            Assert.That(initial.RemainingSec, Is.EqualTo(120));
            _utcNow = _utcNow.AddSeconds(30.0d);
            Assert.That(Array.Find(service.GetSnapshots(), data => data.Id == "A").RemainingSec, Is.EqualTo(90));
            Assert.That(initial.RemainingSec, Is.EqualTo(120));
            snapshots[0] = null;
            Assert.That(service.GetSnapshots(), Has.None.Null);
            service.Release();
            Assert.That(service.TimerCnt, Is.Zero);
            Assert.That(service.GetSnapshots(), Is.Empty);
        }

        [Test]
        public void TaskListNotificationsReflectCommittedChangesAndUnsubscribeOnRelease()
        {
            MemoryStorage storage = new();
            TaskTimerService service = new(_runner, storage, GetUtcNow);
            service.OnTimersChanged += OnTimersChanged;
            Assert.That(service.TryCreate("A", out TaskTimerHandle handle), Is.True);
            Assert.That(_changedCnt, Is.Zero);
            Assert.That(service.TryInit(handle), Is.True);
            Assert.That(_changedCnt, Is.EqualTo(1));
            storage.DisableSave();
            Assert.That(service.TryStart("A", 60.0d), Is.False);
            Assert.That(_changedCnt, Is.EqualTo(1));
            storage.EnableSave();
            Assert.That(handle.TryStart(60.0d), Is.True);
            Assert.That(_changedCnt, Is.GreaterThan(1));
            int prevCnt = _changedCnt;
            Assert.That(handle.TryComplete(), Is.True);
            Assert.That(_changedCnt, Is.GreaterThan(prevCnt));
            prevCnt = _changedCnt;
            Assert.That(handle.TryClaim(), Is.True);
            Assert.That(_changedCnt, Is.GreaterThan(prevCnt));
            service.Release();
            prevCnt = _changedCnt;
            handle.NotifyCurType();
            service.Release();
            Assert.That(_changedCnt, Is.EqualTo(prevCnt));
            service.OnTimersChanged -= OnTimersChanged;
        }

        [Test]
        public void PeriodSnapshotsAndNotificationsFollowDeletionAndReplacement()
        {
            MemoryStorage storage = new();
            PeriodTimerService service = new(_runner, storage, GetUtcNow);
            service.OnTimersChanged += OnTimersChanged;
            Assert.That(service.TryCreate("A", out PeriodTimerHandle first), Is.True);
            Assert.That(service.TryInit(first, 1.0d, 2.0d), Is.True);
            Assert.That(service.TimerCnt, Is.EqualTo(1));
            Assert.That(service.TryCreate("A", out PeriodTimerHandle second), Is.True);
            Assert.That(service.TryInit(second, 1.0d, 2.0d), Is.True);
            Assert.That(service.TimerCnt, Is.EqualTo(1));
            Assert.That(service.GetSnapshots().Length, Is.EqualTo(1));
            int prevCnt = _changedCnt;
            Assert.That(second.TryForceClosed(), Is.True);
            Assert.That(_changedCnt, Is.GreaterThan(prevCnt));
            Assert.That(service.GetSnapshots()[0].CurType, Is.EqualTo(second.CurType));
            prevCnt = _changedCnt;
            storage.DisableSave();
            Assert.That(service.TryDelete("A"), Is.False);
            Assert.That(_changedCnt, Is.EqualTo(prevCnt));
            Assert.That(service.TimerCnt, Is.EqualTo(1));
            storage.EnableSave();
            Assert.That(service.TryDelete("A"), Is.True);
            Assert.That(_changedCnt, Is.EqualTo(prevCnt + 1));
            Assert.That(service.GetSnapshots(), Is.Empty);
            prevCnt = _changedCnt;
            Assert.That(first.TryForceOpen(), Is.False);
            Assert.That(second.TryForceOpen(), Is.False);
            service.Release();
            Assert.That(_changedCnt, Is.EqualTo(prevCnt));
            service.OnTimersChanged -= OnTimersChanged;
        }

        [Test]
        public void PausedTimerSurvivesOfflineAndResumesWithSameProgress()
        {
            MemoryStorage storage = new();
            Assert.That(TaskTimer.TryCreate("Pause", _runner, out TaskTimer timer, storage, () => _utcNow), Is.True);
            Assert.That(timer.TryInit(), Is.True);
            Assert.That(timer.TryStart(60.5d), Is.True);
            _utcNow = _utcNow.AddSeconds(20.0d);
            Assert.That(timer.TryPause(), Is.True);
            Assert.That(timer.CurType, Is.EqualTo(ETaskTimerType.Paused));
            Assert.That(timer.RemainingSec, Is.EqualTo(41));
            float progress = timer.Progress;
            timer.Release();
            _utcNow = _utcNow.AddDays(2.0d);
            Assert.That(TaskTimer.TryCreate("Pause", _runner, out TaskTimer restored, storage, () => _utcNow), Is.True);
            Assert.That(restored.TryInit(), Is.True);
            Assert.That(restored.CurType, Is.EqualTo(ETaskTimerType.Paused));
            Assert.That(restored.RemainingSec, Is.EqualTo(41));
            Assert.That(restored.Progress, Is.EqualTo(progress));
            Assert.That(restored.TryStart(10.0d), Is.False);
            Assert.That(restored.TryReduce(10.0d), Is.False);
            Assert.That(restored.TryComplete(), Is.False);
            Assert.That(restored.TryClaim(), Is.False);
            Assert.That(restored.TryResume(), Is.True);
            Assert.That(restored.Progress, Is.EqualTo(progress).Within(0.02f));
            Assert.That(restored.RemainingSec, Is.EqualTo(41));
            _utcNow = _utcNow.AddSeconds(41.0d);
            Assert.That(restored.RemainingSec, Is.Zero);
            Assert.That(restored.TryComplete(), Is.True);
            Assert.That(restored.TryClaim(), Is.True);
            restored.Release();
        }

        [Test]
        public void PauseResumeSaveFailurePreservesStateAndSnapshot()
        {
            MemoryStorage storage = new();
            TaskTimerService service = new(_runner, storage, () => _utcNow);
            Assert.That(service.TryCreate("Pause", out TaskTimerHandle handle), Is.True);
            Assert.That(service.TryInit(handle), Is.True);
            Assert.That(service.TryStart("Pause", 60.0d), Is.True);
            service.OnTimersChanged += OnTimersChanged;
            try
            {
                Assert.That(storage.TryLoad("TaskTimer_Pause_SNAPSHOT", out string processing), Is.True);
                storage.DisableSave();
                Assert.That(service.TryPause("Pause"), Is.False);
                Assert.That(handle.CurType, Is.EqualTo(ETaskTimerType.Processing));
                Assert.That(_changedCnt, Is.Zero);
                Assert.That(storage.TryLoad("TaskTimer_Pause_SNAPSHOT", out string unchanged), Is.True);
                Assert.That(unchanged, Is.EqualTo(processing));
                storage.EnableSave();
                Assert.That(service.TryPause("Pause"), Is.True);
                Assert.That(service.TryPause("Pause"), Is.False);
                Assert.That(storage.TryLoad("TaskTimer_Pause_SNAPSHOT", out string paused), Is.True);
                _changedCnt = 0;
                _utcNow = _utcNow.AddHours(-1.0d);
                storage.DisableSave();
                Assert.That(service.TryResume("Pause"), Is.False);
                Assert.That(handle.CurType, Is.EqualTo(ETaskTimerType.Paused));
                Assert.That(handle.RemainingSec, Is.EqualTo(60));
                Assert.That(_changedCnt, Is.Zero);
                Assert.That(storage.TryLoad("TaskTimer_Pause_SNAPSHOT", out unchanged), Is.True);
                Assert.That(unchanged, Is.EqualTo(paused));
                storage.EnableSave();
                Assert.That(service.TryResume("Pause"), Is.True);
                Assert.That(handle.RemainingSec, Is.EqualTo(60));
                Assert.That(_changedCnt, Is.GreaterThan(0));
                Assert.That(service.TryResume("Pause"), Is.False);
                Assert.That(service.TryPause("missing"), Is.False);
            }
            finally
            {
                service.OnTimersChanged -= OnTimersChanged;
                service.Release();
            }
        }

        [TestCase("1", "30")]
        [TestCase("2", "-1")]
        [TestCase("2", "NaN")]
        [TestCase("2", "Infinity")]
        [TestCase("2", "61")]
        [TestCase("2", "0")]
        public void InvalidPauseSnapshotIsRejectedWithoutOverwrite(string version, string remaining)
        {
            MemoryStorage storage = new();
            string snapshot = $"{version}|{_utcNow.Ticks}|60|3|{_utcNow.Ticks}|0|{remaining}";
            storage.Seed("TaskTimer_Pause_SNAPSHOT", snapshot);
            Assert.That(TaskTimer.TryCreate("Pause", _runner, out TaskTimer timer, storage, () => _utcNow), Is.True);
            Assert.That(timer.TryInit(), Is.False);
            Assert.That(storage.SaveCnt, Is.Zero);
            timer.Release();
        }

        [UnityTest]
        public IEnumerator TaskUnregisterStopsExecutionAndRestoresElapsedTime()
        {
            MemoryStorage storage = new();
            TaskTimerService service = new(_runner, storage, GetUtcNow);
            Assert.That(service.TryCreate("A", out TaskTimerHandle first), Is.True);
            Assert.That(service.TryInit(first), Is.True);
            Assert.That(service.TryStart("A", 60.0d), Is.True);
            Assert.That(service.TryCreate("B", out TaskTimerHandle second), Is.True);
            Assert.That(service.TryInit(second), Is.True);
            Assert.That(service.TryStart("B", 120.0d), Is.True);
            Assert.That(storage.TryLoad("TaskTimer_A_SNAPSHOT", out string snapshot), Is.True);
            int saveCnt = storage.SaveCnt;
            service.OnTimersChanged += OnTimersChanged;
            try
            {
                storage.DisableRead();
                storage.DisableSave();
                Assert.That(service.TryUnregister(" A "), Is.True);
                Assert.That(_changedCnt, Is.EqualTo(1));
                Assert.That(service.TimerCnt, Is.EqualTo(1));
                Assert.That(service.GetSnapshots()[0].Id, Is.EqualTo("B"));
                Assert.That(service.TryGetHandle("A", out _), Is.False);
                Assert.That(service.TryUnregister("A"), Is.False);
                Assert.That(service.TryUnregister(null), Is.False);
                Assert.That(service.TryUnregister(" "), Is.False);
                Assert.That(first.TryComplete(), Is.False);
                Assert.That(_changedCnt, Is.EqualTo(1));
                _utcNow = _utcNow.AddSeconds(61.0d);
                yield return new WaitForSecondsRealtime(1.2f);
                Assert.That(first.CurType, Is.EqualTo(ETaskTimerType.Processing));
                Assert.That(storage.SaveCnt, Is.EqualTo(saveCnt));
                storage.EnableRead();
                storage.EnableSave();
                Assert.That(storage.TryLoad("TaskTimer_A_SNAPSHOT", out string unchanged), Is.True);
                Assert.That(unchanged, Is.EqualTo(snapshot));
                Assert.That(service.TryInit(first), Is.True);
                Assert.That(first.CurType, Is.EqualTo(ETaskTimerType.Completed));
                Assert.That(service.TimerCnt, Is.EqualTo(2));
                int prevCnt = _changedCnt;
                Assert.That(service.TryClaim("A"), Is.True);
                Assert.That(_changedCnt, Is.EqualTo(prevCnt + 2));
            }
            finally
            {
                service.OnTimersChanged -= OnTimersChanged;
                service.Release();
            }
        }

        [Test]
        public void PeriodUnregisterPreservesSnapshotAndRebindsOnce()
        {
            MemoryStorage storage = new();
            PeriodTimerService service = new(_runner, storage, GetUtcNow);
            Assert.That(service.TryCreate("A", out PeriodTimerHandle first), Is.True);
            Assert.That(service.TryInit(first, 1.0d, 2.0d), Is.True);
            Assert.That(service.TryCreate("B", out PeriodTimerHandle second), Is.True);
            Assert.That(service.TryInit(second, 1.0d, 2.0d), Is.True);
            Assert.That(storage.TryLoad("PeriodTimer_A_SNAPSHOT", out string snapshot), Is.True);
            int saveCnt = storage.SaveCnt;
            service.OnTimersChanged += OnTimersChanged;
            try
            {
                storage.DisableRead();
                storage.DisableSave();
                Assert.That(service.TryUnregister(" A "), Is.True);
                Assert.That(_changedCnt, Is.EqualTo(1));
                Assert.That(service.TimerCnt, Is.EqualTo(1));
                Assert.That(service.GetSnapshots()[0].Id, Is.EqualTo("B"));
                Assert.That(service.TryGetHandle("A", out _), Is.False);
                Assert.That(first.IsReady, Is.False);
                Assert.That(first.TryForceClosed(), Is.False);
                Assert.That(service.TryUnregister("A"), Is.False);
                Assert.That(service.TryUnregister(null), Is.False);
                Assert.That(service.TryUnregister(" "), Is.False);
                Assert.That(storage.SaveCnt, Is.EqualTo(saveCnt));
                Assert.That(_changedCnt, Is.EqualTo(1));
                storage.EnableRead();
                storage.EnableSave();
                Assert.That(storage.TryLoad("PeriodTimer_A_SNAPSHOT", out string unchanged), Is.True);
                Assert.That(unchanged, Is.EqualTo(snapshot));
                Assert.That(service.TryInit(first, 1.0d, 2.0d), Is.True);
                Assert.That(first.IsReady, Is.True);
                Assert.That(service.TimerCnt, Is.EqualTo(2));
                int prevCnt = _changedCnt;
                Assert.That(first.TryForceClosed(), Is.True);
                Assert.That(_changedCnt, Is.EqualTo(prevCnt + 2));
                Assert.That(service.TryUnregister("A"), Is.True);
                Assert.That(service.TryUnregister("B"), Is.True);
                Assert.That(service.GetSnapshots(), Is.Empty);
                prevCnt = _changedCnt;
                service.Release();
                Assert.That(_changedCnt, Is.EqualTo(prevCnt));
            }
            finally
            {
                service.OnTimersChanged -= OnTimersChanged;
                service.Release();
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CancelClearsWorkWithoutClaimAndAllowsRestart(bool isPaused)
        {
            MemoryStorage storage = new();
            TaskTimerService service = new(_runner, storage, GetUtcNow);
            Assert.That(service.TryCreate("Cancel", out TaskTimerHandle handle), Is.True);
            Assert.That(service.TryInit(handle), Is.True);
            Assert.That(service.TryStart("Cancel", 60.0d), Is.True);
            _utcNow = _utcNow.AddSeconds(20.0d);
            if (isPaused)
                Assert.That(service.TryPause("Cancel"), Is.True);

            service.OnTimersChanged += OnTimersChanged;
            handle.OnCompleted += OnTaskCompleted;
            handle.OnClaimed += OnTaskClaimed;
            try
            {
                Assert.That(service.TryCancel(" Cancel "), Is.True);
                Assert.That(_changedCnt, Is.EqualTo(1));
                TaskTimerData data = service.GetSnapshots()[0];
                Assert.That(data.CurType, Is.EqualTo(ETaskTimerType.None));
                Assert.That(data.RemainingSec, Is.Zero);
                Assert.That(data.DurationSec, Is.Zero);
                Assert.That(data.Progress, Is.Zero);
                Assert.That(service.TimerCnt, Is.EqualTo(1));
                Assert.That(_completedCnt, Is.Zero);
                Assert.That(_claimedCnt, Is.Zero);
                Assert.That(service.TryGetClaimed("Cancel", out bool isClaimed), Is.True);
                Assert.That(isClaimed, Is.False);
                Assert.That(storage.TryLoad("TaskTimer_Cancel_SNAPSHOT", out string snapshot), Is.True);
                Assert.That(snapshot, Is.EqualTo($"1|0|0|0|{_utcNow.Ticks}|0"));
                Assert.That(service.TryCancel("Cancel"), Is.False);
                Assert.That(_changedCnt, Is.EqualTo(1));
                Assert.That(service.TryUnregister("Cancel"), Is.True);
                _utcNow = _utcNow.AddDays(1.0d);
                Assert.That(service.TryInit(handle), Is.True);
                Assert.That(handle.CurType, Is.EqualTo(ETaskTimerType.None));
                Assert.That(handle.TryResume(), Is.False);
                Assert.That(service.TryGetClaimed("Cancel", out isClaimed), Is.True);
                Assert.That(isClaimed, Is.False);
                Assert.That(handle.TryStart(30.0d), Is.True);
                Assert.That(handle.RemainingSec, Is.EqualTo(30));
                Assert.That(_completedCnt, Is.Zero);
                Assert.That(_claimedCnt, Is.Zero);
            }
            finally
            {
                handle.OnCompleted -= OnTaskCompleted;
                handle.OnClaimed -= OnTaskClaimed;
                service.OnTimersChanged -= OnTimersChanged;
                service.Release();
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CancelSaveFailurePreservesWorkAndSnapshot(bool isPaused)
        {
            MemoryStorage storage = new();
            TaskTimerService service = new(_runner, storage, GetUtcNow);
            Assert.That(service.TryCreate("Cancel", out TaskTimerHandle handle), Is.True);
            Assert.That(service.TryInit(handle), Is.True);
            Assert.That(handle.TryStart(60.0d), Is.True);
            if (isPaused)
                Assert.That(handle.TryPause(), Is.True);

            TaskTimerData before = handle.ToData();
            Assert.That(storage.TryLoad("TaskTimer_Cancel_SNAPSHOT", out string snapshot), Is.True);
            service.OnTimersChanged += OnTimersChanged;
            try
            {
                storage.DisableSave();
                Assert.That(service.TryCancel("Cancel"), Is.False);
                Assert.That(handle.CurType, Is.EqualTo(before.CurType));
                Assert.That(handle.RemainingSec, Is.EqualTo(before.RemainingSec));
                Assert.That(handle.ToData().Progress, Is.EqualTo(before.Progress));
                Assert.That(storage.TryLoad("TaskTimer_Cancel_SNAPSHOT", out string unchanged), Is.True);
                Assert.That(unchanged, Is.EqualTo(snapshot));
                Assert.That(_changedCnt, Is.Zero);
                storage.EnableSave();
                if (isPaused)
                    Assert.That(handle.TryResume(), Is.True);

                Assert.That(handle.TryComplete(), Is.True);
                int saveCnt = storage.SaveCnt;
                int changedCnt = _changedCnt;
                Assert.That(handle.TryCancel(), Is.False);
                Assert.That(handle.CurType, Is.EqualTo(ETaskTimerType.Completed));
                Assert.That(storage.SaveCnt, Is.EqualTo(saveCnt));
                Assert.That(_changedCnt, Is.EqualTo(changedCnt));
                Assert.That(service.TryClaim("Cancel"), Is.True);
            }
            finally
            {
                service.OnTimersChanged -= OnTimersChanged;
                service.Release();
            }
        }

        [Test]
        public void CancelRejectsMissingAndUninitializedTimersWithoutSaving()
        {
            MemoryStorage storage = new();
            TaskTimerService service = new(_runner, storage, GetUtcNow);
            Assert.That(service.TryCreate("Cancel", out TaskTimerHandle handle), Is.True);
            Assert.That(handle.TryCancel(), Is.False);
            Assert.That(service.TryCancel("Cancel"), Is.False);
            Assert.That(service.TryCancel(null), Is.False);
            Assert.That(service.TryCancel(" "), Is.False);
            Assert.That(storage.SaveCnt, Is.Zero);
            handle.Release();
            service.Release();
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OnTimersChanged()
        {
            _changedCnt++;
        }

        private void OnTaskCompleted()
        {
            _completedCnt++;
        }

        private void OnTaskClaimed()
        {
            _claimedCnt++;
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

            public void EnableRead()
            {
                _canRead = true;
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
