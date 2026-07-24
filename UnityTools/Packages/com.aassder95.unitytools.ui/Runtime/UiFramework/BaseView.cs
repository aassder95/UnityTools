using UnityEngine;

namespace UnityTools.Ui
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
        public bool IsVisible => IsViewVisible;
        protected virtual bool IsViewVisible => gameObject.activeSelf;

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

            OnInit();
            _isInit = true;
        }

        public void Release()
        {
            if (!_isInit)
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
            if (IsViewVisible)
                return;

            ShowView();
        }

        public void Hide()
        {
            if (!IsViewVisible)
                return;

            HideView();
        }

        public void Refresh(TModel model)
        {
            OnRefresh(model);
        }

        protected virtual void ShowView()
        {
            gameObject.SetActive(true);
        }

        protected virtual void HideView()
        {
            gameObject.SetActive(false);
        }

        protected abstract void OnRefresh(TModel model);
    }
}
