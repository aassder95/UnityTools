#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityTools.Ui;

namespace UnityTools.Samples.Features
{
    public static class UiFeatureSceneBuilder
    {
        //============================================================
        // Logic
        //============================================================
        [MenuItem("Tools/UnityTools/Create UI Feature Demo")]
        public static void CreateScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            string path = EditorUtility.SaveFilePanelInProject("UI Feature Demo", "UiFeatureDemo", "unity", "샘플 장면 저장 위치를 선택하세요.");
            if (!string.IsNullOrEmpty(path))
                BuildScene(path);
        }

        public static void BuildValidationScene()
        {
            BuildScene("Assets/UiFeatureDemo.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/UiFeatureDemo.unity", true) };
        }

        public static void BuildScene(string path)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject goCanvas = new GameObject("UiFeatureDemo", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            goCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = goCanvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280.0f, 720.0f);
            scaler.matchWidthOrHeight = 0.5f;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            RectTransform root = (RectTransform)goCanvas.transform;
            Image bg = root.gameObject.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.09f, 0.15f);
            Label(root, "UNITYTOOLS / UI FEATURE DEMO", 24.0f, 20.0f, 1232.0f, 50.0f, 30);
            Label(root, "1. VARIABLE HEIGHTS", 24.0f, 95.0f, 390.0f, 40.0f, 22);
            Label(root, "2. TRANSITION RESULT", 440.0f, 95.0f, 370.0f, 40.0f, 22);
            Label(root, "3. CLOSE ONE POPUP", 850.0f, 95.0f, 400.0f, 40.0f, 22);
            Button resize = Button(root, "RESIZE ROW 1", 24.0f, 150.0f, 180.0f);
            Button jump = Button(root, "JUMP TO MIDDLE", 214.0f, 150.0f, 200.0f);
            Text listStatus = Label(root, "", 24.0f, 205.0f, 390.0f, 90.0f, 17);
            RectTransform viewport = Rect(root, "VariableList", 24.0f, 305.0f, 390.0f, 385.0f);
            viewport.gameObject.AddComponent<Image>().color = new Color(0.12f, 0.18f, 0.26f);
            viewport.gameObject.AddComponent<RectMask2D>();
            ScrollRect scrollRect = viewport.gameObject.AddComponent<ScrollRect>();
            scrollRect.viewport = viewport;
            scrollRect.horizontal = false;
            RectTransform content = Rect(viewport, "Content", 0.0f, 0.0f, 390.0f, 385.0f);
            scrollRect.content = content;
            RectTransform template = Rect(root, "FeatureItemTemplate", 0.0f, 0.0f, 390.0f, 70.0f);
            template.gameObject.AddComponent<Image>().color = new Color(0.15f, 0.26f, 0.36f);
            Text row = Label(template, "ROW", 12.0f, 0.0f, 366.0f, 70.0f, 16);
            row.rectTransform.anchorMin = Vector2.zero;
            row.rectTransform.anchorMax = Vector2.one;
            row.rectTransform.offsetMin = new Vector2(12.0f, 0.0f);
            row.rectTransform.offsetMax = new Vector2(-12.0f, 0.0f);
            FeatureItem item = template.gameObject.AddComponent<FeatureItem>();
            Assign(item, "_txtRow", row);
            template.gameObject.SetActive(false);
            FeatureScroll scroll = viewport.gameObject.AddComponent<FeatureScroll>();
            Assign(scroll, "_item", item);
            SerializedObject scrollData = new SerializedObject(scroll);
            scrollData.FindProperty("_spacing").vector2Value = new Vector2(0.0f, 8.0f);
            scrollData.ApplyModifiedPropertiesWithoutUndo();

            Button show = Button(root, "SHOW", 440.0f, 150.0f, 115.0f);
            Button hide = Button(root, "HIDE", 565.0f, 150.0f, 115.0f);
            Button cancel = Button(root, "CANCEL", 690.0f, 150.0f, 120.0f);
            Text transitionStatus = Label(root, "", 440.0f, 205.0f, 370.0f, 100.0f, 17);
            UiCanvasTransition transition = Panel(root, "FadePanel", "FADING PANEL\nShow / Hide returns a result", 440.0f, 325.0f, 370.0f, 220.0f, new Color(0.12f, 0.42f, 0.43f));
            SerializedObject fadeData = new SerializedObject(transition);
            fadeData.FindProperty("_showDurationSec").floatValue = 1.5f;
            fadeData.FindProperty("_hideDurationSec").floatValue = 1.5f;
            fadeData.ApplyModifiedPropertiesWithoutUndo();
            transition.gameObject.SetActive(false);
            Label(root, "Cancel snaps to the requested target.\nShow cancel: visible / Hide cancel: hidden.\nReplacing a request cancels the earlier Task.", 440.0f, 575.0f, 370.0f, 110.0f, 17);

            Button open = Button(root, "OPEN BOTH", 850.0f, 150.0f, 180.0f);
            Button closeLower = Button(root, "CLOSE LOWER", 1040.0f, 150.0f, 210.0f);
            Button back = Button(root, "BACK / CLOSE TOP", 850.0f, 205.0f, 400.0f);
            Text popupStatus = Label(root, "", 850.0f, 265.0f, 400.0f, 90.0f, 17);
            UiCanvasTransition lower = Panel(root, "LowerPopup", "LOWER POPUP", 850.0f, 390.0f, 400.0f, 220.0f, new Color(0.23f, 0.29f, 0.47f));
            UiCanvasTransition top = Panel(root, "TopPopup", "TOP POPUP\nThis stays open when lower closes", 900.0f, 440.0f, 330.0f, 120.0f, new Color(0.40f, 0.27f, 0.23f));
            lower.gameObject.SetActive(false);
            top.gameObject.SetActive(false);
            UiFeatureDemo demo = root.gameObject.AddComponent<UiFeatureDemo>();
            Assign(demo, "_scroll", scroll);
            Assign(demo, "_btnResize", resize);
            Assign(demo, "_btnJump", jump);
            Assign(demo, "_txtList", listStatus);
            Assign(demo, "_transition", transition);
            Assign(demo, "_btnShow", show);
            Assign(demo, "_btnHide", hide);
            Assign(demo, "_btnCancel", cancel);
            Assign(demo, "_txtTransition", transitionStatus);
            Assign(demo, "_lowerPopup", lower);
            Assign(demo, "_topPopup", top);
            Assign(demo, "_btnOpen", open);
            Assign(demo, "_btnCloseLower", closeLower);
            Assign(demo, "_btnBack", back);
            Assign(demo, "_txtPopup", popupStatus);
            SerializedObject demoData = new SerializedObject(demo);
            SerializedProperty heights = demoData.FindProperty("_heights");
            heights.arraySize = 60;
            for (int idx = 0; idx < heights.arraySize; ++idx)
            {
                heights.GetArrayElementAtIndex(idx).floatValue = idx % 3 == 0 ? 110.0f : 70.0f;
            }

            demoData.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene, path);
            AssetDatabase.SaveAssets();
        }

        //============================================================
        // Utilities
        //============================================================
        private static RectTransform Rect(Transform parent, string name, float x, float y, float width, float height)
        {
            RectTransform rt = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.0f, 1.0f);
            rt.pivot = new Vector2(0.0f, 1.0f);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(width, height);
            return rt;
        }

        private static Text Label(Transform parent, string value, float x, float y, float width, float height, int size)
        {
            Text txt = Rect(parent, value.Length > 0 ? value.Split('\n')[0] : "Status", x, y, width, height).gameObject.AddComponent<Text>();
            txt.text = value;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = size;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.raycastTarget = false;
            return txt;
        }

        private static Button Button(Transform parent, string caption, float x, float y, float width)
        {
            RectTransform rt = Rect(parent, caption, x, y, width, 42.0f);
            Image img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(0.22f, 0.35f, 0.48f);
            Button btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            Label(rt, caption, 8.0f, 0.0f, width - 16.0f, 42.0f, 17).alignment = TextAnchor.MiddleCenter;
            return btn;
        }

        private static UiCanvasTransition Panel(Transform parent, string name, string caption, float x, float y, float width, float height, Color color)
        {
            RectTransform rt = Rect(parent, name, x, y, width, height);
            rt.gameObject.AddComponent<Image>().color = color;
            rt.gameObject.AddComponent<CanvasGroup>();
            UiCanvasTransition transition = rt.gameObject.AddComponent<UiCanvasTransition>();
            Label(rt, caption, 16.0f, 8.0f, width - 32.0f, height - 16.0f, 19);
            return transition;
        }

        private static void Assign(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            SerializedObject data = new SerializedObject(target);
            data.FindProperty(field).objectReferenceValue = value;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
