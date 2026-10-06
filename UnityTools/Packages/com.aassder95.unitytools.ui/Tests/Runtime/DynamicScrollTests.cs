using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityTools.Ui;
using UnityTools.Ui.Tests.Lifecycle;

namespace UnityTools.Ui.Tests.UiFramework
{
    public class DynamicScrollTests
    {
        //============================================================
        // Fields
        //============================================================
        private GameObject _goTestRoot;

        //============================================================
        // Init/Register
        //============================================================
        [SetUp]
        public void SetUp()
        {
            _goTestRoot = new GameObject("DynamicScrollTests");
        }

        [TearDown]
        public void TearDown()
        {
            if (_goTestRoot != null)
                Object.DestroyImmediate(_goTestRoot);
        }

        //============================================================
        // Logic
        //============================================================
        [Test]
        public void RefreshUpdatesVisibleTargetsOnly()
        {
            ScrollTestView scrollView = CreateScrollView(20);
            ScrollTestItem firstItem = FindActiveItem(scrollView, 0);
            ScrollTestItem secondItem = FindActiveItem(scrollView, 1);
            int firstInitialUpdateCnt = firstItem.UpdateCnt;
            int secondInitialUpdateCnt = secondItem.UpdateCnt;

            scrollView.RefreshItem(0);

            Assert.That(firstItem.UpdateCnt, Is.EqualTo(firstInitialUpdateCnt + 1));
            Assert.That(secondItem.UpdateCnt, Is.EqualTo(secondInitialUpdateCnt));

            scrollView.RefreshRange(0, 2);

            Assert.That(firstItem.UpdateCnt, Is.EqualTo(firstInitialUpdateCnt + 2));
            Assert.That(secondItem.UpdateCnt, Is.EqualTo(secondInitialUpdateCnt + 1));
        }

        [Test]
        public void ViewportMutationsPreserveAnchor()
        {
            ScrollTestView scrollView = CreateScrollView(20);
            scrollView.ScrollTo(5, true);
            Vector2 initialPos = scrollView.ContentPos;

            scrollView.InsertItems(0, 2);

            Assert.That(scrollView.ItemCnt, Is.EqualTo(22));
            Assert.That(scrollView.ContentPos.y, Is.EqualTo(initialPos.y + 200.0f).Within(0.01f));

            scrollView.RemoveItems(0, 2);

            Assert.That(scrollView.ItemCnt, Is.EqualTo(20));
            Assert.That(scrollView.ContentPos.y, Is.EqualTo(initialPos.y).Within(0.01f));
        }

        [Test]
        public void ItemCountUpdateControlsScrollPreservation()
        {
            ScrollTestView scrollView = CreateScrollView(20);
            scrollView.ScrollTo(5, true);
            Vector2 initialPos = scrollView.ContentPos;

            scrollView.UpdateItemCnt(30);

            Assert.That(scrollView.ItemCnt, Is.EqualTo(30));
            Assert.That(scrollView.ContentPos.y, Is.EqualTo(initialPos.y).Within(0.01f));

            scrollView.UpdateItemCnt(30, false);

            Assert.That(scrollView.ContentPos.y, Is.EqualTo(0.0f).Within(0.01f));
        }

        [Test]
        public void ScrollToSupportsVerticalAlignments()
        {
            ScrollTestView scrollView = CreateScrollView(20);

            scrollView.ScrollTo(5, true, alignment: EDynamicScrollAlignment.Start);
            Assert.That(scrollView.ContentPos.y, Is.EqualTo(500.0f).Within(0.01f));

            scrollView.ScrollTo(5, true, alignment: EDynamicScrollAlignment.Center);
            Assert.That(scrollView.ContentPos.y, Is.EqualTo(390.0f).Within(0.01f));

            scrollView.ScrollTo(5, true, alignment: EDynamicScrollAlignment.End);
            Assert.That(scrollView.ContentPos.y, Is.EqualTo(280.0f).Within(0.01f));
        }

        [Test]
        public void ScrollToSupportsHorizontalCenterAlignment()
        {
            ScrollTestView scrollView = CreateScrollView(20, EDynamicScrollAxisType.Horizontal);

            scrollView.ScrollTo(5, true, alignment: EDynamicScrollAlignment.Center);

            Assert.That(scrollView.ContentPos.x, Is.EqualTo(-390.0f).Within(0.01f));
        }

