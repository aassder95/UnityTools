using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityTools.Ui;

namespace UnityTools.Ui.Tests.InputSystem
{
    public class UiBackInputTests
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
            _goTestRoot = new GameObject("UiBackInputTests");
        }

        [TearDown]
        public void TearDown()
        {
            if (_goTestRoot != null)
                UnityEngine.Object.DestroyImmediate(_goTestRoot);
        }

        //============================================================
        // Logic
        //============================================================
        [Test]
        public void BackInputRoutesCallbackToNavigator()
        {
            UiNavigator navigator = new();
            navigator.PushScreen(new UiNavigationEntry(new InputTestPresenter()));
            navigator.PushScreen(new UiNavigationEntry(new InputTestPresenter()));

            GameObject goInput = new("BackInput");
            goInput.SetActive(false);
            goInput.transform.SetParent(_goTestRoot.transform);
            UiBackInput backInput = goInput.AddComponent<UiBackInput>();
            backInput.Init(navigator);

            MethodInfo callback = typeof(UiBackInput).GetMethod("OnBackPerformed", BindingFlags.Instance | BindingFlags.NonPublic);
            object context = Activator.CreateInstance(callback.GetParameters()[0].ParameterType);
            callback.Invoke(backInput, new[] { context });

            Assert.That(navigator.ScreenCnt, Is.EqualTo(1));

            backInput.Release();

            Assert.That(backInput.IsInit, Is.False);
        }

        //============================================================
        // Nested Types
        //============================================================
        private class InputTestPresenter : IPresenter
        {
            //============================================================
            // Fields
            //============================================================
            private bool _isInit;
            private bool _isVisible;

            //============================================================
            // Properties
            //============================================================
            public bool IsInit => _isInit;
            public bool IsVisible => _isVisible;

            //============================================================
            // Init/Register
            //============================================================
            public void Init()
            {
                _isInit = true;
            }

            public void Release()
            {
                _isInit = false;
                _isVisible = false;
            }

            //============================================================
            // Logic
            //============================================================
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
