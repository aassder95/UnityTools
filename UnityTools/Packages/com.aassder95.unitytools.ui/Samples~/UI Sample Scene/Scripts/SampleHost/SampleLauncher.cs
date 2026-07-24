using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityTools.Util.Core;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Manager
{
    public class SampleLauncher
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly Canvas _hostCanvas;
        private readonly EventSystem _eventSystem;
        private readonly Action<int> _onSampleSelected;
        private readonly Action _onBack;
        private readonly List<SampleButton> _sampleButtons = new();
        private readonly Dictionary<GameObject, int> _moduleIdxByObject = new();

        //============================================================
        // Fields
        //============================================================
        private GameObject _goRuntimeCanvasRoot;
        private GameObject _goSampleListPanel;
        private GameObject _goBackLayer;
        private Button _btnBack;

        //============================================================
        // Constructors
        //============================================================
        public SampleLauncher(Canvas hostCanvas, EventSystem eventSystem, Action<int> onSampleSelected, Action onBack)
        {
            _hostCanvas = hostCanvas;
            _eventSystem = eventSystem;
            _onSampleSelected = onSampleSelected;
            _onBack = onBack;
        }

        //============================================================
        // Init/Register
        //============================================================
        public void Init(IReadOnlyList<ISampleModule> modules)
        {
            Release();
            Canvas runtimeCanvas = CreateRuntimeCanvas();
            GameObject goRuntimeRoot = CreateUiObject("SampleLobbyRuntimeRoot", runtimeCanvas.transform);
            StretchToParent(goRuntimeRoot.transform as RectTransform);

            _goSampleListPanel = CreatePanel(goRuntimeRoot.transform, "GoSampleListPanel", new Color(0.0f, 0.0f, 0.0f, 0.65f));
            StretchToParent(_goSampleListPanel.transform as RectTransform);

            BuildHeader();
            BuildModuleList(modules);
            BuildBackButton(goRuntimeRoot.transform);
        }

        public void Release()
        {
            for(int i = 0; i < _sampleButtons.Count; i++)
            {
                _sampleButtons[i].Button.onClick.RemoveListener(OnSampleButtonClicked);
            }

            if(_btnBack != null)
                _btnBack.onClick.RemoveListener(OnBackButtonClicked);

            _sampleButtons.Clear();
            _moduleIdxByObject.Clear();

            if(_goRuntimeCanvasRoot != null)
                UnityEngine.Object.Destroy(_goRuntimeCanvasRoot);

            _goRuntimeCanvasRoot = null;
            _goSampleListPanel = null;
            _goBackLayer = null;
            _btnBack = null;

        }

        //============================================================
        // Logic
        //============================================================
        public void ShowList()
        {
            _goSampleListPanel.SetActive(true);
            _goBackLayer.SetActive(false);
        }

        public void ShowModule()
        {
            _goSampleListPanel.SetActive(false);
            _goBackLayer.SetActive(true);
        }

        private Canvas CreateRuntimeCanvas()
        {
            GameObject goCanvas = CreateUiObject("SampleLobbyCanvas", _hostCanvas.transform);
            Canvas runtimeCanvas = goCanvas.AddComponent<Canvas>();
            CanvasScaler canvasScaler = goCanvas.AddComponent<CanvasScaler>();
            goCanvas.AddComponent<GraphicRaycaster>();

            StretchToParent(goCanvas.transform as RectTransform);
            runtimeCanvas.renderMode = _hostCanvas.renderMode;
            runtimeCanvas.worldCamera = _hostCanvas.worldCamera;
            runtimeCanvas.overrideSorting = true;
            runtimeCanvas.sortingOrder = 32000;

            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(720.0f, 1280.0f);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 1.0f;

            _goRuntimeCanvasRoot = goCanvas;
            return runtimeCanvas;
        }

        private void BuildHeader()
        {
            GameObject goHeader = CreatePanel(_goSampleListPanel.transform, "GoHeader", new Color(0.06f, 0.11f, 0.2f, 0.96f));
            RectTransform rtHeader = goHeader.transform as RectTransform;
            rtHeader.anchorMin = new Vector2(0.0f, 1.0f);
            rtHeader.anchorMax = new Vector2(1.0f, 1.0f);
            rtHeader.pivot = new Vector2(0.5f, 1.0f);
            rtHeader.anchoredPosition = Vector2.zero;
            rtHeader.sizeDelta = new Vector2(0.0f, 190.0f);

            Text txtTitle = CreateText(goHeader.transform, "TxtLobbyTitle", "Sample Lobby", 52, FontStyle.Bold, new Color(0.95f, 0.97f, 1.0f, 1.0f));
            RectTransform rtTitle = txtTitle.transform as RectTransform;
            rtTitle.anchorMin = new Vector2(0.5f, 1.0f);
            rtTitle.anchorMax = new Vector2(0.5f, 1.0f);
            rtTitle.pivot = new Vector2(0.5f, 1.0f);
            rtTitle.anchoredPosition = new Vector2(0.0f, -30.0f);
            rtTitle.sizeDelta = new Vector2(620.0f, 90.0f);

            Text txtSubTitle = CreateText(goHeader.transform, "TxtLobbySubTitle", "Select a sample and test quickly", 28, FontStyle.Normal, new Color(0.74f, 0.82f, 0.95f, 1.0f));
            RectTransform rtSubTitle = txtSubTitle.transform as RectTransform;
            rtSubTitle.anchorMin = new Vector2(0.5f, 1.0f);
            rtSubTitle.anchorMax = new Vector2(0.5f, 1.0f);
            rtSubTitle.pivot = new Vector2(0.5f, 1.0f);
            rtSubTitle.anchoredPosition = new Vector2(0.0f, -108.0f);
            rtSubTitle.sizeDelta = new Vector2(640.0f, 56.0f);
        }

        private void BuildModuleList(IReadOnlyList<ISampleModule> modules)
        {
            GameObject goListCard = CreatePanel(_goSampleListPanel.transform, "GoListCard", new Color(0.11f, 0.17f, 0.29f, 0.97f));
            RectTransform rtListCard = goListCard.transform as RectTransform;
            rtListCard.anchorMin = new Vector2(0.5f, 1.0f);
            rtListCard.anchorMax = new Vector2(0.5f, 1.0f);
            rtListCard.pivot = new Vector2(0.5f, 1.0f);
            rtListCard.anchoredPosition = new Vector2(0.0f, -230.0f);
            rtListCard.sizeDelta = new Vector2(660.0f, 880.0f);

            Outline listOutline = goListCard.AddComponent<Outline>();
            listOutline.effectColor = new Color(0.0f, 0.0f, 0.0f, 0.34f);
            listOutline.effectDistance = new Vector2(0.0f, -4.0f);

            GameObject goListRoot = CreateUiObject("TrSampleTitleRoot", goListCard.transform);
            RectTransform rtListRoot = goListRoot.transform as RectTransform;
            rtListRoot.anchorMin = Vector2.zero;
            rtListRoot.anchorMax = Vector2.one;
            rtListRoot.offsetMin = new Vector2(36.0f, 40.0f);
            rtListRoot.offsetMax = new Vector2(-36.0f, -36.0f);

            VerticalLayoutGroup layout = goListRoot.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(0, 0, 12, 12);
            layout.spacing = 16.0f;

            if(modules.Count == 0)
            {
                SampleButton emptyButton = CreateButton(goListRoot.transform, "No Samples", false);
                _sampleButtons.Add(emptyButton);
                return;
            }

            for(int i = 0; i < modules.Count; i++)
            {
                SampleButton sampleButton = CreateButton(goListRoot.transform, modules[i].ModuleKey, true);
                sampleButton.Button.onClick.AddListener(OnSampleButtonClicked);
                _sampleButtons.Add(sampleButton);
                _moduleIdxByObject[sampleButton.Button.gameObject] = i;
            }
        }

        private void BuildBackButton(Transform parent)
        {
            _goBackLayer = CreateUiObject("GoBackLayer", parent);
            RectTransform rtBackLayer = _goBackLayer.transform as RectTransform;
            rtBackLayer.anchorMin = Vector2.zero;
            rtBackLayer.anchorMax = Vector2.right;
            rtBackLayer.pivot = new Vector2(0.5f, 0.0f);
            rtBackLayer.offsetMin = Vector2.zero;
            rtBackLayer.offsetMax = new Vector2(0.0f, 108.0f);

            Image backImage = _goBackLayer.AddComponent<Image>();
            backImage.color = new Color(0.04f, 0.1f, 0.2f, 0.94f);

            Canvas backCanvas = _goBackLayer.AddComponent<Canvas>();
            backCanvas.overrideSorting = true;
            backCanvas.sortingOrder = 32767;
            _goBackLayer.AddComponent<GraphicRaycaster>();

            SampleButton backButton = CreateButton(_goBackLayer.transform, "<  Back To Lobby", true);
            _btnBack = backButton.Button;
            RectTransform rtBack = _btnBack.transform as RectTransform;
            rtBack.anchorMin = new Vector2(0.5f, 0.5f);
            rtBack.anchorMax = new Vector2(0.5f, 0.5f);
            rtBack.pivot = new Vector2(0.5f, 0.5f);
            rtBack.anchoredPosition = Vector2.zero;
            rtBack.sizeDelta = new Vector2(640.0f, 72.0f);
            _btnBack.onClick.AddListener(OnBackButtonClicked);
            _goBackLayer.SetActive(false);
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OnSampleButtonClicked()
        {
            GameObject goSelected = _eventSystem.currentSelectedGameObject;
            if(goSelected == null || !_moduleIdxByObject.TryGetValue(goSelected, out int moduleIdx))
            {
                DebugLogger.LogError("Sample 선택 Button과 Module 인덱스 연결을 찾을 수 없습니다.");
                return;
            }

            _onSampleSelected.Invoke(moduleIdx);
        }

        private void OnBackButtonClicked()
        {
            _onBack.Invoke();
        }

        //============================================================
        // Utilities
        //============================================================
        private static GameObject CreateUiObject(string objectName, Transform parent)
        {
            GameObject goObject = new(objectName, typeof(RectTransform));
            goObject.transform.SetParent(parent, false);
            return goObject;
        }

        private static GameObject CreatePanel(Transform parent, string panelName, Color panelColor)
        {
            GameObject goPanel = CreateUiObject(panelName, parent);
            Image panelImage = goPanel.AddComponent<Image>();
            panelImage.color = panelColor;
            return goPanel;
        }

        private static SampleButton CreateButton(Transform parent, string labelValue, bool isInteractable)
        {
            GameObject goButton = CreateUiObject("BtnSample", parent);
            Image buttonImage = goButton.AddComponent<Image>();
            buttonImage.color = new Color(0.16f, 0.45f, 0.86f, 1.0f);
            buttonImage.type = Image.Type.Sliced;

            Outline outline = goButton.AddComponent<Outline>();
            outline.effectColor = new Color(0.0f, 0.0f, 0.0f, 0.22f);
            outline.effectDistance = new Vector2(0.0f, -3.0f);

            Button button = goButton.AddComponent<Button>();
            button.interactable = isInteractable;

            LayoutElement layout = goButton.AddComponent<LayoutElement>();
            layout.minHeight = 96.0f;
            layout.preferredHeight = 96.0f;
            layout.flexibleWidth = 1.0f;

            Text txtLabel = CreateText(goButton.transform, "TxtLabel", labelValue, 34, FontStyle.Bold, Color.white);
            StretchToParent(txtLabel.transform as RectTransform);
            return new SampleButton(button);
        }

        private static Text CreateText(Transform parent, string textName, string textValue, int fontSize, FontStyle fontStyle, Color textColor)
        {
            GameObject goText = CreateUiObject(textName, parent);
            Text text = goText.AddComponent<Text>();
            text.text = textValue;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = textColor;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static void StretchToParent(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        //============================================================
        // Nested Types
        //============================================================
        private class SampleButton
        {
            public Button Button { get; }
            public SampleButton(Button button)
            {
                Button = button;
            }
        }
    }
}
