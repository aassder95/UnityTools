namespace UnityTools.Util
{
    // Exception: type-centric file uses Types section.
    //============================================================
    //Types
    //============================================================
    public interface IPresenter
    {
        void Init();
        void Release();
    }

    public abstract class BasePresenter<TModel, TView> : IPresenter where TModel : BaseModel where TView : BaseView<TModel>
    {
        protected readonly TModel MODEL;
        protected readonly TView VIEW;

        private bool _isInit = false;
        
        public bool IsVisible => VIEW.IsVisible;

        protected BasePresenter(TModel model, TView view)
        {
            MODEL = model;
            VIEW = view;

            Init();
        }

        public virtual void Init()
        {
            if (_isInit)
                return;

            VIEW.Init();
            BindEvents();
            _isInit = true;
        }

        public virtual void Release()
        {
            if (!_isInit)
                return;
            
            _isInit = false;
            VIEW.Release();
            UnbindEvents();
        }

        protected virtual void BindEvents()
        {
            MODEL.OnUpdated += OnModelUpdated;
        }

        protected virtual void UnbindEvents()
        {
            MODEL.OnUpdated -= OnModelUpdated;
        }
        
        public virtual void Show()
        {
            if (!_isInit)
                return;
            
            VIEW.Show();
            VIEW.Refresh(MODEL);
        }
        
        public virtual void Hide()
        {
            VIEW.Hide();
        }

        protected virtual void OnModelUpdated()
        {
            if (!_isInit)
                return;

            VIEW.Refresh(MODEL);
        }
    }
}