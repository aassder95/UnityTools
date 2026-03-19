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
        private bool _isInit = false;

        //============================================================
        //Properties
        //============================================================
        public bool IsInitialized => _isInit;
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
        public virtual void Init()
        {
            if (_isInit)
                return;

            _view.Init();
            BindEvents();
            _isInit = true;
        }

        public virtual void Release()
        {
            if (!_isInit)
                return;
            
            _isInit = false;
            UnbindEvents();
            _view.Release();
        }

        //============================================================
        //Logic
        //============================================================
        protected virtual void BindEvents()
        {
            _model.OnUpdated += OnModelUpdated;
        }

        protected virtual void UnbindEvents()
        {
            _model.OnUpdated -= OnModelUpdated;
        }
        
        public virtual void Show()
        {
            if (!_isInit)
                Init();

            if (!_isInit)
                return;
            
            _view.Show();
            _view.Refresh(_model);
        }
        
        public virtual void Hide()
        {
            if (!_isInit || !_view.IsVisible)
                return;

            _view.Hide();
        }

        //============================================================
        //Callbacks
        //============================================================
        protected virtual void OnModelUpdated()
        {
            if (!_isInit)
                return;

            _view.Refresh(_model);
        }
    }
}
