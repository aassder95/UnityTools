using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityTools.Util.Core;
using UnityTools.Util.Core.Singleton;

namespace UnityTools.Manager
{
    public class UIManager : MonoSingleton<UIManager>
    {
        //============================================================
        // Constants
        //============================================================
        private const string RUNTIME_ROOT_NAME = "SampleLobbyRuntimeRoot";
        private const string RUNTIME_CANVAS_NAME = "SampleLobbyCanvas";
        private const string HOST_CANVAS_NAME = "Canvas";
        private const string RUNTIME_EVENT_SYSTEM_NAME = "EventSystem";
        private const string EMPTY_MODULE_LABEL = "No Samples";
        private const int LOBBY_SORTING_ORDER = 32000;
        private const int BACK_BUTTON_SORTING_ORDER = 32767;
        private const float BACK_TAB_HEIGHT = 108.0f;

        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Entry")] [SerializeField] private string _entryModuleKey = SampleModuleKeys.ALL;
        [Header("Modules")] [SerializeField] private MonoBehaviour[] _sampleModuleBehaviours;
        [Header("Launcher")] [SerializeField] private GameObject _goSampleListPanel;
        [SerializeField] private Transform _trSampleTitleRoot;
        [SerializeField] private Button _btnSampleTitlePrefab;
        [SerializeField] private Button _btnBack;

        //============================================================
        // Fields
        //============================================================
        private readonly List<ISampleModule> _modules = new();
        private readonly List<ISampleModule> _activeModules = new();
        private readonly List<Button> _sampleTitleButtons = new();
        private readonly Dictionary<Button, int> _sampleButtonIndexByButton = new();

        private ISampleModule _selectedModule;
        private GameObject _goRuntimeCanvasRoot;
        private GameObject _goRuntimeLobbyRoot;
        private GameObject _goBackLayer;
        private bool _isRuntimeCanvasOwned;
        private bool _isLauncherReady;

        //============================================================
        // Unity Methods
        //============================================================
        private void Awake()
        {
            RegisterModules();
            InitModules();
            TryCreateRuntimeLauncher();
            EnsureLauncherHierarchyUnderCanvas();

            _isLauncherReady = CanUseLauncher();
            if (!_isLauncherReady)
            {
                ShowAllModules();
                return;
            }

            BuildLauncher();
            ShowSampleList();
        }

        private void OnDestroy()
        {
            ReleaseLauncher();
            ReleaseModules();
        }

        //============================================================
        // Init/Register
        //============================================================
        private void RegisterModules()
        {
            _modules.Clear();

            RegisterInspectorModules();
            RegisterSceneModules();
        }

        private void RegisterInspectorModules()
        {
            if (_sampleModuleBehaviours == null)
                return;

            for (int i = 0; i < _sampleModuleBehaviours.Length; i++)
            {
                MonoBehaviour behaviour = _sampleModuleBehaviours[i];
                if (behaviour == null)
                    continue;

                if (behaviour is ISampleModule sampleModule)
                {
                    if (!behaviour.gameObject.scene.IsValid())
                        continue;

                    AddModule(sampleModule);
                }
            }
        }

        private void RegisterSceneModules()
        {
            MonoBehaviour[] behaviours = FindObjectsOfType<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null || behaviour == this)
                    continue;

                if (behaviour is not ISampleModule sampleModule)
                    continue;

