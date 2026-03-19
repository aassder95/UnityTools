using System;

namespace UnityTools.Util
{
    // Exception: type-centric file uses Types section.
    //============================================================
    //Types
    //============================================================
    public interface IPresenter
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

        //============================================================
        //Logic
        //============================================================
        void Show();
        void Hide();

        //============================================================
        //Release
        //============================================================
        void Release();
    }

    public abstract class BasePresenter<TModel, TView> : IPresenter where TModel : BaseModel where TView : BaseView<TModel>
    {
        //============================================================
        //Readonly
        //============================================================
        protected readonly TModel _model;
        protected readonly TView _view;

        //============================================================
        //Fields
        //============================================================
        private bool _isInitialized;

        //============================================================
        //Properties
        //============================================================
        public bool IsInitialized => _isInitialized;
        public bool IsVisible => _view.IsVisible;

        //============================================================
        //Constructors
        //============================================================
        protected BasePresenter(TModel model, TView view)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (view == null)
                throw new ArgumentNullException(nameof(view));

            _model = model;
            _view = view;
        }

        //============================================================
        //Init/Register
        //============================================================
        public void Init()
        {
            if (_isInitialized)
                return;

            _view.Init();
            OnInit();
            BindEvents();
            _isInitialized = true;
        }

        public void Release()
        {
            if (!_isInitialized)
                return;

            _isInitialized = false;
            UnbindEvents();
            OnRelease();
            _view.Release();
        }

        //============================================================
        //Logic
        //============================================================
        protected virtual void OnInit() { }

        protected virtual void BindEvents()
        {
            _model.OnUpdated += OnModelUpdated;
        }

        protected virtual void UnbindEvents()
        {
            _model.OnUpdated -= OnModelUpdated;
        }

        public void Show()
        {
            if (!_isInitialized)
                Init();

            if (!_isInitialized)
                return;

            _view.Show();
            _view.Refresh(_model);
            OnShow();
        }

        public void Hide()
        {
            if (!_isInitialized || !_view.IsVisible)
                return;

            _view.Hide();
            OnHide();
        }

        protected virtual void OnShow() { }

        protected virtual void OnHide() { }

        protected virtual void OnRelease() { }

        //============================================================
        //Callbacks
        //============================================================
        protected virtual void OnModelUpdated()
        {
            if (!_isInitialized)
                return;

            _view.Refresh(_model);
        }
    }
}
