using UnityTools.UI;

namespace UnityTools.Presenter
{
    public abstract class BasePresenter<TModel, TView> where TView : IView<TModel>
    {
        protected readonly TView _view;
        protected readonly TModel _model;

        protected BasePresenter(TModel model, TView view)
        {
            _model = model;
            _view = view;

            Init();

            BindEvents();
            _view.InitView(_model);
        }

        protected virtual void Init() { }
        public virtual void Release() => UnbindEvents();
        protected abstract void BindEvents();
        protected abstract void UnbindEvents();
    }
}