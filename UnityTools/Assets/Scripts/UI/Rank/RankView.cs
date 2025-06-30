using UnityEngine;
using UnityEngine.Events;
using UnityTools.Model;

namespace UnityTools.UI
{
    public class RankView : MonoBehaviour, IView<RankModel>
    {
        [SerializeField] RankScrollView _scrollView;

        public RankScrollView ScrollView => _scrollView;

        public event UnityAction OnRandomScore;
        public event UnityAction OnIncreaseTotalItem;
        public event UnityAction OnDecreaseTotalItem;
        public event UnityAction OnIncreaseVisibleLine;
        public event UnityAction OnDecreaseVisibleLine;

        public void InitView(RankModel model)
        {
            _scrollView.InitView(model.ItemModels.Count);
        }

        public void ShowView(RankModel model) { }

        public void HideView() { }

        public void UpdateView(RankModel model)
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
