namespace UnityTools.Util.UIFramework
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
        public bool Init()
        {
            if(_isInit)
                return true;

            bool shouldReleaseView = false;
            if(!_view.IsInit)
            {
                if(!_view.Init())
                    return false;

                shouldReleaseView = true;
            }

            if(!OnInit())
            {
                if(shouldReleaseView)
                    _view.Release();

                return false;
            }

            BindEvents();
            _isInit = true;
            return true;
        }

        public bool Release()
        {
            if(!_isInit)
                return true;

            _isInit = false;
            UnbindEvents();
            bool isSuccess = OnRelease();
            isSuccess &= _view.Release();
            return isSuccess;
        }

        protected virtual bool OnInit()
        {
            return true;
        }

        protected virtual bool OnRelease()
        {
            return true;
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
        public bool Show()
        {
            if(!Init() || !_view.Show())
                return false;

            if(!_view.Refresh(_model))
            {
                _view.Hide();
                return false;
            }

            if(OnShow())
                return true;

            _view.Hide();
            return false;
        }

        public bool Hide()
        {
            if(!_isInit || !_view.IsVisible)
                return true;

            if(!_view.Hide())
                return false;

            return OnHide();
        }

        protected void StopAfterFailure()
        {
            Release();
        }

        protected virtual bool OnShow()
        {
            return true;
        }

        protected virtual bool OnHide()
        {
            return true;
        }

        //============================================================
        // Callbacks
        //============================================================
        protected virtual void OnModelUpdated()
        {
            if(!_isInit || _view.Refresh(_model))
                return;

            StopAfterFailure();
        }
    }
}
