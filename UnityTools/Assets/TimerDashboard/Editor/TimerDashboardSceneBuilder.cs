using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityTools.Ui;

namespace UnityTools.TimerDashboard.Editor
{
    public static class TimerDashboardSceneBuilder
    {
        //============================================================
        // Logic
        //============================================================
        [MenuItem("Tools/UnityTools/Create Timer Dashboard")]
        public static void CreateScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            BuildValidationScene();
        }

        public static void BuildValidationScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject goCanvas = new GameObject("TimerDashboard", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            goCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = goCanvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1440.0f, 900.0f);
            scaler.matchWidthOrHeight = 0.5f;
            goCanvas.AddComponent<Image>().color = new Color(0.025f, 0.04f, 0.07f);
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            TimerDashboardController controller = goCanvas.AddComponent<TimerDashboardController>();
            goCanvas.AddComponent<TimerDashboardBootstrap>();
            Transform parent = goCanvas.transform;
            CreateText(parent, "Title", "UNITYTOOLS / TIMER DASHBOARD", 30, 28.0f, 22.0f, 1384.0f, 48.0f);
            CreateText(parent, "Help", "Enter a task ID. Register before starting. Data survives unregister / re-enable in this session only.", 18, 28.0f, 78.0f, 1384.0f, 50.0f);
            GameObject goInput = DefaultControls.CreateInputField(new DefaultControls.Resources());
            goInput.name = "TaskId";
            RectTransform rtInput = (RectTransform)goInput.transform;
            rtInput.SetParent(parent, false);
            SetRect(rtInput, 28.0f, 138.0f, 1384.0f, 52.0f);
            InputField input = goInput.GetComponent<InputField>();
            input.text = "BUILD_A";
            input.characterLimit = 64;
            foreach (Text txt in goInput.GetComponentsInChildren<Text>())
            {
                txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txt.fontSize = 22;
            }

            Array actions = Enum.GetValues(typeof(ETimerDashboardAction));
            Button[] buttons = new Button[actions.Length];
            for (int idx = 0; idx < buttons.Length; ++idx)
            {
                RectTransform rt = CreateRect(actions.GetValue(idx).ToString(), parent, 28.0f + idx % 4 * 350.0f, 210.0f + idx / 4 * 64.0f, 334.0f, 54.0f);
                Image img = rt.gameObject.AddComponent<Image>();
                img.color = new Color(0.10f, 0.27f, 0.38f);
                buttons[idx] = rt.gameObject.AddComponent<Button>();
                buttons[idx].targetGraphic = img;
                Text label = CreateText(rt, "Label", actions.GetValue(idx).ToString(), 22, 0.0f, 0.0f, 334.0f, 54.0f);
                label.alignment = TextAnchor.MiddleCenter;
            }

            Text status = CreateText(parent, "Status", "Ready / 60 sec task / SHOP opens 1 min, closes 2 min", 20, 28.0f, 410.0f, 1384.0f, 48.0f);
            RectTransform viewport = CreateRect("TimerViewport", parent, 28.0f, 474.0f, 1384.0f, 310.0f);
            viewport.gameObject.AddComponent<RectMask2D>();
            viewport.gameObject.AddComponent<Image>().color = new Color(0.04f, 0.08f, 0.12f);
            ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
            Text timers = CreateText(viewport, "Timers", "Loading...", 22, 0.0f, 0.0f, 1384.0f, 310.0f);
            RectTransform rtTimers = (RectTransform)timers.transform;
            rtTimers.anchorMin = new Vector2(0.0f, 1.0f);
            rtTimers.anchorMax = Vector2.one;
            rtTimers.sizeDelta = new Vector2(0.0f, 310.0f);
            timers.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport;
            scroll.content = rtTimers;
            scroll.horizontal = false;
            RectTransform toastView = CreateRect("Toast", parent, 28.0f, 806.0f, 1384.0f, 66.0f);
            toastView.gameObject.AddComponent<Image>().color = new Color(0.10f, 0.27f, 0.38f);
            CanvasGroup group = toastView.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0.0f;
            Text toastText = CreateText(toastView, "Message", "", 20, 12.0f, 8.0f, 1360.0f, 50.0f);
            UiToastQueue toast = toastView.gameObject.AddComponent<UiToastQueue>();
            Assign(toast, "_cgToast", group);
            Assign(toast, "_txtToast", toastText);
            Assign(controller, "_txtTimers", timers);
            Assign(controller, "_txtStatus", status);
            Assign(controller, "_inputId", input);
            Assign(controller, "_toast", toast);
            SerializedObject data = new SerializedObject(controller);
            SerializedProperty refs = data.FindProperty("_btnActions");
            refs.arraySize = buttons.Length;
            for (int idx = 0; idx < buttons.Length; ++idx)
            {
                refs.GetArrayElementAtIndex(idx).objectReferenceValue = buttons[idx];
            }

            data.ApplyModifiedPropertiesWithoutUndo();
            Directory.CreateDirectory("Assets/TimerDashboard");
            EditorSceneManager.SaveScene(scene, "Assets/TimerDashboard/TimerDashboard.unity");
        }

        //============================================================
        // Utilities
        //============================================================
        private static RectTransform CreateRect(string name, Transform parent, float x, float y, float width, float height)
        {
            RectTransform rt = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rt.SetParent(parent, false);
            SetRect(rt, x, y, width, height);
            return rt;
        }

        private static void SetRect(RectTransform rt, float x, float y, float width, float height)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.0f, 1.0f);
            rt.pivot = new Vector2(0.0f, 1.0f);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(width, height);
        }

        private static Text CreateText(Transform parent, string name, string value, int size, float x, float y, float width, float height)
        {
            Text txt = CreateRect(name, parent, x, y, width, height).gameObject.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = value;
            txt.fontSize = size;
            txt.color = new Color(0.86f, 0.94f, 1.0f);
            txt.raycastTarget = false;
            return txt;
        }

        private static void Assign(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            SerializedObject data = new SerializedObject(target);
            data.FindProperty(field).objectReferenceValue = value;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
