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

            _isInitialized = true;
        }

        public virtual void Release()
        {
            if (!_isInitialized)
                return;

            _isInitialized = false;
        }

        //============================================================
        //Logic
        //============================================================
        public virtual void Show()
        {
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
            if (model == null)
                return;
        }
    }
}
