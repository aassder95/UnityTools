using NUnit.Framework;
using UnityTools.Util.UiFramework;

namespace UnityTools.Util.Tests.UiFramework
{
    public class UiNavigatorTests
    {
        //============================================================
        // Logic
        //============================================================
        [Test]
        public void ScreenStackHidesAndRestoresPreviousScreen()
        {
            UiNavigator navigator = new();
            NavigationTestEntry first = new();
            NavigationTestEntry second = new();

            navigator.PushScreen(first.Entry);
            navigator.PushScreen(second.Entry);

            Assert.That(navigator.ScreenCnt, Is.EqualTo(2));
            Assert.That(navigator.CurrentScreen, Is.SameAs(second.Entry));
            Assert.That(first.Presenter.IsVisible, Is.False);
            Assert.That(first.Control.IsInteractionEnabled, Is.False);
            Assert.That(second.Presenter.IsVisible, Is.True);
            Assert.That(second.Control.IsInteractionEnabled, Is.True);

            Assert.That(navigator.PopScreen(), Is.True);
            Assert.That(navigator.CurrentScreen, Is.SameAs(first.Entry));
            Assert.That(first.Presenter.IsVisible, Is.True);
            Assert.That(first.Control.IsInteractionEnabled, Is.True);
            Assert.That(first.Control.RestoreFocusCnt, Is.EqualTo(2));
        }

        [Test]
        public void ModalPopupBlocksScreenUntilBackClosesPopup()
        {
            UiNavigator navigator = new();
            NavigationTestEntry screen = new();
            NavigationTestEntry popup = new(true);
            navigator.PushScreen(screen.Entry);

            navigator.OpenPopup(popup.Entry);

            Assert.That(navigator.PopupCnt, Is.EqualTo(1));
            Assert.That(screen.Presenter.IsVisible, Is.True);
            Assert.That(screen.Control.IsInteractionEnabled, Is.False);
            Assert.That(popup.Control.IsInteractionEnabled, Is.True);
            Assert.That(screen.Control.SaveFocusCnt, Is.EqualTo(1));

            Assert.That(navigator.HandleBack(), Is.True);
            Assert.That(navigator.PopupCnt, Is.Zero);
            Assert.That(popup.Presenter.IsVisible, Is.False);
            Assert.That(screen.Control.IsInteractionEnabled, Is.True);
            Assert.That(screen.Control.RestoreFocusCnt, Is.EqualTo(2));
        }

        [Test]
        public void NonModalPopupKeepsCurrentScreenInteractive()
        {
            UiNavigator navigator = new();
            NavigationTestEntry screen = new();
            NavigationTestEntry popup = new(false);
            navigator.PushScreen(screen.Entry);

            navigator.OpenPopup(popup.Entry);

            Assert.That(screen.Control.IsInteractionEnabled, Is.True);
            Assert.That(popup.Control.IsInteractionEnabled, Is.True);
        }

        [Test]
        public void ModalOverlayBlocksLowerLayersOnlyWhileVisible()
        {
            UiNavigator navigator = new();
            NavigationTestEntry screen = new();
            NavigationTestEntry nonModalOverlay = new(false);
            NavigationTestEntry modalOverlay = new(true);
            navigator.PushScreen(screen.Entry);
            navigator.ShowOverlay(nonModalOverlay.Entry);

            Assert.That(screen.Control.IsInteractionEnabled, Is.True);
            Assert.That(nonModalOverlay.Control.IsInteractionEnabled, Is.True);

            navigator.ShowOverlay(modalOverlay.Entry);

            Assert.That(screen.Control.IsInteractionEnabled, Is.False);
            Assert.That(nonModalOverlay.Control.IsInteractionEnabled, Is.False);
            Assert.That(modalOverlay.Control.IsInteractionEnabled, Is.True);

            Assert.That(navigator.HideOverlay(modalOverlay.Entry), Is.True);
            Assert.That(screen.Control.IsInteractionEnabled, Is.True);
            Assert.That(nonModalOverlay.Control.IsInteractionEnabled, Is.True);
        }

        [Test]
        public void ReplaceScreenDoesNotLeavePreviousScreenInStack()
        {
            UiNavigator navigator = new();
            NavigationTestEntry first = new();
            NavigationTestEntry second = new();
            navigator.PushScreen(first.Entry);

            navigator.ReplaceScreen(second.Entry);

            Assert.That(navigator.ScreenCnt, Is.EqualTo(1));
            Assert.That(navigator.CurrentScreen, Is.SameAs(second.Entry));
            Assert.That(first.Presenter.IsVisible, Is.False);
            Assert.That(navigator.PopScreen(), Is.False);
        }

        [Test]
        public void ClearHidesEveryLayerAndNotifiesOnce()
        {
            UiNavigator navigator = new();
            NavigationTestEntry screen = new();
            NavigationTestEntry popup = new();
            NavigationTestEntry overlay = new(false);
            int changedCnt = 0;
            navigator.OnChanged += () => changedCnt++;
            navigator.PushScreen(screen.Entry);
            navigator.OpenPopup(popup.Entry);
            navigator.ShowOverlay(overlay.Entry);

            navigator.Clear();

            Assert.That(navigator.ScreenCnt, Is.Zero);
            Assert.That(navigator.PopupCnt, Is.Zero);
            Assert.That(navigator.OverlayCnt, Is.Zero);
            Assert.That(screen.Presenter.IsVisible, Is.False);
            Assert.That(popup.Presenter.IsVisible, Is.False);
            Assert.That(overlay.Presenter.IsVisible, Is.False);
            Assert.That(changedCnt, Is.EqualTo(4));
        }

        //============================================================
        // Nested Types
        //============================================================
        private class NavigationTestEntry
        {
            private readonly NavigationTestPresenter _presenter = new();
            private readonly NavigationTestControl _control = new();
            private readonly UiNavigationEntry _entry;

            public NavigationTestPresenter Presenter => _presenter;
            public NavigationTestControl Control => _control;
            public UiNavigationEntry Entry => _entry;

            public NavigationTestEntry(bool isModal = true)
            {
                _entry = new UiNavigationEntry(_presenter, _control, _control, isModal);
            }
        }

        private class NavigationTestPresenter : IPresenter
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

        private class NavigationTestControl : IUiInteractionControl, IUiFocusControl
        {
            private bool _isInteractionEnabled;
            private int _saveFocusCnt;
            private int _restoreFocusCnt;

            public bool IsInteractionEnabled => _isInteractionEnabled;
            public int SaveFocusCnt => _saveFocusCnt;
            public int RestoreFocusCnt => _restoreFocusCnt;

            public void SetInteractionEnabled(bool isEnabled)
            {
                _isInteractionEnabled = isEnabled;
            }

            public void SaveFocus()
            {
                _saveFocusCnt++;
            }

            public void RestoreFocus()
            {
                _restoreFocusCnt++;
            }
        }
    }
}
