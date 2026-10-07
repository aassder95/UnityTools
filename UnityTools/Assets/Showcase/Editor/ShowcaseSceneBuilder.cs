using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityTools.Ui;

namespace UnityTools.Showcase.Editor
{
    public static class ShowcaseSceneBuilder
    {
        //============================================================
        // Logic
        //============================================================
        [MenuItem("Tools/UnityTools/Create Showcase")]
        public static void CreateScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            string path = EditorUtility.SaveFilePanelInProject("UnityTools Showcase", "Showcase", "unity", "데모 진입 장면 저장 위치를 선택하세요.");
            if (string.IsNullOrEmpty(path))
                return;

            if (!TryBuildScene(path, new[] { "Assets/PerformanceLab/UiPerformanceLab.unity", "Assets/SaveRecoveryLab/SaveRecoveryLab.unity", "Assets/TimerSimulationLab/TimerSimulationLab.unity" }))
                Debug.LogError("Showcase 생성 실패: 세 실험실 장면의 경로를 확인하세요.");
        }

        public static void BuildValidationScene()
        {
            string[] names = { "UiPerformanceLab", "SaveRecoveryLab", "TimerSimulationLab" };
            string[] paths = new string[names.Length];
            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            for (int idx = 0; idx < names.Length; ++idx)
            {
                int cnt = 0;
                for (int guidIdx = 0; guidIdx < guids.Length; ++guidIdx)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[guidIdx]);
                    if (Path.GetFileNameWithoutExtension(path) != names[idx])
                        continue;

                    paths[idx] = path;
                    ++cnt;
                }

