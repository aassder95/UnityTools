using UnityEngine;
using UnityEngine.Events;

namespace UnityTools.UI
{
    public class RankView : MonoBehaviour
    {
        [SerializeField] RankScrollView _scrollView;

        public RankScrollView ScrollView => _scrollView;

        public event UnityAction OnRandomScore;

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
            _scrollView.IncreaseTotalItem();
        }

        public void OnDecreaseTotalItemViewInspector()
        {
            _scrollView.DecreaseTotalItem();
        }

        public void OnIncreaseVisibleItemViewInspector()
        {
            _scrollView.IncreaseVisibleItem();
        }

        public void OnDecreaseVisibleItemViewInspector()
        {
            _scrollView.DecreaseVisibleItem();
        }
    }
}
