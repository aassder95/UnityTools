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

        [Header("Timer")]
        [SerializeField] TimerView _timerView;
        TimerPresenter _timerPresenter;

        [Header("Inven")]
        [SerializeField] InvenView _invenView;
        [SerializeField] int _invenModelCnt = 50;
        InvenPresenter _invenPresenter;

        void Start()
        {
            if (_rankView != null)
            {
                RankModel model = new RankModel(_rankModelCnt);
                _rankPresenter = new RankPresenter(model, _rankView);
            }

            if (_timerView != null)
            {
                _timerPresenter = new TimerPresenter(_timerView);
            }

            if (_invenView != null)
            {
                _invenPresenter = new InvenPresenter(_invenModelCnt, _invenView);
            }
        }
    }
}
