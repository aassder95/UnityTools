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
        [SerializeField]
        private RankView _rankView;
        [SerializeField] private int _rankModelCnt = 10;
        private RankPresenter _rankPresenter;

        [Header("RankOSA")]
        [SerializeField]
        private RankOSAView _rankOSAView;
        [SerializeField] private int _rankOSAModelCnt = 10;
        private RankOSAPresenter _rankOSAPresenter;

        [Header("Timer")]
        [SerializeField]
        private TimerView _timerView;
        private TimerPresenter _timerPresenter;

        [Header("Inven")]
        [SerializeField]
        private InvenView _invenView;
        [SerializeField] private int _invenModelCnt = 50;
        private InvenPresenter _invenPresenter;

        private void Start()
        {
            if (_rankView != null && _rankView.gameObject.activeInHierarchy)
            {
                RankModel model = new(_rankModelCnt);
                _rankPresenter = new(model, _rankView);
                _rankPresenter.Init();
                _rankPresenter.Show();
            }

            if (_rankOSAView != null && _rankOSAView.gameObject.activeInHierarchy)
            {
                RankOSAModel model = new(_rankOSAModelCnt, new() { 3, 10 });
                _rankOSAPresenter = new(model, _rankOSAView);
                _rankOSAPresenter.Init();
                _rankOSAPresenter.Show();
            }

            if (_timerView != null && _timerView.gameObject.activeInHierarchy)
            {
                TimerModel model = new();
                _timerPresenter = new(model, _timerView);
                _timerPresenter.Init();
                _timerPresenter.Show();
            }

            if (_invenView != null && _invenView.gameObject.activeInHierarchy)
            {
                InvenModel model = new(_invenModelCnt);
                _invenPresenter = new(model, _invenView);
                _invenPresenter.Init();
                _invenPresenter.Show();
            }
        }

        private void OnDestroy()
        {
            _rankPresenter?.Release();
            _rankOSAPresenter?.Release();
            _timerPresenter?.Release();
            _invenPresenter?.Release();
        }
    }
}
