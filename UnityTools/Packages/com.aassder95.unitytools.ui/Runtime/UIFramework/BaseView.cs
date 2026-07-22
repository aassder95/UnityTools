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
        // Init/Register
        //============================================================
        public virtual void Init()
        {
            if(_isInit)
                return;

            OnInit();
            _isInit = true;
        }

        public virtual void Release()
        {
            if(!_isInit)
                return;

            OnRelease();
            _isInit = false;
        }

        protected virtual void OnInit() { }
        protected virtual void OnRelease() { }

        //============================================================
        // Unity Methods
        //============================================================
        private void OnDestroy()
        {
            Release();
        }

        //============================================================
        // Logic
        //============================================================
        public virtual void Show()
        {
            if(!_isInit)
                Init();

            if(gameObject.activeSelf)
                return;

            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            if(!gameObject.activeSelf)
                return;

            gameObject.SetActive(false);
        }

        public void Refresh(TModel model)
        {
            if(!_isInit || model == null)
                return;

            OnRefresh(model);
        }

        protected abstract void OnRefresh(TModel model);
    }
}
