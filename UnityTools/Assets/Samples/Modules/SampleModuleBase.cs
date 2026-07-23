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
        public bool Init()
        {
            if(_isInit)
                return true;

            _isInit = OnInitModule();
            return _isInit;
        }

        public bool Release()
        {
            if(!_isInit)
                return true;

            bool isSuccess = OnReleaseModule();
            _isInit = false;
            return isSuccess;
        }

        //============================================================
        // Logic
        //============================================================
        public bool Show()
        {
            return _isInit && OnShowModule();
        }

        public bool Hide()
        {
            return !_isInit || OnHideModule();
        }

        protected abstract bool OnInitModule();
        protected abstract bool OnShowModule();
        protected abstract bool OnHideModule();
        protected abstract bool OnReleaseModule();
    }
}
