using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UnityTools.Benchmark.Samples.Editor
{
    public static class UiLabSceneBuilder
    {
        //============================================================
        // Logic
        //============================================================
        [MenuItem("Tools/UnityTools/Create UI Performance Lab")]
        public static void CreateScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            string path = EditorUtility.SaveFilePanelInProject("UI Performance Lab", "UiPerformanceLab", "unity", "샘플 장면 저장 위치를 선택하세요.");
            if (string.IsNullOrEmpty(path))
                return;

            BuildScene(path);
        }

        public static void BuildValidationScene()
        {
            BuildScene("Assets/UiPerformanceLab.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/UiPerformanceLab.unity", true) };
        }

        public static void BuildScene(string path)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject goCanvas = new GameObject("UiPerformanceLab", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = goCanvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = goCanvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280.0f, 720.0f);
            scaler.matchWidthOrHeight = 0.5f;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            RectTransform rtRoot = (RectTransform)goCanvas.transform;
            Text title = CreateText("Title", rtRoot, "UNITYTOOLS / UI PERFORMANCE LAB", 28);
            Place((RectTransform)title.transform, 24.0f, 20.0f, 1220.0f, 44.0f);

            RectTransform rtControls = CreateRect("Controls", rtRoot);
            Place(rtControls, 24.0f, 80.0f, 1232.0f, 50.0f);
            InputField inputCnt = CreateInput(rtControls, "Item count", "1000", 0.0f);
            InputField inputSeed = CreateInput(rtControls, "Seed", "42", 175.0f);
            Dropdown choice = CreateDropdown(rtControls, 350.0f);
            Button btnRun = CreateButton(rtControls, "RUN", 630.0f);
            Button btnStop = CreateButton(rtControls, "STOP", 820.0f);
            Button btnExport = CreateButton(rtControls, "EXPORT CSV", 1010.0f);
            btnStop.interactable = false;
            btnExport.interactable = false;

            RectTransform rtCompare = CreateRect("ComparisonControls", rtRoot);
            Place(rtCompare, 24.0f, 140.0f, 1232.0f, 46.0f);
            Button btnCompare = CreateButton(rtCompare, "COMPARE A/B", 0.0f);
            Dropdown order = CreateDropdown(rtCompare, 190.0f);
            order.options = new System.Collections.Generic.List<Dropdown.OptionData>
            {
                new Dropdown.OptionData("Baseline -> Virtualized"),
                new Dropdown.OptionData("Virtualized -> Baseline")
            };
            order.RefreshShownValue();
            InputField inputPairs = CreateInput(rtCompare, "Pairs (1-20)", "4", 470.0f);
            Text hint = CreateText("Hint", rtCompare, "Compare: up to 10,000 / RUN: virtualized only", 17);
            Place((RectTransform)hint.transform, 650.0f, 6.0f, 560.0f, 36.0f);
            Text result = CreateText("Result", rtRoot, "RUN: virtualized only\nCOMPARE A/B: fresh UI objects, same data and workload\nInitialization and steady frame costs are separate.\nUse a Development Build for device profiling.", 18);
            Place((RectTransform)result.transform, 760.0f, 210.0f, 495.0f, 480.0f);
            result.alignment = TextAnchor.UpperLeft;
            RectTransform rtViewport = CreateRect("Viewport", rtRoot);
            Place(rtViewport, 24.0f, 210.0f, 710.0f, 480.0f);
            Image imgViewport = rtViewport.gameObject.AddComponent<Image>();
            imgViewport.color = new Color(0.05f, 0.09f, 0.15f);
            rtViewport.gameObject.AddComponent<RectMask2D>();
            ScrollRect scrollRect = rtViewport.gameObject.AddComponent<ScrollRect>();
            scrollRect.viewport = rtViewport;
            scrollRect.horizontal = false;
            RectTransform rtContent = CreateRect("Content", rtViewport);
            rtContent.anchorMin = new Vector2(0.0f, 1.0f);
            rtContent.anchorMax = Vector2.one;
            rtContent.pivot = new Vector2(0.0f, 1.0f);
            rtContent.sizeDelta = Vector2.zero;
            scrollRect.content = rtContent;
            RectTransform rtTemplate = CreateRect("ItemTemplate", rtRoot);
            Place(rtTemplate, 0.0f, 0.0f, 710.0f, 44.0f);
            Image imgItem = rtTemplate.gameObject.AddComponent<Image>();
            Text row = CreateText("Row", rtTemplate, "ITEM", 20);
            Stretch((RectTransform)row.transform);
            row.alignment = TextAnchor.MiddleLeft;
            UiLabItem item = rtTemplate.gameObject.AddComponent<UiLabItem>();
            Assign(item, "_txtRow", row);
            Assign(item, "_imgBg", imgItem);
            rtTemplate.gameObject.SetActive(false);
            UiLabScroll scroll = rtViewport.gameObject.AddComponent<UiLabScroll>();
            Assign(scroll, "_item", item);
            SerializedObject scrollData = new SerializedObject(scroll);
            scrollData.FindProperty("_layoutMode").enumValueIndex = 0;
            scrollData.FindProperty("_isInertia").boolValue = false;
            scrollData.ApplyModifiedPropertiesWithoutUndo();
            UiLabBaseline baseline = rtViewport.gameObject.AddComponent<UiLabBaseline>();
            Assign(baseline, "_item", item);
            Assign(baseline, "_scrollRect", scrollRect);
            Assign(baseline, "_rtContent", rtContent);
            Assign(baseline, "_rtViewport", rtViewport);
            UiLabController controller = goCanvas.AddComponent<UiLabController>();
            Assign(controller, "_scroll", scroll);
            Assign(controller, "_baseline", baseline);
            Assign(controller, "_inputItemCnt", inputCnt);
            Assign(controller, "_inputSeed", inputSeed);
            Assign(controller, "_scenarioChoice", choice);
            Assign(controller, "_btnRun", btnRun);
            Assign(controller, "_btnStop", btnStop);
            Assign(controller, "_btnExport", btnExport);
            Assign(controller, "_btnCompare", btnCompare);
            Assign(controller, "_orderChoice", order);
            Assign(controller, "_inputPairCnt", inputPairs);
            Assign(controller, "_txtResult", result);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            EditorSceneManager.SaveScene(scene, path);
        }

        //============================================================
        // Utilities
        //============================================================
        private static RectTransform CreateRect(string name, Transform parent)
        {
            RectTransform rt = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rt.SetParent(parent, false);
            return rt;
        }

        private static Text CreateText(string name, Transform parent, string value, int size)
        {
            Text txt = CreateRect(name, parent).gameObject.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = value;
            txt.fontSize = size;
            txt.color = new Color(0.86f, 0.94f, 1.0f);
            txt.raycastTarget = false;
            return txt;
        }

        private static void Place(RectTransform rt, float x, float y, float width, float height)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.0f, 1.0f);
            rt.pivot = new Vector2(0.0f, 1.0f);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(width, height);
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(10.0f, 0.0f);
            rt.offsetMax = new Vector2(-10.0f, 0.0f);
        }

        private static void Assign(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            SerializedObject data = new SerializedObject(target);
            data.FindProperty(field).objectReferenceValue = value;
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Button CreateButton(Transform parent, string label, float x)
        {
            RectTransform rt = CreateRect(label, parent);
            Place(rt, x, 0.0f, 170.0f, 46.0f);
            Image img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(0.12f, 0.32f, 0.48f);
            Button btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            Text txt = CreateText("Label", rt, label, 19);
            Stretch((RectTransform)txt.transform);
            txt.alignment = TextAnchor.MiddleCenter;
            return btn;
        }

        private static InputField CreateInput(Transform parent, string name, string value, float x)
        {
            RectTransform rt = CreateRect(name, parent);
            Place(rt, x, 0.0f, 155.0f, 46.0f);
            Image img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(0.12f, 0.18f, 0.25f);
            Text txt = CreateText("Value", rt, value, 22);
            Stretch((RectTransform)txt.transform);
            txt.alignment = TextAnchor.MiddleLeft;
            InputField input = rt.gameObject.AddComponent<InputField>();
            input.targetGraphic = img;
            input.textComponent = txt;
            input.contentType = InputField.ContentType.IntegerNumber;
            input.text = value;
            Text caption = CreateText("Caption", rt, name, 13);
            Place((RectTransform)caption.transform, 0.0f, -20.0f, 155.0f, 20.0f);
            return input;
        }

        private static Dropdown CreateDropdown(Transform parent, float x)
        {
            RectTransform rt = CreateRect("Scenario", parent);
            Place(rt, x, 0.0f, 260.0f, 46.0f);
            Image img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(0.12f, 0.18f, 0.25f);
            Dropdown dropdown = rt.gameObject.AddComponent<Dropdown>();
            dropdown.targetGraphic = img;
            Text label = CreateText("Label", rt, "Sweep", 20);
            Stretch((RectTransform)label.transform);
            dropdown.captionText = label;
            dropdown.options = new System.Collections.Generic.List<Dropdown.OptionData>
            {
                new Dropdown.OptionData("Sweep"),
                new Dropdown.OptionData("Random Jump"),
                new Dropdown.OptionData("Insert & Remove")
            };
            RectTransform rtTemplate = CreateRect("Template", rt);
            Place(rtTemplate, 0.0f, 46.0f, 260.0f, 150.0f);
            rtTemplate.gameObject.AddComponent<Image>().color = new Color(0.08f, 0.14f, 0.20f);
            ScrollRect optionsScroll = rtTemplate.gameObject.AddComponent<ScrollRect>();
            optionsScroll.horizontal = false;
            RectTransform rtViewport = CreateRect("Viewport", rtTemplate);
            Stretch(rtViewport);
            rtViewport.gameObject.AddComponent<RectMask2D>();
            RectTransform rtContent = CreateRect("Content", rtViewport);
            Place(rtContent, 0.0f, 0.0f, 240.0f, 44.0f);
            optionsScroll.viewport = rtViewport;
            optionsScroll.content = rtContent;
            RectTransform rtItem = CreateRect("Item", rtContent);
            Place(rtItem, 0.0f, 0.0f, 240.0f, 44.0f);
            Toggle toggle = rtItem.gameObject.AddComponent<Toggle>();
            Image imgItem = rtItem.gameObject.AddComponent<Image>();
            imgItem.color = new Color(0.12f, 0.32f, 0.48f);
            toggle.targetGraphic = imgItem;
            toggle.graphic = imgItem;
            Text itemLabel = CreateText("Label", rtItem, "Option", 20);
            Stretch((RectTransform)itemLabel.transform);
            dropdown.template = rtTemplate;
            dropdown.itemText = itemLabel;
            rtTemplate.gameObject.SetActive(false);
            return dropdown;
        }
    }
}
