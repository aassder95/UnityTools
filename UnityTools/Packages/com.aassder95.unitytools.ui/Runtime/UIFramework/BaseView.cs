using UnityEngine;

namespace UnityTools.Util.UIFramework
{
    public abstract class BaseView<TModel> : MonoBehaviour, IView<TModel> where TModel : IModel
    {
        //============================================================
        // Fields
        //============================================================
        private bool _isInit;

        //============================================================
        // Properties
        //============================================================
        public bool IsInit => _isInit;
        public bool IsVisible => gameObject.activeSelf;

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
        public bool Init()
        {
            if(_isInit)
                return true;

            if(!OnInit())
                return false;

            _isInit = true;
            return true;
        }

        public bool Release()
        {
            if(!_isInit)
                return true;

            _isInit = false;
            return OnRelease();
        }

        protected virtual bool OnInit()
        {
            return true;
        }

        protected virtual bool OnRelease()
        {
            return true;
        }

        //============================================================
        // Logic
        //============================================================
        public bool Show()
        {
            if(!Init())
                return false;

            if(gameObject.activeSelf)
                return true;

            gameObject.SetActive(true);
            return true;
        }

        public bool Hide()
        {
            if(!gameObject.activeSelf)
                return true;

            gameObject.SetActive(false);
            return true;
        }

        public bool Refresh(TModel model)
        {
            return OnRefresh(model);
        }

        protected abstract bool OnRefresh(TModel model);
    }
}
