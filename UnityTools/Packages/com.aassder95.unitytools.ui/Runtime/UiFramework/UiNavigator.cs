using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Ui
{
    public class UiNavigator
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly List<UiNavigationEntry> _screens = new();
        private readonly List<UiNavigationEntry> _popups = new();
        private readonly List<UiNavigationEntry> _overlays = new();

        //============================================================
        // Events
        //============================================================
        private event Action _onChanged;
        public event Action OnChanged { add => _onChanged += value; remove => _onChanged -= value; }

        //============================================================
        // Properties
        //============================================================
        public int ScreenCnt => _screens.Count;
        public int PopupCnt => _popups.Count;
        public int OverlayCnt => _overlays.Count;
        public UiNavigationEntry CurrentScreen => _screens.Count > 0 ? _screens[_screens.Count - 1] : null;
        public UiNavigationEntry TopPopup => _popups.Count > 0 ? _popups[_popups.Count - 1] : null;
        public UiNavigationEntry TopOverlay => _overlays.Count > 0 ? _overlays[_overlays.Count - 1] : null;
        public bool CanGoBack => _popups.Count > 0 || _screens.Count > 1;

        //============================================================
        // Logic
        //============================================================
        public bool TryPushScreen(UiNavigationEntry screen)
        {
            if (!CanAdd(screen, "화면"))
                return false;

            UiNavigationEntry currentScreen = CurrentScreen;
            if (currentScreen != null)
                HideEntry(currentScreen);

            _screens.Add(screen);
            ShowEntry(screen);
            RefreshNavigationState();
            return true;
        }

        public bool TryReplaceScreen(UiNavigationEntry screen)
        {
            if (!CanAdd(screen, "화면"))
                return false;

            UiNavigationEntry currentScreen = CurrentScreen;
            if (currentScreen != null)
            {
                HideEntry(currentScreen);
                _screens.RemoveAt(_screens.Count - 1);
            }

            _screens.Add(screen);
            ShowEntry(screen);
            RefreshNavigationState();
            return true;
        }

        public bool TryPopScreen()
        {
            if (_screens.Count <= 1)
                return false;

            UiNavigationEntry screen = CurrentScreen;
            HideEntry(screen);
            _screens.RemoveAt(_screens.Count - 1);
            ShowEntry(CurrentScreen);
            RefreshNavigationState();
            return true;
        }

        public bool TryOpenPopup(UiNavigationEntry popup)
        {
            if (!CanAdd(popup, "팝업"))
                return false;

            SaveActiveFocus();
            _popups.Add(popup);
            ShowEntry(popup);
            RefreshNavigationState();
            return true;
        }

        public bool TryClosePopup()
        {
            if (_popups.Count <= 0)
                return false;

            UiNavigationEntry popup = TopPopup;
            HideEntry(popup);
            _popups.RemoveAt(_popups.Count - 1);
            RefreshNavigationState();
            return true;
        }

        public bool TryShowOverlay(UiNavigationEntry overlay)
        {
            if (!CanAdd(overlay, "오버레이"))
                return false;

            if (overlay.IsModal)
                SaveActiveFocus();

            _overlays.Add(overlay);
            ShowEntry(overlay);
            RefreshNavigationState();
            return true;
        }

        public bool TryHideOverlay(UiNavigationEntry overlay)
        {
            int overlayIdx = _overlays.IndexOf(overlay);
            if (overlayIdx < 0)
                return false;

            HideEntry(overlay);
            _overlays.RemoveAt(overlayIdx);
            RefreshNavigationState();
            return true;
        }

        public bool TryHandleBack()
        {
            if (TryClosePopup())
                return true;

            return TryPopScreen();
        }

        public void Clear()
        {
            for (int i = _overlays.Count - 1; i >= 0; i--)
            {
                HideEntry(_overlays[i]);
            }

            for (int i = _popups.Count - 1; i >= 0; i--)
            {
                HideEntry(_popups[i]);
            }

            for (int i = _screens.Count - 1; i >= 0; i--)
            {
                HideEntry(_screens[i]);
            }

            _overlays.Clear();
            _popups.Clear();
            _screens.Clear();
            _onChanged?.Invoke();
        }

        private void RefreshNavigationState()
        {
            RefreshInteraction();
            RestoreActiveFocus();
            _onChanged?.Invoke();
        }

        private void RefreshInteraction()
        {
            SetInteraction(_screens, false);
            SetInteraction(_popups, false);
            SetInteraction(_overlays, false);

            int modalOverlayIdx = FindTopModalIdx(_overlays);
            if (modalOverlayIdx >= 0)
            {
                for (int i = modalOverlayIdx; i < _overlays.Count; i++)
                {
                    _overlays[i].InteractionControl?.SetInteractionEnabled(true);
                }

                return;
            }

            SetInteraction(_overlays, true);
            if (_popups.Count > 0)
            {
                TopPopup.InteractionControl?.SetInteractionEnabled(true);
                if (FindTopModalIdx(_popups) < 0)
                    CurrentScreen?.InteractionControl?.SetInteractionEnabled(true);

                return;
            }

            CurrentScreen?.InteractionControl?.SetInteractionEnabled(true);
        }

        private void SaveActiveFocus()
        {
            UiNavigationEntry activeEntry = ResolveActiveFocusEntry();
            activeEntry?.FocusControl?.SaveFocus();
        }

        private void RestoreActiveFocus()
        {
            UiNavigationEntry activeEntry = ResolveActiveFocusEntry();
            activeEntry?.FocusControl?.RestoreFocus();
        }

        private UiNavigationEntry ResolveActiveFocusEntry()
        {
            int modalOverlayIdx = FindTopModalIdx(_overlays);
            if (modalOverlayIdx >= 0)
                return _overlays[_overlays.Count - 1];

            if (_popups.Count > 0)
                return TopPopup;

            return CurrentScreen;
        }

        private bool CanAdd(UiNavigationEntry entry, string layerName)
        {
            if (entry == null || entry.Presenter == null)
            {
                Debug.LogError("UI " + layerName + " 등록 정보 또는 Presenter가 비어 있습니다.");
                return false;
            }

            if (HasPresenter(entry.Presenter))
            {
                Debug.LogError("같은 Presenter를 UI 탐색 스택에 중복 등록할 수 없습니다. 타입=" + entry.Presenter.GetType().Name);
                return false;
            }

            return true;
        }

        private bool HasPresenter(IPresenter presenter)
        {
            return HasPresenter(_screens, presenter) || HasPresenter(_popups, presenter) || HasPresenter(_overlays, presenter);
        }

        private static bool HasPresenter(List<UiNavigationEntry> entries, IPresenter presenter)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (ReferenceEquals(entries[i].Presenter, presenter))
                    return true;
            }

            return false;
        }

        private static int FindTopModalIdx(List<UiNavigationEntry> entries)
        {
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                if (entries[i].IsModal)
                    return i;
            }

            return -1;
        }

        private static void SetInteraction(List<UiNavigationEntry> entries, bool isEnabled)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].InteractionControl?.SetInteractionEnabled(isEnabled);
            }
        }

        private static void ShowEntry(UiNavigationEntry entry)
        {
            entry.Presenter.Show();
        }

        private static void HideEntry(UiNavigationEntry entry)
        {
            entry.FocusControl?.SaveFocus();
            entry.InteractionControl?.SetInteractionEnabled(false);
            entry.Presenter.Hide();
        }
    }
}
