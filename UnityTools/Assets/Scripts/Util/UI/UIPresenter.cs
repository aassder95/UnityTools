namespace UnityTools.Util
{
    public class UIPresenter<TModel, TView> where TModel : UIModel, new() where TView : UIView
    {
        TModel _model;
        TView _view;

        public TModel Model => _model;
        public TView View => _view;

        public void Init(TView view)
        {
            _model = new TModel();
            _view = view;
        }
    }
}