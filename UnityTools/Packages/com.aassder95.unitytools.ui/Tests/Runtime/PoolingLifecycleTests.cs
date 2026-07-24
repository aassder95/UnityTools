using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityTools.Util.Core.Pooling;
using UnityTools.Util.UiFramework;

namespace UnityTools.Util.Tests.Lifecycle
{
    public class PoolingLifecycleTests
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
            _goTestRoot = new GameObject("PoolingLifecycleTests");
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
        public void TryReturnRejectsDuplicateAndForeignObjects()
        {
            PoolingTestItem prefab = CreatePoolItem("PoolItemPrefab", _goTestRoot.transform);
            ObjectPool<PoolingTestItem> pool = ObjectPool<PoolingTestItem>.Create(0, prefab, _goTestRoot.transform);
            PoolingTestItem borrowedItem = pool.Get();

            Assert.That(pool.TryReturn(borrowedItem), Is.True);
            LogAssert.Expect(LogType.Error, new Regex(@"\[ObjectPool\.TryReturn\] 이미 풀에 들어 있는 객체를 중복 반환했습니다\."));
            Assert.That(pool.TryReturn(borrowedItem), Is.False);

            PoolingTestItem foreignItem = CreatePoolItem("ForeignItem", _goTestRoot.transform);
            LogAssert.Expect(LogType.Error, new Regex(@"\[ObjectPool\.TryReturn\] 다른 ObjectPool이 소유한 객체를 반환할 수 없습니다\."));
            Assert.That(pool.TryReturn(foreignItem), Is.False);
            pool.Clear();
        }

        [UnityTest]
        public IEnumerator ClearPreservesBorrowedOwnershipUntilReturn()
        {
            PoolingTestItem prefab = CreatePoolItem("PoolItemPrefab", _goTestRoot.transform);
            ObjectPool<PoolingTestItem> pool = ObjectPool<PoolingTestItem>.Create(1, prefab, _goTestRoot.transform);
            PoolingTestItem borrowedItem = pool.Get();

            pool.Clear();
            Assert.That(borrowedItem, Is.Not.Null);
            Assert.That(pool.TryReturn(borrowedItem), Is.True);
            pool.Clear();
            yield return null;

            Assert.That(borrowedItem == null, Is.True);
        }

        [UnityTest]
        public IEnumerator SpawnerCapsActiveObjectsAndReturnsThemWhenDisabled()
        {
            GameObject goSpawner = new("TestSpawner");
            goSpawner.SetActive(false);
            goSpawner.transform.SetParent(_goTestRoot.transform);
            PoolingTestItem prefab = CreatePoolItem("SpawnerItemPrefab", _goTestRoot.transform);
            PoolingTestSpawner spawner = goSpawner.AddComponent<PoolingTestSpawner>();
            SetSpawnerField(spawner, "_prefab", prefab);
            SetSpawnerField(spawner, "_initialSize", 0);
            SetSpawnerField(spawner, "_maxActiveCnt", 2);
            SetSpawnerField(spawner, "_intervalSec", 0.01f);

            goSpawner.SetActive(true);
            yield return new WaitForSeconds(0.08f);

            PoolingTestItem[] spawnedItems = goSpawner.GetComponentsInChildren<PoolingTestItem>(true);
            Assert.That(CountActive(spawnedItems), Is.EqualTo(2));

            goSpawner.SetActive(false);
            yield return null;

            Assert.That(CountActive(spawnedItems), Is.Zero);
        }

        [Test]
        public void DynamicScrollCanReleaseAndReinitializeWithSameItemCount()
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

            ScrollTestItem itemPrefab = CreateScrollItem("ScrollItemPrefab", _goTestRoot.transform);
            SetScrollField(scrollView, "_item", itemPrefab);
            SetScrollField(scrollView, "_padding", new RectOffset());

            goScroll.SetActive(true);
            scrollView.InitView(5);
            Assert.That(scrollView.IsInitialized, Is.True);
            Assert.That(scrollView.ItemCnt, Is.EqualTo(5));
            Assert.That(CountActive(goContent.GetComponentsInChildren<ScrollTestItem>(true)), Is.GreaterThan(0));

            scrollView.ReleaseView();
            Assert.That(scrollView.IsInitialized, Is.False);

            scrollView.InitView(5);
            Assert.That(scrollView.IsInitialized, Is.True);
            Assert.That(scrollView.ItemCnt, Is.EqualTo(5));
            Assert.That(CountActive(goContent.GetComponentsInChildren<ScrollTestItem>(true)), Is.GreaterThan(0));
            scrollView.ReleaseView();
        }

        //============================================================
        // Utilities
        //============================================================
        private static PoolingTestItem CreatePoolItem(string itemName, Transform parent)
        {
            GameObject goItem = new(itemName);
            goItem.transform.SetParent(parent);
            return goItem.AddComponent<PoolingTestItem>();
        }

        private static ScrollTestItem CreateScrollItem(string itemName, Transform parent)
        {
            GameObject goItem = new(itemName, typeof(RectTransform));
            goItem.SetActive(false);
            goItem.transform.SetParent(parent);
            RectTransform rtItem = goItem.transform as RectTransform;
            rtItem.sizeDelta = new Vector2(100.0f, 100.0f);
            return goItem.AddComponent<ScrollTestItem>();
        }

        private static void SetSpawnerField<TValue>(PoolingTestSpawner spawner, string fieldName, TValue value)
        {
            FieldInfo field = typeof(Spawner<PoolingTestItem>).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(spawner, value);
        }

        private static void SetScrollField<TValue>(ScrollTestView scrollView, string fieldName, TValue value)
        {
            FieldInfo field = typeof(DynamicScrollView<ScrollTestItem>).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(scrollView, value);
        }

        private static int CountActive<T>(T[] components) where T : Component
        {
            int activeCnt = 0;
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].gameObject.activeSelf)
                    activeCnt++;
            }

            return activeCnt;
        }
    }

}
