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

        [UnityTest]
        public IEnumerator ComparisonRunsBothOrdersAndExportsPairs()
        {
            yield return SceneManager.LoadSceneAsync("UiPerformanceLab");
            UiLabController controller = Object.FindFirstObjectByType<UiLabController>();
            SetField(controller, "_warmupFrames", 3);
            SetField(controller, "_sampleFrames", 12);
            SetField(controller, "_mutationIntervalFrames", 1);
            GetField<InputField>(controller, "_inputItemCnt").text = "1000";
            for (int mode = 0; mode < 3; ++mode)
            {
                for (int order = 0; order < 2; ++order)
                {
                    GetField<Dropdown>(controller, "_scenarioChoice").value = mode;
                    GetField<Dropdown>(controller, "_orderChoice").value = order;
                    GetField<Button>(controller, "_btnCompare").onClick.Invoke();
                    float deadlineSec = Time.realtimeSinceStartup + 30.0f;
                    while (controller.IsRunning && Time.realtimeSinceStartup < deadlineSec)
                    {
                        yield return null;
                    }

                    Assert.That(controller.IsRunning, Is.False);
                    UiLabComparison pair = controller.Comparison;
                    Assert.That(pair, Is.Not.Null, GetField<Text>(controller, "_txtResult").text);
                    Assert.That(pair.IsBaselineFirst, Is.EqualTo(order == 0));
                    Assert.That(pair.Baseline.Measurement.Seed, Is.EqualTo(pair.Virtualized.Measurement.Seed));
                    Assert.That(pair.Baseline.Measurement.AgentCnt, Is.EqualTo(1000));
                    Assert.That(pair.Baseline.CreatedItemCnt, Is.GreaterThanOrEqualTo(1000));
                    Assert.That(pair.Virtualized.CreatedItemCnt, Is.LessThan(100));
                    Assert.That(pair.BaselineInitMs, Is.GreaterThan(0.0d));
                    Assert.That(pair.VirtualizedInitMs, Is.GreaterThan(0.0d));
                    Assert.That(GetField<UiLabBaseline>(controller, "_baseline").LiveItemCnt, Is.Zero);
                    Assert.That(GetField<UiLabScroll>(controller, "_scroll").IsInitialized, Is.False);
                    string path = Path.Combine(Application.temporaryCachePath, "ui-lab-pair-" + System.Guid.NewGuid().ToString("N") + ".csv");
                    Assert.That(pair.TryExport(path), Is.True);
                    string[] lines = File.ReadAllLines(path);
                    Assert.That(lines.Length, Is.EqualTo(3));
                    Assert.That(lines[0].Split(',').Length, Is.EqualTo(33));
                    Assert.That(lines[1].Split(',').Length, Is.EqualTo(33));
                    Assert.That(lines[1], Does.Contain(pair.PairId + ",Baseline,"));
                    Assert.That(lines[2], Does.Contain(pair.PairId + ",Virtualized,"));
                    File.Delete(path);
                }
            }

            controller.Compare();
            controller.Stop();
            Assert.That(controller.Comparison, Is.Null);
            Assert.That(controller.IsRunning, Is.False);
            controller.Compare();
            yield return null;
            controller.enabled = false;
            Assert.That(controller.IsRunning, Is.False);
            Assert.That(controller.Comparison, Is.Null);
            controller.enabled = true;
            GetField<InputField>(controller, "_inputItemCnt").text = "10001";
            controller.Compare();
            Assert.That(controller.IsRunning, Is.False);
            Assert.That(controller.Comparison, Is.Null);
        }

        [UnityTest]
        public IEnumerator BaselinePreservesAnchorAndMatchesScrollGeometry()
        {
            yield return SceneManager.LoadSceneAsync("UiPerformanceLab");
            UiLabController controller = Object.FindFirstObjectByType<UiLabController>();
            controller.enabled = false;
            UiLabBaseline baseline = GetField<UiLabBaseline>(controller, "_baseline");
            UiLabScroll scroll = GetField<UiLabScroll>(controller, "_scroll");
            UiLabScenario.TryCreate(100, 42, EUiLabScenario.InsertRemove, 3, 10, 1, out UiLabScenario scenario);
            Canvas.ForceUpdateCanvases();
            baseline.Init(scenario, 10);
            baseline.ScrollTo(50);
            float initialPos = baseline.ScrollPos;
            scenario.Advance();
            baseline.Advance(scenario);
            scenario.Advance();
            baseline.Advance(scenario);
            Assert.That(baseline.ScrollPos, Is.EqualTo(initialPos + 440.0f).Within(0.1f));
            Assert.That(baseline.LiveItemCnt, Is.EqualTo(110));
            scenario.Advance();
            baseline.Advance(scenario);
            Assert.That(baseline.ScrollPos, Is.EqualTo(initialPos).Within(0.1f));
            baseline.Release();
            yield return null;
            scroll.InitView(100);
            scroll.ScrollTo(50, true);
            Assert.That(GetField<RectTransform>(baseline, "_rtContent").anchoredPosition.y, Is.EqualTo(initialPos).Within(0.1f));
            scroll.ReleaseView();
        }

        //============================================================
        // Utilities
        //============================================================
        private static void SetField(Component target, string name, int value)
        {
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }

        private static T GetField<T>(Component target, string name)
        {
            return (T)target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
        }
    }
}
