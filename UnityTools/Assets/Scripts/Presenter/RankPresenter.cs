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
            _view.OnIncreaseTotalItem += OnIncreaseTotalItem;
            _view.OnDecreaseTotalItem += OnDecreaseTotalItem;
            _view.OnIncreaseVisibleLine += OnIncreaseVisibleLine;
            _view.OnDecreaseVisibleLine += OnDecreaseVisibleLine;
            _view.ScrollView.OnItemUpdated.AddListener(OnItemViewUpdated);
        }

        void OnRandomScore()
        {
            _model.SetRandomScore();
            _view.UpdateView();
        }

        void OnIncreaseTotalItem()
        {
            _view.ScrollView.IncreaseTotalItem();
        }

        void OnDecreaseTotalItem()
        {
            _view.ScrollView.DecreaseTotalItem();
        }

        void OnIncreaseVisibleLine()
        {
            _view.ScrollView.IncreaseVisibleLine();
        }

        void OnDecreaseVisibleLine()
        {
            _view.ScrollView.DecreaseVisibleLine();
        }

        public void OnItemViewUpdated(RankItemView itemView)
        {
            itemView.UpdateView(_model.Get(itemView.Index));
        }
    }
}
