using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityTools.Util.UiFramework;
using UnityTools.Util.Tests.Lifecycle;

namespace UnityTools.Util.Tests.UiFramework
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
        public void RefreshItemAndRangeOnlyUpdateVisibleTargets()
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
        public void InsertAndRemoveBeforeViewportPreserveAnchor()
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
        public void UpdateItemCountCanPreserveOrResetScrollPosition()
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
        public void ScrollToSupportsVerticalStartCenterAndEndAlignment()
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
