using UnityEngine;
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
                _rankPresenter = new(new(_rankModelCnt), _rankView);

            if (_rankOSAView != null && _rankOSAView.gameObject.activeInHierarchy)
            {
                RankOSAManager manager = Singletons.RankOSAManager;
                manager.Init(_rankOSAModelCnt);

                _rankOSAPresenter = new(manager.Models, _rankOSAView);
            }

            if (_timerView != null && _timerView.gameObject.activeInHierarchy)
                _timerPresenter = new(_timerView);

            if (_invenView != null && _invenView.gameObject.activeInHierarchy)
                _invenPresenter = new(_invenModelCnt, _invenView);
        }

        void OnDestroy()
        {
            _rankPresenter?.Release();
            _rankOSAPresenter?.Release();
            _timerPresenter?.Release();
            _invenPresenter?.Release();
        }
    }
}
