using UnityEngine.Events;
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
        protected override void OnInit()
        {
            RankScrollView scrollView = _view.ScrollView;
            scrollView.OnItemUpdated += _onItemViewUpdated;
            scrollView.InitView(_model.ItemCnt);
            scrollView.RefreshItems();
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
            _view.ScrollView.InitView(_model.ItemCnt);
            _view.ScrollView.RefreshItems();
        }

        private void OnDecreaseTotalItemCallback()
        {
            int prevCnt = _model.ItemCnt;
            _model.RemoveLastItem();
            if(_model.ItemCnt == prevCnt)
                return;

            _view.ScrollView.InitView(_model.ItemCnt);
            _view.ScrollView.RefreshItems();
        }

        private void OnIncreaseVisibleLineCallback()
        {
            _view.ScrollView.IncreaseVisibleLine();
        }

        private void OnDecreaseVisibleLineCallback()
        {
            _view.ScrollView.DecreaseVisibleLine();
        }

        private void OnItemViewUpdatedCallback(RankItemView itemView)
        {
            itemView.Refresh(_model[itemView.Idx]);
        }
    }
}
