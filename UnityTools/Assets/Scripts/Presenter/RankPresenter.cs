using UnityTools.Model;
using UnityTools.UI;

namespace UnityTools.Presenter
{
    public class RankPresenter : BasePresenter<RankModel, RankView>
    {
        public RankPresenter(RankModel model, RankView view) : base(model, view) { }

        protected override void BindEvents()
        {
            _view.OnRandomScore += OnRandomScore;
            _view.OnIncreaseTotalItem += OnIncreaseTotalItem;
            _view.OnDecreaseTotalItem += OnDecreaseTotalItem;
            _view.OnIncreaseVisibleLine += OnIncreaseVisibleLine;
            _view.OnDecreaseVisibleLine += OnDecreaseVisibleLine;
            _view.ScrollView.OnItemUpdated.AddListener(OnItemViewUpdated);
        }

        protected override void UnbindEvents()
        {
            _view.OnRandomScore -= OnRandomScore;
            _view.OnIncreaseTotalItem -= OnIncreaseTotalItem;
            _view.OnDecreaseTotalItem -= OnDecreaseTotalItem;
            _view.OnIncreaseVisibleLine -= OnIncreaseVisibleLine;
            _view.OnDecreaseVisibleLine -= OnDecreaseVisibleLine;
            _view.ScrollView.OnItemUpdated.RemoveListener(OnItemViewUpdated);
        }

        void OnRandomScore()
        {
            _model.SetRandomScore();
            _view.UpdateView(_model);
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
