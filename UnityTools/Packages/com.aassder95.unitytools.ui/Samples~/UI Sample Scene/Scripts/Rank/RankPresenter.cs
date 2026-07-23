using UnityEngine.Events;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.UIFramework;

namespace UnityTools.Samples.Rank
{
    public class RankPresenter : BasePresenter<RankModel, RankView>
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly UnityAction _onRandomScore;
        private readonly UnityAction _onBoostTopScore;
        private readonly UnityAction _onWaveScore;
        private readonly UnityAction<RankItemView> _onItemViewUpdated;

        //============================================================
        // Constructors
        //============================================================
        public RankPresenter(RankModel model, RankView view) : base(model, view)
        {
            _onRandomScore = _model.SetRandomScore;
            _onBoostTopScore = _model.BoostTopScores;
            _onWaveScore = _model.SetWaveScore;
            _onItemViewUpdated = OnItemViewUpdatedCallback;
        }

        //============================================================
        // Init/Register
        //============================================================
        protected override bool OnInit()
        {
            RankScrollView scrollView = _view.ScrollView;
            scrollView.OnItemUpdated += _onItemViewUpdated;
            if(scrollView.TryInitView(_model.ItemCnt) && scrollView.TryRefreshItems())
                return true;

            scrollView.OnItemUpdated -= _onItemViewUpdated;
            return false;
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
            _view.ScrollView.OnItemUpdated -= _onItemViewUpdated;
            base.UnbindEvents();
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OnIncreaseTotalItemCallback()
        {
            _model.AddItem();
            if(!_view.ScrollView.TryInitView(_model.ItemCnt) || !_view.ScrollView.TryRefreshItems())
                StopAfterFailure();
        }

        private void OnDecreaseTotalItemCallback()
        {
            int prevCnt = _model.ItemCnt;
            _model.RemoveLastItem();
            if(_model.ItemCnt == prevCnt)
                return;

            if(!_view.ScrollView.TryInitView(_model.ItemCnt) || !_view.ScrollView.TryRefreshItems())
                StopAfterFailure();
        }

        private void OnIncreaseVisibleLineCallback()
        {
            if(!_view.ScrollView.TryIncreaseVisibleLine())
                StopAfterFailure();
        }

        private void OnDecreaseVisibleLineCallback()
        {
            if(!_view.ScrollView.TryDecreaseVisibleLine())
                StopAfterFailure();
        }

        private void OnItemViewUpdatedCallback(RankItemView itemView)
        {

            RankItemModel itemModel = _model.Get(itemView.Idx);
            if(itemModel == null)
            {
                DebugLogger.LogError("Rank Item 모델을 찾을 수 없습니다. 인덱스=" + itemView.Idx);
                StopAfterFailure();
                return;
            }

            if(!itemView.TryRefresh(itemModel))
                StopAfterFailure();
        }
    }
}
