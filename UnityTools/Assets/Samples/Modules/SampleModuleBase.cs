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
        public bool TryInit()
        {
            if(_isInit)
                return true;

            _isInit = OnInitModule();
            if(_isInit)
                return true;


            enabled = false;
            return false;
        }

        public bool TryRelease()
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
        public bool TryShow()
        {
            return _isInit && OnShowModule();
        }

        public bool TryHide()
        {
            return !_isInit || OnHideModule();
        }

        protected abstract bool OnInitModule();
        protected abstract bool OnShowModule();
        protected abstract bool OnHideModule();
        protected abstract bool OnReleaseModule();
    }
}