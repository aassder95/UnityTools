using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace UnityTools.Timer.Samples.Tests
{
    public class TimerLabTests
    {
        //============================================================
        // Logic
        //============================================================
        [TestCase(ETimerLabScenario.ForwardTime)]
        [TestCase(ETimerLabScenario.OfflineRestore)]
        [TestCase(ETimerLabScenario.ClockRollback)]
        [TestCase(ETimerLabScenario.SaveFailure)]
        [TestCase(ETimerLabScenario.DuplicateClaim)]
        public void FixtureChecksTimerStateAndPersistence(ETimerLabScenario scenario)
        {
            GameObject go = new GameObject("ClockFixture", typeof(RectTransform));
            Image runner = go.AddComponent<Image>();
            try
            {
                TimerLabExperiment experiment = new TimerLabExperiment(runner);
                Assert.That(experiment.TryRun(scenario, out TimerLabReport report), Is.True);
                Assert.That(report.IsPassed, Is.True, report.Result);
                Assert.That(report.Before, Does.Contain("2026-01-01T00:00:00.0000000Z"));
                Assert.That(report.After, Does.Contain("TaskTimer_SIMULATION_SNAPSHOT"));
                if (scenario == ETimerLabScenario.ForwardTime || scenario == ETimerLabScenario.OfflineRestore)
                {
                    Assert.That(report.After, Does.Contain("State: Completed"));
                    Assert.That(report.After, Does.Contain("Remaining: 0 sec"));
                }
                else if (scenario == ETimerLabScenario.ClockRollback)
                {
                    Assert.That(report.After, Does.Contain("Duration: 90 sec"));
                    Assert.That(report.After, Does.Contain("2025-12-31T23:59:30.0000000Z"));
                }
                else if (scenario == ETimerLabScenario.SaveFailure)
                {
                    Assert.That(report.Result, Does.Contain("Rejected saves=3"));
                }
                else
                {
                    Assert.That(report.After, Does.Contain("State: None"));
                    Assert.That(report.Result, Does.Contain("Second claim=False"));
                }

                Assert.That(experiment.TryRun(scenario, out TimerLabReport repeated), Is.True);
                Assert.That(repeated.After, Is.EqualTo(report.After));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void InvalidRunnerAndScenarioAreRejected()
        {
            Assert.That(new TimerLabExperiment(null).TryRun(ETimerLabScenario.ForwardTime, out TimerLabReport report), Is.False);
            Assert.That(report, Is.Null);
            GameObject go = new GameObject("InvalidFixture", typeof(RectTransform));
            try
            {
                Image runner = go.AddComponent<Image>();
                Assert.That(new TimerLabExperiment(runner).TryRun((ETimerLabScenario)99, out report), Is.False);
                Assert.That(report, Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void RejectedSavePreservesSnapshotAndCountsFailures()
        {
            TimerLabStorage storage = new TimerLabStorage();
            Assert.That(storage.TrySave("key", "original"), Is.True);
            storage.SetSaving(false);
            Assert.That(storage.TrySave("key", "replacement"), Is.False);
            Assert.That(storage.TryLoad("key", out string data), Is.True);
            Assert.That(data, Is.EqualTo("original"));
            Assert.That(storage.SaveCnt, Is.EqualTo(1));
            Assert.That(storage.RejectedSaveCnt, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ButtonsExecuteFixturesAndUnsubscribe()
        {
            yield return SceneManager.LoadSceneAsync("TimerSimulationLab");
            TimerLabController controller = Object.FindFirstObjectByType<TimerLabController>();
            string[] fields = { "_btnForwardTime", "_btnOffline", "_btnRollback", "_btnFailure", "_btnClaim" };
            for (int idx = 0; idx < fields.Length; ++idx)
            {
                ReadField<Button>(controller, fields[idx]).onClick.Invoke();
                Assert.That(controller.Report.IsPassed, Is.True, controller.Report.Result);
                Assert.That(ReadField<Text>(controller, "_txtBefore").text, Is.EqualTo(controller.Report.Before));
                Assert.That(ReadField<Text>(controller, "_txtAfter").text, Is.EqualTo(controller.Report.After));
                Assert.That(ReadField<Text>(controller, "_txtResult").text, Does.StartWith("PASS"));
                TimerLabReport snapshot = controller.Report;
                yield return new WaitForSecondsRealtime(1.1f);
                Assert.That(controller.Report, Is.SameAs(snapshot));
            }

            TimerLabReport previous = controller.Report;
            controller.enabled = false;
            ReadField<Button>(controller, "_btnForwardTime").onClick.Invoke();
            Assert.That(controller.Report, Is.SameAs(previous));
            controller.enabled = true;
            ReadField<Button>(controller, "_btnForwardTime").onClick.Invoke();
            Assert.That(controller.Report, Is.Not.SameAs(previous));
            Assert.That(controller.Report.IsPassed, Is.True);
        }

        //============================================================
        // Utilities
        //============================================================
        private static T ReadField<T>(TimerLabController controller, string name)
        {
            return (T)typeof(TimerLabController).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(controller);
        }
    }
}