                if (cnt != 1)
                {
                    Debug.LogError("검증 장면은 하나여야 합니다: " + names[idx]);
                    EditorApplication.Exit(1);
                    return;
                }
            }

            if (!TryBuildScene("Assets/Showcase/Showcase.unity", paths))
            {
                Debug.LogError("Showcase 검증 장면 생성 실패");
                EditorApplication.Exit(1);
            }
        }

        public static bool TryBuildScene(string path, string[] labPaths)
        {
            if (labPaths == null || labPaths.Length != 3 || string.IsNullOrEmpty(path))
                return false;

            for (int idx = 0; idx < labPaths.Length; ++idx)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(labPaths[idx]) == null)
                    return false;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            ShowcaseController controller = new GameObject("ShowcaseNavigation").AddComponent<ShowcaseController>();
            Canvas canvas = CreateCanvas("Showcase", 0);
            Image bg = canvas.gameObject.AddComponent<Image>();
            bg.color = new Color(0.025f, 0.035f, 0.06f);
            bg.raycastTarget = false;
            RectTransform safe = CreateRect(canvas.transform, "SafeArea", 0.0f, 0.0f, 1440.0f, 900.0f);
            safe.gameObject.AddComponent<RectMask2D>();
            safe.gameObject.AddComponent<Image>().color = Color.clear;
            ScrollRect scroll = safe.gameObject.AddComponent<ScrollRect>();
            RectTransform content = CreateRect(safe, "Content", 0.0f, 0.0f, 1440.0f, 900.0f);
            scroll.viewport = safe;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            UiSafeArea area = canvas.gameObject.AddComponent<UiSafeArea>();
            Assign(area, "_rtSafeArea", safe);
            ShowcaseLayout layout = canvas.gameObject.AddComponent<ShowcaseLayout>();
            Assign(layout, "_canvas", canvas);
            Assign(layout, "_scaler", canvas.GetComponent<CanvasScaler>());
            Assign(layout, "_safeArea", area);
            Assign(layout, "_rtContent", content);
            Transform parent = content;
            Color accent = new Color(0.32f, 0.91f, 0.77f);
            Text eyebrow = CreateText(parent, "Eyebrow", "UNITYTOOLS  /  ENGINEERING SHOWCASE", 19, accent, 48.0f, 34.0f, 1344.0f, 32.0f);
            Text heading = CreateText(parent, "Title", "Explore the design. Run the evidence.", 42, Color.white, 48.0f, 92.0f, 1344.0f, 64.0f);
            Text intro = CreateText(parent, "Intro", "Independent Unity packages. Three reproducible labs for performance, data recovery and time-based state.", 21, new Color(0.65f, 0.72f, 0.83f), 48.0f, 170.0f, 1344.0f, 60.0f);
            string[] titles = { "UI PERFORMANCE", "SAVE RECOVERY", "TIMER SIMULATION" };
            string[] tags = { "01  /  COST & SCALE", "02  /  DATA INTEGRITY", "03  /  TIME & STATE" };
            string[] descriptions = {
                "Compare full creation and virtualization with the same rows, seed and workload.\n\nInspect initialization, frame costs, GC and UI object counts. Export paired CSV results.",
                "Run migration, corrupted-file recovery and future-version protection.\n\nInspect actual input/output snapshots and the invariants that preserve user data.",
                "Advance virtual UTC, recreate services and reject storage writes.\n\nInspect offline restoration, clock rollback and claim state without changing OS time."
            };
            string[] limits = { "Performance depends on device and workload.", "Integrity checks are not encryption.", "Claim state is not a reward transaction." };
            string[] fields = { "_btnUi", "_btnSave", "_btnTimer" };
            RectTransform[] cards = new RectTransform[titles.Length];
            for (int idx = 0; idx < titles.Length; ++idx)
            {
                RectTransform card = CreateRect(parent, titles[idx], 48.0f + idx * 456.0f, 260.0f, 432.0f, 438.0f);
                cards[idx] = card;
                card.gameObject.AddComponent<Image>().color = new Color(0.065f, 0.09f, 0.14f);
                CreateText(card, "Tag", tags[idx], 16, accent, 24.0f, 24.0f, 384.0f, 28.0f);
                CreateText(card, "Title", titles[idx], 27, Color.white, 24.0f, 70.0f, 384.0f, 44.0f);
                CreateText(card, "Description", descriptions[idx], 21, new Color(0.77f, 0.83f, 0.91f), 24.0f, 136.0f, 384.0f, 190.0f);
                CreateText(card, "Limit", limits[idx], 16, new Color(0.60f, 0.68f, 0.79f), 24.0f, 332.0f, 384.0f, 42.0f);
                Button button = CreateButton(card, "OPEN LAB", 24.0f, 382.0f, 384.0f, 40.0f);
                Assign(controller, fields[idx], button);
                for (int childIdx = 0; childIdx < card.childCount; ++childIdx)
                {
                    RectTransform child = (RectTransform)card.GetChild(childIdx);
                    child.anchorMax = new Vector2(1.0f, 1.0f);
                    child.sizeDelta = new Vector2(-48.0f, child.sizeDelta.y);
                }

                RectTransform rtLabel = (RectTransform)button.transform.GetChild(0);
                rtLabel.anchorMax = new Vector2(1.0f, 1.0f);
                rtLabel.sizeDelta = new Vector2(0.0f, rtLabel.sizeDelta.y);
            }

            Text status = CreateText(parent, "Status", "Choose a lab. Each visit starts with a fresh scene.", 19, accent, 48.0f, 742.0f, 1344.0f, 36.0f);
            Text footer = CreateText(parent, "Footer", "Design notes and reproducible commands: docs/SHOWCASE.md\nLab results expose measured behavior and limitations. No preset performance claims.", 17, new Color(0.56f, 0.63f, 0.74f), 48.0f, 810.0f, 1344.0f, 62.0f);
            GameObject input = new GameObject("HubInput", typeof(EventSystem), typeof(StandaloneInputModule));
            Canvas returnCanvas = CreateCanvas("ReturnNavigation", 100);
            returnCanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(720.0f, 900.0f);
            RectTransform returnRoot = CreateRect(returnCanvas.transform, "SafeArea", 0.0f, 0.0f, 720.0f, 900.0f);
            UiSafeArea returnArea = returnCanvas.gameObject.AddComponent<UiSafeArea>();
            Assign(returnArea, "_rtSafeArea", returnRoot);
            Button back = CreateButton(returnRoot, "< SHOWCASE", 1214.0f, 16.0f, 210.0f, 38.0f);
            RectTransform rtBack = (RectTransform)back.transform;
            rtBack.anchorMin = rtBack.anchorMax = Vector2.one;
            rtBack.pivot = Vector2.one;
            rtBack.anchoredPosition = new Vector2(-16.0f, -16.0f);
            Assign(controller, "_goHubView", canvas.gameObject);
            Assign(controller, "_goHubInput", input);
            Assign(controller, "_goReturnView", returnCanvas.gameObject);
            Assign(controller, "_btnReturn", back);
            Assign(controller, "_txtStatus", status);
            SerializedObject data = new SerializedObject(controller);
            SerializedProperty paths = data.FindProperty("_scenePaths");
            paths.arraySize = labPaths.Length;
            for (int idx = 0; idx < labPaths.Length; ++idx)
            {
                paths.GetArrayElementAtIndex(idx).stringValue = labPaths[idx];
            }
            data.ApplyModifiedPropertiesWithoutUndo();
            Assign(layout, "_txtEyebrow", eyebrow);
            Assign(layout, "_txtTitle", heading);
            Assign(layout, "_txtIntro", intro);
            Assign(layout, "_txtStatus", status);
            Assign(layout, "_txtFooter", footer);
            SerializedObject layoutData = new SerializedObject(layout);
            SerializedProperty cardRefs = layoutData.FindProperty("_rtCards");
            cardRefs.arraySize = cards.Length;
            for (int idx = 0; idx < cards.Length; ++idx)
            {
                cardRefs.GetArrayElementAtIndex(idx).objectReferenceValue = cards[idx];
            }

            layoutData.ApplyModifiedPropertiesWithoutUndo();
            if (!layout.TryApplyViewport(new Vector2(Screen.width, Screen.height), Screen.safeArea))
                return false;

            returnCanvas.gameObject.SetActive(false);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            return EditorSceneManager.SaveScene(scene, path);
        }

        [MenuItem("Tools/UnityTools/Configure Showcase Build Scenes")]
        public static void ConfigureBuildScenes()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene> {
                new EditorBuildSettingsScene("Assets/Showcase/Showcase.unity", true),
                new EditorBuildSettingsScene("Assets/PerformanceLab/UiPerformanceLab.unity", true),
                new EditorBuildSettingsScene("Assets/SaveRecoveryLab/SaveRecoveryLab.unity", true),
                new EditorBuildSettingsScene("Assets/TimerSimulationLab/TimerSimulationLab.unity", true)
            };
            foreach (EditorBuildSettingsScene scene in scenes)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path) == null)
                {
                    Debug.LogError("Build Settings에 추가할 장면이 없습니다: " + scene.path);
                    return;
                }
            }

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (!scenes.Exists(item => item.path == scene.path))
                    scenes.Add(scene);
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        public static void CapturePreview()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/Showcase/Showcase.unity");
            Canvas canvas = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.TryGetComponent(out Canvas candidate) && candidate.sortingOrder == 0)
                    canvas = candidate;
            }

            if (canvas == null)
            {
                Debug.LogError("Showcase 미리보기 Canvas가 없습니다.");
                return;
            }

            Camera camera = new GameObject("PreviewCamera").AddComponent<Camera>();
            RenderTexture target = new RenderTexture(1440, 900, 24);
            Texture2D image = new Texture2D(1440, 900, TextureFormat.RGB24, false);
            RenderTexture prevTarget = RenderTexture.active;
            try
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.025f, 0.035f, 0.06f);
                camera.targetTexture = target;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 1.0f;
                Canvas.ForceUpdateCanvases();
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0.0f, 0.0f, 1440.0f, 900.0f), 0, 0);
                image.Apply();
                File.WriteAllBytes("showcase-preview.png", image.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = prevTarget;
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.worldCamera = null;
                camera.targetTexture = null;
                Object.DestroyImmediate(image);
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(camera.gameObject);
            }
        }

        //============================================================
        // Utilities
        //============================================================
        private static Canvas CreateCanvas(string name, int order)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1440.0f, 900.0f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static RectTransform CreateRect(Transform parent, string name, float x, float y, float width, float height)
        {
            RectTransform rt = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.0f, 1.0f);
            rt.pivot = new Vector2(0.0f, 1.0f);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(width, height);
            return rt;
        }

        private static Text CreateText(Transform parent, string name, string value, int size, Color color, float x, float y, float width, float height)
        {
            Text text = CreateRect(parent, name, x, y, width, height).gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static Button CreateButton(Transform parent, string label, float x, float y, float width, float height)
        {
            RectTransform rt = CreateRect(parent, label, x, y, width, height);
            Image image = rt.gameObject.AddComponent<Image>();
            image.color = new Color(0.12f, 0.35f, 0.37f);
            Button button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            Text text = CreateText(rt, "Label", label, 17, Color.white, 0.0f, 0.0f, width, height);
            text.alignment = TextAnchor.MiddleCenter;
            return button;
        }

        private static void Assign(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            SerializedObject data = new SerializedObject(target);
            data.FindProperty(field).objectReferenceValue = value;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
