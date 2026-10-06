using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace UnityTools.Ui.Tests
{
    public class UiButtonInputTests
    {
        //============================================================
        // Fields
        //============================================================
        private readonly List<double> _clickTimes = new List<double>();
        private GameObject _goButton;
        private GameObject _goEvents;
        private UiRepeatButton _btn;
        private UiButtonPressScale _scale;
        private EventSystem _events;
        private int _clickCnt;
        private float _prevTimeScale;

        //============================================================
        // Init/Register
        //============================================================
        [SetUp]
        public void SetUp()
        {
            _prevTimeScale = Time.timeScale;
            _goEvents = new GameObject("Events", typeof(EventSystem));
            _events = _goEvents.GetComponent<EventSystem>();
            _goButton = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(UiRepeatButton), typeof(UiButtonPressScale));
            _btn = _goButton.GetComponent<UiRepeatButton>();
            _scale = _goButton.GetComponent<UiButtonPressScale>();
            _btn.targetGraphic = _goButton.GetComponent<Image>();
            _btn.onClick.AddListener(CountClick);
            Assert.That(_btn.TryConfigure(0.05f, 0.08f, 0.02f, 0.5f), Is.True);
            Assert.That(_scale.TryConfigure(0.8f, 0.02f), Is.True);
            _clickCnt = 0;
            _clickTimes.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = _prevTimeScale;
            _btn.onClick.RemoveListener(CountClick);
            _btn.onClick.RemoveListener(RecordTime);
            Object.DestroyImmediate(_goButton);
            Object.DestroyImmediate(_goEvents);
        }

        //============================================================
        // Logic
        //============================================================
        [Test]
        public void ShortPressClicksOnceAndDuplicateClickIsIgnored()
        {
            PointerEventData pointer = Pointer(-1);
            Down(pointer);
            Up(pointer);
            ExecuteEvents.Execute(_goButton, pointer, ExecuteEvents.pointerClickHandler);
            ExecuteEvents.Execute(_goButton, pointer, ExecuteEvents.pointerClickHandler);
            Assert.That(_clickCnt, Is.EqualTo(1));
            Assert.That(_btn.RepeatCnt, Is.Zero);
            Assert.That(_btn.IsHeld, Is.False);
        }

        [UnityTest]
        public IEnumerator HoldAcceleratesWithoutReleaseClickAndWorksWhenPaused()
        {
            Time.timeScale = 0.0f;
            _btn.onClick.AddListener(RecordTime);
            PointerEventData pointer = Pointer(-1);
            double startSec = Time.unscaledTimeAsDouble;
            Down(pointer);
            double timeoutSec = startSec + 3.0;
            while (_clickCnt < 4 && Time.unscaledTimeAsDouble < timeoutSec)
            {
                yield return null;
            }

            Up(pointer);
            ExecuteEvents.Execute(_goButton, pointer, ExecuteEvents.pointerClickHandler);
            _btn.onClick.RemoveListener(RecordTime);
            Assert.That(_clickTimes.Count, Is.GreaterThanOrEqualTo(4));
            Assert.That(_clickTimes[0] - startSec, Is.GreaterThanOrEqualTo(0.045));
            Assert.That(_clickTimes[1] - _clickTimes[0], Is.GreaterThanOrEqualTo(0.075));
            Assert.That(_clickTimes[2] - _clickTimes[1], Is.LessThan(_clickTimes[1] - _clickTimes[0] + 0.03));
            Assert.That(_clickCnt, Is.EqualTo(_btn.RepeatCnt));
        }

        [UnityTest]
        public IEnumerator PointerExitCancelsClickAndRestoresScale()
        {
            Vector3 origin = new Vector3(2.0f, 1.5f, 1.0f);
            _goButton.transform.localScale = origin;
            PointerEventData pointer = Pointer(-1);
            Down(pointer);
            yield return new WaitForSecondsRealtime(0.025f);
            Assert.That(_goButton.transform.localScale.x, Is.LessThan(origin.x));
            ExecuteEvents.Execute(_goButton, pointer, ExecuteEvents.pointerExitHandler);
            Up(pointer);
            ExecuteEvents.Execute(_goButton, pointer, ExecuteEvents.pointerClickHandler);
            int cnt = _clickCnt;
            yield return new WaitForSecondsRealtime(0.15f);
            Assert.That(_clickCnt, Is.EqualTo(cnt));
            Assert.That(_btn.IsHeld, Is.False);
            Assert.That(_goButton.transform.localScale, Is.EqualTo(origin));
        }

        [UnityTest]
        public IEnumerator OtherTouchCannotReleaseOrRestartOwnedPress()
        {
            PointerEventData main = Pointer(11);
            PointerEventData other = Pointer(22);
            Down(main);
            Down(other);
            Up(other);
            ExecuteEvents.Execute(_goButton, other, ExecuteEvents.pointerExitHandler);
            ExecuteEvents.Execute(_goButton, other, ExecuteEvents.pointerClickHandler);
            Assert.That(_btn.IsHeld, Is.True);
            yield return new WaitForSecondsRealtime(0.07f);
            Assert.That(_clickCnt, Is.GreaterThanOrEqualTo(1));
            Up(main);
            ExecuteEvents.Execute(_goButton, main, ExecuteEvents.pointerClickHandler);
            Assert.That(_clickCnt, Is.EqualTo(_btn.RepeatCnt));
        }

        [UnityTest]
        public IEnumerator DisabledAndCanvasGroupBlockedButtonsDoNotRepeat()
        {
            _btn.interactable = false;
            Down(Pointer(-1));
            yield return new WaitForSecondsRealtime(0.08f);
            Assert.That(_clickCnt, Is.Zero);
            _btn.interactable = true;
            CanvasGroup group = _goButton.AddComponent<CanvasGroup>();
            group.interactable = false;
            Down(Pointer(-1));
            yield return new WaitForSecondsRealtime(0.08f);
            Assert.That(_clickCnt, Is.Zero);
            Assert.That(_btn.IsHeld, Is.False);
        }

        [UnityTest]
        public IEnumerator InteractableChangeStopsExistingHold()
        {
            Down(Pointer(-1));
            yield return new WaitForSecondsRealtime(0.03f);
            _btn.interactable = false;
            yield return new WaitForSecondsRealtime(0.08f);
            Assert.That(_btn.IsHeld, Is.False);
            Assert.That(_clickCnt, Is.Zero);
            Assert.That(_goButton.transform.localScale, Is.EqualTo(Vector3.one));
        }

        [UnityTest]
        public IEnumerator DisableRestoresOriginalScaleAndCancelsPendingClick()
        {
            Vector3 origin = new Vector3(1.3f, 1.2f, 1.0f);
            _goButton.transform.localScale = origin;
            PointerEventData pointer = Pointer(-1);
            Down(pointer);
            yield return new WaitForSecondsRealtime(0.025f);
            _goButton.SetActive(false);
            Assert.That(_goButton.transform.localScale, Is.EqualTo(origin));
            _goButton.SetActive(true);
            Up(pointer);
            ExecuteEvents.Execute(_goButton, pointer, ExecuteEvents.pointerClickHandler);
            Assert.That(_clickCnt, Is.Zero);
        }

        [Test]
        public void FocusLossPauseAndCancelClearPendingPress()
        {
            PointerEventData pointer = Pointer(-1);
            Down(pointer);
            _goButton.SendMessage("OnApplicationFocus", false);
            Up(pointer);
            ExecuteEvents.Execute(_goButton, pointer, ExecuteEvents.pointerClickHandler);
            Assert.That(_clickCnt, Is.Zero);
            Down(pointer);
            _goButton.SendMessage("OnApplicationPause", true);
            Assert.That(_btn.IsHeld, Is.False);
            Down(pointer);
            ExecuteEvents.Execute(_goButton, new BaseEventData(_events), ExecuteEvents.cancelHandler);
            Assert.That(_btn.IsHeld, Is.False);
        }

        [Test]
        public void SubmitClicksOnceWithoutStartingHold()
        {
            ExecuteEvents.Execute(_goButton, new BaseEventData(_events), ExecuteEvents.submitHandler);
            Assert.That(_clickCnt, Is.EqualTo(1));
            Assert.That(_btn.IsHeld, Is.False);
        }

        [UnityTest]
        public IEnumerator RapidPressesDoNotAccumulateScaleDrift()
        {
            Vector3 origin = new Vector3(2.0f, 3.0f, 1.0f);
            _goButton.transform.localScale = origin;
            for (int idx = 0; idx < 4; idx++)
            {
                PointerEventData pointer = Pointer(-1);
                Down(pointer);
                yield return null;
                Up(pointer);
                ExecuteEvents.Execute(_goButton, pointer, ExecuteEvents.pointerClickHandler);
            }

            yield return new WaitForSecondsRealtime(0.05f);
            Assert.That(_goButton.transform.localScale, Is.EqualTo(origin));
            _goButton.transform.localScale = Vector3.one;
            Down(Pointer(-1));
            _scale.ResetImmediate();
            Assert.That(_goButton.transform.localScale, Is.EqualTo(Vector3.one));
        }

        [Test]
        public void InvalidConfigurationDoesNotInterruptActiveGesture()
        {
            Down(Pointer(-1));
            Assert.That(_btn.TryConfigure(float.NaN, 0.1f, 0.01f, 0.8f), Is.False);
            Assert.That(_btn.TryConfigure(0.0f, 0.01f, 0.1f, 0.8f), Is.False);
            Assert.That(_scale.TryConfigure(float.PositiveInfinity, 0.01f), Is.False);
            Assert.That(_btn.IsHeld, Is.True);
        }

        [Test]
        public void RightClickIsIgnored()
        {
            PointerEventData pointer = Pointer(-1);
            pointer.button = PointerEventData.InputButton.Right;
            Down(pointer);
            Up(pointer);
            ExecuteEvents.Execute(_goButton, pointer, ExecuteEvents.pointerClickHandler);
            Assert.That(_clickCnt, Is.Zero);
            Assert.That(_btn.IsHeld, Is.False);
        }

        //============================================================
        // Utilities
        //============================================================
        private PointerEventData Pointer(int id)
        {
            return new PointerEventData(_events) { pointerId = id, button = PointerEventData.InputButton.Left };
        }

        private void Down(PointerEventData pointer)
        {
            ExecuteEvents.Execute(_goButton, pointer, ExecuteEvents.pointerDownHandler);
        }

        private void Up(PointerEventData pointer)
        {
            ExecuteEvents.Execute(_goButton, pointer, ExecuteEvents.pointerUpHandler);
        }

        private void CountClick()
        {
            _clickCnt++;
        }

        private void RecordTime()
        {
            _clickTimes.Add(Time.unscaledTimeAsDouble);
        }
    }
}
