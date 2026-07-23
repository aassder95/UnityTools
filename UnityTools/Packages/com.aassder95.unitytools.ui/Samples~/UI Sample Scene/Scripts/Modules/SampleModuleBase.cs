using UnityEngine;
using UnityTools.Util.Core;

namespace UnityTools.Samples.Modules
{
    public abstract class SampleModuleBase : MonoBehaviour, ISampleModule
    {
        //============================================================
        // Fields
        //============================================================
        private bool _isInit;

        //============================================================
        // Properties
        //============================================================
        public abstract string ModuleKey { get; }
        public bool IsInit => _isInit;

        //============================================================
        // Init/Register
        //============================================================
        public void Init()
        {
            if(_isInit)
                return;

            OnInitModule();
            _isInit = true;
        }

        public void Release()
        {
            if(!_isInit)
                return;

            OnReleaseModule();
            _isInit = false;
        }

        //============================================================
        // Logic
        //============================================================
        public void Show()
        {
            Init();
            OnShowModule();
        }

        public void Hide()
        {
            if(_isInit)
                OnHideModule();
        }

        protected abstract void OnInitModule();
        protected abstract void OnShowModule();
        protected abstract void OnHideModule();
        protected abstract void OnReleaseModule();
    }
}
