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

            _scrollView.OnItemIndexUpdated += _presenter.OnItemViewUpdated;
            _scrollView.InitView(totalCnt);
        }

        public void UpdateView()
        {
            _scrollView.UpdateItems();
        }

        public void OnRandomScore()
        {
            _presenter.OnRandomScore();
        }

        public void OnIncreaseTotalItemView()
        {
            _scrollView.SetTotalItemCount(_scrollView.TotalItemCount + 1);
        }

        public void OnDecreaseTotalItemView()
        {
            _scrollView.SetTotalItemCount(_scrollView.TotalItemCount - 1);
        }

        public void OnIncreaseVisibleItemView()
        {
            _scrollView.SetVisibleItemCount(_scrollView.VisibleItemCount + 1);
        }

        public void OnDecreaseVisibleItemView()
        {
            _scrollView.SetVisibleItemCount(_scrollView.VisibleItemCount - 1);
        }
    }
}
