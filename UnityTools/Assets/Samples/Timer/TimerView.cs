using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityTools.Samples.Util;
using UnityTools.Util.UiFramework;
using UnityTools.Util.Utilities;

namespace UnityTools.Samples.Timer
{
    public class TimerView : BaseView<TimerModel>
    {
        //============================================================
        // Constants
        //============================================================
        private const string UTC_TIME_FORMAT = "yyyy-MM-dd HH:mm:ss";

        //============================================================
        // Readonly
        //============================================================
        private readonly WaitForSecondsRealtime _clockWait = new(1.0f);

        //============================================================
        // Inspector Fields
        //============================================================
        [Header("State")]
        [SerializeField] private TextMeshProUGUI _txtState;
        [SerializeField] private TextMeshProUGUI _txtSubState;

        [Header("Time")]
        [SerializeField] private TextMeshProUGUI _txtCur;
        [SerializeField] private TextMeshProUGUI _txtLoop;
        [SerializeField] private TextMeshProUGUI _txtOpenUpdated;
        [SerializeField] private TextMeshProUGUI _txtOpenEnd;
        [SerializeField] private TextMeshProUGUI _txtClosedEnd;

        //============================================================
        // Fields
        //============================================================
        private Coroutine _coClock;
        private bool _isTestLayoutBuilt;

        //============================================================
        // Events
        //============================================================
        public event UnityAction OnForceOpen { add => _onForceOpen += value; remove => _onForceOpen -= value; }
        public event UnityAction OnForceClosed { add => _onForceClosed += value; remove => _onForceClosed -= value; }
        private event UnityAction _onForceOpen;
        private event UnityAction _onForceClosed;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnEnable()
        {
            if (IsInit)
                StartClock();
        }

        private void OnDisable()
        {
            StopClock();
        }

        //============================================================
        // Init/Register
        //============================================================
        protected override void OnInit()
        {
            BuildTestLayout();
            if (isActiveAndEnabled)
                StartClock();
        }

        protected override void OnRelease()
        {
            StopClock();
        }

        //============================================================
        // Logic
        //============================================================
        protected override void OnRefresh(TimerModel model)
        {
            SetState(model.State, model.SubState);
            SetLoop(model.LoopMin);
            SetTimer(model.OpenUpdatedTime, model.OpenEndTime, model.ClosedEndTime);
        }

        //============================================================
        // Coroutines
        //============================================================
        private IEnumerator CoClock()
        {
            while (true)
            {
                DateTime curUtcTime = DateTimeUtils.RemoveMs(DateTime.UtcNow);
                _txtCur.SetText("cur: " + curUtcTime.ToString(UTC_TIME_FORMAT));
                yield return _clockWait;
            }
        }

        //============================================================
        // Callbacks
        //============================================================
        public void OnForceOpenInspector()
        {
            _onForceOpen?.Invoke();
        }

        public void OnForceClosedInspector()
        {
            _onForceClosed?.Invoke();
        }

        //============================================================
        // Utilities
        //============================================================
        private void StartClock()
        {
            if (_coClock != null)
                return;

            _coClock = StartCoroutine(CoClock());
        }

        private void StopClock()
        {
            if (_coClock == null)
                return;

            StopCoroutine(_coClock);
            _coClock = null;
        }

        private void BuildTestLayout()
        {
            if (_isTestLayoutBuilt)
                return;

            SampleTestLayout layout = SampleTestUiBuilder.Build(transform, "Timer Test Sample", "UTC state transition / force controls", 2);
            SampleTestUiBuilder.CreateActionButton(layout.RtControls, "BtnTestOpen", "Force Open", OnForceOpenInspector);
            SampleTestUiBuilder.CreateActionButton(layout.RtControls, "BtnTestClosed", "Force Closed", OnForceClosedInspector);

            BuildStateCard(layout.RtContentViewport);
            BuildInfoCard(layout.RtContentViewport);
            _isTestLayoutBuilt = true;
        }

        private void BuildStateCard(RectTransform rtContentViewport)
        {
            RectTransform rtStateCard = CreatePanel("GoTimerStateCard", rtContentViewport, new Color(0.12f, 0.23f, 0.41f, 0.96f));
            Stretch(rtStateCard, new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f), new Vector2(0.0f, -208.0f), Vector2.zero);

            RectTransform rtStateRoot = CreateRect("GoTimerStateRoot", rtStateCard);
            Stretch(rtStateRoot, Vector2.zero, Vector2.one, new Vector2(14.0f, 12.0f), new Vector2(-14.0f, -12.0f));
            VerticalLayoutGroup stateLayout = rtStateRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            stateLayout.childAlignment = TextAnchor.MiddleCenter;
            stateLayout.childControlWidth = true;
            stateLayout.childControlHeight = true;
            stateLayout.childForceExpandWidth = true;
            stateLayout.childForceExpandHeight = false;
            stateLayout.spacing = 10.0f;

