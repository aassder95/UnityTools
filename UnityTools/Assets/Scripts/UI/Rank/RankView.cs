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
    }
}
