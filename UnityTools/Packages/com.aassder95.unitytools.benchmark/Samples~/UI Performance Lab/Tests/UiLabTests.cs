using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace UnityTools.Benchmark.Samples.Tests
{
    public class UiLabTests
    {
        //============================================================
        // Logic
        //============================================================
        [Test]
        public void SeedReproducesRowsAndJumps()
        {
            Assert.That(UiLabScenario.TryCreate(100, 42, EUiLabScenario.RandomJump, 3, 10, 30, out UiLabScenario first), Is.True);
            Assert.That(UiLabScenario.TryCreate(100, 42, EUiLabScenario.RandomJump, 3, 10, 30, out UiLabScenario second), Is.True);
            for (int idx = 0; idx < 100; ++idx)
            {
                Assert.That(first.RowAt(idx), Is.EqualTo(second.RowAt(idx)));
                first.Advance();
                second.Advance();
                Assert.That(first.TargetIdx, Is.EqualTo(second.TargetIdx));
                Assert.That(first.TargetIdx, Is.InRange(0, 99));
            }
        }

        [Test]
        public void MutationRestoresOriginalRows()
        {
            UiLabScenario.TryCreate(100, 42, EUiLabScenario.InsertRemove, 3, 10, 1, out UiLabScenario scenario);
            string original = scenario.RowAt(50);
            scenario.Advance();
            scenario.Advance();
            Assert.That(scenario.MutationDelta, Is.EqualTo(10));
            Assert.That(scenario.RowAt(60), Is.EqualTo(original));
            scenario.Advance();
            Assert.That(scenario.MutationDelta, Is.EqualTo(-10));
            Assert.That(scenario.ItemCnt, Is.EqualTo(100));
            Assert.That(scenario.RowAt(50), Is.EqualTo(original));
        }

        [TestCase(0)]
        [TestCase(100001)]
        public void InvalidItemCountIsRejected(int itemCnt)
        {
            Assert.That(UiLabScenario.TryCreate(itemCnt, 42, EUiLabScenario.Sweep, 3, 10, 30, out UiLabScenario scenario), Is.False);
            Assert.That(scenario, Is.Null);
        }

        [UnityTest]
        public IEnumerator SceneMeasuresExportsAndRestarts()
        {
            yield return SceneManager.LoadSceneAsync("UiPerformanceLab");
            UiLabController controller = Object.FindFirstObjectByType<UiLabController>();
            SetField(controller, "_warmupFrames", 3);
            SetField(controller, "_sampleFrames", 12);
            SetField(controller, "_mutationIntervalFrames", 1);
            Dropdown choice = GetField<Dropdown>(controller, "_scenarioChoice");
            InputField inputCnt = GetField<InputField>(controller, "_inputItemCnt");
            for (int mode = 0; mode < 3; ++mode)
            {
                int itemCnt = mode == 0 ? 1000 : mode == 1 ? 10000 : 100000;
                inputCnt.text = itemCnt.ToString();
                choice.value = mode;
                GetField<Button>(controller, "_btnRun").onClick.Invoke();
                Assert.That(controller.IsRunning, Is.True);
                float deadlineSec = Time.realtimeSinceStartup + 15.0f;
                while (controller.IsRunning && Time.realtimeSinceStartup < deadlineSec)
                {
                    yield return null;
                }

                Assert.That(controller.IsRunning, Is.False);
                Assert.That(controller.Report, Is.Not.Null);
                Assert.That(controller.Report.Measurement.AgentCnt, Is.EqualTo(itemCnt));
                Assert.That(controller.Report.CreatedItemCnt, Is.LessThan(100));
                Assert.That(controller.Report.PeakLiveItemCnt, Is.GreaterThan(0));
                string path = Path.Combine(Application.temporaryCachePath, "ui-lab-test-" + mode + ".csv");
                Assert.That(controller.Report.TryExport(path), Is.True);
                string[] lines = File.ReadAllLines(path);
                Assert.That(lines[0], Does.EndWith("mutation_interval_frames"));
                Assert.That(lines[lines.Length - 1].Split(',').Length, Is.EqualTo(27));
                File.Delete(path);
            }

            controller.Run();
            controller.Stop();
            Assert.That(controller.IsRunning, Is.False);
            Assert.That(controller.Report, Is.Null);
            controller.Run();
            Assert.That(controller.IsRunning, Is.True);
            controller.gameObject.SetActive(false);
            Assert.That(controller.IsRunning, Is.False);
            controller.gameObject.SetActive(true);
            controller.Run();
            Assert.That(controller.IsRunning, Is.True);
            controller.Stop();
        }

        //============================================================
        // Utilities
        //============================================================
        private static void SetField(UiLabController target, string name, int value)
        {
            typeof(UiLabController).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }

        private static T GetField<T>(UiLabController target, string name)
        {
            return (T)typeof(UiLabController).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
        }
    }
}
