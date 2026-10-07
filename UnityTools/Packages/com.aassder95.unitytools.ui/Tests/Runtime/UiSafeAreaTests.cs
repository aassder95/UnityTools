using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace UnityTools.Ui.Tests
{
    public class UiSafeAreaTests
    {
        //============================================================
        // Logic
        //============================================================
        [TestCase(1080.0f, 2400.0f, 0.0f, 100.0f, 1080.0f, 2180.0f)]
        [TestCase(2400.0f, 1080.0f, 120.0f, 0.0f, 2220.0f, 1080.0f)]
        [TestCase(1440.0f, 900.0f, 0.0f, 0.0f, 1440.0f, 900.0f)]
        public void InsetsBecomeNormalizedAnchors(float width, float height, float x, float y, float safeWidth, float safeHeight)
        {
            GameObject root = new GameObject("Canvas", typeof(RectTransform));
            root.SetActive(false);
            GameObject child = new GameObject("SafeArea", typeof(RectTransform));
            child.transform.SetParent(root.transform, false);
            RectTransform rt = (RectTransform)child.transform;
            UiSafeArea area = root.AddComponent<UiSafeArea>();
            typeof(UiSafeArea).GetField("_rtSafeArea", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(area, rt);
            try
            {
                Assert.That(area.TryApplyViewport(new Vector2(width, height), new Rect(x, y, safeWidth, safeHeight)), Is.True);
                Assert.That(rt.anchorMin.x, Is.EqualTo(x / width).Within(0.001f));
                Assert.That(rt.anchorMin.y, Is.EqualTo(y / height).Within(0.001f));
                Assert.That(rt.anchorMax.x, Is.EqualTo((x + safeWidth) / width).Within(0.001f));
                Assert.That(rt.anchorMax.y, Is.EqualTo((y + safeHeight) / height).Within(0.001f));
                Vector2 previous = rt.anchorMin;
                Assert.That(area.TryApplyViewport(Vector2.zero, new Rect()), Is.False);
                Assert.That(area.TryApplyViewport(new Vector2(float.NaN, height), new Rect(x, y, safeWidth, safeHeight)), Is.False);
                Assert.That(area.TryApplyViewport(new Vector2(width, height), new Rect(-1.0f, y, safeWidth, safeHeight)), Is.False);
                Assert.That(rt.anchorMin, Is.EqualTo(previous));
                Assert.That(area.TryApplyViewport(new Vector2(height, width), new Rect(0.0f, 0.0f, height, width)), Is.True);
                Assert.That(rt.anchorMin, Is.EqualTo(Vector2.zero));
                Assert.That(rt.anchorMax, Is.EqualTo(Vector2.one));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
