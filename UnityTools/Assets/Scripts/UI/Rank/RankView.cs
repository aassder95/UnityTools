using UnityEngine;
using UnityEngine.Events;

namespace UnityTools.UI
{
    public class RankView : MonoBehaviour
    {
        [SerializeField] RankScrollView _scrollView;

        public RankScrollView ScrollView => _scrollView;

        public event UnityAction OnRandomScore;
        public event UnityAction OnIncreaseTotalItem;
        public event UnityAction OnDecreaseTotalItem;
        public event UnityAction OnIncreaseVisibleLine;
        public event UnityAction OnDecreaseVisibleLine;

        public void InitView(int totalCnt)
        {
            _scrollView.InitView(totalCnt);
        }

        public void UpdateView()
        {
            _scrollView.UpdateView();
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
