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
        public void Init()
        {
            if(_isInit)
                return;

            OnInit();
            _isInit = true;
        }

        public void Release()
        {
            if(!_isInit)
                return;

            _isInit = false;
            OnRelease();
        }

        protected virtual void OnInit()
        {
        }

        protected virtual void OnRelease()
        {
        }

        //============================================================
        // Logic
        //============================================================
        public void Show()
        {
            Init();
            if(gameObject.activeSelf)
                return;

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            if(!gameObject.activeSelf)
                return;

            gameObject.SetActive(false);
        }

        public void Refresh(TModel model)
        {
            OnRefresh(model);
        }

        protected abstract void OnRefresh(TModel model);
    }
}
