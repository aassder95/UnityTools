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

        protected BasePresenter(TModel model, TView view)
        {
            _model = model;
            _view = view;
        }

        public virtual void Init()
        {
            BindEvents();
            _view.Init();
        }

        public virtual void Release()
        {
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
            _view.Show();
            _view.Refresh(_model);
        }
        
        public virtual void Hide()
        {
            _view.Hide();
        }

        protected virtual void OnModelUpdated()
        {
            _view.Refresh(_model);
        }
    }
}