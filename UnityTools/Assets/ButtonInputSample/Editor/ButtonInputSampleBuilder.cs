using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityTools.Ui.Samples.Buttons.Editor
{
    public static class ButtonInputSampleBuilder
    {
        //============================================================
        // Constants
        //============================================================
        private const string OUTPUT_PATH = "Assets/ButtonInputSample";

        //============================================================
        // Logic
        //============================================================
        [MenuItem("Tools/UnityTools/Create Button Input Sample")]
        public static void CreateScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            BuildScene();
        }

        public static void BuildScene()
        {
            Directory.CreateDirectory(OUTPUT_PATH);
            AssetDatabase.Refresh();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            GameObject goCanvas = new GameObject("Button Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            goCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = goCanvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280.0f, 720.0f);
            scaler.matchWidthOrHeight = 0.5f;
            RectTransform rtCanvas = goCanvas.GetComponent<RectTransform>();
            RectTransform rtBg = CreateRect(rtCanvas, "Background", Vector2.zero, Vector2.zero);
            rtBg.anchorMin = Vector2.zero;
            rtBg.anchorMax = Vector2.one;
            Image imgBg = rtBg.gameObject.AddComponent<Image>();
            imgBg.color = new Color(0.04f, 0.06f, 0.12f);
            imgBg.raycastTarget = false;
            CreateText(rtCanvas, "BUTTON INPUT", new Vector2(0.0f, 260.0f), 42);
            CreateText(rtCanvas, "Click once. Hold to repeat faster. Release or leave the button to stop.", new Vector2(0.0f, 190.0f), 22);
            Button btnSingle = CreateButton(rtCanvas, "SINGLE", new Vector2(-240.0f, 40.0f), false);
            UiRepeatButton btnRepeat = (UiRepeatButton)CreateButton(rtCanvas, "HOLD TO REPEAT", new Vector2(240.0f, 40.0f), true);
            Button btnToggle = CreateButton(rtCanvas, "TOGGLE REPEAT", new Vector2(-240.0f, -150.0f), false);
            Button btnPause = CreateButton(rtCanvas, "PAUSE / RESUME", new Vector2(240.0f, -150.0f), false);
            Text txtStatus = CreateText(rtCanvas, "Status", new Vector2(0.0f, -275.0f), 24);
            ButtonInputSample sample = goCanvas.AddComponent<ButtonInputSample>();
            SerializedObject data = new SerializedObject(sample);
            data.FindProperty("_btnRepeat").objectReferenceValue = btnRepeat;
            data.FindProperty("_btnSingle").objectReferenceValue = btnSingle;
            data.FindProperty("_btnToggle").objectReferenceValue = btnToggle;
            data.FindProperty("_btnPause").objectReferenceValue = btnPause;
            data.FindProperty("_txtStatus").objectReferenceValue = txtStatus;
            data.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene, OUTPUT_PATH + "/ButtonInputSample.unity");
            AssetDatabase.SaveAssets();
        }

        //============================================================
        // Utilities
        //============================================================
        private static RectTransform CreateRect(RectTransform rtParent, string name, Vector2 pos, Vector2 size)
        {
            RectTransform rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(rtParent, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        private static Text CreateText(RectTransform rtParent, string text, Vector2 pos, int fontSize)
        {
            Text txt = CreateRect(rtParent, text, pos, new Vector2(1100.0f, 80.0f)).gameObject.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = text;
            txt.fontSize = fontSize;
            txt.color = new Color(0.9f, 0.95f, 1.0f);
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false;
            return txt;
        }

        private static Button CreateButton(RectTransform rtParent, string label, Vector2 pos, bool isRepeat)
        {
            RectTransform rt = CreateRect(rtParent, label, pos, new Vector2(360.0f, 80.0f));
            Image img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(0.15f, 0.35f, 0.6f);
            Button btn = isRepeat ? rt.gameObject.AddComponent<UiRepeatButton>() : rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            rt.gameObject.AddComponent<UiButtonPressScale>();
            Text txt = CreateText(rt, label, Vector2.zero, 24);
            txt.rectTransform.sizeDelta = rt.sizeDelta;
            return btn;
        }
    }
}
