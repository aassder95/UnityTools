namespace UnityTools.Util.UiFramework
{
    public abstract class BasePresenter<TModel, TView> : IPresenter where TModel : IModel where TView : IView<TModel>
    {
        //============================================================
        // Readonly
        //============================================================
        protected readonly TModel _model;
        protected readonly TView _view;

        //============================================================
        // Fields
        //============================================================
        private bool _isInit;

        //============================================================
        // Properties
        //============================================================
        public bool IsInit => _isInit;
        public bool IsVisible => _view.IsVisible;

        //============================================================
        // Constructors
        //============================================================
        protected BasePresenter(TModel model, TView view)
        {
            _model = model;
            _view = view;
        }

        //============================================================
        // Init/Register
        //============================================================
        public void Init()
        {
            if (_isInit)
                return;

            if (!_view.IsInit)
                _view.Init();

            OnInit();
            BindEvents();
            _isInit = true;
        }

        public void Release()
        {
            if (!_isInit)
                return;

            _isInit = false;
            UnbindEvents();
            OnRelease();
            _view.Release();
        }

        protected virtual void OnInit()
        {
        }

        protected virtual void OnRelease()
        {
        }

        protected virtual void BindEvents()
        {
            _model.OnUpdated += OnModelUpdated;
        }

        protected virtual void UnbindEvents()
        {
            _model.OnUpdated -= OnModelUpdated;
        }

        //============================================================
        // Logic
        //============================================================
        public void Show()
        {
            Init();
            _view.Show();
            _view.Refresh(_model);
            OnShow();
        }

        public void Hide()
        {
            if (!_isInit || !_view.IsVisible)
                return;

            _view.Hide();
            OnHide();
        }

        protected virtual void OnShow()
        {
        }

        protected virtual void OnHide()
        {
        }

        //============================================================
        // Callbacks
        //============================================================
        protected virtual void OnModelUpdated()
        {
            if (!_isInit)
                return;

            if (!_view.IsVisible)
                return;

            _view.Refresh(_model);
        }
    }
}
