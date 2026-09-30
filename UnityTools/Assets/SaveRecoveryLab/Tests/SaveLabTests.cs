using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace UnityTools.Persistence.Samples.Tests
{
    public class SaveLabTests
    {
        //============================================================
        // Logic
        //============================================================
        [TestCase(ESaveLabScenario.Migration)]
        [TestCase(ESaveLabScenario.BackupRecovery)]
        [TestCase(ESaveLabScenario.FutureVersion)]
        [TestCase(ESaveLabScenario.MissingFile)]
        [TestCase(ESaveLabScenario.CorruptFile)]
        [TestCase(ESaveLabScenario.RejectedMigration)]
        public void FixtureChecksActualSaveInvariants(ESaveLabScenario scenario)
        {
            Assert.That(new SaveLabExperiment().TryRun(scenario, out SaveLabReport report), Is.True);
            Assert.That(report.IsPassed, Is.True, report.Result);
            Assert.That(report.Before, Does.Contain("PRIMARY"));
            Assert.That(report.After, Does.Contain("BACKUP"));
            if (scenario == ESaveLabScenario.Migration)
            {
                Assert.That(report.Before, Does.Contain("\"_version\":1"));
                Assert.That(report.After, Does.Contain("\"_version\":2"));
                Assert.That(report.Result, Does.Contain("level=3, branchCnt=4"));
            }
            else if (scenario == ESaveLabScenario.BackupRecovery)
            {
                Assert.That(report.Before, Does.Contain("PRIMARY\n{}"));
                Assert.That(report.After, Does.Not.Contain("PRIMARY\n{}"));
                Assert.That(report.Result, Does.Contain("backup unchanged=True"));
            }
            else
            {
                Assert.That(report.After, Is.EqualTo(report.Before));
                Assert.That(report.Result, Does.Contain("Load=False"));
            }
        }

        [Test]
        public void InvalidScenarioCreatesNoReport()
        {
            Assert.That(new SaveLabExperiment().TryRun((ESaveLabScenario)100, out SaveLabReport report), Is.False);
            Assert.That(report, Is.Null);
        }

        [UnityTest]
        public IEnumerator SceneButtonsRunAndUnsubscribe()
        {
            yield return SceneManager.LoadSceneAsync("SaveRecoveryLab");
            SaveLabController controller = Object.FindFirstObjectByType<SaveLabController>();
            string[] fields = { "_btnMigration", "_btnRecovery", "_btnFuture", "_btnMissing", "_btnCorrupt", "_btnRejected" };
            Text txtResult = ReadField<Text>(controller, "_txtResult");
            for (int idx = 0; idx < fields.Length; ++idx)
            {
                ReadField<Button>(controller, fields[idx]).onClick.Invoke();
                Assert.That(controller.Report.IsPassed, Is.True, controller.Report.Result);
                Assert.That(txtResult.text, Does.StartWith("PASS"));
                Assert.That(ReadField<Text>(controller, "_txtBefore").text, Is.EqualTo(controller.Report.Before));
                Assert.That(ReadField<Text>(controller, "_txtAfter").text, Is.EqualTo(controller.Report.After));
            }

            SaveLabReport previous = controller.Report;
            controller.enabled = false;
            ReadField<Button>(controller, "_btnMigration").onClick.Invoke();
            Assert.That(controller.Report, Is.SameAs(previous));
            controller.enabled = true;
            ReadField<Button>(controller, "_btnMigration").onClick.Invoke();
            Assert.That(controller.Report, Is.Not.SameAs(previous));
            Assert.That(controller.Report.IsPassed, Is.True);
        }

        //============================================================
        // Utilities
        //============================================================
        private static T ReadField<T>(SaveLabController controller, string name)
        {
            return (T)typeof(SaveLabController).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(controller);
        }
    }
}
