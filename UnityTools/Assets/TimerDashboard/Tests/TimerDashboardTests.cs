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
        private TimerDashboardController _controller;
        private TimerDashboardBootstrap _bootstrap;
        private InputField _input;
        private Button[] _buttons;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("TimerDashboard");
            _controller = Object.FindFirstObjectByType<TimerDashboardController>();
            _bootstrap = _controller.GetComponent<TimerDashboardBootstrap>();
            _input = _controller.GetComponentInChildren<InputField>();
            _buttons = _controller.GetComponentsInChildren<Button>();
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
            _input.text = "OTHER";
            Click(ETimerDashboardAction.Register);
            _input.text = "BUILD_A";
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
            _input.text = " ";
            Click(ETimerDashboardAction.Register);
            Assert.That(_controller.TaskTimers.TimerCnt, Is.Zero);
            _input.text = "BUILD_A";
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

        private void Click(ETimerDashboardAction action)
        {
            _buttons[(int)action].onClick.Invoke();
        }
    }
}
