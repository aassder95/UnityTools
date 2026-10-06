using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityTools.Ui.Samples.Rewards.Editor
{
    public static class RewardFlyerSampleBuilder
    {
        //============================================================
        // Constants
        //============================================================
        private const string OUTPUT_PATH = "Assets/RewardFlyerSample";

        //============================================================
        // Logic
        //============================================================
        [MenuItem("Tools/UnityTools/Create Reward Flyer Sample")]
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
            GameObject goCanvas = new GameObject("Reward Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = goCanvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = goCanvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280.0f, 720.0f);
            scaler.matchWidthOrHeight = 0.5f;
            RectTransform rtCanvas = goCanvas.GetComponent<RectTransform>();
            Image imgBg = CreateImage(rtCanvas, "Background", Vector2.zero, scaler.referenceResolution, new Color(0.04f, 0.06f, 0.12f));
            imgBg.rectTransform.anchorMin = Vector2.zero;
            imgBg.rectTransform.anchorMax = Vector2.one;
            imgBg.rectTransform.sizeDelta = Vector2.zero;
            CreateText(rtCanvas, "Title", "REWARD FLYER", new Vector2(0.0f, 275.0f), new Vector2(1000.0f, 70.0f), 44);
            CreateText(rtCanvas, "Subtitle", "Scatter, collect, reuse. Toggle the HUD motion while icons are flying.", new Vector2(0.0f, 210.0f), new Vector2(1100.0f, 45.0f), 20);
            RectTransform rtSource = CreateImage(rtCanvas, "Reward", new Vector2(-330.0f, -35.0f), new Vector2(110.0f, 110.0f), new Color(0.95f, 0.66f, 0.15f)).rectTransform;
            CreateText(rtCanvas, "Source Label", "REWARD", new Vector2(-330.0f, -115.0f), new Vector2(240.0f, 40.0f), 22);
            RectTransform rtTarget = CreateImage(rtCanvas, "HUD", new Vector2(330.0f, 150.0f), new Vector2(85.0f, 85.0f), new Color(0.25f, 0.8f, 0.65f)).rectTransform;
            Text txtHud = CreateText(rtTarget, "HUD Label", "HUD", Vector2.zero, new Vector2(85.0f, 85.0f), 22);
            txtHud.color = new Color(0.03f, 0.12f, 0.12f);
            Button btnPlay = CreateButton(rtCanvas, "PLAY REWARD", new Vector2(-250.0f, -235.0f));
            Button btnCancel = CreateButton(rtCanvas, "CANCEL", new Vector2(0.0f, -235.0f));
            Button btnMove = CreateButton(rtCanvas, "MOVE HUD", new Vector2(250.0f, -235.0f));
            Text txtStatus = CreateText(rtCanvas, "Status", "Press PLAY REWARD to send 12 icons to the HUD.", new Vector2(0.0f, -305.0f), new Vector2(1100.0f, 40.0f), 20);

            UiRewardMotion motion = AssetDatabase.LoadAssetAtPath<UiRewardMotion>(OUTPUT_PATH + "/RewardMotion.asset");
            if (motion == null)
            {
                motion = ScriptableObject.CreateInstance<UiRewardMotion>();
                AssetDatabase.CreateAsset(motion, OUTPUT_PATH + "/RewardMotion.asset");
            }

            Image imgIcon = CreateImage(rtCanvas, "Reward Icon", Vector2.zero, new Vector2(28.0f, 28.0f), new Color(1.0f, 0.8f, 0.24f));
            imgIcon.gameObject.SetActive(false);
            GameObject goIconPrefab = PrefabUtility.SaveAsPrefabAsset(imgIcon.gameObject, OUTPUT_PATH + "/RewardIcon.prefab");
            Object.DestroyImmediate(imgIcon.gameObject);
            RectTransform rtLayer = CreateRect(rtCanvas, "Flyer Layer", Vector2.zero, Vector2.zero);
            rtLayer.anchorMin = Vector2.zero;
            rtLayer.anchorMax = Vector2.one;
            UiRewardFlyer flyer = rtLayer.gameObject.AddComponent<UiRewardFlyer>();
            SerializedObject flyerData = new SerializedObject(flyer);
            flyerData.FindProperty("_rtIconPrefab").objectReferenceValue = goIconPrefab.GetComponent<RectTransform>();
            flyerData.FindProperty("_motion").objectReferenceValue = motion;
            flyerData.ApplyModifiedPropertiesWithoutUndo();

            RewardFlyerSample sample = goCanvas.AddComponent<RewardFlyerSample>();
            SerializedObject sampleData = new SerializedObject(sample);
            sampleData.FindProperty("_flyer").objectReferenceValue = flyer;
            sampleData.FindProperty("_rtSource").objectReferenceValue = rtSource;
            sampleData.FindProperty("_rtTarget").objectReferenceValue = rtTarget;
            sampleData.FindProperty("_btnPlay").objectReferenceValue = btnPlay;
            sampleData.FindProperty("_btnCancel").objectReferenceValue = btnCancel;
            sampleData.FindProperty("_btnMove").objectReferenceValue = btnMove;
            sampleData.FindProperty("_txtStatus").objectReferenceValue = txtStatus;
            sampleData.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene, OUTPUT_PATH + "/RewardFlyerSample.unity");
        }

        //============================================================
        // Utilities
        //============================================================
        private static RectTransform CreateRect(RectTransform rtParent, string name, Vector2 pos, Vector2 size)
        {
            RectTransform rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(rtParent, false);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        private static Image CreateImage(RectTransform rtParent, string name, Vector2 pos, Vector2 size, Color color)
        {
            Image img = CreateRect(rtParent, name, pos, size).gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        private static Text CreateText(RectTransform rtParent, string name, string text, Vector2 pos, Vector2 size, int fontSize)
        {
            Text txt = CreateRect(rtParent, name, pos, size).gameObject.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = text;
            txt.fontSize = fontSize;
            txt.color = new Color(0.9f, 0.95f, 1.0f);
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false;
            return txt;
        }

        private static Button CreateButton(RectTransform rtParent, string label, Vector2 pos)
        {
            Image img = CreateImage(rtParent, label, pos, new Vector2(215.0f, 56.0f), new Color(0.12f, 0.25f, 0.42f));
            img.raycastTarget = true;
            Button btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            CreateText(img.rectTransform, "Label", label, Vector2.zero, img.rectTransform.sizeDelta, 20);
            return btn;
        }
    }
}
