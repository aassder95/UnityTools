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

        [Test]
        public void SafeAreaFitterConvertsPixelBoundsToAnchors()
        {
            GameObject goTarget = new("SafeAreaTarget", typeof(RectTransform));
            goTarget.transform.SetParent(_goTestRoot.transform);
            RectTransform rtTarget = goTarget.transform as RectTransform;

            GameObject goFitter = new("SafeAreaFitter");
            goFitter.SetActive(false);
            goFitter.transform.SetParent(_goTestRoot.transform);
            UiSafeAreaFitter fitter = goFitter.AddComponent<UiSafeAreaFitter>();
            SetField(fitter, "_rtTarget", rtTarget);

            fitter.Apply(new Rect(0.0f, 100.0f, 1080.0f, 1820.0f), new Vector2(1080.0f, 1920.0f));

            Assert.That(rtTarget.anchorMin.x, Is.EqualTo(0.0f).Within(0.0001f));
            Assert.That(rtTarget.anchorMin.y, Is.EqualTo(100.0f / 1920.0f).Within(0.0001f));
            Assert.That(rtTarget.anchorMax.x, Is.EqualTo(1.0f).Within(0.0001f));
            Assert.That(rtTarget.anchorMax.y, Is.EqualTo(1.0f).Within(0.0001f));
            Assert.That(rtTarget.offsetMin, Is.EqualTo(Vector2.zero));
            Assert.That(rtTarget.offsetMax, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void SafeAreaFitterPreservesDisabledAxisLayout()
        {
            GameObject goTarget = new("SafeAreaTarget", typeof(RectTransform));
            goTarget.transform.SetParent(_goTestRoot.transform);
            RectTransform rtTarget = goTarget.transform as RectTransform;
            rtTarget.anchorMin = new Vector2(0.2f, 0.3f);
            rtTarget.anchorMax = new Vector2(0.8f, 0.9f);
            rtTarget.offsetMin = new Vector2(10.0f, 20.0f);
            rtTarget.offsetMax = new Vector2(30.0f, 40.0f);

            GameObject goFitter = new("SafeAreaFitter");
            goFitter.SetActive(false);
            goFitter.transform.SetParent(_goTestRoot.transform);
            UiSafeAreaFitter fitter = goFitter.AddComponent<UiSafeAreaFitter>();
            SetField(fitter, "_rtTarget", rtTarget);
            SetField(fitter, "_shouldFitHorizontal", false);

            fitter.Apply(new Rect(100.0f, 200.0f, 800.0f, 1600.0f), new Vector2(1000.0f, 2000.0f));

            Assert.That(rtTarget.anchorMin.x, Is.EqualTo(0.2f).Within(0.0001f));
            Assert.That(rtTarget.anchorMin.y, Is.EqualTo(0.1f).Within(0.0001f));
            Assert.That(rtTarget.anchorMax.x, Is.EqualTo(0.8f).Within(0.0001f));
            Assert.That(rtTarget.anchorMax.y, Is.EqualTo(0.9f).Within(0.0001f));
            Assert.That(rtTarget.offsetMin.x, Is.EqualTo(10.0f).Within(0.0001f));
            Assert.That(rtTarget.offsetMin.y, Is.EqualTo(0.0f).Within(0.0001f));
            Assert.That(rtTarget.offsetMax.x, Is.EqualTo(30.0f).Within(0.0001f));
            Assert.That(rtTarget.offsetMax.y, Is.EqualTo(0.0f).Within(0.0001f));
        }

        [Test]
        public void BackInputRoutesCallbackToNavigator()
        {
            UiNavigator navigator = new();
            navigator.PushScreen(new UiNavigationEntry(new ComponentTestPresenter()));
            navigator.PushScreen(new UiNavigationEntry(new ComponentTestPresenter()));

            GameObject goInput = new("BackInput");
            goInput.SetActive(false);
            goInput.transform.SetParent(_goTestRoot.transform);
            UiBackInput backInput = goInput.AddComponent<UiBackInput>();
            backInput.Init(navigator);

            MethodInfo callback = typeof(UiBackInput).GetMethod("OnBackPerformed", BindingFlags.Instance | BindingFlags.NonPublic);
            object context = System.Activator.CreateInstance(callback.GetParameters()[0].ParameterType);
            callback.Invoke(backInput, new[] { context });

            Assert.That(navigator.ScreenCnt, Is.EqualTo(1));

            backInput.Release();

            Assert.That(backInput.IsInit, Is.False);
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

        //============================================================
        // Nested Types
        //============================================================
        private class ComponentTestPresenter : IPresenter
        {
            private bool _isInit;
            private bool _isVisible;

            public bool IsInit => _isInit;
            public bool IsVisible => _isVisible;

            public void Init()
            {
                _isInit = true;
            }

            public void Release()
            {
                _isInit = false;
                _isVisible = false;
            }

            public void Show()
            {
                Init();
                _isVisible = true;
            }

            public void Hide()
            {
                _isVisible = false;
            }
        }
    }
}
