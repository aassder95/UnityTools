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

        public void OnIncreaseItemView()
        {
            _scrollView.SetVisibleItemCount(_scrollView.VisibleItemCount + 1);
        }

        public void OnDecreaseItemView()
        {
            _scrollView.SetVisibleItemCount(_scrollView.VisibleItemCount - 1);
        }
    }
}
