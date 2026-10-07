using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityTools.Timer.Task;

namespace UnityTools.TimerDashboard.Tests
{
    public class TimerDashboardTests
    {
        //============================================================
        // Fields
        //============================================================
        private TimerDashboardController _controller;
        private TimerDashboardBootstrap _bootstrap;
        private InputField _inputId;
        private Button[] _btnActions;

        //============================================================
        // Logic
        //============================================================
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("TimerDashboard");
            _controller = Object.FindFirstObjectByType<TimerDashboardController>();
            _bootstrap = _controller.GetComponent<TimerDashboardBootstrap>();
            _inputId = _controller.GetComponentInChildren<InputField>();
            _btnActions = _controller.GetComponentsInChildren<Button>();
            yield return null;
            Assert.That(_controller.IsInitialized, Is.True);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            _controller.Release();
            yield return null;
        }

        [UnityTest]
        public IEnumerator ButtonsDriveTaskLifecycleAndRefreshRows()
        {
            Click(ETimerDashboardAction.Register);
            Click(ETimerDashboardAction.Start);
            Click(ETimerDashboardAction.Pause);
            Assert.That(_controller.TaskTimers.GetSnapshots()[0].CurType, Is.EqualTo(ETaskTimerType.Paused));
            Click(ETimerDashboardAction.Resume);
            Click(ETimerDashboardAction.Cancel);
            Click(ETimerDashboardAction.Start);
            Click(ETimerDashboardAction.Complete);
            Click(ETimerDashboardAction.Claim);
            Assert.That(_controller.TaskTimers.TryGetHandle("BUILD_A", out var claimed), Is.True);
            Assert.That(claimed.TryGetClaimed(out bool isClaimed) && isClaimed, Is.True);
            yield return null;
            yield return null;
            Assert.That(GameObject.Find("Timers").GetComponent<Text>().text, Does.Contain("BUILD_A"));
            Click(ETimerDashboardAction.Delete);
            Assert.That(_controller.TaskTimers.TimerCnt, Is.Zero);
            Click(ETimerDashboardAction.Register);
            Assert.That(_controller.TaskTimers.GetSnapshots()[0].CurType, Is.EqualTo(ETaskTimerType.None));
        }

        [UnityTest]
        public IEnumerator FailedDeletePreservesTaskAndRetryLeavesOtherId()
        {
            Click(ETimerDashboardAction.Register);
            Click(ETimerDashboardAction.Start);
            _inputId.text = "OTHER";
            Click(ETimerDashboardAction.Register);
            _inputId.text = "BUILD_A";
            _bootstrap.Storage.SetSaving(false);
            Click(ETimerDashboardAction.Delete);
            Assert.That(_controller.TaskTimers.TimerCnt, Is.EqualTo(2));
            _bootstrap.Storage.SetSaving(true);
            Click(ETimerDashboardAction.Delete);
            Assert.That(_controller.TaskTimers.GetSnapshots()[0].Id, Is.EqualTo("OTHER"));
            yield return null;
        }

        [UnityTest]
        public IEnumerator DisableReenableRestoresSavedStateAndRejectsDuplicateIds()
        {
            _inputId.text = " ";
            Click(ETimerDashboardAction.Register);
            Assert.That(_controller.TaskTimers.TimerCnt, Is.Zero);
            _inputId.text = "BUILD_A";
            Click(ETimerDashboardAction.Register);
            Click(ETimerDashboardAction.Register);
            Assert.That(_controller.TaskTimers.TimerCnt, Is.EqualTo(1));
            Click(ETimerDashboardAction.Start);
            _controller.gameObject.SetActive(false);
            Click(ETimerDashboardAction.Register);
            Assert.That(_controller.IsInitialized, Is.False);
            _controller.gameObject.SetActive(true);
            yield return null;
            yield return null;
            Click(ETimerDashboardAction.Register);
            Assert.That(_controller.TaskTimers.GetSnapshots()[0].CurType, Is.EqualTo(ETaskTimerType.Processing));
            Click(ETimerDashboardAction.Unregister);
            Assert.That(_controller.TaskTimers.TimerCnt, Is.Zero);
            Click(ETimerDashboardAction.Register);
            Assert.That(_controller.TaskTimers.GetSnapshots()[0].CurType, Is.EqualTo(ETaskTimerType.Processing));
            yield return null;
        }

        [UnityTest]
        public IEnumerator SafeAreaRotationKeepsButtonsReadableAndScrollingStable()
        {
            TimerDashboardLayout layout = _controller.GetComponent<TimerDashboardLayout>();
            Vector2[] sizes = { new Vector2(1080.0f, 2400.0f), new Vector2(2400.0f, 1080.0f), new Vector2(1440.0f, 900.0f) };
            Rect[] areas = { new Rect(0.0f, 100.0f, 1080.0f, 2180.0f), new Rect(120.0f, 0.0f, 2220.0f, 1080.0f), new Rect(0.0f, 0.0f, 1440.0f, 900.0f) };
            for (int idx = 0; idx < 30; ++idx)
            {
                _inputId.text = "TASK_" + idx;
                Click(ETimerDashboardAction.Register);
            }

            yield return null;
            yield return null;
            ScrollRect scroll = _controller.transform.Find("SafeArea").GetComponent<ScrollRect>();
            for (int idx = 0; idx < sizes.Length; ++idx)
            {
                Assert.That(layout.TryApplyViewport(sizes[idx], areas[idx]), Is.True);
                Canvas.ForceUpdateCanvases();
                foreach (Button button in _btnActions)
                {
                    RectTransform rt = (RectTransform)button.transform;
                    Assert.That(rt.sizeDelta.x, Is.GreaterThan(250.0f));
                    Assert.That(-rt.anchoredPosition.y + rt.sizeDelta.y, Is.LessThan(scroll.content.sizeDelta.y));
                }

                RectTransform toast = (RectTransform)_controller.transform.Find("SafeArea/Toast");
                Assert.That(toast.anchorMin, Is.EqualTo(Vector2.zero));
                Assert.That(toast.anchoredPosition.y, Is.GreaterThanOrEqualTo(16.0f));
                Text timers = _controller.transform.Find("SafeArea/Content/TimerViewport/Timers").GetComponent<Text>();
                Assert.That(timers.text, Does.Contain("TASK_29"));
                Assert.That(timers.preferredHeight, Is.LessThanOrEqualTo(((RectTransform)timers.transform.parent).sizeDelta.y));
                scroll.content.anchoredPosition = new Vector2(0.0f, 100.0f);
                Assert.That(layout.TryApplyViewport(sizes[idx], areas[idx]), Is.True);
                Assert.That(scroll.content.anchoredPosition.y, Is.EqualTo(100.0f));
            }
        }

        //============================================================
        // Utilities
        //============================================================
        private void Click(ETimerDashboardAction action)
        {
            _btnActions[(int)action].onClick.Invoke();
        }
    }
}
