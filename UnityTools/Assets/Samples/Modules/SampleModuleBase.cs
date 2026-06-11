using UnityEngine;
using UnityTools.Util.Core;

namespace UnityTools.Samples.Modules
{
    public abstract class SampleModuleBase : MonoBehaviour, ISampleModule
    {
        //============================================================
        // Fields
        //============================================================
        private bool _isInitialized;

        //============================================================
        // Properties
        //============================================================
        public abstract string ModuleKey { get; }
        public bool IsInit => _isInitialized;

        //============================================================
        // Logic
        //============================================================
        public void Init()
        {
            if(_isInitialized)
                return;

            _isInitialized = OnInitModule();
        }

        public void Show()
        {
            if(!_isInitialized)
                return;

            OnShowModule();
        }

        public void Hide()
        {
            if(!_isInitialized)
                return;

            OnHideModule();
        }

        public void Release()
        {
            if(!_isInitialized)
                return;

            OnReleaseModule();
            _isInitialized = false;
        }

        protected abstract bool OnInitModule();
        protected abstract void OnShowModule();
        protected abstract void OnHideModule();
        protected abstract void OnReleaseModule();

        //============================================================
        // Utilities
        //============================================================
        protected bool TryResolveView<TView>(ref TView view) where TView : Component
        {
            if(view != null)
                return true;

            view = GetComponentInChildren<TView>(true);
            return view != null;
        }
    }
}