            ReparentText(_txtState, rtStateRoot, 56, TextAlignmentOptions.Center, Color.white, 84.0f);
            ReparentText(_txtSubState, rtStateRoot, 34, TextAlignmentOptions.Center, new Color(0.76f, 0.85f, 0.98f, 1.0f), 56.0f);
        }

        private void BuildInfoCard(RectTransform rtContentViewport)
        {
            RectTransform rtInfoCard = CreatePanel("GoTimerInfoCard", rtContentViewport, new Color(0.09f, 0.18f, 0.34f, 0.88f));
            Stretch(rtInfoCard, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0.0f, -220.0f));

            RectTransform rtInfoRoot = CreateRect("GoTimerInfoRoot", rtInfoCard);
            Stretch(rtInfoRoot, Vector2.zero, Vector2.one, new Vector2(20.0f, 18.0f), new Vector2(-20.0f, -18.0f));
            VerticalLayoutGroup infoLayout = rtInfoRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            infoLayout.childAlignment = TextAnchor.UpperCenter;
            infoLayout.childControlWidth = true;
            infoLayout.childControlHeight = true;
            infoLayout.childForceExpandWidth = true;
            infoLayout.childForceExpandHeight = false;
            infoLayout.spacing = 12.0f;

            ReparentText(_txtCur, rtInfoRoot, 30, TextAlignmentOptions.MidlineLeft, Color.white, 52.0f);
            ReparentText(_txtLoop, rtInfoRoot, 34, TextAlignmentOptions.MidlineRight, new Color(0.86f, 0.9f, 0.98f, 1.0f), 56.0f);
            ReparentText(_txtOpenUpdated, rtInfoRoot, 28, TextAlignmentOptions.MidlineLeft, new Color(0.85f, 0.89f, 0.97f, 1.0f), 48.0f);
            ReparentText(_txtOpenEnd, rtInfoRoot, 28, TextAlignmentOptions.MidlineLeft, new Color(0.58f, 0.78f, 1.0f, 1.0f), 48.0f);
            ReparentText(_txtClosedEnd, rtInfoRoot, 28, TextAlignmentOptions.MidlineLeft, new Color(1.0f, 0.66f, 0.66f, 1.0f), 48.0f);
        }

        private static void ReparentText(TextMeshProUGUI txtTarget, Transform trParent, int fontSize, TextAlignmentOptions alignment, Color textColor, float preferredHeight)
        {
            RectTransform rtText = txtTarget.rectTransform;
            rtText.SetParent(trParent, false);
            Stretch(rtText, Vector2.zero, Vector2.one, new Vector2(4.0f, 0.0f), new Vector2(-4.0f, 0.0f));

            txtTarget.enableWordWrapping = false;
            txtTarget.overflowMode = TextOverflowModes.Truncate;
            txtTarget.fontSize = fontSize;
            txtTarget.alignment = alignment;
            txtTarget.color = textColor;

            LayoutElement layoutElement = txtTarget.GetComponent<LayoutElement>();
            if (layoutElement == null)
                layoutElement = txtTarget.gameObject.AddComponent<LayoutElement>();

            layoutElement.minHeight = preferredHeight;
            layoutElement.preferredHeight = preferredHeight;
            layoutElement.flexibleHeight = 0.0f;
        }

        private static RectTransform CreatePanel(string objectName, Transform parent, Color panelColor)
        {
            RectTransform rtPanel = CreateRect(objectName, parent);
            Image panelImage = rtPanel.gameObject.AddComponent<Image>();
            panelImage.color = panelColor;
            return rtPanel;
        }

        private static RectTransform CreateRect(string objectName, Transform parent)
        {
            GameObject goObject = new(objectName, typeof(RectTransform));
            goObject.transform.SetParent(parent, false);
            return goObject.transform as RectTransform;
        }

        private static void Stretch(RectTransform rtTarget, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rtTarget.anchorMin = anchorMin;
            rtTarget.anchorMax = anchorMax;
            rtTarget.offsetMin = offsetMin;
            rtTarget.offsetMax = offsetMax;
        }

        private void SetState(string state, string subState)
        {
            SetText(_txtState, state);
            SetText(_txtSubState, subState);
        }

        private void SetLoop(int min)
        {
            SetText(_txtLoop, $"({min})");
        }

        private void SetTimer(DateTime openUpdatedTime, DateTime openEndTime, DateTime closedEndTime)
        {
            SetText(_txtOpenUpdated, "updated: " + openUpdatedTime.ToString(UTC_TIME_FORMAT));
            SetText(_txtOpenEnd, "open: " + openEndTime.ToString(UTC_TIME_FORMAT));
            SetText(_txtClosedEnd, "closed: " + closedEndTime.ToString(UTC_TIME_FORMAT));
        }

        private static void SetText(TextMeshProUGUI txtTarget, string textValue)
        {
            txtTarget.SetText(string.IsNullOrWhiteSpace(textValue) ? "-" : textValue);
        }
    }
}