                AddModule(sampleModule);
            }
        }

        private void AddModule(ISampleModule sampleModule)
        {
            if (sampleModule == null || _modules.Contains(sampleModule))
                return;

            _modules.Add(sampleModule);
        }

        private void InitModules()
        {
            _activeModules.Clear();

            for (int i = 0; i < _modules.Count; i++)
            {
                ISampleModule module = _modules[i];
                if (!IsTargetModule(module.ModuleKey))
                    continue;

                module.Init();
                if (!module.IsInit)
                    continue;

                module.Hide();
                _activeModules.Add(module);
            }
        }

        private void BuildLauncher()
        {
            ClearSampleTitleButtons();

            _btnBack.onClick.RemoveListener(OnBackButtonClicked);
            _btnBack.onClick.AddListener(OnBackButtonClicked);

            _btnSampleTitlePrefab.gameObject.SetActive(false);

            for (int i = 0; i < _activeModules.Count; i++)
            {
                ISampleModule module = _activeModules[i];
                Button sampleTitleButton = Instantiate(_btnSampleTitlePrefab, _trSampleTitleRoot);
                if (sampleTitleButton == null)
                    continue;

                SetSampleTitle(sampleTitleButton, module.ModuleKey);
                sampleTitleButton.onClick.RemoveListener(OnSampleTitleButtonClicked);
                sampleTitleButton.onClick.AddListener(OnSampleTitleButtonClicked);
                sampleTitleButton.gameObject.SetActive(true);

                _sampleTitleButtons.Add(sampleTitleButton);
                _sampleButtonIndexByButton[sampleTitleButton] = i;
            }

            if (_activeModules.Count > 0)
                return;

            Button emptyButton = Instantiate(_btnSampleTitlePrefab, _trSampleTitleRoot);
            if (emptyButton == null)
                return;

            SetSampleTitle(emptyButton, EMPTY_MODULE_LABEL);
            emptyButton.interactable = false;
            emptyButton.gameObject.SetActive(true);
            _sampleTitleButtons.Add(emptyButton);
        }

        private void ReleaseLauncher()
        {
            if (_btnBack != null)
                _btnBack.onClick.RemoveListener(OnBackButtonClicked);

            ClearSampleTitleButtons();

            if (_goRuntimeLobbyRoot != null)
                Destroy(_goRuntimeLobbyRoot);

            if (_goRuntimeCanvasRoot != null && _isRuntimeCanvasOwned)
                Destroy(_goRuntimeCanvasRoot);

            _goRuntimeCanvasRoot = null;
            _goRuntimeLobbyRoot = null;
            _goBackLayer = null;
            _isRuntimeCanvasOwned = false;
            _goSampleListPanel = null;
            _trSampleTitleRoot = null;
            _btnSampleTitlePrefab = null;
            _btnBack = null;
        }

        private void ReleaseModules()
        {
            HideSelectedModule();

            for (int i = 0; i < _activeModules.Count; i++)
            {
                _activeModules[i].Release();
            }

            _activeModules.Clear();
            _modules.Clear();
        }

        //============================================================
        // Logic
        //============================================================
        private void TryCreateRuntimeLauncher()
        {
            if (CanUseLauncher())
                return;

            if (!CreateRuntimeLauncher())
                return;
        }

        private void EnsureLauncherHierarchyUnderCanvas()
        {
            if (!CanUseLauncher())
                return;

            Canvas hostCanvas = GetOrCreateHostCanvas();
            if (hostCanvas == null)
                return;

            Canvas runtimeCanvas = GetOrCreateRuntimeCanvasUnderHost(hostCanvas);
            if (runtimeCanvas == null)
                return;

            Transform runtimeCanvasTransform = runtimeCanvas.transform;

            if (_goSampleListPanel != null && _goSampleListPanel.transform.parent != runtimeCanvasTransform)
                _goSampleListPanel.transform.SetParent(runtimeCanvasTransform, false);

            if (_goBackLayer != null && _goBackLayer.transform.parent != runtimeCanvasTransform)
                _goBackLayer.transform.SetParent(runtimeCanvasTransform, false);

        }

        private bool CreateRuntimeLauncher()
        {
            Canvas uiCanvas = CreateRuntimeCanvas();
            if (uiCanvas == null)
                return false;

            EnsureEventSystem();

            _goRuntimeLobbyRoot = CreateUiObject(RUNTIME_ROOT_NAME, uiCanvas.transform);
            RectTransform runtimeRootRect = _goRuntimeLobbyRoot.GetComponent<RectTransform>();
            StretchToParent(runtimeRootRect);

            _goSampleListPanel = CreatePanel(_goRuntimeLobbyRoot.transform, "GoSampleListPanel", new Color(0.0f, 0.0f, 0.0f, 0.65f));

            RectTransform panelRect = _goSampleListPanel.GetComponent<RectTransform>();
            StretchToParent(panelRect);

            GameObject goHeader = CreatePanel(_goSampleListPanel.transform, "GoHeader", new Color(0.06f, 0.11f, 0.2f, 0.96f));
            RectTransform headerRect = goHeader.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.0f, 1.0f);
            headerRect.anchorMax = new Vector2(1.0f, 1.0f);
            headerRect.pivot = new Vector2(0.5f, 1.0f);
            headerRect.anchoredPosition = Vector2.zero;
            headerRect.sizeDelta = new Vector2(0.0f, 190.0f);

            GameObject goTitle = CreateTextObject(goHeader.transform, "TxtLobbyTitle", "Sample Lobby", 52, FontStyle.Bold, new Color(0.95f, 0.97f, 1.0f, 1.0f));
            RectTransform titleRect = goTitle.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1.0f);
            titleRect.anchorMax = new Vector2(0.5f, 1.0f);
            titleRect.pivot = new Vector2(0.5f, 1.0f);
            titleRect.anchoredPosition = new Vector2(0.0f, -30.0f);
            titleRect.sizeDelta = new Vector2(620.0f, 90.0f);

            GameObject goSubTitle = CreateTextObject(goHeader.transform, "TxtLobbySubTitle", "Select a sample and test quickly", 28, FontStyle.Normal, new Color(0.74f, 0.82f, 0.95f, 1.0f));
            RectTransform subTitleRect = goSubTitle.GetComponent<RectTransform>();
            subTitleRect.anchorMin = new Vector2(0.5f, 1.0f);
            subTitleRect.anchorMax = new Vector2(0.5f, 1.0f);
            subTitleRect.pivot = new Vector2(0.5f, 1.0f);
            subTitleRect.anchoredPosition = new Vector2(0.0f, -108.0f);
            subTitleRect.sizeDelta = new Vector2(640.0f, 56.0f);

            GameObject goListCard = CreatePanel(_goSampleListPanel.transform, "GoListCard", new Color(0.11f, 0.17f, 0.29f, 0.97f));
            RectTransform listCardRect = goListCard.GetComponent<RectTransform>();
            listCardRect.anchorMin = new Vector2(0.5f, 1.0f);
            listCardRect.anchorMax = new Vector2(0.5f, 1.0f);
            listCardRect.pivot = new Vector2(0.5f, 1.0f);
            listCardRect.anchoredPosition = new Vector2(0.0f, -230.0f);
            listCardRect.sizeDelta = new Vector2(660.0f, 880.0f);
            Outline listCardOutline = goListCard.AddComponent<Outline>();
            listCardOutline.effectColor = new Color(0.0f, 0.0f, 0.0f, 0.34f);
            listCardOutline.effectDistance = new Vector2(0.0f, -4.0f);

            GameObject goListRoot = CreateUiObject("TrSampleTitleRoot", goListCard.transform);
            RectTransform listRootRect = goListRoot.GetComponent<RectTransform>();
            listRootRect.anchorMin = new Vector2(0.0f, 0.0f);
            listRootRect.anchorMax = new Vector2(1.0f, 1.0f);
            listRootRect.offsetMin = new Vector2(36.0f, 40.0f);
            listRootRect.offsetMax = new Vector2(-36.0f, -36.0f);

            VerticalLayoutGroup titleLayout = goListRoot.AddComponent<VerticalLayoutGroup>();
            titleLayout.childAlignment = TextAnchor.UpperCenter;
            titleLayout.childControlWidth = true;
            titleLayout.childControlHeight = false;
            titleLayout.childForceExpandWidth = true;
            titleLayout.childForceExpandHeight = false;
            titleLayout.padding = new RectOffset(0, 0, 12, 12);
            titleLayout.spacing = 16.0f;

            _trSampleTitleRoot = goListRoot.transform;
            _btnSampleTitlePrefab = CreateButton(_trSampleTitleRoot, "BtnSampleTitlePrefab", "Sample", new Vector2(0.0f, 96.0f), new Color(0.16f, 0.45f, 0.86f, 1.0f), 34);
            _btnSampleTitlePrefab.gameObject.SetActive(false);

            GameObject goBackLayer = CreateUiObject("GoBackLayer", _goRuntimeLobbyRoot.transform);
            _goBackLayer = goBackLayer;
            RectTransform backLayerRect = goBackLayer.GetComponent<RectTransform>();
            backLayerRect.anchorMin = new Vector2(0.0f, 0.0f);
            backLayerRect.anchorMax = new Vector2(1.0f, 0.0f);
            backLayerRect.pivot = new Vector2(0.5f, 0.0f);
            backLayerRect.offsetMin = new Vector2(0.0f, 0.0f);
            backLayerRect.offsetMax = new Vector2(0.0f, BACK_TAB_HEIGHT);

            Image backLayerImage = goBackLayer.AddComponent<Image>();
            backLayerImage.color = new Color(0.04f, 0.1f, 0.2f, 0.94f);

            Canvas backCanvas = goBackLayer.AddComponent<Canvas>();
            backCanvas.overrideSorting = true;
            backCanvas.sortingOrder = BACK_BUTTON_SORTING_ORDER;
            goBackLayer.AddComponent<GraphicRaycaster>();

            _btnBack = CreateButton(goBackLayer.transform, "BtnBack", "<  Back To Lobby", new Vector2(640.0f, 72.0f), new Color(0.13f, 0.35f, 0.72f, 0.98f), 30);
            RectTransform backRect = _btnBack.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0.5f, 0.5f);
            backRect.anchorMax = new Vector2(0.5f, 0.5f);
            backRect.pivot = new Vector2(0.5f, 0.5f);
            backRect.anchoredPosition = Vector2.zero;
            _btnBack.transform.SetAsLastSibling();
            _btnBack.gameObject.SetActive(false);
            _goBackLayer.SetActive(false);

            return CanUseLauncher();
        }

        private Canvas CreateRuntimeCanvas()
        {
            Canvas hostCanvas = GetOrCreateHostCanvas();
            if (hostCanvas == null)
                return null;

            return GetOrCreateRuntimeCanvasUnderHost(hostCanvas);
        }

        private Canvas GetOrCreateRuntimeCanvasUnderHost(Canvas hostCanvas)
        {
            if (hostCanvas == null)
                return null;

            if (_goRuntimeCanvasRoot != null)
            {
                Canvas currentRuntimeCanvas = _goRuntimeCanvasRoot.GetComponent<Canvas>();
                if (currentRuntimeCanvas != null)
                    return currentRuntimeCanvas;
            }

            Transform foundRuntimeCanvasTransform = hostCanvas.transform.Find(RUNTIME_CANVAS_NAME);
            if (foundRuntimeCanvasTransform != null)
            {
                Canvas foundRuntimeCanvas = foundRuntimeCanvasTransform.GetComponent<Canvas>();
                if (foundRuntimeCanvas != null)
                {
                    _goRuntimeCanvasRoot = foundRuntimeCanvas.gameObject;
                    _isRuntimeCanvasOwned = false;
                    ConfigureRuntimeCanvas(foundRuntimeCanvas, hostCanvas);
                    return foundRuntimeCanvas;
                }
            }

            GameObject goCanvas = new(RUNTIME_CANVAS_NAME, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            goCanvas.transform.SetParent(hostCanvas.transform, false);
            RectTransform canvasRect = goCanvas.GetComponent<RectTransform>();
            StretchToParent(canvasRect);

            Canvas uiCanvas = goCanvas.GetComponent<Canvas>();
            ConfigureRuntimeCanvas(uiCanvas, hostCanvas);

            _goRuntimeCanvasRoot = goCanvas;
            _isRuntimeCanvasOwned = true;
            return uiCanvas;
        }

        private Canvas GetOrCreateHostCanvas()
        {
            Canvas foundCanvas = FindHostCanvas();
            if (foundCanvas != null)
                return foundCanvas;

            GameObject goCanvas = new(HOST_CANVAS_NAME, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas createdCanvas = goCanvas.GetComponent<Canvas>();
            createdCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler canvasScaler = goCanvas.GetComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(720.0f, 1280.0f);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 1.0f;

            return createdCanvas;
        }

        private Canvas FindHostCanvas()
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>(true);

            Canvas rootCanvas = null;
            for (int i = 0; i < canvases.Length; i++)
            {
                Canvas canvas = canvases[i];
                if (canvas == null)
                    continue;

                if (!canvas.isRootCanvas)
                    continue;

                if (string.Equals(canvas.name, HOST_CANVAS_NAME, StringComparison.OrdinalIgnoreCase))
                    return canvas;

                if (rootCanvas == null)
                    rootCanvas = canvas;
            }

            if (rootCanvas != null)
                return rootCanvas;

            for (int i = 0; i < canvases.Length; i++)
            {
                Canvas canvas = canvases[i];
                if (canvas == null)
                    continue;

                if (string.Equals(canvas.name, HOST_CANVAS_NAME, StringComparison.OrdinalIgnoreCase))
                    return canvas;
            }

            return canvases.Length > 0 ? canvases[0] : null;
        }

        private static void ConfigureRuntimeCanvas(Canvas runtimeCanvas, Canvas hostCanvas)
        {
            if (runtimeCanvas == null || hostCanvas == null)
                return;

            runtimeCanvas.renderMode = hostCanvas.renderMode;
            runtimeCanvas.worldCamera = hostCanvas.worldCamera;
            runtimeCanvas.planeDistance = hostCanvas.planeDistance;
            runtimeCanvas.overrideSorting = true;
            runtimeCanvas.sortingOrder = LOBBY_SORTING_ORDER;

            CanvasScaler canvasScaler = runtimeCanvas.GetComponent<CanvasScaler>();
            if (canvasScaler == null)
                return;

            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(720.0f, 1280.0f);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 1.0f;
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
                return;

            GameObject goEventSystem = new(RUNTIME_EVENT_SYSTEM_NAME, typeof(EventSystem));
            Type inputSystemModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemModuleType != null)
            {
                goEventSystem.AddComponent(inputSystemModuleType);
                return;
            }

            goEventSystem.AddComponent<StandaloneInputModule>();
        }

        private void ShowAllModules()
        {
            for (int i = 0; i < _activeModules.Count; i++)
            {
                _activeModules[i].Show();
            }
        }

        private void ShowSampleList()
        {
            HideSelectedModule();

            if (!_goSampleListPanel.activeSelf)
                _goSampleListPanel.SetActive(true);

            if(_goBackLayer != null && _goBackLayer.activeSelf)
                _goBackLayer.SetActive(false);
        }

        private void OpenSample(int moduleIndex)
        {
            if (moduleIndex < 0 || moduleIndex >= _activeModules.Count)
                return;

            HideSelectedModule();

            _selectedModule = _activeModules[moduleIndex];
            _selectedModule.Show();

            if (_goSampleListPanel.activeSelf)
                _goSampleListPanel.SetActive(false);

            if(_goBackLayer != null && !_goBackLayer.activeSelf)
                _goBackLayer.SetActive(true);

            if(!_btnBack.gameObject.activeSelf)
                _btnBack.gameObject.SetActive(true);
        }

        private void HideSelectedModule()
        {
            if (_selectedModule == null)
                return;

            _selectedModule.Hide();
            _selectedModule = null;
        }

        private bool IsTargetModule(string moduleKey)
        {
            if (string.IsNullOrWhiteSpace(moduleKey))
                return false;

            if (string.IsNullOrWhiteSpace(_entryModuleKey))
                return true;

            if (string.Equals(_entryModuleKey, SampleModuleKeys.ALL, StringComparison.OrdinalIgnoreCase))
                return true;

            return string.Equals(moduleKey, _entryModuleKey, StringComparison.OrdinalIgnoreCase);
        }

        private bool CanUseLauncher()
        {
            return _goSampleListPanel != null && _trSampleTitleRoot != null && _btnSampleTitlePrefab != null && _btnBack != null;
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OnSampleTitleButtonClicked()
        {
            GameObject goSelected = EventSystem.current == null ? null : EventSystem.current.currentSelectedGameObject;
            if (goSelected == null)
                return;

            Button selectedButton = goSelected.GetComponent<Button>();
            if (selectedButton == null)
                return;

            if (!_sampleButtonIndexByButton.TryGetValue(selectedButton, out int moduleIndex))
                return;

            OpenSample(moduleIndex);
        }

        private void OnBackButtonClicked()
        {
            ShowSampleList();
        }

        //============================================================
        // Utilities
        //============================================================
        private void ClearSampleTitleButtons()
        {
            for (int i = 0; i < _sampleTitleButtons.Count; i++)
            {
                Button sampleTitleButton = _sampleTitleButtons[i];
                if (sampleTitleButton == null)
                    continue;

                sampleTitleButton.onClick.RemoveListener(OnSampleTitleButtonClicked);
                Destroy(sampleTitleButton.gameObject);
            }

            _sampleTitleButtons.Clear();
            _sampleButtonIndexByButton.Clear();
        }

        private void SetSampleTitle(Button sampleTitleButton, string sampleTitle)
        {
            if (sampleTitleButton == null || string.IsNullOrWhiteSpace(sampleTitle))
                return;

            Text legacyText = sampleTitleButton.GetComponentInChildren<Text>(true);
            if (legacyText != null)
            {
                legacyText.text = sampleTitle;
                return;
            }

            Component[] labelComponents = sampleTitleButton.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < labelComponents.Length; i++)
            {
                Component labelComponent = labelComponents[i];
                if (labelComponent == null)
                    continue;

                Type labelType = labelComponent.GetType();
                bool isTmpType = string.Equals(labelType.FullName, "TMPro.TextMeshProUGUI", StringComparison.Ordinal) || string.Equals(labelType.FullName, "TMPro.TMP_Text", StringComparison.Ordinal);
                if (!isTmpType)
                    continue;

                PropertyInfo textProperty = labelType.GetProperty("text");
                if (textProperty == null || !textProperty.CanWrite)
                    return;

                textProperty.SetValue(labelComponent, sampleTitle);
                return;
            }
        }

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

        private static Button CreateButton(Transform parent, string buttonName, string labelText, Vector2 buttonSize, Color buttonColor, int labelFontSize)
        {
            GameObject goButton = CreateUiObject(buttonName, parent);
            RectTransform buttonRect = goButton.GetComponent<RectTransform>();
            buttonRect.sizeDelta = buttonSize;
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = Vector2.zero;

            Image buttonImage = goButton.AddComponent<Image>();
            buttonImage.color = buttonColor;
            Outline buttonOutline = goButton.AddComponent<Outline>();
            buttonOutline.effectColor = new Color(0.0f, 0.0f, 0.0f, 0.22f);
            buttonOutline.effectDistance = new Vector2(0.0f, -3.0f);
            buttonImage.type = Image.Type.Sliced;

            Button button = goButton.AddComponent<Button>();
            LayoutElement layoutElement = goButton.AddComponent<LayoutElement>();
            layoutElement.minHeight = buttonSize.y;
            layoutElement.preferredHeight = buttonSize.y;
            if (buttonSize.x > 0.0f)
            {
                layoutElement.minWidth = buttonSize.x;
                layoutElement.preferredWidth = buttonSize.x;
                layoutElement.flexibleWidth = 0.0f;
            }
            else
            {
                layoutElement.minWidth = 0.0f;
                layoutElement.preferredWidth = 0.0f;
                layoutElement.flexibleWidth = 1.0f;
            }

            GameObject goLabel = CreateTextObject(goButton.transform, "TxtLabel", labelText, labelFontSize, FontStyle.Bold, Color.white);
            RectTransform labelRect = goLabel.GetComponent<RectTransform>();
            StretchToParent(labelRect);

            return button;
        }

        private static GameObject CreateTextObject(Transform parent, string textName, string textValue, int fontSize, FontStyle fontStyle, Color textColor)
        {
            GameObject goText = CreateUiObject(textName, parent);
            Text labelText = goText.AddComponent<Text>();
            labelText.text = textValue;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = fontSize;
            labelText.fontStyle = fontStyle;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = textColor;
            labelText.horizontalOverflow = HorizontalWrapMode.Overflow;
            labelText.verticalOverflow = VerticalWrapMode.Truncate;
            return goText;
        }

        private static void StretchToParent(RectTransform rectTransform)
        {
            if (rectTransform == null)
                return;

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
