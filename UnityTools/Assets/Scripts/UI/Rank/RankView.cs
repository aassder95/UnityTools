using UnityEngine;
using UnityTools.Presenter;

namespace UnityTools.UI
{
    public class RankView : MonoBehaviour
    {
        [SerializeField] RankScrollView _scrollView;

        RankPresenter _presenter;

        public void InitView(RankPresenter presenter, int totalCnt)
        {
            _presenter = presenter;

            _scrollView.OnItemUpdated += _presenter.OnItemViewUpdated;
            _scrollView.InitView(totalCnt);
        }

        public void UpdateView()
        {
            _scrollView.UpdateView();
        }

        public void OnRandomScore()
        {
            _presenter.OnRandomScore();
        }

        public void OnIncreaseTotalItemView()
        {
            _scrollView.SetTotalCount(_scrollView.TotalCount + 3);
        }

        public void OnDecreaseTotalItemView()
        {
            _scrollView.SetTotalCount(_scrollView.TotalCount - 3);
        }

        public void OnIncreaseVisibleItemView()
        {
            _scrollView.SetVisibleCount(_scrollView.VisibleCount + 3);
        }

        public void OnDecreaseVisibleItemView()
        {
            _scrollView.SetVisibleCount(_scrollView.VisibleCount - 3);
        }
    }
}
