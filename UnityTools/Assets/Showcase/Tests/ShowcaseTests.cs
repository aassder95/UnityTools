using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityTools.Ui;
using UnityTools.Benchmark.Samples;
using UnityTools.Persistence.Samples;
using UnityTools.Timer.Samples;

namespace UnityTools.Showcase.Tests
{
    public class ShowcaseTests
    {
        //============================================================
        // Logic
        //============================================================
        [UnityTest]
        public IEnumerator ButtonsVisitEveryLabAndReturnWithoutInputConflicts()
        {
            yield return SceneManager.LoadSceneAsync("Showcase");
            ShowcaseController controller = Object.FindFirstObjectByType<ShowcaseController>();
            string[] fields = { "_btnUi", "_btnSave", "_btnTimer" };
            string[] paths = Field<string[]>(controller, "_scenePaths");
            int sceneCnt = SceneManager.sceneCount;
            for (int visit = 0; visit < 6; ++visit)
            {
                int idx = visit % 3;
                Field<Button>(controller, fields[idx]).onClick.Invoke();
                yield return WaitForNavigation(controller);
                Assert.That(controller.ActiveLabIdx, Is.EqualTo(idx));
                Assert.That(controller.IsHubVisible, Is.False);
                Assert.That(SceneManager.GetActiveScene().path, Is.EqualTo(paths[idx]));
                Assert.That(SceneManager.sceneCount, Is.EqualTo(sceneCnt + 1));
                AssertSingleInput();
                if (idx == 1)
                {
                    SaveLabController lab = Object.FindFirstObjectByType<SaveLabController>();
                    lab.Run(ESaveLabScenario.MissingFile);
                    Assert.That(lab.Report.IsPassed, Is.True);
                }
                else if (idx == 2)
                {
                    TimerLabController lab = Object.FindFirstObjectByType<TimerLabController>();
                    lab.Run(ETimerLabScenario.ForwardTime);
                    Assert.That(lab.Report.IsPassed, Is.True);
                }

                Field<Button>(controller, "_btnReturn").onClick.Invoke();
                yield return WaitForNavigation(controller);
                Assert.That(controller.ActiveLabIdx, Is.EqualTo(-1));
                Assert.That(controller.IsHubVisible, Is.True);
                Assert.That(SceneManager.GetSceneByPath(paths[idx]).isLoaded, Is.False);
                Assert.That(SceneManager.sceneCount, Is.EqualTo(sceneCnt));
                AssertSingleInput();
            }
        }

        [UnityTest]
        public IEnumerator DuplicateRequestsDoNotLoadAnotherLab()
        {
            yield return SceneManager.LoadSceneAsync("Showcase");
            ShowcaseController controller = Object.FindFirstObjectByType<ShowcaseController>();
            controller.OpenLab(2);
            Assert.That(controller.IsBusy, Is.True);
            controller.OpenLab(1);
            controller.ReturnToHub();
            yield return WaitForNavigation(controller);
            Assert.That(controller.ActiveLabIdx, Is.EqualTo(2));
            controller.OpenLab(0);
            Assert.That(controller.IsBusy, Is.False);
            controller.ReturnToHub();
            controller.ReturnToHub();
            yield return WaitForNavigation(controller);
            Assert.That(controller.IsHubVisible, Is.True);
            AssertSingleInput();
        }

        [UnityTest]
        public IEnumerator ReturningDuringPerformanceRunDestroysLabAndAllowsFreshRun()
        {
            yield return SceneManager.LoadSceneAsync("Showcase");
            ShowcaseController controller = Object.FindFirstObjectByType<ShowcaseController>();
            controller.OpenLab(0);
            yield return WaitForNavigation(controller);
            UiLabController lab = Object.FindFirstObjectByType<UiLabController>();
            lab.Run();
            Assert.That(lab.IsRunning, Is.True);
            controller.ReturnToHub();
            yield return WaitForNavigation(controller);
            Assert.That(lab == null, Is.True);
            controller.OpenLab(0);
            yield return WaitForNavigation(controller);
            UiLabController fresh = Object.FindFirstObjectByType<UiLabController>();
            Assert.That(fresh.IsRunning, Is.False);
            Assert.That(fresh.Report, Is.Null);
            fresh.Run();
            Assert.That(fresh.IsRunning, Is.True);
            controller.ReturnToHub();
            yield return WaitForNavigation(controller);
        }