        [Test]
        public void RebuildReturnsVisibleItemsThroughLifecycle()
        {
            ScrollTestView scrollView = CreateScrollView(20);
            ScrollTestItem[] items = scrollView.GetComponentsInChildren<ScrollTestItem>(true);
            int getCnt = 0;
            for (int i = 0; i < items.Length; i++)
            {
                getCnt += items[i].GetCnt;
            }

            scrollView.UpdateItemCnt(0);

            int returnCnt = 0;
            for (int i = 0; i < items.Length; i++)
            {
                returnCnt += items[i].ReturnCnt;
            }

            Assert.That(getCnt, Is.GreaterThan(0));
            Assert.That(returnCnt, Is.EqualTo(getCnt));
        }

        [Test]
        public void ScrollingBothDirectionsReusesVisibleItems()
        {
            ScrollTestView scrollView = CreateScrollView(30);
            ScrollTestItem[] items = scrollView.GetComponentsInChildren<ScrollTestItem>(true);
            int initialItemCnt = items.Length;

            scrollView.ScrollTo(5, true);
            Assert.That(FindActiveItem(scrollView, 5), Is.Not.Null);
            scrollView.ScrollTo(8, true);
            Assert.That(FindActiveItem(scrollView, 8), Is.Not.Null);
            scrollView.ScrollTo(3, true);
            ScrollTestItem item = FindActiveItem(scrollView, 3);
            Assert.That(item, Is.Not.Null);
            int prevUpdateCnt = item.UpdateCnt;
            scrollView.RefreshItem(3);
            Assert.That(item.UpdateCnt, Is.EqualTo(prevUpdateCnt + 1));

            scrollView.ScrollTo(29, true);
            Assert.That(FindActiveItem(scrollView, 29), Is.Not.Null);
            scrollView.ScrollTo(0, true);
            Assert.That(FindActiveItem(scrollView, 0), Is.Not.Null);
            Assert.That(scrollView.GetComponentsInChildren<ScrollTestItem>(true).Length, Is.EqualTo(initialItemCnt));
        }

        [Test]
        public void VariableHeightsControlPositionsAndScrollAlignment()
        {
            ScrollTestView view = CreateScrollView(20);
            float[] heights = { 50.0f, 150.0f, 80.0f, 200.0f, 100.0f, 300.0f, 60.0f, 400.0f };
            Assert.That(view.TryInitView(heights), Is.True);
            ScrollTestItem first = FindActiveItem(view, 0);
            ScrollTestItem second = FindActiveItem(view, 1);
            Assert.That((second.transform as RectTransform).rect.height, Is.EqualTo(150.0f));
            Assert.That((first.transform as RectTransform).anchoredPosition.y - (second.transform as RectTransform).anchoredPosition.y, Is.EqualTo(100.0f));
            view.ScrollTo(3, true);
            Assert.That(view.ContentPos.y, Is.EqualTo(280.0f));
            Assert.That(FindActiveItem(view, 3), Is.Not.Null);
            view.ScrollTo(3, true, alignment: EDynamicScrollAlignment.Center);
            Assert.That(view.ContentPos.y, Is.EqualTo(220.0f));
            view.ScrollTo(3, true, alignment: EDynamicScrollAlignment.End);
            Assert.That(view.ContentPos.y, Is.EqualTo(160.0f));
        }

        [Test]
        public void VariableHeightMutationsPreserveAnchorAndUseInputSnapshot()
        {
            ScrollTestView view = CreateScrollView(20);
            float[] heights = { 100.0f, 150.0f, 100.0f, 100.0f, 100.0f, 100.0f, 100.0f, 100.0f, 100.0f, 100.0f };
            Assert.That(view.TryInitView(heights), Is.True);
            heights[0] = 999.0f;
            view.ScrollTo(3, true);
            Assert.That(view.ContentPos.y, Is.EqualTo(350.0f));
            Assert.That(view.TrySetItemHeight(0, 200.0f), Is.True);
            Assert.That(view.ContentPos.y, Is.EqualTo(450.0f));
            Assert.That(view.TryInsertItems(0, new float[] { 50.0f, 80.0f }), Is.True);
            Assert.That(view.ContentPos.y, Is.EqualTo(580.0f));
            Assert.That(FindActiveItem(view, 5), Is.Not.Null);
            view.RemoveItems(0, 2);
            Assert.That(view.ContentPos.y, Is.EqualTo(450.0f));
            Assert.That(FindActiveItem(view, 3), Is.Not.Null);
        }

