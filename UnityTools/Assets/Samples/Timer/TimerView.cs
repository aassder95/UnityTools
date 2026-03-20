using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityTools.Samples.Util;
using UnityTools.Util.UIFramework;
using UnityTools.Util.Utilities;

namespace UnityTools.Samples.Timer
{
    public class TimerView : BaseView<TimerModel>
    {
        //============================================================
        //Constants
        //============================================================
        private const string UTC_TIME_FORMAT = "yyyy-MM-dd HH:mm:ss";

        //============================================================
        //Inspector Fields
        //============================================================
        [SerializeField] private TextMeshProUGUI _txtState;
        [SerializeField] private TextMeshProUGUI _txtSubState;
        [SerializeField] private TextMeshProUGUI _txtCur;
        [SerializeField] private TextMeshProUGUI _txtLoop;
        [SerializeField] private TextMeshProUGUI _txtOpenUpdated;
        [SerializeField] private TextMeshProUGUI _txtOpenEnd;
        [SerializeField] private TextMeshProUGUI _txtClosedEnd;

        //============================================================
        //Fields
        //============================================================
        private bool _isTestLayoutBuilt;

        //============================================================
        //Events
        //============================================================
        public event UnityAction OnForceOpen { add => _onForceOpen += value; remove => _onForceOpen -= value; }
        public event UnityAction OnForceClosed { add => _onForceClosed += value; remove => _onForceClosed -= value; }
        private event UnityAction _onForceOpen;
        private event UnityAction _onForceClosed;

        //============================================================
        //Unity Methods
        //============================================================
        private void Update()
        {
            if(_txtCur == null)
                return;

            DateTime currentUtcTime = DateTimeUtils.RemoveMilliseconds(DateTime.UtcNow);
            _txtCur.SetText($"cur: {currentUtcTime.ToString(UTC_TIME_FORMAT)}");
        }

        //============================================================
        //Init/Register
        //============================================================
        protected override void OnInit()
        {
            BuildTestLayout();
        }

        //============================================================
        //Logic
        //============================================================
        protected override void OnRefresh(TimerModel model)
        {
            SetState(model.State, model.SubState);
            SetLoop(model.LoopMinutes);
            SetTimer(model.OpenUpdated, model.OpenEnd, model.ClosedEnd);
        }

        //============================================================
        //Callbacks
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
        //Utilities
        //============================================================
        private void BuildTestLayout()
        {
            if(_isTestLayoutBuilt)
                return;

            SampleTestUiBuilder.Layout layout = SampleTestUiBuilder.Build(transform, "Timer Test Sample", "UTC state transition / force controls");
            SampleTestUiBuilder.DisableObjectsByName(transform, "BtnOpen", "BtnClosed", "ImgStateUpdated", "ImgState", "ImgTime");
            SampleTestUiBuilder.CreateActionButton(layout.RtControls, "BtnTestOpen", "Force Open", OnForceOpenInspector);
            SampleTestUiBuilder.CreateActionButton(layout.RtControls, "BtnTestClosed", "Force Closed", OnForceClosedInspector);

            BuildStateCard(layout.RtContentViewport);
            BuildInfoCard(layout.RtContentViewport);
            SampleTestUiBuilder.DisableLegacyDirectChildren(transform, _txtState?.transform, _txtSubState?.transform, _txtCur?.transform, _txtLoop?.transform, _txtOpenUpdated?.transform, _txtOpenEnd?.transform, _txtClosedEnd?.transform);
            _isTestLayoutBuilt = true;
        }

        private void BuildStateCard(RectTransform rtContentViewport)
        {
            RectTransform rtStateCard = CreatePanel("GoTimerStateCard", rtContentViewport, new Color(0.12f, 0.23f, 0.41f, 0.96f));
            Stretch(rtStateCard, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -208f), Vector2.zero);

            RectTransform rtStateRoot = CreateRect("GoTimerStateRoot", rtStateCard);
            Stretch(rtStateRoot, Vector2.zero, Vector2.one, new Vector2(14f, 12f), new Vector2(-14f, -12f));
            VerticalLayoutGroup stateLayout = rtStateRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            stateLayout.childAlignment = TextAnchor.MiddleCenter;
            stateLayout.childControlWidth = true;
            stateLayout.childControlHeight = true;
            stateLayout.childForceExpandWidth = true;
            stateLayout.childForceExpandHeight = false;
            stateLayout.spacing = 10f;

