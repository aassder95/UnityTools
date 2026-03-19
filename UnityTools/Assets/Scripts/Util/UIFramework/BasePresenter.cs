using System;

namespace UnityTools.Util.UIFramework
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
        bool IsInit { get; }
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
        void Release();
    }

    public abstract class BasePresenter<TModel, TView> : IPresenter where TModel : IModel where TView : IView<TModel>
    {
        //============================================================
        //Readonly
        //============================================================
        protected readonly TModel _model;
        protected readonly TView _view;

        //============================================================
        //Fields
        //============================================================
        private bool _isInit;

        //============================================================
        //Properties
        //============================================================
        public bool IsInit => _isInit;
        public bool IsVisible => _view.IsVisible;

        //============================================================
        //Constructors
        //============================================================
        protected BasePresenter(TModel model, TView view)
        {
            if(model == null)
                throw new ArgumentNullException(nameof(model));

            if(view == null)
                throw new ArgumentNullException(nameof(view));

            _model = model;
            _view = view;
        }

        //============================================================
        //Init/Register
        //============================================================
        public void Init()
        {
            if(_isInit)
                return;

            bool isViewInitializedByPresenter = false;
            bool isPresenterInitialized = false;
            bool isEventsBound = false;

            try
            {
                if(!_view.IsInit)
                {
                    _view.Init();
                    isViewInitializedByPresenter = true;
                }

                OnInit();
                isPresenterInitialized = true;

                BindEvents();
                isEventsBound = true;

                _isInit = true;
            }
            catch(Exception initException)
            {
                _isInit = false;
                Exception rollbackException = null;

                if(isEventsBound)
                    TryExecuteAndCapture(UnbindEvents, ref rollbackException);

                if(isPresenterInitialized)
                    TryExecuteAndCapture(OnRelease, ref rollbackException);

                if(isViewInitializedByPresenter)
                    TryExecuteAndCapture(_view.Release, ref rollbackException);

                if(rollbackException != null)
                    throw MergeException(initException, rollbackException);

                throw;
            }
        }

        public void Release()
        {
            if(!_isInit)
                return;

            _isInit = false;

            Exception releaseException = null;
            TryExecuteAndCapture(UnbindEvents, ref releaseException);
            TryExecuteAndCapture(OnRelease, ref releaseException);
            TryExecuteAndCapture(_view.Release, ref releaseException);

            if(releaseException != null)
                throw releaseException;
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
            if(!_isInit)
                Init();

            if(!_isInit)
                return;

            _view.Show();
            _view.Refresh(_model);
            OnShow();
        }

        public void Hide()
        {
            if(!_isInit || !_view.IsVisible)
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
            if(!_isInit)
                return;

            _view.Refresh(_model);
        }

        //============================================================
        //Utilities
        //============================================================
        private static void TryExecuteAndCapture(Action action, ref Exception releaseException)
        {
            try
            {
                action?.Invoke();
            }
            catch(Exception exception)
            {
                releaseException = MergeException(releaseException, exception);
            }
        }

        private static Exception MergeException(Exception currentException, Exception nextException)
        {
            if(currentException == null)
                return nextException;

            if(currentException is AggregateException aggregateException)
            {
                int prevCount = aggregateException.InnerExceptions.Count;
                Exception[] mergedExceptions = new Exception[prevCount + 1];

                for(int i = 0; i < prevCount; i++)
                    mergedExceptions[i] = aggregateException.InnerExceptions[i];

                mergedExceptions[prevCount] = nextException;
                return new AggregateException(mergedExceptions);
            }

            return new AggregateException(currentException, nextException);
        }
    }
}