        [Test]
        public void TallVariableItemStaysVisibleInsideItsHeight()
        {
            ScrollTestView view = CreateScrollView(20);
            Assert.That(view.TryInitView(new float[] { 900.0f, 40.0f, 50.0f, 60.0f }), Is.True);
            view.SetContentPos(new Vector2(0.0f, 500.0f));
            Assert.That(FindActiveItem(view, 0), Is.Not.Null);
            Assert.That((FindActiveItem(view, 0).transform as RectTransform).rect.height, Is.EqualTo(900.0f));
            view.ScrollTo(0, true, alignment: EDynamicScrollAlignment.End);
            Assert.That(view.ContentPos.y, Is.EqualTo(580.0f));
        }

        [Test]
        public void VariableScrollingReusesItemsAcrossBothDirections()
        {
            ScrollTestView view = CreateScrollView(0);
            float[] heights = new float[100];
            for (int idx = 0; idx < heights.Length; ++idx)
            {
                heights[idx] = idx % 2 == 0 ? 40.0f : 80.0f;
            }

            Assert.That(view.TryInitView(heights), Is.True);
            int itemCnt = view.GetComponentsInChildren<ScrollTestItem>(true).Length;
            view.ScrollTo(50, true);
            Assert.That(FindActiveItem(view, 50), Is.Not.Null);
            view.ScrollTo(10, true);
            Assert.That(FindActiveItem(view, 10), Is.Not.Null);
            view.ScrollTo(99, true);
            Assert.That(FindActiveItem(view, 99), Is.Not.Null);
            view.ScrollTo(0, true);
            Assert.That(view.GetComponentsInChildren<ScrollTestItem>(true).Length, Is.EqualTo(itemCnt));
        }

        [Test]
        public void VariableHeightRejectionsLeaveExistingLayoutUnchanged()
        {
            ScrollTestView view = CreateScrollView(20);
            Assert.That(view.TryInitView(new float[] { 100.0f, 200.0f, 300.0f, 400.0f }), Is.True);
            view.ScrollTo(1, true);
            Vector2 pos = view.ContentPos;
            Assert.That(view.TrySetItemHeight(0, float.NaN), Is.False);
            Assert.That(view.TrySetItemHeight(0, 0.0f), Is.False);
            Assert.That(view.TryInsertItems(0, new float[] { float.PositiveInfinity }), Is.False);
            Assert.That(view.TryInitView(new float[] { float.MaxValue, float.MaxValue }), Is.False);
            Assert.That(view.ItemCnt, Is.EqualTo(4));
            Assert.That(view.ContentPos, Is.EqualTo(pos));
            Assert.That((FindActiveItem(view, 1).transform as RectTransform).rect.height, Is.EqualTo(200.0f));
        }

        [Test]
        public void EmptyVariableViewSupportsInsertionRemovalAndFixedReinitialization()
        {
            ScrollTestView view = CreateScrollView(0);
            Assert.That(view.TryInitView(new float[0]), Is.True);
            view.UpdateItemCnt(0, false);
            Assert.That(view.TryInsertItems(0, new float[] { 200.0f, 300.0f }), Is.True);
            Assert.That(view.ItemCnt, Is.EqualTo(2));
            view.RemoveItems(0, 2);
            Assert.That(view.ItemCnt, Is.EqualTo(0));
            view.InitView(20);
            view.ScrollTo(5, true);
            Assert.That(view.ContentPos.y, Is.EqualTo(500.0f));
            Assert.That((FindActiveItem(view, 5).transform as RectTransform).rect.height, Is.EqualTo(100.0f));
        }

        [Test]
        public void HorizontalViewRejectsVariableHeights()
        {
            ScrollTestView view = CreateScrollView(20, EDynamicScrollAxisType.Horizontal);
            Assert.That(view.TryInitView(new float[] { 100.0f, 200.0f }), Is.False);
            view.ScrollTo(5, true);
            Assert.That(view.ContentPos.x, Is.EqualTo(-500.0f));
            Assert.That(view.ItemCnt, Is.EqualTo(20));
        }

