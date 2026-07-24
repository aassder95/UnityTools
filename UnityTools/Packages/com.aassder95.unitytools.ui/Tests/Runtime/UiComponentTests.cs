using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityTools.Util.UiFramework;

namespace UnityTools.Util.Tests.UiFramework
{
    public class UiComponentTests
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
            _goTestRoot = new GameObject("UiComponentTests");
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
        public void CanvasTransitionAppliesImmediateVisibilityAndInteraction()
        {
            GameObject goView = new("TransitionView");
            goView.transform.SetParent(_goTestRoot.transform);
            CanvasGroup cgView = goView.AddComponent<CanvasGroup>();
            UiCanvasTransition transition = goView.AddComponent<UiCanvasTransition>();
            SetField(transition, "_showDurationSec", 0.0f);
            SetField(transition, "_hideDurationSec", 0.0f);

            transition.Hide();

            Assert.That(transition.IsVisible, Is.False);
            Assert.That(goView.activeSelf, Is.False);
            Assert.That(cgView.alpha, Is.EqualTo(0.0f));
            Assert.That(cgView.blocksRaycasts, Is.False);

            transition.Show();

            Assert.That(transition.IsVisible, Is.True);
            Assert.That(goView.activeSelf, Is.True);
            Assert.That(cgView.alpha, Is.EqualTo(1.0f));
            Assert.That(cgView.interactable, Is.True);
            Assert.That(cgView.blocksRaycasts, Is.True);

            transition.SetInteractionEnabled(false);

            Assert.That(cgView.interactable, Is.False);
            Assert.That(cgView.blocksRaycasts, Is.False);
        }

        [Test]
        public void FocusScopeRestoresDefaultAndLastSelection()
        {
            GameObject goEventSystem = new("EventSystem");
            goEventSystem.transform.SetParent(_goTestRoot.transform);
            EventSystem eventSystem = goEventSystem.AddComponent<EventSystem>();

            GameObject goScope = new("FocusScope");
            goScope.transform.SetParent(_goTestRoot.transform);
            UiFocusScope focusScope = goScope.AddComponent<UiFocusScope>();
            Button defaultButton = CreateButton("DefaultButton", goScope.transform);
            Button lastButton = CreateButton("LastButton", goScope.transform);
            SetField(focusScope, "_eventSystem", eventSystem);
            SetField(focusScope, "_defaultSelectable", defaultButton);

            focusScope.RestoreFocus();

            Assert.That(eventSystem.currentSelectedGameObject, Is.SameAs(defaultButton.gameObject));

            eventSystem.SetSelectedGameObject(lastButton.gameObject);
            focusScope.SaveFocus();
            eventSystem.SetSelectedGameObject(null);
            focusScope.RestoreFocus();

            Assert.That(eventSystem.currentSelectedGameObject, Is.SameAs(lastButton.gameObject));
        }

        //============================================================
        // Utilities
        //============================================================
        private static Button CreateButton(string objectName, Transform parent)
        {
            GameObject goButton = new(objectName);
            goButton.transform.SetParent(parent);
            return goButton.AddComponent<Button>();
        }

        private static void SetField<TTarget, TValue>(TTarget target, string fieldName, TValue value)
        {
            FieldInfo field = typeof(TTarget).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(target, value);
        }
    }
}
