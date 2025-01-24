using UnityEngine;
using UnityEngine.Events;

namespace UnityTools.UI
{
    public class RankView : MonoBehaviour
    {
        [SerializeField] RankScrollView _scrollView;

        public RankScrollView ScrollView => _scrollView;

        public event UnityAction OnRandomScore;
        public event UnityAction OnIncreaseTotalItemView;
        public event UnityAction OnDecreaseTotalItemView;
        public event UnityAction OnIncreaseVisibleItemView;
        public event UnityAction OnDecreaseVisibleItemView;

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

        public void OnIncreaseTotalItemViewInspector()
        {
            OnIncreaseTotalItemView?.Invoke();
        }

        public void OnDecreaseTotalItemViewInspector()
        {
            OnDecreaseTotalItemView?.Invoke();
        }

        public void OnIncreaseVisibleItemViewInspector()
        {
            OnIncreaseVisibleItemView?.Invoke();
        }

        public void OnDecreaseVisibleItemViewInspector()
        {
            OnDecreaseVisibleItemView?.Invoke();
        }
    }
}
