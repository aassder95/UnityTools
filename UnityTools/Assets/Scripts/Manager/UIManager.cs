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

        RankModel _rankModel;
        RankPresenter _rankPresenter;

        void Awake()
        {
            _rankModel = new RankModel(10);
            _rankPresenter = new RankPresenter(_rankModel, _rankView);
        }
    }
}
