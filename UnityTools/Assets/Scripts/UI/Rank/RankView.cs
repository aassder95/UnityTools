using UnityEngine;
using UnityEngine.Events;
using UnityTools.Model;
using UnityTools.Util;

namespace UnityTools.UI
{
    public class RankView : BaseView<RankModel>
    {
        [SerializeField] private RankScrollView _scrollView;

        public RankScrollView ScrollView => _scrollView;

        public event UnityAction OnRandomScore;
        public event UnityAction OnIncreaseTotalItem;
        public event UnityAction OnDecreaseTotalItem;
        public event UnityAction OnIncreaseVisibleLine;
        public event UnityAction OnDecreaseVisibleLine;

        public override void Refresh(RankModel model)
        {
            _scrollView.UpdateItemView();
        }

        public void OnRandomScoreInspector()
        {
            OnRandomScore?.Invoke();
        }

        public void OnIncreaseTotalItemInspector()
        {
            OnIncreaseTotalItem?.Invoke();
        }

        public void OnDecreaseTotalItemInspector()
        {
            OnDecreaseTotalItem?.Invoke();
        }

        public void OnIncreaseVisibleLineInspector()
        {
            OnIncreaseVisibleLine?.Invoke();
        }

        public void OnDecreaseVisibleLineInspector()
        {
            OnDecreaseVisibleLine?.Invoke();
        }
    }
}
