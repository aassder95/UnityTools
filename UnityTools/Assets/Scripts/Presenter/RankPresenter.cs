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

            _view.InitView(_model.Count);
        }

        void BindEvents()
        {
            _view.OnRandomScore += OnRandomScore;
            _view.OnIncreaseTotalItemView += OnIncreaseTotalItemView;
            _view.OnDecreaseTotalItemView += OnDecreaseTotalItemView;
            _view.OnIncreaseVisibleItemView += OnIncreaseVisibleItemView;
            _view.OnDecreaseVisibleItemView += OnDecreaseVisibleItemView;
            _view.ScrollView.OnItemUpdated.AddListener(OnItemViewUpdated);
        }

        void OnRandomScore()
        {
            _model.SetRandomScore();
            _view.UpdateView();
        }

        void OnIncreaseTotalItemView()
        {
            // _view.ScrollView.IncreaseTotalItem();
        }

        void OnDecreaseTotalItemView()
        {
            // _view.ScrollView.DecreaseTotalItem();
        }

        void OnIncreaseVisibleItemView()
        {
            // _view.ScrollView.IncreaseVisibleItem();
        }

        void OnDecreaseVisibleItemView()
        {
            // _view.ScrollView.DecreaseVisibleItem();
        }

        public void OnItemViewUpdated(RankItemView itemView)
        {
            itemView.UpdateView(_model.Get(itemView.Index));
        }
    }
}
