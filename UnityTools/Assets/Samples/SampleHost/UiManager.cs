using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityTools.Samples.Modules;
using UnityTools.Util.Core;

namespace UnityTools.Manager
{
    public class UiManager : MonoBehaviour
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly List<ISampleModule> _modules = new();
        private readonly List<ISampleModule> _activeModules = new();

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
        private ISampleModule _selectedModule;
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
            _isInit = true;
            ShowSampleList();
        }

        public void Release()
        {
            HideSelectedModule();
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
            for (int i = 0; i < _modules.Count; i++)
            {
                ISampleModule module = _modules[i];
                if (!IsTargetModule(module.ModuleKey))
                    continue;

                module.Init();
                _activeModules.Add(module);
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
            _modules.Clear();
        }

        //============================================================
        // Logic
        //============================================================
        private void OpenSample(int moduleIdx)
        {
            HideSelectedModule();
            ISampleModule module = _activeModules[moduleIdx];
            module.Show();
            _launcher.ShowModule();
            _selectedModule = module;
        }

        private void ShowSampleList()
        {
            HideSelectedModule();
            _launcher.ShowList();
        }

        private void HideSelectedModule()
        {
            if (_selectedModule == null)
                return;

            _selectedModule.Hide();
            _selectedModule = null;
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
            ShowSampleList();
        }
    }
}
