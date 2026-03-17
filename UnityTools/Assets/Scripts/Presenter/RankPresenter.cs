using UnityTools.Model;
using UnityTools.UI;
using UnityTools.Util;

namespace UnityTools.Presenter
{
    //============================================================
    //Logic
    //============================================================
    public class RankPresenter : BasePresenter<RankModel, RankView>
    {
        public RankPresenter(RankModel model, RankView view) : base(model, view)
        {
            
        }

        public override void Init()
        {
            base.Init();
            VIEW.ScrollView.InitView(MODEL.ItemCount);
        }

        protected override void BindEvents()
        {
            base.BindEvents();
            VIEW.OnRandomScore += OnRandomScore;
            VIEW.OnIncreaseTotalItem += OnIncreaseTotalItem;
            VIEW.OnDecreaseTotalItem += OnDecreaseTotalItem;
            VIEW.OnIncreaseVisibleLine += OnIncreaseVisibleLine;
            VIEW.OnDecreaseVisibleLine += OnDecreaseVisibleLine;
            VIEW.ScrollView.OnItemUpdated += OnItemViewUpdated;
        }

        protected override void UnbindEvents()
        {
            VIEW.OnRandomScore -= OnRandomScore;
            VIEW.OnIncreaseTotalItem -= OnIncreaseTotalItem;
            VIEW.OnDecreaseTotalItem -= OnDecreaseTotalItem;
            VIEW.OnIncreaseVisibleLine -= OnIncreaseVisibleLine;
            VIEW.OnDecreaseVisibleLine -= OnDecreaseVisibleLine;
            VIEW.ScrollView.OnItemUpdated -= OnItemViewUpdated;
            base.UnbindEvents();       
        }

        private void OnRandomScore()
        {
            MODEL.SetRandomScore();
        }

        private void OnIncreaseTotalItem()
        {
            VIEW.ScrollView.IncreaseTotalItem();
        }

        private void OnDecreaseTotalItem()
        {
            VIEW.ScrollView.DecreaseTotalItem();
        }

        private void OnIncreaseVisibleLine()
        {
            VIEW.ScrollView.IncreaseVisibleLine();
        }

        private void OnDecreaseVisibleLine()
        {
            VIEW.ScrollView.DecreaseVisibleLine();
        }

        private void OnItemViewUpdated(RankItemView itemView)
        {
            itemView.Refresh(MODEL.Get(itemView.Index));
        }
    }
}
