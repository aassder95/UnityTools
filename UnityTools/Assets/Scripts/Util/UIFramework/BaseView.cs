using UnityEngine;

namespace UnityTools.Util
{
    // Exception: type-centric file uses Types section.
    //============================================================
    //Types
    //============================================================
    public interface IView<TModel> where TModel : BaseModel
    {
        //============================================================
        //Properties
        //============================================================
        bool IsInitialized { get; }
        bool IsVisible { get; }

        //============================================================
        //Init/Register
        //============================================================
        void Init();
        void Release();

        //============================================================
        //Logic
        //============================================================
        void Show();
        void Hide();
        void Refresh(TModel model);
    }

    public abstract class BaseView<TModel> : MonoBehaviour, IView<TModel> where TModel : BaseModel
    {
        //============================================================
        //Fields
        //============================================================
        private bool _isInitialized;

        //============================================================
        //Properties
        //============================================================
        public bool IsInitialized => _isInitialized;
        public bool IsVisible => gameObject.activeSelf;

        //============================================================
        //Init/Register
        //============================================================
        public virtual void Init()
        {
            if (_isInitialized)
                return;

            OnInit();
            _isInitialized = true;
        }

        public virtual void Release()
        {
            if (!_isInitialized)
                return;

            OnRelease();
            _isInitialized = false;
        }

        protected virtual void OnInit() { }

        protected virtual void OnRelease() { }

        //============================================================
        //Logic
        //============================================================
        public virtual void Show()
        {
            if (!_isInitialized)
                Init();

            if (gameObject.activeSelf)
                return;

            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            if (!gameObject.activeSelf)
                return;

            gameObject.SetActive(false);
        }

        public virtual void Refresh(TModel model)
        {
            if (!_isInitialized || model == null)
                return;
        }
    }
}
