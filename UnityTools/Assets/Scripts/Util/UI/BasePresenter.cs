namespace UnityTools.Util
{
    public interface IPresenter
    {
        void Init();
        void Release();
    }

    public abstract class BasePresenter<TModel, TView> : IPresenter where TModel : BaseModel where TView : BaseView<TModel>
    {
        protected readonly TModel _model;
        protected readonly TView _view;

        private bool _isInit = false;
        
        public bool IsVisible => _view.IsVisible;

        protected BasePresenter(TModel model, TView view)
        {
            _model = model;
            _view = view;
        }

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
            _view.Hide();
        }

        protected virtual void OnModelUpdated()
        {
            if (!_isInit)
                return;

            _view.Refresh(_model);
        }
    }
}