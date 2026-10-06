using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace UnityTools.Samples.Features.Tests
{
    public class UiFeatureDemoTests
    {
        //============================================================
        // Logic
        //============================================================
        [UnityTest]
        public IEnumerator ButtonsExerciseListTransitionAndPopupStack()
        {
            yield return SceneManager.LoadSceneAsync("UiFeatureDemo", LoadSceneMode.Additive);
            Scene scene = SceneManager.GetSceneByName("UiFeatureDemo");
            UiFeatureDemo demo = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.TryGetComponent(out UiFeatureDemo found))
                    demo = found;
            }

            Assert.That(demo, Is.Not.Null);
            Assert.That(demo.CreatedItemCnt, Is.LessThan(60));
            try
            {
                Button[] buttons = demo.GetComponentsInChildren<Button>(true);
                Click(buttons, "JUMP TO MIDDLE");
                ScrollRect scroll = demo.GetComponentInChildren<ScrollRect>();
                float prevPos = scroll.content.anchoredPosition.y;
                Click(buttons, "RESIZE ROW 1");
                Assert.That(scroll.content.anchoredPosition.y, Is.EqualTo(prevPos + 220.0f).Within(0.1f));
                Click(buttons, "RESIZE ROW 1");
                Assert.That(scroll.content.anchoredPosition.y, Is.EqualTo(prevPos).Within(0.1f));

                Click(buttons, "SHOW");
                Assert.That(demo.IsTransitioning, Is.True);
                Click(buttons, "CANCEL");
                yield return null;
                yield return null;
                Assert.That(demo.IsTransitioning, Is.False);
                Assert.That(HasText(demo, "Transition result: Cancelled"), Is.True);
                Click(buttons, "HIDE");
                yield return new WaitForSecondsRealtime(1.7f);
                Assert.That(demo.IsTransitioning, Is.False);
                Assert.That(HasText(demo, "Transition result: Completed"), Is.True);

                Click(buttons, "OPEN BOTH");
                Assert.That(demo.PopupCnt, Is.EqualTo(2));
                Click(buttons, "CLOSE LOWER");
                Assert.That(demo.PopupCnt, Is.EqualTo(1));
                Click(buttons, "CLOSE LOWER");
                Assert.That(demo.PopupCnt, Is.EqualTo(1));
                Click(buttons, "BACK / CLOSE TOP");
                Assert.That(demo.PopupCnt, Is.Zero);

                demo.gameObject.SetActive(false);
                demo.gameObject.SetActive(true);
                Click(buttons, "OPEN BOTH");
                Assert.That(demo.PopupCnt, Is.EqualTo(2));
                Click(buttons, "CLOSE LOWER");
                Assert.That(demo.PopupCnt, Is.EqualTo(1));
            }
            finally
            {
                demo.gameObject.SetActive(false);
            }

            yield return SceneManager.UnloadSceneAsync(scene);
        }

        [UnityTest]
        public IEnumerator PointerRaycastsReachControlsAcrossScreenSizes()
        {
            yield return SceneManager.LoadSceneAsync("UiFeatureDemo", LoadSceneMode.Additive);
            Scene scene = SceneManager.GetSceneByName("UiFeatureDemo");
            UiFeatureDemo demo = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.TryGetComponent(out UiFeatureDemo found))
                    demo = found;
            }

            Assert.That(demo, Is.Not.Null);
            Canvas canvas = demo.GetComponent<Canvas>();
            Camera camera = new GameObject("FeatureInputCamera").AddComponent<Camera>();
            camera.transform.position = new Vector3(0.0f, 0.0f, -10.0f);
            camera.orthographic = true;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 5.0f;
            Button[] buttons = demo.GetComponentsInChildren<Button>(true);
            Vector2Int[] sizes = { new(1280, 720), new(1920, 1080), new(720, 1280), new(2560, 1080) };
            foreach (Vector2Int size in sizes)
            {
                RenderTexture target = new RenderTexture(size.x, size.y, 24);
                camera.targetTexture = target;
                Canvas.ForceUpdateCanvases();
                yield return null;
                PointerClick(buttons, "OPEN BOTH", camera, size);
                Assert.That(demo.PopupCnt, Is.EqualTo(2));
                PointerClick(buttons, "CLOSE LOWER", camera, size);
                Assert.That(demo.PopupCnt, Is.EqualTo(1));
                PointerClick(buttons, "BACK / CLOSE TOP", camera, size);
                Assert.That(demo.PopupCnt, Is.Zero);
                PointerClick(buttons, "JUMP TO MIDDLE", camera, size);
                PointerClick(buttons, "RESIZE ROW 1", camera, size);
                PointerClick(buttons, "RESIZE ROW 1", camera, size);
                PointerClick(buttons, "SHOW", camera, size);
                PointerClick(buttons, "CANCEL", camera, size);
                Assert.That(demo.IsTransitioning, Is.False);
                camera.targetTexture = null;
                UnityEngine.Object.Destroy(target);
            }

            UnityEngine.Object.Destroy(camera.gameObject);
            yield return SceneManager.UnloadSceneAsync(scene);
        }

        //============================================================
        // Utilities
        //============================================================
        private static void Click(Button[] buttons, string name)
        {
            foreach (Button btn in buttons)
            {
                if (btn.name != name)
                    continue;

                btn.onClick.Invoke();
                return;
            }

            Assert.Fail("샘플 버튼이 없습니다: " + name);
        }

        private static void PointerClick(Button[] buttons, string name, Camera camera, Vector2Int size)
        {
            Button button = System.Array.Find(buttons, item => item.name == name);
            Assert.That(button, Is.Not.Null);
            Vector2 pos = RectTransformUtility.WorldToScreenPoint(camera, button.transform.TransformPoint(((RectTransform)button.transform).rect.center));
            Assert.That(pos.x, Is.InRange(0.0f, (float)size.x));
            Assert.That(pos.y, Is.InRange(0.0f, (float)size.y));
            PointerEventData pointer = new PointerEventData(EventSystem.current) { position = pos, button = PointerEventData.InputButton.Left };
            List<RaycastResult> hits = new();
            EventSystem.current.RaycastAll(pointer, hits);
            Assert.That(hits.Count, Is.GreaterThan(0), name);
            GameObject handler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);
            Assert.That(handler, Is.SameAs(button.gameObject), name);
            ExecuteEvents.Execute(handler, pointer, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(handler, pointer, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(handler, pointer, ExecuteEvents.pointerClickHandler);
        }

        private static bool HasText(UiFeatureDemo demo, string value)
        {
            foreach (Text txt in demo.GetComponentsInChildren<Text>(true))
            {
                if (txt.text == value)
                    return true;
            }

            return false;
        }
    }
}
