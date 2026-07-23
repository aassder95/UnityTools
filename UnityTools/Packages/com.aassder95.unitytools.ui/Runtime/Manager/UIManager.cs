using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityTools.Util.Core;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.Core.Singleton;

namespace UnityTools.Manager
{
    public class UIManager : MonoSingleton<UIManager>
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly List<ISampleModule> _modules = new();
        private readonly List<ISampleModule> _activeModules = new();

        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Entry")] [SerializeField] private string _entryModuleKey = SampleModuleKeys.All;
        [Header("Dependencies")] [SerializeField] private Canvas _hostCanvas;
        [SerializeField] private EventSystem _eventSystem;
        [Header("Modules")] [SerializeField] private MonoBehaviour[] _sampleModuleBehaviours;

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
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(!Release())
                DebugLogger.LogError("UIManager 파괴 중 Module 정리를 완료하지 못했습니다.", this);
        }

        //============================================================
        // Init/Register
        //============================================================
        public bool Init()
        {
            if(_isInit)
                return true;



            if(!RegisterModules() || !InitModules())
            {
                if(!ReleaseModules())
                    DebugLogger.LogError("UIManager 초기화 실패 후 Module 정리를 완료하지 못했습니다.", this);

                enabled = false;
                return false;
            }

            _launcher = new SampleLauncher(_hostCanvas, _eventSystem, OnSampleSelected, OnBackClicked);
            _launcher.Init(_activeModules);

            _isInit = true;
            if(ShowSampleList())
                return true;

            if(!Release())
                DebugLogger.LogError("UIManager 초기화 실패 후 정리를 완료하지 못했습니다.", this);

            enabled = false;
            return false;
        }

        public bool Release()
        {
            bool isSuccess = HideSelectedModule();
            _launcher?.Release();
            _launcher = null;
            isSuccess &= ReleaseModules();
            _isInit = false;
            return isSuccess;
        }

        private bool RegisterModules()
        {
            _modules.Clear();

            HashSet<string> moduleKeys = new(StringComparer.OrdinalIgnoreCase);
            for(int i = 0; i < _sampleModuleBehaviours.Length; i++)
            {
                MonoBehaviour behaviour = _sampleModuleBehaviours[i];

                if(!behaviour.gameObject.scene.IsValid() || behaviour is not ISampleModule module)
                {
                    DebugLogger.LogError("UIManager에 연결된 오브젝트가 유효한 Sample Module이 아닙니다. 이름=" + behaviour.name, this);
                    return false;
                }

                if(string.IsNullOrWhiteSpace(module.ModuleKey))
                {
                    DebugLogger.LogError("Sample Module 키가 비어 있습니다. 이름=" + behaviour.name, this);
                    return false;
                }

                if(!moduleKeys.Add(module.ModuleKey))
                {
                    DebugLogger.LogError("중복된 Sample Module 키가 연결되어 있습니다. 키=" + module.ModuleKey, this);
                    return false;
                }

                _modules.Add(module);
            }

            return true;
        }

        private bool InitModules()
        {
            _activeModules.Clear();
            for(int i = 0; i < _modules.Count; i++)
            {
                ISampleModule module = _modules[i];
                if(!IsTargetModule(module.ModuleKey))
                    continue;

                if(!module.Init())
                    return false;

                _activeModules.Add(module);
                if(!module.Hide())
                    return false;
            }

            if(_activeModules.Count > 0)
                return true;

            DebugLogger.LogError("Entry 조건에 맞는 Sample Module이 없습니다. 키=" + _entryModuleKey, this);
            return false;
        }

        private bool ReleaseModules()
        {
            bool isSuccess = true;
            for(int i = 0; i < _activeModules.Count; i++)
            {
                isSuccess &= _activeModules[i].Release();
            }

            _activeModules.Clear();
            _modules.Clear();
            return isSuccess;
        }

        //============================================================
        // Logic
        //============================================================
        private bool OpenSample(int moduleIdx)
        {
            if(moduleIdx < 0 || moduleIdx >= _activeModules.Count)
            {
                DebugLogger.LogError("요청한 Sample Module 인덱스가 범위를 벗어났습니다. 인덱스=" + moduleIdx, this);
                return false;
            }

            if(!HideSelectedModule())
                return false;

            ISampleModule module = _activeModules[moduleIdx];
            if(!module.Show())
                return false;

            _launcher.ShowModule();
            _selectedModule = module;
            return true;
        }

        private bool ShowSampleList()
        {
            if(!HideSelectedModule())
                return false;

            _launcher.ShowList();
            return true;
        }

        private bool HideSelectedModule()
        {
            if(_selectedModule == null)
                return true;

            if(!_selectedModule.Hide())
                return false;

            _selectedModule = null;
            return true;
        }

        private bool IsTargetModule(string moduleKey)
        {
            if(string.IsNullOrWhiteSpace(_entryModuleKey) || string.Equals(_entryModuleKey, SampleModuleKeys.All, StringComparison.OrdinalIgnoreCase))
                return true;

            return string.Equals(moduleKey, _entryModuleKey, StringComparison.OrdinalIgnoreCase);
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OnSampleSelected(int moduleIdx)
        {
            if(OpenSample(moduleIdx))
                return;

            DebugLogger.LogError("Sample Module 열기에 실패해 UIManager를 중단합니다. 인덱스=" + moduleIdx, this);
            if(!Release())
                DebugLogger.LogError("Sample Module 열기 실패 후 UIManager 정리를 완료하지 못했습니다.", this);

            enabled = false;
        }

        private void OnBackClicked()
        {
            if(ShowSampleList())
                return;

            DebugLogger.LogError("Sample 목록 복귀에 실패해 UIManager를 중단합니다.", this);
            if(!Release())
                DebugLogger.LogError("Sample 목록 복귀 실패 후 UIManager 정리를 완료하지 못했습니다.", this);

            enabled = false;
        }
    }
}
