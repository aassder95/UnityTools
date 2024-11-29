using UnityEngine;
using UnityTools.Util;

namespace UnityTools.UI
{
    public class RankView : MonoBehaviour
    {
        [SerializeField] RankScrollView _scrollView;

        public RankScrollView ScrollView => _scrollView;

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
            EventDispatcher.Instance.Dispatch(EEventDispatcherType.RankRandomScore, this);
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
