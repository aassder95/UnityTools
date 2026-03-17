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
            _onRandomScore = _model.SetRandomScore;
            _onIncreaseTotalItem = _view.ScrollView.IncreaseTotalItem;
            _onDecreaseTotalItem = _view.ScrollView.DecreaseTotalItem;
            _onIncreaseVisibleLine = _view.ScrollView.IncreaseVisibleLine;
            _onDecreaseVisibleLine = _view.ScrollView.DecreaseVisibleLine;
            _onItemViewUpdated = itemView =>
            {
                if(itemView == null)
                    return;

                itemView.Refresh(_model.Get(itemView.Index));
            };
        }

        //============================================================
        //Init/Register
        //============================================================
        public override void Init()
        {
            base.Init();
            _view.ScrollView.InitView(_model.ItemCount);
        }

        protected override void BindEvents()
        {
            base.BindEvents();
            _view.OnRandomScore += _onRandomScore;
            _view.OnIncreaseTotalItem += _onIncreaseTotalItem;
            _view.OnDecreaseTotalItem += _onDecreaseTotalItem;
            _view.OnIncreaseVisibleLine += _onIncreaseVisibleLine;
            _view.OnDecreaseVisibleLine += _onDecreaseVisibleLine;
            _view.ScrollView.OnItemUpdated += _onItemViewUpdated;
        }

        protected override void UnbindEvents()
        {
            _view.OnRandomScore -= _onRandomScore;
            _view.OnIncreaseTotalItem -= _onIncreaseTotalItem;
            _view.OnDecreaseTotalItem -= _onDecreaseTotalItem;
            _view.OnIncreaseVisibleLine -= _onIncreaseVisibleLine;
            _view.OnDecreaseVisibleLine -= _onDecreaseVisibleLine;
            _view.ScrollView.OnItemUpdated -= _onItemViewUpdated;
            base.UnbindEvents();       
        }
    }
}
