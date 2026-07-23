using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UnityTools.Samples.Util
{
    public static class SampleTestUiBuilder
    {
        //============================================================
        // Logic
        //============================================================
        public static SampleTestLayout Build(Transform parent, string title, string subtitle, int actionCnt)
        {
            RectTransform rtParent = parent as RectTransform;
            EnsureRootStretch(rtParent);
            RectTransform rtRoot = CreateRect("GoSampleTestRoot", parent);
            Stretch(rtRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image bgImage = rtRoot.gameObject.AddComponent<Image>();
            bgImage.color = new Color(0.07f, 0.16f, 0.33f, 1.0f);
            bgImage.raycastTarget = true;

            float rootWidth = ResolveRectWidth(rtRoot, 720.0f);
            float rootHeight = ResolveRectHeight(rtRoot, 1280.0f);

            float sidePadding = Mathf.Clamp(rootWidth * 0.033f, 16.0f, 28.0f);
            float topPadding = Mathf.Clamp(rootHeight * 0.016f, 12.0f, 24.0f);
            float sectionSpacing = Mathf.Clamp(rootHeight * 0.012f, 8.0f, 16.0f);
            float headerHeight = Mathf.Clamp(rootHeight * 0.13f, 120.0f, 188.0f);
            float controlsWrapInset = 8.0f;
            int controlsGridPadding = 8;
            Vector2 controlsSpacing = new(10.0f, 8.0f);
            float controlsCellHeight = Mathf.Clamp(rootHeight * 0.045f, 46.0f, 58.0f);
            float controlsInnerWidth = Mathf.Max(1.0f, rootWidth - (sidePadding * 2.0f) - (controlsWrapInset * 2.0f) - (controlsGridPadding * 2.0f));
            int controlsColumnCnt = ResolveControlsColumnCnt(controlsInnerWidth, controlsSpacing.x);
            int controlsRowCnt = Mathf.Max(1, Mathf.CeilToInt((float)actionCnt / controlsColumnCnt));
            float controlsHeight = Mathf.Max(96.0f, (controlsWrapInset * 2.0f) + (controlsGridPadding * 2.0f) + (controlsCellHeight * controlsRowCnt) + (controlsSpacing.y * (controlsRowCnt - 1)));
            float bottomSafeMargin = 128.0f;

            float headerBottomOffset = topPadding + headerHeight;
            float controlsTopOffset = headerBottomOffset + sectionSpacing;
            float controlsBottomOffset = controlsTopOffset + controlsHeight;
            float contentTopOffset = controlsBottomOffset + sectionSpacing;

            RectTransform rtHeader = CreateRect("GoHeader", rtRoot);
            Stretch(rtHeader, new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f), new Vector2(sidePadding, -headerBottomOffset), new Vector2(-sidePadding, -topPadding));
            Image headerImage = rtHeader.gameObject.AddComponent<Image>();
            headerImage.color = new Color(0.03f, 0.1f, 0.23f, 0.96f);
            headerImage.raycastTarget = false;

            GameObject goTitle = CreateText(rtHeader, "TxtTitle", title, 42, FontStyle.Bold, Color.white);
            RectTransform rtTitle = goTitle.transform as RectTransform;
            Stretch(rtTitle, new Vector2(0.0f, 0.48f), new Vector2(1.0f, 1.0f), new Vector2(16.0f, 8.0f), new Vector2(-16.0f, -10.0f));

            GameObject goSubtitle = CreateText(rtHeader, "TxtSubtitle", subtitle, 22, FontStyle.Normal, new Color(0.78f, 0.86f, 0.97f, 1.0f));
            RectTransform rtSubtitle = goSubtitle.transform as RectTransform;
            Stretch(rtSubtitle, new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.52f), new Vector2(16.0f, 8.0f), new Vector2(-16.0f, -8.0f));

            RectTransform rtControls = CreateRect("GoControls", rtRoot);
            Stretch(rtControls, new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f), new Vector2(sidePadding, -controlsBottomOffset), new Vector2(-sidePadding, -controlsTopOffset));
            Image controlsImage = rtControls.gameObject.AddComponent<Image>();
            controlsImage.color = new Color(0.1f, 0.2f, 0.38f, 0.95f);
            controlsImage.raycastTarget = false;

            RectTransform rtControlsWrap = CreateRect("GoControlsWrap", rtControls);
            Stretch(rtControlsWrap, Vector2.zero, Vector2.one, new Vector2(controlsWrapInset, controlsWrapInset), new Vector2(-controlsWrapInset, -controlsWrapInset));
            GridLayoutGroup controlsLayout = rtControlsWrap.gameObject.AddComponent<GridLayoutGroup>();
            controlsLayout.padding = new RectOffset(controlsGridPadding, controlsGridPadding, controlsGridPadding, controlsGridPadding);
            controlsLayout.spacing = controlsSpacing;
            float controlsCellWidth = ResolveControlsCellWidth(controlsInnerWidth, controlsColumnCnt, controlsLayout.spacing.x);
            controlsLayout.cellSize = new Vector2(controlsCellWidth, controlsCellHeight);
            controlsLayout.childAlignment = TextAnchor.UpperCenter;
            controlsLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            controlsLayout.constraintCount = controlsColumnCnt;

            RectTransform rtCard = CreateRect("GoContentCard", rtRoot);
            Stretch(rtCard, new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2(sidePadding, bottomSafeMargin), new Vector2(-sidePadding, -contentTopOffset));
            Image cardImage = rtCard.gameObject.AddComponent<Image>();
            cardImage.color = new Color(0.47f, 0.57f, 0.71f, 0.72f);
            cardImage.raycastTarget = false;

            RectTransform rtContentViewport = CreateRect("GoContentViewport", rtCard);
            Stretch(rtContentViewport, new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2(18.0f, 18.0f), new Vector2(-18.0f, -18.0f));

            return new SampleTestLayout(rtRoot, rtControlsWrap, rtContentViewport);
        }

        public static void ReparentToContent(RectTransform rtTarget, RectTransform rtContentViewport, Vector2 offsetMin, Vector2 offsetMax)
        {
            rtTarget.SetParent(rtContentViewport, false);
            Stretch(rtTarget, new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f), offsetMin, offsetMax);
            rtTarget.localScale = Vector3.one;
            rtTarget.anchoredPosition3D = Vector3.zero;
        }

        public static void CreateActionButton(Transform parent, string buttonName, string labelText, UnityAction onClick)
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
            button.onClick.AddListener(onClick);

            LayoutElement layoutElement = rtButton.gameObject.AddComponent<LayoutElement>();
            layoutElement.minWidth = 110.0f;
            layoutElement.preferredWidth = 150.0f;
            layoutElement.flexibleWidth = 1.0f;
            layoutElement.minHeight = 46.0f;
            layoutElement.preferredHeight = 58.0f;
            layoutElement.flexibleHeight = 0.0f;

            GameObject goText = CreateText(rtButton, "TxtLabel", labelText, 24, FontStyle.Bold, Color.white);
            RectTransform rtText = goText.transform as RectTransform;
            Stretch(rtText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }

        //============================================================
        // Utilities
        //============================================================
        private static void EnsureRootStretch(RectTransform rtRoot)
        {
            rtRoot.anchorMin = Vector2.zero;
            rtRoot.anchorMax = Vector2.one;
            rtRoot.pivot = new Vector2(0.5f, 0.5f);
            rtRoot.anchoredPosition = Vector2.zero;
            rtRoot.sizeDelta = Vector2.zero;
            rtRoot.offsetMin = Vector2.zero;
            rtRoot.offsetMax = Vector2.zero;
            rtRoot.localScale = Vector3.one;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject go = new(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.transform as RectTransform;
        }

        private static void Stretch(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {

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
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
            float width = rt.rect.width;
            if(width > 0.0f)
                return width;

            return Screen.width > 0 ? Screen.width : fallback;
        }

        private static float ResolveRectHeight(RectTransform rt, float fallback)
        {
            float height = rt.rect.height;
            if(height > 0.0f)
                return height;

            return Screen.height > 0 ? Screen.height : fallback;
        }

        private static int ResolveControlsColumnCnt(float controlsInnerWidth, float spacingX)
        {
            int fitColumnCnt = Mathf.FloorToInt((controlsInnerWidth + spacingX) / (110.0f + spacingX));
            return Mathf.Clamp(fitColumnCnt, 1, 4);
        }

        private static float ResolveControlsCellWidth(float controlsInnerWidth, int columnCnt, float spacingX)
        {
            float totalSpacing = spacingX * (columnCnt - 1);
            float availableWidth = Mathf.Max(1.0f, controlsInnerWidth - totalSpacing);
            float rawCellWidth = availableWidth / columnCnt;
            return Mathf.Min(Mathf.Clamp(rawCellWidth, 110.0f, 180.0f), availableWidth);
        }
    }
}
