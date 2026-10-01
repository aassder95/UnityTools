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
            if (_runner == null || !_runner.isActiveAndEnabled || scenario < ETimerLabScenario.ForwardTime || scenario > ETimerLabScenario.DuplicateClaim)
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
                    bool isCompleted = service.TryComplete(TIMER_ID);
                    bool isCompletionPreserved = handle.CurType == ETaskTimerType.Processing && storage.Capture() == saved;
                    storage.SetSaving(true);
                    bool isCompletedAgain = service.TryComplete(TIMER_ID);
                    string completed = storage.Capture();
                    storage.SetSaving(false);
                    bool isClaimed = service.TryClaim(TIMER_ID);
                    bool isClaimPreserved = handle.CurType == ETaskTimerType.Completed && storage.Capture() == completed;
                    isPassed = !isStarted && isStartPreserved && isStartedAgain && !isCompleted && isCompletionPreserved && isCompletedAgain && !isClaimed && isClaimPreserved && storage.RejectedSaveCnt == 3;
                    result = $"Rejected start={isStarted}, None + empty storage preserved={isStartPreserved}\nRetry start={isStartedAgain}\nRejected completion={isCompleted}, Processing + snapshot preserved={isCompletionPreserved}\nRetry forced completion={isCompletedAgain}\nRejected claim={isClaimed}, Completed + snapshot preserved={isClaimPreserved}\nRejected saves={storage.RejectedSaveCnt}";
                }
                else
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
