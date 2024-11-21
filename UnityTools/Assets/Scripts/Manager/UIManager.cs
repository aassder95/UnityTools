using UnityEngine;
using UnityTools.Model;
using UnityTools.Presenter;
using UnityTools.UI;
using UnityTools.Util;

namespace UnityTools.Manager
{
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] RankView _rankView;

        RankPresenter _rankPresenter;

        void Start()
        {
            if (_rankView != null)
            {
                RankModel model = new RankModel(10);
                _rankPresenter = new RankPresenter(model, _rankView);
            }
        }
    }
}
