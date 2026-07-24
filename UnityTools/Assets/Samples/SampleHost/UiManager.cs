using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityTools.Samples.Modules;
using UnityTools.Util.Core;
using UnityTools.Util.UiFramework;

namespace UnityTools.Manager
{
    public class UiManager : MonoBehaviour
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly List<ISampleModule> _modules = new();
        private readonly List<ISampleModule> _activeModules = new();
        private readonly List<UiNavigationEntry> _moduleEntries = new();

        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Entry")]
        [SerializeField] private string _entryModuleKey = SampleModuleKeys.All;
        [Header("Dependencies")]
        [SerializeField] private Canvas _hostCanvas;
        [SerializeField] private EventSystem _eventSystem;
        [Header("Modules")]
        [SerializeField] private SampleModuleBase[] _sampleModules;

        //============================================================
        // Fields
        //============================================================
        private SampleLauncher _launcher;
        private UiNavigator _navigator;
        private SampleLobbyPresenter _lobbyPresenter;
        private bool _isInit;

        //============================================================
        // Properties
        //============================================================
        public bool IsInit => _isInit;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnDestroy()
        {
            Release();
        }

        //============================================================
        // Init/Register
        //============================================================
        public void Init()
        {
            if (_isInit)
                return;

            RegisterModules();
            InitModules();
            _launcher = new SampleLauncher(_hostCanvas, _eventSystem, OnSampleSelected, OnBackClicked);
            _launcher.Init(_activeModules);
            _navigator = new UiNavigator();
            _lobbyPresenter = new SampleLobbyPresenter(_launcher);
            _navigator.PushScreen(new UiNavigationEntry(_lobbyPresenter));
            _isInit = true;
        }

        public void Release()
        {
            _navigator?.Clear();
            _lobbyPresenter?.Release();
            _navigator = null;
            _lobbyPresenter = null;
            _launcher?.Release();
            _launcher = null;
            ReleaseModules();
            _isInit = false;
        }

        private void RegisterModules()
        {
            _modules.Clear();
            for (int i = 0; i < _sampleModules.Length; i++)
            {
                _modules.Add(_sampleModules[i]);
            }
        }

        private void InitModules()
        {
            _activeModules.Clear();
            _moduleEntries.Clear();
            for (int i = 0; i < _modules.Count; i++)
            {
                ISampleModule module = _modules[i];
                if (!IsTargetModule(module.ModuleKey))
                    continue;

                module.Init();
                _activeModules.Add(module);
                _moduleEntries.Add(new UiNavigationEntry(module));
                module.Hide();
            }
        }

        private void ReleaseModules()
        {
            for (int i = 0; i < _activeModules.Count; i++)
            {
                _activeModules[i].Release();
            }

            _activeModules.Clear();
            _moduleEntries.Clear();
            _modules.Clear();
        }

        //============================================================
        // Logic
        //============================================================
        private void OpenSample(int moduleIdx)
        {
            _navigator.PushScreen(_moduleEntries[moduleIdx]);
        }

        private bool IsTargetModule(string moduleKey)
        {
            if (string.IsNullOrWhiteSpace(_entryModuleKey) || string.Equals(_entryModuleKey, SampleModuleKeys.All, StringComparison.OrdinalIgnoreCase))
                return true;

            return string.Equals(moduleKey, _entryModuleKey, StringComparison.OrdinalIgnoreCase);
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OnSampleSelected(int moduleIdx)
        {
            OpenSample(moduleIdx);
        }

        private void OnBackClicked()
        {
            _navigator.HandleBack();
        }

        //============================================================
        // Nested Types
        //============================================================
        private class SampleLobbyPresenter : IPresenter
        {
            private readonly SampleLauncher _launcher;

            private bool _isInit;
            private bool _isVisible;

            public bool IsInit => _isInit;
            public bool IsVisible => _isVisible;

            public SampleLobbyPresenter(SampleLauncher launcher)
            {
                _launcher = launcher;
            }

            public void Init()
            {
                _isInit = true;
            }

            public void Release()
            {
                if (!_isInit)
                    return;

                Hide();
                _isInit = false;
            }

            public void Show()
            {
                Init();
                _launcher.ShowList();
                _isVisible = true;
            }

            public void Hide()
            {
                if (!_isInit)
                    return;

                _launcher.ShowModule();
                _isVisible = false;
            }
        }
    }
}
