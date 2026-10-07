using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace UnityTools.Ui.Tests
{
    public class UiToastTests
    {
        //============================================================
        // Fields
        //============================================================
        private GameObject _goRoot;
        private UiToastQueue _toast;
        private CanvasGroup _cg;
        private Text _txt;
        private int _shownCnt;
        private int _dismissedCnt;

        //============================================================
        // Init/Register
        //============================================================
        [SetUp]
        public void SetUp()
        {
            _goRoot = new GameObject("ToastTests", typeof(RectTransform), typeof(CanvasGroup));
            _goRoot.SetActive(false);
            _cg = _goRoot.GetComponent<CanvasGroup>();
            _txt = new GameObject("Text", typeof(RectTransform)).AddComponent<Text>();
            _txt.transform.SetParent(_goRoot.transform, false);
            _toast = _goRoot.AddComponent<UiToastQueue>();
            typeof(UiToastQueue).GetField("_cgToast", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_toast, _cg);
            typeof(UiToastQueue).GetField("_txtToast", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_toast, _txt);
            typeof(UiToastQueue).GetField("_maxPendingCnt", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_toast, 1);
            _shownCnt = _dismissedCnt = 0;
            _toast.OnShown += OnShown;
            _toast.OnDismissed += OnDismissed;
            _goRoot.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1.0f;
            _toast.OnShown -= OnShown;
            _toast.OnDismissed -= OnDismissed;
            Object.DestroyImmediate(_goRoot);
        }

        //============================================================
        // Logic
        //============================================================
        [Test]
        public void QueuePreservesOrderAndSuppressesDuplicateKeys()
        {
            Assert.That(_toast.TryEnqueue(" A ", "First", 1.0f), Is.True);
            Assert.That(_toast.TryEnqueue("A", "Duplicate", 1.0f), Is.False);
            Assert.That(_toast.TryEnqueue("B", "Second", 1.0f), Is.True);
            Assert.That(_toast.TryEnqueue("B", "Duplicate", 1.0f), Is.False);
            Assert.That(_toast.TryEnqueue("C", "Overflow", 1.0f), Is.False);
            Assert.That(_toast.PendingCnt, Is.EqualTo(1));
            Assert.That(_txt.text, Is.EqualTo("First"));
            Assert.That(_toast.TryDismiss(), Is.True);
            Assert.That(_toast.CurrentKey, Is.EqualTo("B"));
            Assert.That(_txt.text, Is.EqualTo("Second"));
            Assert.That(_shownCnt, Is.EqualTo(2));
            Assert.That(_dismissedCnt, Is.EqualTo(1));
            Assert.That(_cg.blocksRaycasts, Is.False);
        }

        [UnityTest]
        public IEnumerator TimeoutUsesUnscaledTime()
        {
            Assert.That(_toast.TryEnqueue("A", "First", 0.05f), Is.True);
            Assert.That(_toast.TryEnqueue("B", "Second", 1.0f), Is.True);
            Time.timeScale = 0.0f;
            yield return new WaitForSecondsRealtime(0.15f);
            Assert.That(_toast.CurrentKey, Is.EqualTo("B"));
            Assert.That(_dismissedCnt, Is.EqualTo(1));
        }

        [Test]
        public void DisableClearsPendingAndAllowsKeyReuse()
        {
            Assert.That(_toast.TryEnqueue("A", "First", 1.0f), Is.True);
            Assert.That(_toast.TryEnqueue("B", "Second", 1.0f), Is.True);
            _toast.enabled = false;
            Assert.That(_toast.IsShowing, Is.False);
            Assert.That(_toast.PendingCnt, Is.Zero);
            Assert.That(_cg.alpha, Is.Zero);
            Assert.That(_dismissedCnt, Is.EqualTo(1));
            Assert.That(_toast.TryEnqueue("C", "Disabled", 1.0f), Is.False);
            _toast.enabled = true;
            Assert.That(_toast.TryEnqueue("A", "Again", 1.0f), Is.True);
            _toast.Clear();
            _toast.Clear();
            Assert.That(_dismissedCnt, Is.EqualTo(2));
            Assert.That(_toast.TryDismiss(), Is.False);
        }

        [Test]
        public void InvalidRequestsDoNotChangeQueue()
        {
            Assert.That(_toast.TryEnqueue(null, "Text", 1.0f), Is.False);
            Assert.That(_toast.TryEnqueue("A", " ", 1.0f), Is.False);
            Assert.That(_toast.TryEnqueue("A", "Text", float.NaN), Is.False);
            Assert.That(_toast.TryEnqueue("A", "Text", float.PositiveInfinity), Is.False);
            Assert.That(_toast.TryEnqueue("A", "Text", 0.0f), Is.False);
            Assert.That(_shownCnt, Is.Zero);
            Assert.That(_toast.IsShowing, Is.False);
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OnShown(string key)
        {
            ++_shownCnt;
        }

        private void OnDismissed(string key)
        {
            ++_dismissedCnt;
        }
    }
}
