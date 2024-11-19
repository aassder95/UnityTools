using System.Collections.Generic;
using UnityEngine;
using UnityTools.Presenter;
using UnityTools.Model;

namespace UnityTools.UI
{
    public class RankView : MonoBehaviour
    {
        [SerializeField] RankScrollView _scrollView;

        RankPresenter _presenter;

        public void InitView(RankPresenter presenter, int totalCnt)
        {
            _presenter = presenter;
            _scrollView.InitView(totalCnt);
        }

        public void UpdateView(List<RankItemModel> itemModels)
        {
            _scrollView.UpdateView(itemModels);
        }

        public void OnRandomScore()
        {
            _presenter.OnRandomScore();
        }
    }
}
