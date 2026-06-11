using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UnityTools.Samples.Util
{
    public static class SampleTestUiBuilder
    {
        //============================================================
        // Constants
        //============================================================
        private const string TEST_ROOT_NAME = "GoSampleTestRoot";
        private const string CONTROLS_WRAP_NAME = "GoControlsWrap";
        private const string CONTENT_VIEWPORT_NAME = "GoContentViewport";
        private const string TITLE_FONT_PATH = "LegacyRuntime.ttf";
        private const float BOTTOM_TAB_SAFE_MARGIN = 128.0f;
        private const float DEFAULT_ROOT_WIDTH = 720.0f;
        private const float DEFAULT_ROOT_HEIGHT = 1280.0f;
        private const float MIN_CONTENT_HEIGHT = 220.0f;

        //============================================================
        // Logic
        //============================================================
        public static SampleTestLayout Build(Transform parent, string title, string subtitle)
        {
            EnsureRootStretch(parent);
            EnsureCanvas(parent);

            Transform trExistingRoot = parent.Find(TEST_ROOT_NAME);
            if(trExistingRoot != null)
                return ResolveExistingLayout(trExistingRoot);

            RectTransform rtRoot = CreateRect(TEST_ROOT_NAME, parent);
            Stretch(rtRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image bgImage = rtRoot.gameObject.AddComponent<Image>();
            bgImage.color = new Color(0.07f, 0.16f, 0.33f, 0.92f);
            bgImage.raycastTarget = false;

            float rootWidth = ResolveRectWidth(rtRoot, DEFAULT_ROOT_WIDTH);
            float rootHeight = ResolveRectHeight(rtRoot, DEFAULT_ROOT_HEIGHT);

            float sidePadding = Mathf.Clamp(rootWidth * 0.033f, 16.0f, 28.0f);
            float topPadding = Mathf.Clamp(rootHeight * 0.016f, 12.0f, 24.0f);
            float sectionSpacing = Mathf.Clamp(rootHeight * 0.012f, 8.0f, 16.0f);
            float headerHeight = Mathf.Clamp(rootHeight * 0.13f, 120.0f, 188.0f);
            float controlsHeight = Mathf.Clamp(rootHeight * 0.14f, 96.0f, 188.0f);
            float bottomSafeMargin = Mathf.Clamp(rootHeight * 0.06f, 36.0f, BOTTOM_TAB_SAFE_MARGIN);
            float contentTopOffset = topPadding + headerHeight + sectionSpacing + controlsHeight + sectionSpacing;
            float maxContentTopOffset = Mathf.Max(topPadding + sectionSpacing, rootHeight - bottomSafeMargin - MIN_CONTENT_HEIGHT);
            if(contentTopOffset > maxContentTopOffset)
                contentTopOffset = maxContentTopOffset;

            float headerBottomOffset = topPadding + headerHeight;
            float controlsTopOffset = headerBottomOffset + sectionSpacing;
            float controlsBottomOffset = controlsTopOffset + controlsHeight;

            RectTransform rtHeader = CreateRect("GoHeader", rtRoot);
            Stretch(rtHeader, new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f), new Vector2(sidePadding, -headerBottomOffset), new Vector2(-sidePadding, -topPadding));
            Image headerImage = rtHeader.gameObject.AddComponent<Image>();
            headerImage.color = new Color(0.03f, 0.1f, 0.23f, 0.96f);
            headerImage.raycastTarget = false;

            GameObject goTitle = CreateText(rtHeader, "TxtTitle", title, 42, FontStyle.Bold, Color.white);
            RectTransform rtTitle = goTitle.GetComponent<RectTransform>();
            Stretch(rtTitle, new Vector2(0.0f, 0.48f), new Vector2(1.0f, 1.0f), new Vector2(16.0f, 8.0f), new Vector2(-16.0f, -10.0f));

            GameObject goSubtitle = CreateText(rtHeader, "TxtSubtitle", subtitle, 22, FontStyle.Normal, new Color(0.78f, 0.86f, 0.97f, 1.0f));
            RectTransform rtSubtitle = goSubtitle.GetComponent<RectTransform>();
            Stretch(rtSubtitle, new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.52f), new Vector2(16.0f, 8.0f), new Vector2(-16.0f, -8.0f));

            RectTransform rtControls = CreateRect("GoControls", rtRoot);
            Stretch(rtControls, new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f), new Vector2(sidePadding, -controlsBottomOffset), new Vector2(-sidePadding, -controlsTopOffset));
            Image controlsImage = rtControls.gameObject.AddComponent<Image>();
            controlsImage.color = new Color(0.1f, 0.2f, 0.38f, 0.95f);
            controlsImage.raycastTarget = false;

            RectTransform rtControlsWrap = CreateRect(CONTROLS_WRAP_NAME, rtControls);
            Stretch(rtControlsWrap, Vector2.zero, Vector2.one, new Vector2(8.0f, 8.0f), new Vector2(-8.0f, -8.0f));
            GridLayoutGroup controlsLayout = rtControlsWrap.gameObject.AddComponent<GridLayoutGroup>();
            controlsLayout.padding = new RectOffset(8, 8, 8, 8);
            controlsLayout.spacing = new Vector2(10.0f, 8.0f);
            float controlsCellHeight = Mathf.Clamp(rootHeight * 0.045f, 46.0f, 58.0f);
            float controlsInnerWidth = Mathf.Max(240.0f, rootWidth - (sidePadding * 2.0f) - 16.0f - controlsLayout.padding.left - controlsLayout.padding.right);
            int controlsColumnCount = ResolveControlsColumnCount(controlsInnerWidth);
            float controlsCellWidth = ResolveControlsCellWidth(controlsInnerWidth, controlsColumnCount, controlsLayout.spacing.x);
            controlsLayout.cellSize = new Vector2(controlsCellWidth, controlsCellHeight);
            controlsLayout.childAlignment = TextAnchor.UpperCenter;
            controlsLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            controlsLayout.constraintCount = controlsColumnCount;

            RectTransform rtCard = CreateRect("GoContentCard", rtRoot);
            Stretch(rtCard, new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2(sidePadding, bottomSafeMargin), new Vector2(-sidePadding, -contentTopOffset));
            Image cardImage = rtCard.gameObject.AddComponent<Image>();
            cardImage.color = new Color(0.47f, 0.57f, 0.71f, 0.72f);
            cardImage.raycastTarget = false;

            RectTransform rtContentViewport = CreateRect(CONTENT_VIEWPORT_NAME, rtCard);
            Stretch(rtContentViewport, new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2(18.0f, 18.0f), new Vector2(-18.0f, -18.0f));

            return new SampleTestLayout(rtRoot, rtControlsWrap, rtContentViewport);
        }

        public static void ReparentToContent(RectTransform rtTarget, RectTransform rtContentViewport, Vector2 offsetMin, Vector2 offsetMax)
        {
            if(rtTarget == null || rtContentViewport == null)
                return;

            rtTarget.SetParent(rtContentViewport, false);
            Stretch(rtTarget, new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f), offsetMin, offsetMax);
            rtTarget.localScale = Vector3.one;
            rtTarget.anchoredPosition3D = Vector3.zero;
        }

        public static Button CreateActionButton(Transform parent, string buttonName, string labelText, UnityAction onClick)
        {
            RectTransform rtButton = CreateRect(buttonName, parent);
            rtButton.sizeDelta = new Vector2(0.0f, 0.0f);

            Image buttonImage = rtButton.gameObject.AddComponent<Image>();
            buttonImage.color = new Color(0.13f, 0.36f, 0.72f, 1.0f);
            Outline buttonOutline = rtButton.gameObject.AddComponent<Outline>();
            buttonOutline.effectColor = new Color(0.0f, 0.0f, 0.0f, 0.28f);
            buttonOutline.effectDistance = new Vector2(0.0f, -2.0f);

            Button button = rtButton.gameObject.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            if(onClick != null)
                button.onClick.AddListener(onClick);

            LayoutElement layoutElement = rtButton.gameObject.AddComponent<LayoutElement>();
            layoutElement.minWidth = 110.0f;
            layoutElement.preferredWidth = 150.0f;
            layoutElement.flexibleWidth = 1.0f;
            layoutElement.minHeight = 46.0f;
            layoutElement.preferredHeight = 58.0f;
            layoutElement.flexibleHeight = 0.0f;

            GameObject goText = CreateText(rtButton, "TxtLabel", labelText, 24, FontStyle.Bold, Color.white);
            RectTransform rtText = goText.GetComponent<RectTransform>();
            Stretch(rtText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        public static void DisableObjectsByName(Transform root, params string[] objectNames)
        {
            if(root == null || objectNames == null)
                return;

            for(int i = 0; i < objectNames.Length; i++)
            {
                string objectName = objectNames[i];
                if(string.IsNullOrWhiteSpace(objectName))
                    continue;

                Transform[] children = root.GetComponentsInChildren<Transform>(true);
                for(int j = 0; j < children.Length; j++)
                {
                    Transform child = children[j];
                    if(child == null || child.name != objectName)
                        continue;

                    child.gameObject.SetActive(false);
                }
            }
        }

        public static void DisableLegacyDirectChildren(Transform root, params Transform[] keepRoots)
        {
            if(root == null)
                return;

            Transform trTestRoot = root.Find(TEST_ROOT_NAME);
            int childCount = root.childCount;
            for(int i = 0; i < childCount; i++)
            {
                Transform child = root.GetChild(i);
                if(child == null || child == trTestRoot)
                    continue;

                if(ShouldKeepChild(child, keepRoots))
                    continue;

                child.gameObject.SetActive(false);
            }
        }

        //============================================================
        // Utilities
        //============================================================
        private static void EnsureRootStretch(Transform root)
        {
            if(root is not RectTransform rtRoot)
                return;

            RectTransform rtParent = rtRoot.parent as RectTransform;
            if(rtParent == null)
                return;

            rtRoot.anchorMin = Vector2.zero;
            rtRoot.anchorMax = Vector2.one;
            rtRoot.pivot = new Vector2(0.5f, 0.5f);
            rtRoot.anchoredPosition = Vector2.zero;
            rtRoot.sizeDelta = Vector2.zero;
            rtRoot.offsetMin = Vector2.zero;
            rtRoot.offsetMax = Vector2.zero;
            rtRoot.localScale = Vector3.one;
        }

        private static SampleTestLayout ResolveExistingLayout(Transform trRoot)
        {
            RectTransform rtRoot = trRoot as RectTransform;
            RectTransform rtControlsWrap = trRoot.Find("GoControls/" + CONTROLS_WRAP_NAME) as RectTransform;
            RectTransform rtContentViewport = trRoot.Find("GoContentCard/" + CONTENT_VIEWPORT_NAME) as RectTransform;
            return new SampleTestLayout(rtRoot, rtControlsWrap, rtContentViewport);
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject go = new(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static void Stretch(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            if(rt == null)
                return;

            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }

        private static GameObject CreateText(Transform parent, string name, string textValue, int fontSize, FontStyle fontStyle, Color textColor)
        {
            RectTransform rt = CreateRect(name, parent);
            Text txt = rt.gameObject.AddComponent<Text>();
            txt.text = textValue;
            txt.font = Resources.GetBuiltinResource<Font>(TITLE_FONT_PATH);
            txt.fontSize = fontSize;
            txt.fontStyle = fontStyle;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = textColor;
            txt.raycastTarget = false;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Truncate;
            return rt.gameObject;
        }

        private static float ResolveRectWidth(RectTransform rt, float fallback)
        {
            if(rt == null)
                return fallback;

            float width = rt.rect.width;
            if(width > 0.0f)
                return width;

            return Screen.width > 0 ? Screen.width : fallback;
        }

        private static float ResolveRectHeight(RectTransform rt, float fallback)
        {
            if(rt == null)
                return fallback;

            float height = rt.rect.height;
            if(height > 0.0f)
                return height;

            return Screen.height > 0 ? Screen.height : fallback;
        }

        private static int ResolveControlsColumnCount(float controlsInnerWidth)
        {
            if(controlsInnerWidth < 420.0f)
                return 2;

            if(controlsInnerWidth < 700.0f)
                return 3;

            return 4;
        }

        private static float ResolveControlsCellWidth(float controlsInnerWidth, int columnCount, float spacingX)
        {
            int safeColumnCount = Mathf.Max(1, columnCount);
            float totalSpacing = spacingX * (safeColumnCount - 1);
            float availableWidth = Mathf.Max(120.0f, controlsInnerWidth - totalSpacing);
            float rawCellWidth = availableWidth / safeColumnCount;
            return Mathf.Clamp(rawCellWidth, 110.0f, 180.0f);
        }

        private static bool ShouldKeepChild(Transform child, Transform[] keepRoots)
        {
            if(child == null || keepRoots == null)
                return false;

            for(int i = 0; i < keepRoots.Length; i++)
            {
                Transform keepRoot = keepRoots[i];
                if(keepRoot == null)
                    continue;

                if(child == keepRoot || keepRoot.IsChildOf(child))
                    return true;
            }

            return false;
        }

        private static void EnsureCanvas(Transform root)
        {
            if(root == null)
                return;

            Canvas canvas = root.GetComponent<Canvas>();
            if(canvas == null)
                canvas = root.GetComponentInParent<Canvas>();

            if(canvas == null)
                canvas = root.GetComponentInChildren<Canvas>(true);

            if(canvas == null)
                return;

            Transform trCanvas = canvas.transform;
            CanvasScaler scaler = trCanvas.GetComponent<CanvasScaler>();
            if(scaler == null)
                scaler = trCanvas.gameObject.AddComponent<CanvasScaler>();

            GraphicRaycaster raycaster = trCanvas.GetComponent<GraphicRaycaster>();
            if(raycaster == null)
                trCanvas.gameObject.AddComponent<GraphicRaycaster>();

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(720.0f, 1280.0f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }
    }
}
