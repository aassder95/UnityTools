using NUnit.Framework;
using UnityTools.Ui;

namespace UnityTools.Ui.Tests.UiFramework
{
    public class UiNavigatorTests
    {
        //============================================================
        // Logic
        //============================================================
        [Test]
        public void ScreenStackRestoresPreviousScreen()
        {
            UiNavigator navigator = new();
            NavigationTestEntry first = new();
            NavigationTestEntry second = new();

            Assert.That(navigator.TryPushScreen(first.Entry), Is.True);
            Assert.That(navigator.TryPushScreen(second.Entry), Is.True);

            Assert.That(navigator.ScreenCnt, Is.EqualTo(2));
            Assert.That(navigator.CurrentScreen, Is.SameAs(second.Entry));
            Assert.That(first.Presenter.IsVisible, Is.False);
            Assert.That(first.Control.IsInteractionEnabled, Is.False);
            Assert.That(second.Presenter.IsVisible, Is.True);
            Assert.That(second.Control.IsInteractionEnabled, Is.True);

            Assert.That(navigator.TryPopScreen(), Is.True);
            Assert.That(navigator.CurrentScreen, Is.SameAs(first.Entry));
            Assert.That(first.Presenter.IsVisible, Is.True);
            Assert.That(first.Control.IsInteractionEnabled, Is.True);
            Assert.That(first.Control.RestoreFocusCnt, Is.EqualTo(2));
        }

        [Test]
        public void ModalPopupBlocksUntilBack()
        {
            UiNavigator navigator = new();
            NavigationTestEntry screen = new();
            NavigationTestEntry popup = new(true);
            Assert.That(navigator.TryPushScreen(screen.Entry), Is.True);

            Assert.That(navigator.TryOpenPopup(popup.Entry), Is.True);

            Assert.That(navigator.PopupCnt, Is.EqualTo(1));
            Assert.That(screen.Presenter.IsVisible, Is.True);
            Assert.That(screen.Control.IsInteractionEnabled, Is.False);
            Assert.That(popup.Control.IsInteractionEnabled, Is.True);
            Assert.That(screen.Control.SaveFocusCnt, Is.EqualTo(1));

            Assert.That(navigator.TryHandleBack(), Is.True);
            Assert.That(navigator.PopupCnt, Is.Zero);
            Assert.That(popup.Presenter.IsVisible, Is.False);
            Assert.That(screen.Control.IsInteractionEnabled, Is.True);
            Assert.That(screen.Control.RestoreFocusCnt, Is.EqualTo(2));
        }

        [Test]
        public void NonModalPopupKeepsScreenInteractive()
        {
            UiNavigator navigator = new();
            NavigationTestEntry screen = new();
            NavigationTestEntry popup = new(false);
            Assert.That(navigator.TryPushScreen(screen.Entry), Is.True);

            Assert.That(navigator.TryOpenPopup(popup.Entry), Is.True);

            Assert.That(screen.Control.IsInteractionEnabled, Is.True);
            Assert.That(popup.Control.IsInteractionEnabled, Is.True);
        }

        [Test]
        public void ModalOverlayBlocksLowerLayers()
        {
            UiNavigator navigator = new();
            NavigationTestEntry screen = new();
            NavigationTestEntry nonModalOverlay = new(false);
            NavigationTestEntry modalOverlay = new(true);
            Assert.That(navigator.TryPushScreen(screen.Entry), Is.True);
            Assert.That(navigator.TryShowOverlay(nonModalOverlay.Entry), Is.True);

            Assert.That(screen.Control.IsInteractionEnabled, Is.True);
            Assert.That(nonModalOverlay.Control.IsInteractionEnabled, Is.True);

            Assert.That(navigator.TryShowOverlay(modalOverlay.Entry), Is.True);

            Assert.That(screen.Control.IsInteractionEnabled, Is.False);
            Assert.That(nonModalOverlay.Control.IsInteractionEnabled, Is.False);
            Assert.That(modalOverlay.Control.IsInteractionEnabled, Is.True);

            Assert.That(navigator.TryHideOverlay(modalOverlay.Entry), Is.True);
            Assert.That(screen.Control.IsInteractionEnabled, Is.True);
            Assert.That(nonModalOverlay.Control.IsInteractionEnabled, Is.True);
        }

        [Test]
        public void TryReplaceKeepsSingleScreen()
        {
            UiNavigator navigator = new();
            NavigationTestEntry first = new();
            NavigationTestEntry second = new();
            Assert.That(navigator.TryPushScreen(first.Entry), Is.True);

            Assert.That(navigator.TryReplaceScreen(second.Entry), Is.True);

            Assert.That(navigator.ScreenCnt, Is.EqualTo(1));
            Assert.That(navigator.CurrentScreen, Is.SameAs(second.Entry));
            Assert.That(first.Presenter.IsVisible, Is.False);
            Assert.That(navigator.TryPopScreen(), Is.False);
        }

