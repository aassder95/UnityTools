using UnityEngine;
using UnityEngine.Events;
using UnityTools.Model;
using UnityTools.Util;

namespace UnityTools.UI
{
    public class RankView : BaseView<RankModel>
    {
        //============================================================
        //Inspector Fields
        //============================================================
        [SerializeField] private RankScrollView _scrollView;

        //============================================================
        //Events
        //============================================================
        public event UnityAction OnRandomScore { add => _onRandomScore += value; remove => _onRandomScore -= value; }
        public event UnityAction OnIncreaseTotalItem { add => _onIncreaseTotalItem += value; remove => _onIncreaseTotalItem -= value; }
        public event UnityAction OnDecreaseTotalItem { add => _onDecreaseTotalItem += value; remove => _onDecreaseTotalItem -= value; }
        public event UnityAction OnIncreaseVisibleLine { add => _onIncreaseVisibleLine += value; remove => _onIncreaseVisibleLine -= value; }
        public event UnityAction OnDecreaseVisibleLine { add => _onDecreaseVisibleLine += value; remove => _onDecreaseVisibleLine -= value; }
        private event UnityAction _onRandomScore;
        private event UnityAction _onIncreaseTotalItem;
        private event UnityAction _onDecreaseTotalItem;
        private event UnityAction _onIncreaseVisibleLine;
        private event UnityAction _onDecreaseVisibleLine;

        //============================================================
        //Properties
        //============================================================
        public RankScrollView ScrollView => _scrollView;

        //============================================================
        //Logic
        //============================================================
        public override void Refresh(RankModel model)
        {
            if(model == null || _scrollView == null)
                return;

            _scrollView.UpdateItemView();
        }

        //============================================================
        //Callbacks
        //============================================================
        public void OnRandomScoreInspector()
        {
            _onRandomScore?.Invoke();
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
    }
}
