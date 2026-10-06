using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityTools.Ui.Tests
{
    public class UiRewardFlyerTests
    {
        //============================================================
        // Fields
        //============================================================
        private GameObject _goRoot;
        private GameObject _goCameras;
        private UiRewardFlyer _flyer;
        private UiRewardMotion _motion;
        private Transform _trSource;
        private Transform _trTarget;
        private RectTransform _rtLayer;
        private int _completedCnt;
        private int _cancelledCnt;
        private int _lastArrivedCnt;
        private bool _shouldRestart;

        //============================================================
        // Init/Register
        //============================================================
        [SetUp]
        public void SetUp()
        {
            _goRoot = new GameObject("RewardTests", typeof(RectTransform), typeof(Canvas));
            _goCameras = new GameObject("RewardTestCameras");
            _goRoot.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            _rtLayer = CreateRect("Layer", _goRoot.transform);
            _flyer = _rtLayer.gameObject.AddComponent<UiRewardFlyer>();
            RectTransform rtIcon = CreateRect("IconPrefab", _goRoot.transform);
            rtIcon.gameObject.SetActive(false);
            _trSource = CreateRect("Source", _goRoot.transform);
            _trTarget = CreateRect("Target", _goRoot.transform);
            _trTarget.localPosition = new Vector3(200.0f, 100.0f, 0.0f);
            _motion = ScriptableObject.CreateInstance<UiRewardMotion>();
            JsonUtility.FromJsonOverwrite("{\"_spreadRadius\":0,\"_spreadDurationSec\":0,\"_waitDurationSec\":0,\"_flyDurationSec\":0,\"_staggerSec\":0}", _motion);
            SetField(_flyer, "_rtIconPrefab", rtIcon);
            SetField(_flyer, "_motion", _motion);
            SetField(_flyer, "_maxIconCnt", 4);
            _completedCnt = 0;
            _cancelledCnt = 0;
            _lastArrivedCnt = 0;
            _shouldRestart = false;
            _flyer.OnArrived += HandleArrived;
            _flyer.OnCompleted += HandleCompleted;
            _flyer.OnCancelled += HandleCancelled;
        }

        [TearDown]
        public void TearDown()
        {
            _flyer.OnArrived -= HandleArrived;
            _flyer.OnCompleted -= HandleCompleted;
            _flyer.OnCancelled -= HandleCancelled;
            Time.timeScale = 1.0f;
            Object.DestroyImmediate(_goRoot);
            Object.DestroyImmediate(_goCameras);
            Object.DestroyImmediate(_motion);
        }

        //============================================================
        // Logic
        //============================================================
        [UnityTest]
        public IEnumerator ZeroDurationCompletesOnceAndReusesIcons()
        {
            _flyer.Prewarm();
            Assert.That(_rtLayer.childCount, Is.EqualTo(4));
            Assert.That(_flyer.TryPlay(_trSource, null, _trTarget, null, 3), Is.True);
            yield return null;
            yield return null;
            Assert.That(_flyer.IsPlaying, Is.False);
            Assert.That(_lastArrivedCnt, Is.EqualTo(3));
            Assert.That(_completedCnt, Is.EqualTo(1));
            Assert.That(_cancelledCnt, Is.Zero);
            AssertAllIconsHidden();
            Assert.That(_flyer.TryPlay(_trSource, null, _trTarget, null, 2), Is.True);
            yield return null;
            yield return null;
            Assert.That(_completedCnt, Is.EqualTo(2));
            Assert.That(_rtLayer.childCount, Is.EqualTo(4));
        }

        [Test]
        public void InvalidOrOverlappingRequestPreservesPlayback()
        {
            Assert.That(_flyer.TryPlay(_trSource, null, _trTarget, null, 0), Is.False);
            Assert.That(_flyer.TryPlay(_trSource, null, _trTarget, null, 5), Is.False);
            Assert.That(_flyer.TryPlay(null, null, _trTarget, null, 1), Is.False);
            Assert.That(_rtLayer.childCount, Is.Zero);
            Assert.That(_flyer.TryPlay(_trSource, null, _trTarget, null, 2), Is.True);
            Assert.That(_flyer.TryPlay(_trSource, null, _trTarget, null, 1), Is.False);
            Assert.That(_flyer.RequestedCnt, Is.EqualTo(2));
            Assert.That(_flyer.IsPlaying, Is.True);
        }

        [UnityTest]
        public IEnumerator CancelDoesNotReportCompletionAndCanRestart()
        {
            Assert.That(_flyer.TryPlay(_trSource, null, _trTarget, null, 2), Is.True);
            _flyer.Cancel();
            _flyer.Cancel();
            yield return null;
            Assert.That(_cancelledCnt, Is.EqualTo(1));
            Assert.That(_completedCnt, Is.Zero);
            Assert.That(_lastArrivedCnt, Is.Zero);
            AssertAllIconsHidden();
            Assert.That(_flyer.TryPlay(_trSource, null, _trTarget, null, 1), Is.True);
            yield return null;
            yield return null;
            Assert.That(_completedCnt, Is.EqualTo(1));
        }

        [Test]
        public void DisablingHostCancelsAndRejectsRequests()
        {
            Assert.That(_flyer.TryPlay(_trSource, null, _trTarget, null, 2), Is.True);
            _flyer.enabled = false;
            Assert.That(_flyer.IsPlaying, Is.False);
            Assert.That(_cancelledCnt, Is.EqualTo(1));
            AssertAllIconsHidden();
            Assert.That(_flyer.TryPlay(_trSource, null, _trTarget, null, 1), Is.False);
        }

        [UnityTest]
        public IEnumerator DestroyedTargetCancelsInsteadOfArriving()
        {
            Assert.That(_flyer.TryPlay(_trSource, null, _trTarget, null, 2), Is.True);
            Object.DestroyImmediate(_trTarget.gameObject);
            yield return null;
            yield return null;
            Assert.That(_cancelledCnt, Is.EqualTo(1));
            Assert.That(_completedCnt, Is.Zero);
            AssertAllIconsHidden();
        }

        [UnityTest]
        public IEnumerator UnscaledFlightCompletesWhileGameIsPaused()
        {
            SetField(_motion, "_flyDurationSec", 0.03f);
            Time.timeScale = 0.0f;
            Assert.That(_flyer.TryPlay(_trSource, null, _trTarget, null, 1), Is.True);
            yield return new WaitForSecondsRealtime(0.15f);
            Assert.That(_completedCnt, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ScaledFlightWaitsWhileGameIsPaused()
        {
            SetField(_motion, "_flyDurationSec", 0.03f);
            SetField(_motion, "_isUnscaledTime", false);
            Time.timeScale = 0.0f;
            Assert.That(_flyer.TryPlay(_trSource, null, _trTarget, null, 1), Is.True);
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.That(_flyer.ArrivedCnt, Is.Zero);
            Assert.That(_flyer.IsPlaying, Is.True);
            Time.timeScale = 1.0f;
            yield return new WaitForSecondsRealtime(0.15f);
            Assert.That(_completedCnt, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator MovingTargetIsProjectedAgainAtArrival()
        {
            Assert.That(_flyer.TryPlay(_trSource, null, _trTarget, null, 1), Is.True);
            _trTarget.localPosition = new Vector3(-150.0f, 80.0f, 0.0f);
            Canvas.ForceUpdateCanvases();
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, _trTarget.position);
            Assert.That(RectTransformUtility.ScreenPointToLocalPointInRectangle(_rtLayer, screenPos, null, out Vector2 expected), Is.True);
            yield return null;
            yield return null;
            Assert.That(Vector2.Distance(_rtLayer.GetChild(0).localPosition, expected), Is.LessThan(0.01f));
        }

        [Test]
        public void WorldSourceUsesExplicitCameraAndRejectsBehindCamera()
        {
            GameObject goCamera = new GameObject("Camera", typeof(Camera));
            goCamera.transform.SetParent(_goRoot.transform);
            Camera camSource = goCamera.GetComponent<Camera>();
            _trSource.position = camSource.transform.position + camSource.transform.forward * 10.0f;
            Assert.That(_flyer.TryPlay(_trSource, camSource, _trTarget, null, 1), Is.True);
            Vector2 screenPos = camSource.WorldToScreenPoint(_trSource.position);
            Assert.That(RectTransformUtility.ScreenPointToLocalPointInRectangle(_rtLayer, screenPos, null, out Vector2 expected), Is.True);
            Assert.That(Vector2.Distance(_rtLayer.GetChild(0).localPosition, expected), Is.LessThan(0.01f));
            _flyer.Cancel();
            _trSource.position = camSource.transform.position - camSource.transform.forward;
            Assert.That(_flyer.TryPlay(_trSource, camSource, _trTarget, null, 1), Is.False);
        }

        [UnityTest]
        public IEnumerator ArrivalHandlerCanRestartWithoutStaleCompletion()
        {
            _shouldRestart = true;
            Assert.That(_flyer.TryPlay(_trSource, null, _trTarget, null, 2), Is.True);
            yield return null;
            yield return null;
            yield return null;
            Assert.That(_cancelledCnt, Is.EqualTo(1));
            Assert.That(_completedCnt, Is.EqualTo(1));
            Assert.That(_flyer.RequestedCnt, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator SeparateCamerasProjectSourceAndTargetIntoLayer()
        {
            Camera camLayer = CreateCamera("Layer Camera", new Vector3(0.0f, 0.0f, -10.0f));
            Camera camSource = CreateCamera("Source Camera", new Vector3(3.0f, 0.0f, -10.0f));
            Camera camTarget = CreateCamera("Target Camera", new Vector3(-3.0f, 0.0f, -10.0f));
            Canvas canvas = _goRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camLayer;
            canvas.planeDistance = 1.0f;
            SetField(_flyer, "_camLayer", camLayer);
            yield return null;
            yield return null;
            Canvas.ForceUpdateCanvases();
            _trSource.position = new Vector3(0.0f, 1.0f, 0.0f);
            _trTarget.position = new Vector3(1.0f, -1.0f, 0.0f);
            Vector2 sourceScreenPos = camSource.WorldToScreenPoint(_trSource.position);
            Vector2 targetScreenPos = camTarget.WorldToScreenPoint(_trTarget.position);
            Assert.That(RectTransformUtility.ScreenPointToLocalPointInRectangle(_rtLayer, sourceScreenPos, camLayer, out Vector2 expectedStart), Is.True);
            Assert.That(RectTransformUtility.ScreenPointToLocalPointInRectangle(_rtLayer, targetScreenPos, camLayer, out Vector2 expectedEnd), Is.True);
            Assert.That(_flyer.TryPlay(_trSource, camSource, _trTarget, camTarget, 1), Is.True);
            Assert.That(Vector2.Distance(_rtLayer.GetChild(0).localPosition, expectedStart), Is.LessThan(0.01f));
            yield return null;
            yield return null;
            Assert.That(Vector2.Distance(_rtLayer.GetChild(0).localPosition, expectedEnd), Is.LessThan(0.01f));
        }

        [UnityTest]
        public IEnumerator LostTargetCameraCancelsInsteadOfUsingOverlayCoordinates()
        {
            Camera camTarget = CreateCamera("Target Camera", _trTarget.position - Vector3.forward * 10.0f);
            Assert.That(_flyer.TryPlay(_trSource, null, _trTarget, camTarget, 1), Is.True);
            Object.DestroyImmediate(camTarget.gameObject);
            yield return null;
            yield return null;
            Assert.That(_cancelledCnt, Is.EqualTo(1));
            Assert.That(_completedCnt, Is.Zero);
        }

        [UnityTest]
        public IEnumerator StaggeredIconsStayHiddenUntilTheirStart()
        {
            SetField(_motion, "_staggerSec", 10.0f);
            Assert.That(_flyer.TryPlay(_trSource, null, _trTarget, null, 3), Is.True);
            yield return null;
            yield return null;
            Assert.That(_flyer.ArrivedCnt, Is.EqualTo(1));
            Assert.That(_flyer.IsPlaying, Is.True);
            Assert.That(_rtLayer.GetChild(1).gameObject.activeSelf, Is.False);
            Assert.That(_rtLayer.GetChild(2).gameObject.activeSelf, Is.False);
            _flyer.Cancel();
            AssertAllIconsHidden();
        }

        [UnityTest]
        public IEnumerator DestroyingComponentAlsoDestroysOwnedIcons()
        {
            _flyer.Prewarm();
            Object.DestroyImmediate(_flyer);
            yield return null;
            yield return null;
            Assert.That(_rtLayer.childCount, Is.Zero);
        }

        //============================================================
        // Callbacks
        //============================================================
        private void HandleArrived(int arrivedCnt)
        {
            _lastArrivedCnt = arrivedCnt;
            if (!_shouldRestart)
                return;

            _shouldRestart = false;
            _flyer.Cancel();
            Assert.That(_flyer.TryPlay(_trSource, null, _trTarget, null, 1), Is.True);
        }

        private void HandleCompleted()
        {
            _completedCnt++;
        }

        private void HandleCancelled()
        {
            _cancelledCnt++;
        }

        //============================================================
        // Utilities
        //============================================================
        private void AssertAllIconsHidden()
        {
            for (int idx = 0; idx < _rtLayer.childCount; idx++)
            {
                Assert.That(_rtLayer.GetChild(idx).gameObject.activeSelf, Is.False);
            }
        }

        private static RectTransform CreateRect(string name, Transform trParent)
        {
            RectTransform rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(trParent, false);
            return rt;
        }

        private Camera CreateCamera(string name, Vector3 pos)
        {
            Camera cam = new GameObject(name, typeof(Camera)).GetComponent<Camera>();
            cam.transform.SetParent(_goCameras.transform);
            cam.transform.position = pos;
            cam.orthographic = true;
            cam.orthographicSize = 5.0f;
            return cam;
        }

        private static void SetField<T>(T target, string name, object value)
        {
            typeof(T).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }
    }
}
