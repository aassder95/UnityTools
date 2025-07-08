using UnityTools.Model;
using UnityTools.UI;
using UnityTools.Util;

namespace UnityTools.Presenter
{
    public class RankPresenter : BasePresenter<RankModel, RankView>
    {
        public RankPresenter(RankModel model, RankView view) : base(model, view)
        {
            
        }

        public override void Init()
        {
            base.Init();
            _view.ScrollView.InitView(_model.ItemCount);
        }

        protected override void BindEvents()
        {
            base.BindEvents();
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
            base.UnbindEvents();       
        }

        private void OnRandomScore()
        {
            _model.SetRandomScore();
        }

        private void OnIncreaseTotalItem()
        {
            _view.ScrollView.IncreaseTotalItem();
        }

        private void OnDecreaseTotalItem()
        {
            _view.ScrollView.DecreaseTotalItem();
        }

        private void OnIncreaseVisibleLine()
        {
            _view.ScrollView.IncreaseVisibleLine();
        }

        private void OnDecreaseVisibleLine()
        {
            _view.ScrollView.DecreaseVisibleLine();
        }

        private void OnItemViewUpdated(RankItemView itemView)
        {
            itemView.Refresh(_model.Get(itemView.Index));
        }
    }
}
