using UnityTools.Model;
using UnityTools.UI;
using UnityTools.Util;
using UnityEngine.Events;

namespace UnityTools.Presenter
{
    //============================================================
    //Logic
    //============================================================
    public class RankPresenter : BasePresenter<RankModel, RankView>
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly UnityAction _onRandomScore;
        private readonly UnityAction _onIncreaseTotalItem;
        private readonly UnityAction _onDecreaseTotalItem;
        private readonly UnityAction _onIncreaseVisibleLine;
        private readonly UnityAction _onDecreaseVisibleLine;
        private readonly UnityAction<RankItemView> _onItemViewUpdated;

        //============================================================
        //Constructors
        //============================================================
        public RankPresenter(RankModel model, RankView view) : base(model, view)
        {
            _onRandomScore = MODEL.SetRandomScore;
            _onIncreaseTotalItem = VIEW.ScrollView.IncreaseTotalItem;
            _onDecreaseTotalItem = VIEW.ScrollView.DecreaseTotalItem;
            _onIncreaseVisibleLine = VIEW.ScrollView.IncreaseVisibleLine;
            _onDecreaseVisibleLine = VIEW.ScrollView.DecreaseVisibleLine;
            _onItemViewUpdated = itemView =>
            {
                if(itemView == null)
                    return;

                itemView.Refresh(MODEL.Get(itemView.Index));
            };
        }

        //============================================================
        //Init/Register
        //============================================================
        public override void Init()
        {
            base.Init();
            VIEW.ScrollView.InitView(MODEL.ItemCount);
        }

        protected override void BindEvents()
        {
            base.BindEvents();
            VIEW.OnRandomScore += _onRandomScore;
            VIEW.OnIncreaseTotalItem += _onIncreaseTotalItem;
            VIEW.OnDecreaseTotalItem += _onDecreaseTotalItem;
            VIEW.OnIncreaseVisibleLine += _onIncreaseVisibleLine;
            VIEW.OnDecreaseVisibleLine += _onDecreaseVisibleLine;
            VIEW.ScrollView.OnItemUpdated += _onItemViewUpdated;
        }

        protected override void UnbindEvents()
        {
            VIEW.OnRandomScore -= _onRandomScore;
            VIEW.OnIncreaseTotalItem -= _onIncreaseTotalItem;
            VIEW.OnDecreaseTotalItem -= _onDecreaseTotalItem;
            VIEW.OnIncreaseVisibleLine -= _onIncreaseVisibleLine;
            VIEW.OnDecreaseVisibleLine -= _onDecreaseVisibleLine;
            VIEW.ScrollView.OnItemUpdated -= _onItemViewUpdated;
            base.UnbindEvents();       
        }
    }
}
