using System.Collections.Generic;
using UnityEngine;
using UnityTools.Presenter;
using UnityTools.Model;
using UnityTools.Util;

namespace UnityTools.UI
{
    public class RankView : MonoBehaviour
    {
        [SerializeField] RankScrollView _scrollView;

        RankPresenter _presenter;

        public void InitView(RankPresenter presenter)
        {
            _presenter = presenter;
            _scrollView.InitView(100);
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