        [Test]
        public void FixedReinitializationWithSameCountResetsContentHeight()
        {
            ScrollTestView view = CreateScrollView(20);
            float[] heights = new float[20];
            for (int idx = 0; idx < heights.Length; ++idx)
            {
                heights[idx] = 200.0f;
            }

            Assert.That(view.TryInitView(heights), Is.True);
            view.InitView(20);
            view.ScrollTo(19, true);
            Assert.That(view.ContentPos.y, Is.EqualTo(1680.0f));
            Assert.That((FindActiveItem(view, 19).transform as RectTransform).rect.height, Is.EqualTo(100.0f));
            Assert.That(view.ContentHeight, Is.EqualTo(2000.0f));
        }

        [Test]
        public void VariableHeightsIncludeSpacingAndPaddingAndSurviveRelease()
        {
            ScrollTestView view = CreateScrollView(0);
            view.ReleaseView();
            SetScrollField(view, "_spacing", new Vector2(0.0f, 10.0f));
            SetScrollField(view, "_padding", new RectOffset(0, 0, 20, 30));
            Assert.That(view.TryInitView(new float[] { 100.0f, 200.0f, 300.0f, 400.0f }), Is.True);
            Assert.That(view.ContentHeight, Is.EqualTo(1080.0f));
            view.ScrollTo(2, true);
            Assert.That(view.ContentPos.y, Is.EqualTo(340.0f));
            view.InsertItems(0, 1);
            Assert.That(view.ContentPos.y, Is.EqualTo(450.0f));
            view.RemoveItems(0, 1);
            Assert.That(view.ContentPos.y, Is.EqualTo(340.0f));
            view.ReleaseView();
            Assert.That(view.TryInitView(new float[] { 50.0f, 80.0f }), Is.True);
            Assert.That(view.ContentHeight, Is.EqualTo(190.0f));
            Assert.That(view.ContentPos.y, Is.EqualTo(0.0f));
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OnItemUpdated(ScrollTestItem item)
        {
            item.MarkUpdated();
        }

        //============================================================
        // Utilities
        //============================================================
        private ScrollTestView CreateScrollView(int itemCnt, EDynamicScrollAxisType axisType = EDynamicScrollAxisType.Vertical)
        {
            GameObject goScroll = new("TestScroll", typeof(RectTransform));
            goScroll.SetActive(false);
            goScroll.transform.SetParent(_goTestRoot.transform);
            RectTransform rtScroll = goScroll.transform as RectTransform;
            rtScroll.sizeDelta = new Vector2(320.0f, 320.0f);

            GameObject goViewport = new("Viewport", typeof(RectTransform));
            goViewport.transform.SetParent(goScroll.transform, false);
            RectTransform rtViewport = goViewport.transform as RectTransform;
            rtViewport.sizeDelta = new Vector2(320.0f, 320.0f);

            GameObject goContent = new("Content", typeof(RectTransform));
            goContent.transform.SetParent(goViewport.transform, false);
            RectTransform rtContent = goContent.transform as RectTransform;

            ScrollRect scrollRect = goScroll.AddComponent<ScrollRect>();
            scrollRect.viewport = rtViewport;
            scrollRect.content = rtContent;
            ScrollTestView scrollView = goScroll.AddComponent<ScrollTestView>();
            ScrollTestItem itemPrefab = CreateItemPrefab();
            SetScrollField(scrollView, "_item", itemPrefab);
            SetScrollField(scrollView, "_padding", new RectOffset());
            SetScrollField(scrollView, "_axisType", axisType);
            scrollView.OnItemUpdated += OnItemUpdated;

            goScroll.SetActive(true);
            scrollView.InitView(itemCnt);
            return scrollView;
        }

        private ScrollTestItem CreateItemPrefab()
        {
            GameObject goItem = new("ScrollItemPrefab", typeof(RectTransform));
            goItem.SetActive(false);
            goItem.transform.SetParent(_goTestRoot.transform);
            RectTransform rtItem = goItem.transform as RectTransform;
            rtItem.sizeDelta = new Vector2(100.0f, 100.0f);
            return goItem.AddComponent<ScrollTestItem>();
        }

        private static ScrollTestItem FindActiveItem(ScrollTestView scrollView, int itemIdx)
        {
            ScrollTestItem[] items = scrollView.GetComponentsInChildren<ScrollTestItem>(true);
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].gameObject.activeSelf && items[i].Idx == itemIdx)
                    return items[i];
            }

            return null;
        }

        private static void SetScrollField<TValue>(ScrollTestView scrollView, string fieldName, TValue value)
        {
            FieldInfo field = typeof(DynamicScrollView<ScrollTestItem>).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(scrollView, value);
        }
    }
}
