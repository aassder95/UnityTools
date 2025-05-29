using UnityEngine;
using UnityTools.Model;
using UnityTools.Presenter;
using UnityTools.UI;
using UnityTools.Util;

namespace UnityTools.Manager
{
    public class UIManager : Singleton<UIManager>
    {
        [Header("Rank")]
        [SerializeField] RankView _rankView;
        [SerializeField] int _rankModelCnt = 10;
        RankPresenter _rankPresenter;

        [Header("RankOSA")]
        [SerializeField] RankOSAView _rankOSAView;
        [SerializeField] int _rankOSAModelCnt = 10;
        RankOSAPresenter _rankOSAPresenter;

        [Header("Timer")]
        [SerializeField] TimerView _timerView;
        TimerPresenter _timerPresenter;

        [Header("Inven")]
        [SerializeField] InvenView _invenView;
        [SerializeField] int _invenModelCnt = 50;
        InvenPresenter _invenPresenter;

        void Start()
        {
            if (_rankView != null && _rankView.gameObject.activeInHierarchy)
            {
                RankModel model = new(_rankModelCnt);
                _rankPresenter = new(model, _rankView);
            }

            if (_rankOSAView != null && _rankOSAView.gameObject.activeInHierarchy)
            {
                RankModel model = new(_rankOSAModelCnt);
                _rankOSAPresenter = new(model, _rankOSAView);
            }

            if (_timerView != null && _timerView.gameObject.activeInHierarchy)
            {
                _timerPresenter = new(_timerView);
            }

            if (_invenView != null && _invenView.gameObject.activeInHierarchy)
            {
                _invenPresenter = new(_invenModelCnt, _invenView);
            }
        }
    }
}
