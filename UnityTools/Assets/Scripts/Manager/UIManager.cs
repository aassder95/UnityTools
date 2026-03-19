using System;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Samples.Modules;
using UnityTools.Util;

namespace UnityTools.Manager
{
    public class UIManager : MonoSingleton<UIManager>
    {
        //============================================================
        //Inspector Fields
        //============================================================
        [Header("Entry")]
        [SerializeField] private string _entryModuleKey = SampleModuleKeys.ALL;

        [Header("Modules")]
        [SerializeField] private MonoBehaviour[] _sampleModuleBehaviours;

        //============================================================
        //Fields
        //============================================================
        private readonly List<ISampleModule> _modules = new();
        private readonly List<ISampleModule> _activeModules = new();

        //============================================================
        //Unity Methods
        //============================================================
        private void Awake()
        {
            RegisterModules();
            InitModules();
        }

        private void Start()
        {
            for (int i = 0; i < _activeModules.Count; i++)
            {
                _activeModules[i].Show();
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _activeModules.Count; i++)
            {
                _activeModules[i].Release();
            }

            _activeModules.Clear();
            _modules.Clear();
        }

        //============================================================
        //Init/Register
        //============================================================
        private void RegisterModules()
        {
            _modules.Clear();

            if (_sampleModuleBehaviours == null)
                return;

            for (int i = 0; i < _sampleModuleBehaviours.Length; i++)
            {
                MonoBehaviour behaviour = _sampleModuleBehaviours[i];
                if (behaviour == null)
                    continue;

                if (behaviour is ISampleModule sampleModule)
                    _modules.Add(sampleModule);
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
                if (module.IsInit)
                    _activeModules.Add(module);
            }
        }

        private bool IsTargetModule(string moduleKey)
        {
            if (string.IsNullOrWhiteSpace(moduleKey))
                return false;

            if (string.IsNullOrWhiteSpace(_entryModuleKey))
                return true;

            if (string.Equals(_entryModuleKey, SampleModuleKeys.ALL, StringComparison.OrdinalIgnoreCase))
                return true;

            return string.Equals(moduleKey, _entryModuleKey, StringComparison.OrdinalIgnoreCase);
        }
    }
}
