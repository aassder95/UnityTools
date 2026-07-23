using UnityEngine;
using UnityEngine.Events;
using UnityTools.Samples.Util;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.UIFramework;

namespace UnityTools.Samples.Rank
{
    public class RankView : BaseView<RankModel>
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private RankScrollView _scrollView;

        //============================================================
        // Fields
        //============================================================
        private bool _isTestLayoutBuilt;

        //============================================================
        // Events
        //============================================================
        public event UnityAction OnRandomScore { add => _onRandomScore += value; remove => _onRandomScore -= value; }
        public event UnityAction OnBoostTopScore { add => _onBoostTopScore += value; remove => _onBoostTopScore -= value; }
        public event UnityAction OnWaveScore { add => _onWaveScore += value; remove => _onWaveScore -= value; }
        public event UnityAction OnIncreaseTotalItem { add => _onIncreaseTotalItem += value; remove => _onIncreaseTotalItem -= value; }
        public event UnityAction OnDecreaseTotalItem { add => _onDecreaseTotalItem += value; remove => _onDecreaseTotalItem -= value; }
        public event UnityAction OnIncreaseVisibleLine { add => _onIncreaseVisibleLine += value; remove => _onIncreaseVisibleLine -= value; }
        public event UnityAction OnDecreaseVisibleLine { add => _onDecreaseVisibleLine += value; remove => _onDecreaseVisibleLine -= value; }
        private event UnityAction _onRandomScore;
        private event UnityAction _onBoostTopScore;
        private event UnityAction _onWaveScore;
        private event UnityAction _onIncreaseTotalItem;
        private event UnityAction _onDecreaseTotalItem;
        private event UnityAction _onIncreaseVisibleLine;
        private event UnityAction _onDecreaseVisibleLine;

        //============================================================
        // Properties
        //============================================================
        public RankScrollView ScrollView => _scrollView;

        //============================================================
        // Init/Register
        //============================================================
        protected override bool OnInit()
        {
            return BuildTestLayout();
        }

        //============================================================
        // Logic
        //============================================================
        protected override bool OnRefresh(RankModel model)
        {
            return _scrollView.TryRefreshItems();
        }

        //============================================================
        // Callbacks
        //============================================================
        public void OnRandomScoreInspector()
        {
            _onRandomScore?.Invoke();
        }

        public void OnBoostTopScoreInspector()
        {
            _onBoostTopScore?.Invoke();
        }

        public void OnWaveScoreInspector()
        {
            _onWaveScore?.Invoke();
        }

        public void OnIncreaseTotalItemInspector()
        {
            _onIncreaseTotalItem?.Invoke();
        }

        public void OnDecreaseTotalItemInspector()
        {
            _onDecreaseTotalItem?.Invoke();
        }

        public void OnIncreaseVisibleLineInspector()
        {
            _onIncreaseVisibleLine?.Invoke();
        }

        public void OnDecreaseVisibleLineInspector()
        {
            _onDecreaseVisibleLine?.Invoke();
        }

        //============================================================
        // Utilities
        //============================================================
        private bool BuildTestLayout()
        {
            if(_isTestLayoutBuilt)
                return true;

            SampleTestLayout layout = SampleTestUiBuilder.Build(transform, "Rank Test Sample", "Random only rerolls all / Item +/- keeps existing scores", 7);
            RectTransform rtScroll = _scrollView.transform as RectTransform;
            SampleTestUiBuilder.ReparentToContent(rtScroll, layout.RtContentViewport, Vector2.zero, Vector2.zero);
            SampleTestUiBuilder.CreateActionButton(layout.RtControls, "BtnTestRandom", "Random", OnRandomScoreInspector);
            SampleTestUiBuilder.CreateActionButton(layout.RtControls, "BtnTestBoostTop", "Top+Boost", OnBoostTopScoreInspector);
            SampleTestUiBuilder.CreateActionButton(layout.RtControls, "BtnTestWave", "Wave", OnWaveScoreInspector);
            SampleTestUiBuilder.CreateActionButton(layout.RtControls, "BtnTestPlusItem", "Item +", OnIncreaseTotalItemInspector);
            SampleTestUiBuilder.CreateActionButton(layout.RtControls, "BtnTestMinusItem", "Item -", OnDecreaseTotalItemInspector);
            SampleTestUiBuilder.CreateActionButton(layout.RtControls, "BtnTestPlusLine", "Line +", OnIncreaseVisibleLineInspector);
            SampleTestUiBuilder.CreateActionButton(layout.RtControls, "BtnTestMinusLine", "Line -", OnDecreaseVisibleLineInspector);

            _isTestLayoutBuilt = true;
            return true;
        }
    }
}