        [UnityTest]
        public IEnumerator UnavailableDestinationAndDisabledSubscriptionsKeepHubUsable()
        {
            yield return SceneManager.LoadSceneAsync("Showcase");
            ShowcaseController controller = Object.FindFirstObjectByType<ShowcaseController>();
            controller.OpenLab(-1);
            Assert.That(controller.IsBusy, Is.False);
            string[] paths = Field<string[]>(controller, "_scenePaths");
            string original = paths[0];
            paths[0] = "Assets/MissingLab.unity";
            controller.OpenLab(0);
            Assert.That(controller.IsHubVisible, Is.True);
            Assert.That(Field<Text>(controller, "_txtStatus").text, Does.Contain("unavailable"));
            paths[0] = original;
            controller.enabled = false;
            Field<Button>(controller, "_btnTimer").onClick.Invoke();
            Assert.That(controller.IsBusy, Is.False);
            controller.enabled = true;
            Field<Button>(controller, "_btnTimer").onClick.Invoke();
            yield return WaitForNavigation(controller);
            Assert.That(controller.ActiveLabIdx, Is.EqualTo(2));
            controller.ReturnToHub();
            yield return WaitForNavigation(controller);
            AssertSingleInput();
        }

        [UnityTest]
        public IEnumerator PortraitCardsAndSafeAreaRemainReachableAfterRotation()
        {
            yield return SceneManager.LoadSceneAsync("Showcase");
            ShowcaseLayout layout = Object.FindFirstObjectByType<ShowcaseLayout>();
            Vector2[] sizes = { new Vector2(1080.0f, 2400.0f), new Vector2(2400.0f, 1080.0f) };
            Rect[] areas = { new Rect(0.0f, 100.0f, 1080.0f, 2180.0f), new Rect(120.0f, 0.0f, 2220.0f, 1080.0f) };
            string[] names = { "UI PERFORMANCE", "SAVE RECOVERY", "TIMER SIMULATION" };
            for (int idx = 0; idx < sizes.Length; ++idx)
            {
                Assert.That(layout.TryApplyViewport(sizes[idx], areas[idx]), Is.True);
                Canvas.ForceUpdateCanvases();
                ScrollRect scroll = layout.transform.Find("SafeArea").GetComponent<ScrollRect>();
                RectTransform previous = null;
                foreach (string name in names)
                {
                    RectTransform card = (RectTransform)scroll.content.Find(name);
                    Assert.That(-card.anchoredPosition.y + card.sizeDelta.y, Is.LessThan(scroll.content.sizeDelta.y));
                    foreach (Text text in card.GetComponentsInChildren<Text>())
                    {
                        Assert.That(text.preferredHeight, Is.LessThanOrEqualTo(text.rectTransform.rect.height + 0.1f), name + " / " + text.name);
                    }

                    if (previous != null && idx == 0)
                        Assert.That(-card.anchoredPosition.y, Is.GreaterThan(-previous.anchoredPosition.y + previous.sizeDelta.y));

                    previous = card;
                }

                Assert.That(scroll.viewport.anchorMin.x, Is.EqualTo(areas[idx].xMin / sizes[idx].x).Within(0.001f));
                Assert.That(scroll.viewport.anchorMax.y, Is.EqualTo(areas[idx].yMax / sizes[idx].y).Within(0.001f));
            }
        }

        //============================================================
        // Utilities
        //============================================================
        private static IEnumerator WaitForNavigation(ShowcaseController controller)
        {
            float deadlineSec = Time.realtimeSinceStartup + 15.0f;
            while (controller.IsBusy && Time.realtimeSinceStartup < deadlineSec)
            {
                yield return null;
            }

            Assert.That(controller.IsBusy, Is.False, "Scene navigation timed out");
        }

        private static void AssertSingleInput()
        {
            Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length, Is.EqualTo(1));
        }

        private static T Field<T>(ShowcaseController controller, string name)
        {
            return (T)typeof(ShowcaseController).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(controller);
        }
    }
}
