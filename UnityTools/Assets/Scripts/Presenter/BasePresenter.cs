using UnityEngine;
using UnityTools.UI;

namespace UnityTools.Presenter
{
    public abstract class BasePresenter<TModel, TView> where TView : MonoBehaviour, IView<TModel>
    {
        protected readonly TView _view;
        protected TModel _model;

        public bool IsViewVisible => _view.gameObject.activeSelf;

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

        public virtual void ShowView()
        {
            _view.gameObject.SetActive(true);
            _view.ShowView(_model);
            _view.UpdateView(_model);
        }

        public virtual void HideView()
        {
            _view.HideView();
            _view.gameObject.SetActive(false);
        }

        public virtual void UpdateModel(TModel model)
        {
            _model = model;
            _view.UpdateView(_model);
        }
    }
}