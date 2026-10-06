using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
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
