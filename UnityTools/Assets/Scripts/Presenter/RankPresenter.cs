using UnityTools.Model;
using UnityTools.UI;

namespace UnityTools.Presenter
{
    public class RankPresenter
    {
        readonly RankModel _model;
        readonly RankView _view;

        public RankPresenter(RankModel model, RankView view)
        {
            _model = model;
            _view = view;

            _view.InitView(this, _model.ItemModels.Count);
        }

        public void OnRandomScore()
        {
            _model.SetRandomScore();
            _view.UpdateView();
        }

        public void OnItemViewUpdated(RankItemView itemView)
        {
            itemView.UpdateView(_model.GetItemModel(itemView.Index));
        }
    }
}