            ReparentText(_txtState, rtStateRoot, 56, TextAlignmentOptions.Center, Color.white, 84f);
            ReparentText(_txtSubState, rtStateRoot, 34, TextAlignmentOptions.Center, new Color(0.76f, 0.85f, 0.98f, 1f), 56f);
        }

        private void BuildInfoCard(RectTransform rtContentViewport)
        {
            RectTransform rtInfoCard = CreatePanel("GoTimerInfoCard", rtContentViewport, new Color(0.09f, 0.18f, 0.34f, 0.88f));
            Stretch(rtInfoCard, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0f, -220f));

            RectTransform rtInfoRoot = CreateRect("GoTimerInfoRoot", rtInfoCard);
            Stretch(rtInfoRoot, Vector2.zero, Vector2.one, new Vector2(20f, 18f), new Vector2(-20f, -18f));
            VerticalLayoutGroup infoLayout = rtInfoRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            infoLayout.childAlignment = TextAnchor.UpperCenter;
            infoLayout.childControlWidth = true;
            infoLayout.childControlHeight = true;
            infoLayout.childForceExpandWidth = true;
            infoLayout.childForceExpandHeight = false;
            infoLayout.spacing = 12f;

            ReparentText(_txtCur, rtInfoRoot, 30, TextAlignmentOptions.MidlineLeft, Color.white, 52f);
            ReparentText(_txtLoop, rtInfoRoot, 34, TextAlignmentOptions.MidlineRight, new Color(0.86f, 0.9f, 0.98f, 1f), 56f);
            ReparentText(_txtOpenUpdated, rtInfoRoot, 28, TextAlignmentOptions.MidlineLeft, new Color(0.85f, 0.89f, 0.97f, 1f), 48f);
            ReparentText(_txtOpenEnd, rtInfoRoot, 28, TextAlignmentOptions.MidlineLeft, new Color(0.58f, 0.78f, 1f, 1f), 48f);
            ReparentText(_txtClosedEnd, rtInfoRoot, 28, TextAlignmentOptions.MidlineLeft, new Color(1f, 0.66f, 0.66f, 1f), 48f);
        }

        private static void ReparentText(TextMeshProUGUI txtTarget, Transform trParent, int fontSize, TextAlignmentOptions alignment, Color textColor, float preferredHeight)
        {
            if(txtTarget == null || trParent == null)
                return;

            RectTransform rtText = txtTarget.rectTransform;
            rtText.SetParent(trParent, false);
            Stretch(rtText, Vector2.zero, Vector2.one, new Vector2(4f, 0f), new Vector2(-4f, 0f));

            txtTarget.enableWordWrapping = false;
            txtTarget.overflowMode = TextOverflowModes.Truncate;
            txtTarget.fontSize = fontSize;
            txtTarget.alignment = alignment;
            txtTarget.color = textColor;

            LayoutElement layoutElement = txtTarget.GetComponent<LayoutElement>();
            if(layoutElement == null)
                layoutElement = txtTarget.gameObject.AddComponent<LayoutElement>();

            layoutElement.minHeight = preferredHeight;
            layoutElement.preferredHeight = preferredHeight;
            layoutElement.flexibleHeight = 0f;
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
            return goObject.GetComponent<RectTransform>();
        }

        private static void Stretch(RectTransform rtTarget, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            if(rtTarget == null)
                return;

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

        private void SetTimer(DateTime openUpdated, DateTime openEnd, DateTime closedEnd)
        {
            SetText(_txtOpenUpdated, $"updated: {openUpdated.ToString(UTC_TIME_FORMAT)}");
            SetText(_txtOpenEnd, $"open: {openEnd.ToString(UTC_TIME_FORMAT)}");
            SetText(_txtClosedEnd, $"closed: {closedEnd.ToString(UTC_TIME_FORMAT)}");
        }

        private static void SetText(TextMeshProUGUI txtTarget, string textValue)
        {
            if(txtTarget == null)
                return;

            txtTarget.SetText(string.IsNullOrWhiteSpace(textValue) ? "-" : textValue);
        }
    }
}
