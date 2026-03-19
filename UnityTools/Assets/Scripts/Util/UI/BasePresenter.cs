namespace UnityTools.Util
{
    // Exception: type-centric file uses Types section.
    //============================================================
    //Types
    //============================================================
    public interface IPresenter
    {
        //============================================================
        //Init/Register
        //============================================================
        void Init();
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
        public bool IsVisible => _view.IsVisible;

        //============================================================
        //Constructors
        //============================================================
        protected BasePresenter(TModel model, TView view)
        {
            _model = model;
            _view = view;

            Init();
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
            _view.Release();
            UnbindEvents();
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
