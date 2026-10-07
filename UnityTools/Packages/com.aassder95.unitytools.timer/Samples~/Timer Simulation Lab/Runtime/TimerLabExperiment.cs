using System;
using System.Globalization;
using UnityEngine;
using UnityTools.Timer.Task;

namespace UnityTools.Timer.Samples
{
    public class TimerLabExperiment
    {
        //============================================================
        // Constants
        //============================================================
        private const string TIMER_ID = "SIMULATION";

        //============================================================
        // Readonly
        //============================================================
        private readonly MonoBehaviour _runner;

        //============================================================
        // Fields
        //============================================================
        private DateTime _utcNow;

        //============================================================
        // Constructors
        //============================================================
        public TimerLabExperiment(MonoBehaviour runner)
        {
            _runner = runner;
        }

        //============================================================
        // Logic
        //============================================================
        public bool TryRun(ETimerLabScenario scenario, out TimerLabReport report)
        {
            report = null;
            if (_runner == null || !_runner.isActiveAndEnabled || scenario < ETimerLabScenario.ForwardTime || scenario > ETimerLabScenario.DeleteRestore)
                return false;

            _utcNow = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimerLabStorage storage = new TimerLabStorage();
            TaskTimerService service = new TaskTimerService(_runner, storage, ReadUtc);
            try
            {
                if (!service.TryCreate(TIMER_ID, out TaskTimerHandle handle) || !service.TryInit(handle))
                    return false;

                if (scenario != ETimerLabScenario.SaveFailure && !service.TryStart(TIMER_ID, 60.0d))
                    return false;

                string before = Capture(handle, storage);
                bool isPassed;
                string result;
                if (scenario == ETimerLabScenario.ForwardTime)
                {
                    _utcNow = _utcNow.AddSeconds(90.0d);
                    int expiredRemainSec = handle.RemainingSec;
                    service.Release();
                    bool isRefreshed = service.TryInit(handle);
                    isPassed = isRefreshed && expiredRemainSec == 0 && handle.CurType == ETaskTimerType.Completed;
                    result = $"Advance virtual UTC by 90 sec\nRemaining before refresh={expiredRemainSec}\nRelease + reinitialize same handle={isRefreshed}\nFinal state={handle.CurType}\nExpiration is evaluated by initialization; no forced TryComplete call.";
                }
                else if (scenario == ETimerLabScenario.OfflineRestore)
                {
                    service.Release();
                    _utcNow = _utcNow.AddSeconds(30.0d);
                    service = new TaskTimerService(_runner, storage, ReadUtc);
                    if (!service.TryCreate(TIMER_ID, out handle) || !service.TryInit(handle))
                        return false;

                    int remainingSec = handle.RemainingSec;
                    bool isProcessing = handle.CurType == ETaskTimerType.Processing;
                    service.Release();
                    _utcNow = _utcNow.AddSeconds(90.0d);
                    service = new TaskTimerService(_runner, storage, ReadUtc);
                    if (!service.TryCreate(TIMER_ID, out handle) || !service.TryInit(handle))
                        return false;

                    isPassed = remainingSec == 30 && isProcessing && handle.CurType == ETaskTimerType.Completed;
                    result = $"Release service, advance 30 sec, recreate service\nRestored state Processing={isProcessing}, remaining={remainingSec} sec\nRelease again, advance 90 sec, recreate again\nFinal state={handle.CurType}\nSame in-memory storage survives service recreation.";
                }
                else if (scenario == ETimerLabScenario.ClockRollback)
                {
                    service.Release();
                    _utcNow = _utcNow.AddSeconds(-30.0d);
                    bool isRestored = service.TryInit(handle);
                    int durationSec = handle.ToData().DurationSec;
                    string adjusted = storage.Capture();
                    service.Release();
                    bool isRestoredAgain = service.TryInit(handle);
                    int repeatedDurationSec = handle.ToData().DurationSec;
                    isPassed = isRestored && isRestoredAgain && durationSec == 90 && repeatedDurationSec == 90 && adjusted == storage.Capture();
                    result = $"Move virtual UTC backward 30 sec, reinitialize\nInit={isRestored}, adjusted duration={durationSec} sec\nReinitialize at same UTC={isRestoredAgain}\nDuration again={repeatedDurationSec} sec\nAdjustment snapshot unchanged={adjusted == storage.Capture()}\nCurrent policy adds rollback to duration once; this is not anti-cheat.";
                }
                else if (scenario == ETimerLabScenario.SaveFailure)
                {
                    storage.SetSaving(false);
                    bool isStarted = service.TryStart(TIMER_ID, 60.0d);
                    bool isStartPreserved = handle.CurType == ETaskTimerType.None && storage.Capture() == "<empty>";
                    storage.SetSaving(true);
                    bool isStartedAgain = service.TryStart(TIMER_ID, 60.0d);
                    string saved = storage.Capture();
                    storage.SetSaving(false);
                    bool isPaused = service.TryPause(TIMER_ID);
                    bool isPausePreserved = handle.CurType == ETaskTimerType.Processing && storage.Capture() == saved;
                    storage.SetSaving(true);
                    bool isPausedAgain = service.TryPause(TIMER_ID);
                    string paused = storage.Capture();
                    storage.SetSaving(false);
                    bool isResumed = service.TryResume(TIMER_ID);
                    bool isCancelled = service.TryCancel(TIMER_ID);
                    bool isPausedPreserved = handle.CurType == ETaskTimerType.Paused && storage.Capture() == paused;
                    storage.SetSaving(true);
                    bool isResumedAgain = service.TryResume(TIMER_ID);
                    saved = storage.Capture();
                    storage.SetSaving(false);
                    bool isCompleted = service.TryComplete(TIMER_ID);
                    bool isCompletionPreserved = handle.CurType == ETaskTimerType.Processing && storage.Capture() == saved;
                    storage.SetSaving(true);
                    bool isCompletedAgain = service.TryComplete(TIMER_ID);
                    string completed = storage.Capture();
                    storage.SetSaving(false);
                    bool isClaimed = service.TryClaim(TIMER_ID);
                    bool isClaimPreserved = handle.CurType == ETaskTimerType.Completed && storage.Capture() == completed;
                    isPassed = !isStarted && isStartPreserved && isStartedAgain && !isPaused && isPausePreserved && isPausedAgain && !isResumed && !isCancelled && isPausedPreserved && isResumedAgain && !isCompleted && isCompletionPreserved && isCompletedAgain && !isClaimed && isClaimPreserved && storage.RejectedSaveCnt == 6;
                    result = $"Rejected start={isStarted}, None + empty storage preserved={isStartPreserved}\nRetry start={isStartedAgain}\nRejected pause={isPaused}, Processing + snapshot preserved={isPausePreserved}\nRetry pause={isPausedAgain}\nRejected resume={isResumed}, rejected cancel={isCancelled}\nPaused + snapshot preserved={isPausedPreserved}, retry resume={isResumedAgain}\nRejected completion={isCompleted}, snapshot preserved={isCompletionPreserved}\nRetry completion={isCompletedAgain}\nRejected claim={isClaimed}, Completed + snapshot preserved={isClaimPreserved}\nRejected saves={storage.RejectedSaveCnt}";
                }
                else if (scenario == ETimerLabScenario.DuplicateClaim)
                {
                    bool isCompleted = service.TryComplete(TIMER_ID);
                    bool isClaimed = service.TryClaim(TIMER_ID);
                    service.Release();
                    service = new TaskTimerService(_runner, storage, ReadUtc);
                    if (!service.TryCreate(TIMER_ID, out handle) || !service.TryInit(handle))
                        return false;

                    string saved = storage.Capture();
                    bool isClaimedAgain = service.TryClaim(TIMER_ID);
                    bool isFlagRead = service.TryGetClaimed(TIMER_ID, out bool hasClaimed);
                    isPassed = isCompleted && isClaimed && !isClaimedAgain && isFlagRead && hasClaimed && handle.CurType == ETaskTimerType.None && storage.Capture() == saved;
                    result = $"Forced completion={isCompleted}, first claim={isClaimed}\nRecreate service from saved claim snapshot\nSecond claim={isClaimedAgain}\nClaim flag read={isFlagRead}, claimed={hasClaimed}\nState={handle.CurType}, snapshot unchanged={storage.Capture() == saved}\nChecks timer claim state only; no external reward transaction is simulated.";
                }
                else if (scenario == ETimerLabScenario.PauseResume)
                {
                    _utcNow = _utcNow.AddSeconds(20.0d);
                    bool isPaused = service.TryPause(TIMER_ID);
                    string paused = storage.Capture();
                    service.Release();
                    _utcNow = _utcNow.AddDays(1.0d);
                    bool isRestored = service.TryInit(handle);
                    bool isFrozen = handle.CurType == ETaskTimerType.Paused && handle.RemainingSec == 40 && storage.Capture() == paused;
                    bool isResumed = service.TryResume(TIMER_ID);
                    int remainingSec = handle.RemainingSec;
                    service.Release();
                    _utcNow = _utcNow.AddSeconds(40.0d);
                    bool isCompleted = service.TryInit(handle);
                    isPassed = isPaused && isRestored && isFrozen && isResumed && remainingSec == 40 && isCompleted && handle.CurType == ETaskTimerType.Completed;
                    result = $"Advance 20 sec, pause={isPaused}\nPAUSED STORAGE\n{paused}\nRelease, advance one day, restore={isRestored}\nPaused state + 40 sec + snapshot preserved={isFrozen}\nResume={isResumed}, remaining={remainingSec} sec\nAdvance 40 sec, reinitialize={isCompleted}, state={handle.CurType}";
                }
                else if (scenario == ETimerLabScenario.CancelRestart)
                {
                    bool isPaused = service.TryPause(TIMER_ID);
                    bool isCancelled = service.TryCancel(TIMER_ID);
                    string cancelled = storage.Capture();
                    bool isFlagRead = service.TryGetClaimed(TIMER_ID, out bool hasClaimed);
                    bool isReset = handle.CurType == ETaskTimerType.None && handle.RemainingSec == 0 && handle.ToData().Progress == 0.0f;
                    bool isCancelledAgain = service.TryCancel(TIMER_ID);
                    service.Release();
                    _utcNow = _utcNow.AddDays(1.0d);
                    bool isRestored = service.TryInit(handle);
                    bool isPreserved = handle.CurType == ETaskTimerType.None && storage.Capture() == cancelled;
                    bool isRestarted = service.TryStart(TIMER_ID, 30.0d);
                    isPassed = isPaused && isCancelled && isFlagRead && !hasClaimed && isReset && !isCancelledAgain && isRestored && isPreserved && isRestarted && handle.RemainingSec == 30 && service.TimerCnt == 1;
                    result = $"Pause={isPaused}, cancel={isCancelled}, reset={isReset}\nCANCELLED STORAGE\n{cancelled}\nClaim flag read={isFlagRead}, claimed={hasClaimed}\nDuplicate cancel={isCancelledAgain}\nAdvance one day, restore={isRestored}, None preserved={isPreserved}\nRestart={isRestarted}, remaining={handle.RemainingSec} sec, registered={service.TimerCnt}";
                }
                else if (scenario == ETimerLabScenario.DeleteRestore)
                {
                    string saved = storage.Capture();
                    storage.SetSaving(false);
                    bool isRejected = !service.TryDelete(TIMER_ID);
                    bool isPreserved = storage.Capture() == saved && service.TimerCnt == 1 && handle.CurType == ETaskTimerType.Processing;
                    storage.SetSaving(true);
                    bool isDeleted = service.TryDelete(TIMER_ID);
                    int deletedCnt = service.TimerCnt;
                    bool isClaimRead = service.TryGetClaimed(TIMER_ID, out bool isClaimed);
                    _utcNow = _utcNow.AddDays(1.0d);
                    bool isRestored = service.TryInit(handle);
                    bool isReset = handle.CurType == ETaskTimerType.None && handle.ToData().DurationSec == 0;
                    bool isRestarted = service.TryStart(TIMER_ID, 30.0d);
                    isPassed = isRejected && isPreserved && isDeleted && deletedCnt == 0 && isClaimRead && !isClaimed && isRestored && isReset && isRestarted && handle.RemainingSec == 30;
                    result = $"Rejected delete={isRejected}, state + snapshot preserved={isPreserved}\nDelete retry={isDeleted}, registered count={deletedCnt}\nClaim flag read={isClaimRead}, claimed={isClaimed}\nAdvance one day, restore={isRestored}, reset None={isReset}\nRestart={isRestarted}, remaining={handle.RemainingSec} sec\nDelete resets stored history; unregister preserves it.";
                }
                else
                {
                    _utcNow = _utcNow.AddSeconds(20.0d);
                    string saved = storage.Capture();
                    bool isUnregistered = service.TryUnregister(TIMER_ID);
                    int unregisteredCnt = service.TimerCnt;
                    bool isPreserved = storage.Capture() == saved;
                    bool isUnregisteredAgain = service.TryUnregister(TIMER_ID);
                    _utcNow = _utcNow.AddSeconds(30.0d);
                    bool isRestored = service.TryInit(handle);
                    int remainingSec = handle.RemainingSec;
                    bool isProcessing = handle.CurType == ETaskTimerType.Processing;
                    bool isUnregisteredLater = service.TryUnregister(TIMER_ID);
                    _utcNow = _utcNow.AddSeconds(20.0d);
                    bool isExpired = service.TryInit(handle);
                    isPassed = isUnregistered && unregisteredCnt == 0 && isPreserved && !isUnregisteredAgain && isRestored && isProcessing && remainingSec == 10 && isUnregisteredLater && isExpired && handle.CurType == ETaskTimerType.Completed && service.TimerCnt == 1;
                    result = $"Advance 20 sec, unregister={isUnregistered}\nRegistered count={unregisteredCnt}, snapshot preserved={isPreserved}\nDuplicate unregister={isUnregisteredAgain}\nAdvance 30 sec, re-register={isRestored}\nProcessing={isProcessing}, remaining={remainingSec} sec\nUnregister again={isUnregisteredLater}, advance 20 sec\nRe-register={isExpired}, final state={handle.CurType}, count={service.TimerCnt}\nUnregister releases execution; it does not freeze UTC time.";
                }

                report = new TimerLabReport(isPassed, before, Capture(handle, storage), result);
                return true;
            }
            finally
            {
                service.Release();
            }
        }

        //============================================================
        // Utilities
        //============================================================
        private DateTime ReadUtc() => _utcNow;

        private string Capture(TaskTimerHandle handle, TimerLabStorage storage)
        {
            TaskTimerData data = handle.ToData();
            return "UTC: " + _utcNow.ToString("O", CultureInfo.InvariantCulture) + $"\nState: {data.CurType}\nRemaining: {handle.RemainingSec} sec\nDuration: {data.DurationSec} sec\n\nSTORAGE\n" + storage.Capture();
        }
    }
}
