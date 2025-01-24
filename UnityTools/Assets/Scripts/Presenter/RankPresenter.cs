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

            BindEvents();

            _view.InitView(_model.ItemModels.Count);
        }

        void BindEvents()
        {
            _view.OnRandomScore += OnRandomScore;
            _view.ScrollView.OnItemUpdated.AddListener(OnItemViewUpdated);
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
