using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UnityTools.Timer.Samples.Editor
{
    public static class TimerLabSceneBuilder
    {
        //============================================================
        // Logic
        //============================================================
        [MenuItem("Tools/UnityTools/Create Timer Simulation Lab")]
        public static void CreateScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            string path = EditorUtility.SaveFilePanelInProject("Timer Simulation Lab", "TimerSimulationLab", "unity", "샘플 장면 저장 위치를 선택하세요.");
            if (string.IsNullOrEmpty(path))
                return;

            BuildScene(path);
        }

        public static void BuildValidationScene()
        {
            BuildScene("Assets/TimerSimulationLab.unity");
        }

        public static void BuildScene(string path)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject goCanvas = new GameObject("TimerSimulationLab", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            goCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = goCanvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1440.0f, 900.0f);
            scaler.matchWidthOrHeight = 0.5f;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            RectTransform rtRoot = (RectTransform)goCanvas.transform;
            Image imgBg = goCanvas.AddComponent<Image>();
            imgBg.color = new Color(0.025f, 0.04f, 0.07f);
            imgBg.raycastTarget = false;
            CreateText(rtRoot, "Title", "UNITYTOOLS / TIMER SIMULATION LAB", 30, 28.0f, 22.0f, 1384.0f, 46.0f);
            CreateText(rtRoot, "Help", "Choose a controlled fixture. Inspect virtual UTC, storage snapshots and timer transitions. OS time stays unchanged.", 18, 28.0f, 76.0f, 1384.0f, 30.0f);
            TimerLabController controller = goCanvas.AddComponent<TimerLabController>();
            string[] labels = { "FORWARD TIME", "OFFLINE RESTORE", "CLOCK ROLLBACK", "SAVE FAILURE", "DUPLICATE CLAIM", "PAUSE / RESUME", "CANCEL / RESTART", "UNREGISTER / RESTORE" };
            string[] fields = { "_btnForwardTime", "_btnOffline", "_btnRollback", "_btnFailure", "_btnClaim", "_btnPause", "_btnCancel", "_btnUnregister" };
            for (int idx = 0; idx < labels.Length; ++idx)
            {
                RectTransform rtButton = CreateRect(labels[idx], rtRoot, 28.0f + idx % 4 * 352.0f, 124.0f + idx / 4 * 60.0f, 328.0f, 50.0f);
                Image img = rtButton.gameObject.AddComponent<Image>();
                img.color = new Color(0.10f, 0.27f, 0.38f);
                Button btn = rtButton.gameObject.AddComponent<Button>();
                btn.targetGraphic = img;
                Text label = CreateText(rtButton, "Label", labels[idx], 17, 0.0f, 0.0f, 328.0f, 50.0f);
                label.alignment = TextAnchor.MiddleCenter;
                Assign(controller, fields[idx], btn);
            }

            CreateText(rtRoot, "BeforeTitle", "01 / INITIAL STATE", 20, 28.0f, 254.0f, 670.0f, 32.0f);
            CreateText(rtRoot, "AfterTitle", "03 / FINAL STATE", 20, 742.0f, 254.0f, 670.0f, 32.0f);
            Text before = CreateText(rtRoot, "Before", "VIRTUAL UTC + STATE + STORAGE\n\nSelect a scenario to inspect its input.", 17, 28.0f, 294.0f, 670.0f, 270.0f);
            Text after = CreateText(rtRoot, "After", "VIRTUAL UTC + STATE + STORAGE\n\nCaptured after clock changes / initialization / claim.", 17, 742.0f, 294.0f, 670.0f, 270.0f);
            CreateText(rtRoot, "ResultTitle", "02 / OBSERVED OPERATIONS", 20, 28.0f, 590.0f, 1384.0f, 32.0f);
            Text result = CreateText(rtRoot, "Result", "PASS means the observed behavior matches the scenario invariant, including expected failures.\nEach fixture uses a fresh injected clock and in-memory storage.\nService recreation simulates restart; this is not a disk persistence or anti-cheat demo.", 17, 28.0f, 634.0f, 1384.0f, 240.0f);
            Assign(controller, "_txtBefore", before);
            Assign(controller, "_txtAfter", after);
            Assign(controller, "_txtResult", result);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            EditorSceneManager.SaveScene(scene, path);
        }

        //============================================================
        // Utilities
        //============================================================
        private static RectTransform CreateRect(string name, Transform parent, float x, float y, float width, float height)
        {
            RectTransform rt = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.0f, 1.0f);
            rt.pivot = new Vector2(0.0f, 1.0f);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(width, height);
            return rt;
        }

        private static Text CreateText(Transform parent, string name, string value, int size, float x, float y, float width, float height)
        {
            Text txt = CreateRect(name, parent, x, y, width, height).gameObject.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = value;
            txt.fontSize = size;
            txt.color = new Color(0.86f, 0.94f, 1.0f);
            txt.raycastTarget = false;
            txt.alignment = TextAnchor.UpperLeft;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Truncate;
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