        [Test]
        public void ClearHidesLayersAndNotifiesOnce()
        {
            UiNavigator navigator = new();
            NavigationTestEntry screen = new();
            NavigationTestEntry popup = new();
            NavigationTestEntry overlay = new(false);
            int changedCnt = 0;
            navigator.OnChanged += () => changedCnt++;
            Assert.That(navigator.TryPushScreen(screen.Entry), Is.True);
            Assert.That(navigator.TryOpenPopup(popup.Entry), Is.True);
            Assert.That(navigator.TryShowOverlay(overlay.Entry), Is.True);

            navigator.Clear();

            Assert.That(navigator.ScreenCnt, Is.Zero);
            Assert.That(navigator.PopupCnt, Is.Zero);
            Assert.That(navigator.OverlayCnt, Is.Zero);
            Assert.That(screen.Presenter.IsVisible, Is.False);
            Assert.That(popup.Presenter.IsVisible, Is.False);
            Assert.That(overlay.Presenter.IsVisible, Is.False);
            Assert.That(changedCnt, Is.EqualTo(4));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CloseLowerPopupPreservesTopFocus(bool isTopModal)
        {
            UiNavigator navigator = new();
            NavigationTestEntry screen = new();
            NavigationTestEntry lower = new(true);
            NavigationTestEntry top = new(isTopModal);
            Assert.That(navigator.TryPushScreen(screen.Entry), Is.True);
            Assert.That(navigator.TryOpenPopup(lower.Entry), Is.True);
            Assert.That(navigator.TryOpenPopup(top.Entry), Is.True);
            int restoreCnt = top.Control.RestoreFocusCnt;

            Assert.That(navigator.TryClosePopup(lower.Entry), Is.True);

            Assert.That(navigator.PopupCnt, Is.EqualTo(1));
            Assert.That(navigator.TopPopup, Is.SameAs(top.Entry));
            Assert.That(lower.Presenter.IsVisible, Is.False);
            Assert.That(lower.Control.IsInteractionEnabled, Is.False);
            Assert.That(top.Presenter.IsVisible, Is.True);
            Assert.That(top.Control.IsInteractionEnabled, Is.True);
            Assert.That(top.Control.RestoreFocusCnt, Is.EqualTo(restoreCnt));
            Assert.That(screen.Control.IsInteractionEnabled, Is.EqualTo(!isTopModal));
            Assert.That(navigator.TryHandleBack(), Is.True);
            Assert.That(navigator.PopupCnt, Is.Zero);
            Assert.That(screen.Control.IsInteractionEnabled, Is.True);
        }

        [Test]
        public void CloseSpecificTopPopupRestoresPreviousFocus()
        {
            UiNavigator navigator = new();
            NavigationTestEntry lower = new();
            NavigationTestEntry top = new();
            Assert.That(navigator.TryOpenPopup(lower.Entry), Is.True);
            Assert.That(navigator.TryOpenPopup(top.Entry), Is.True);
            int restoreCnt = lower.Control.RestoreFocusCnt;

            Assert.That(navigator.TryClosePopup(top.Entry), Is.True);

            Assert.That(navigator.TopPopup, Is.SameAs(lower.Entry));
            Assert.That(lower.Control.IsInteractionEnabled, Is.True);
            Assert.That(lower.Control.RestoreFocusCnt, Is.EqualTo(restoreCnt + 1));
            Assert.That(top.Presenter.IsVisible, Is.False);
            Assert.That(top.Control.IsInteractionEnabled, Is.False);
        }

        [Test]
        public void CloseSpecificPopupNotifiesOnlyOnRemoval()
        {
            UiNavigator navigator = new();
            NavigationTestEntry popup = new();
            NavigationTestEntry missing = new();
            int changedCnt = 0;
            void OnChanged() => changedCnt++;
            navigator.OnChanged += OnChanged;
            try
            {
                Assert.That(navigator.TryClosePopup(null), Is.False);
                Assert.That(navigator.TryClosePopup(missing.Entry), Is.False);
                Assert.That(navigator.TryOpenPopup(popup.Entry), Is.True);
                Assert.That(navigator.TryClosePopup(missing.Entry), Is.False);
                Assert.That(navigator.TopPopup, Is.SameAs(popup.Entry));
                Assert.That(popup.Control.IsInteractionEnabled, Is.True);
                Assert.That(navigator.TryClosePopup(popup.Entry), Is.True);
                Assert.That(navigator.TryClosePopup(popup.Entry), Is.False);
                Assert.That(navigator.TryClosePopup(), Is.False);
                Assert.That(changedCnt, Is.EqualTo(2));
                Assert.That(missing.Control.SaveFocusCnt, Is.Zero);
            }
            finally
            {
                navigator.OnChanged -= OnChanged;
            }
        }

        [Test]
        public void CloseLowerPopupKeepsModalOverlayActive()
        {
            UiNavigator navigator = new();
            NavigationTestEntry screen = new();
            NavigationTestEntry lower = new();
            NavigationTestEntry top = new();
            NavigationTestEntry overlay = new();
            Assert.That(navigator.TryPushScreen(screen.Entry), Is.True);
            Assert.That(navigator.TryOpenPopup(lower.Entry), Is.True);
            Assert.That(navigator.TryOpenPopup(top.Entry), Is.True);
            Assert.That(navigator.TryShowOverlay(overlay.Entry), Is.True);
            int restoreCnt = overlay.Control.RestoreFocusCnt;

            Assert.That(navigator.TryClosePopup(lower.Entry), Is.True);

            Assert.That(navigator.TopPopup, Is.SameAs(top.Entry));
            Assert.That(screen.Control.IsInteractionEnabled, Is.False);
            Assert.That(top.Control.IsInteractionEnabled, Is.False);
            Assert.That(overlay.Control.IsInteractionEnabled, Is.True);
            Assert.That(overlay.Control.RestoreFocusCnt, Is.EqualTo(restoreCnt));
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
