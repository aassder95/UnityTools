using UnityTools.Model;
using UnityTools.UI;

namespace UnityTools.Presenter
{
    public class RankPresenter
    {
        readonly RankModel _model;
        readonly RankView _view;

        public RankPresenter(RankView view)
        {
            _model = new RankModel(10);
            _view = view;

            _view.InitView(this, _model.ItemModels.Count);
            UpdateView();
        }

        void UpdateView()
        {
            _view.UpdateView(_model.ItemModels);
        }

        public void OnRandomScore()
        {
            _model.SetRandomScore();
            UpdateView();
        }
    }
}
