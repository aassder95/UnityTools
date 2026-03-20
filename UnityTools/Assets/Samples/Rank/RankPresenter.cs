using UnityEngine.Events;
using UnityTools.Util.UIFramework;

namespace UnityTools.Samples.Rank
{
    public class RankPresenter : BasePresenter<RankModel, RankView>
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly UnityAction _onRandomScore;
        private readonly UnityAction _onBoostTopScore;
        private readonly UnityAction _onWaveScore;
        private readonly UnityAction<RankItemView> _onItemViewUpdated;

        //============================================================
        //Constructors
        //============================================================
        public RankPresenter(RankModel model, RankView view) : base(model, view)
        {
            _onRandomScore = _model.SetRandomScore;
            _onBoostTopScore = _model.BoostTopScores;
            _onWaveScore = _model.SetWaveScore;
            _onItemViewUpdated = OnItemViewUpdatedCallback;
        }

        //============================================================
        //Init/Register
        //============================================================
        protected override void OnInit()
        {
            if(_view.ScrollView == null)
                return;

            _view.ScrollView.InitView(_model.ItemCount);
        }

        protected override void BindEvents()
        {
            base.BindEvents();
            _view.OnRandomScore += _onRandomScore;
            _view.OnBoostTopScore += _onBoostTopScore;
            _view.OnWaveScore += _onWaveScore;
            _view.OnIncreaseTotalItem += OnIncreaseTotalItemCallback;
            _view.OnDecreaseTotalItem += OnDecreaseTotalItemCallback;
            _view.OnIncreaseVisibleLine += OnIncreaseVisibleLineCallback;
            _view.OnDecreaseVisibleLine += OnDecreaseVisibleLineCallback;

            if(_view.ScrollView == null)
                return;

            _view.ScrollView.OnItemUpdated += _onItemViewUpdated;
            _view.ScrollView.RefreshItems();
        }

        protected override void UnbindEvents()
        {
            _view.OnRandomScore -= _onRandomScore;
            _view.OnBoostTopScore -= _onBoostTopScore;
            _view.OnWaveScore -= _onWaveScore;
            _view.OnIncreaseTotalItem -= OnIncreaseTotalItemCallback;
            _view.OnDecreaseTotalItem -= OnDecreaseTotalItemCallback;
            _view.OnIncreaseVisibleLine -= OnIncreaseVisibleLineCallback;
            _view.OnDecreaseVisibleLine -= OnDecreaseVisibleLineCallback;

            if(_view.ScrollView != null)
                _view.ScrollView.OnItemUpdated -= _onItemViewUpdated;

            base.UnbindEvents();
        }

        //============================================================
        //Callbacks
        //============================================================
        private void OnIncreaseTotalItemCallback()
        {
            if(_view.ScrollView == null)
                return;

            _model.AddItemAndRandomize();
            _view.ScrollView.InitView(_model.ItemCount);
            _view.ScrollView.RefreshItems();
        }

        private void OnDecreaseTotalItemCallback()
        {
            if(_view.ScrollView == null)
                return;

            _model.RemoveLastItemAndRandomize();
            _view.ScrollView.InitView(_model.ItemCount);
            _view.ScrollView.RefreshItems();
        }

        private void OnIncreaseVisibleLineCallback()
        {
            _view.ScrollView?.IncreaseVisibleLine();
        }

        private void OnDecreaseVisibleLineCallback()
        {
            _view.ScrollView?.DecreaseVisibleLine();
        }

        private void OnItemViewUpdatedCallback(RankItemView itemView)
        {
            if(itemView == null)
                return;

            if(!itemView.IsInit)
                itemView.Init();

            RankItemModel itemModel = _model.Get(itemView.Index);
            if(itemModel == null)
                return;

            itemView.Refresh(itemModel);
        }
    }
}
