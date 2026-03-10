using UnityEngine;
using UnityTools.Presenter;
using UnityTools.UI;
using UnityTools.Util;

namespace UnityTools.Manager
{
    public class UIManager : MonoSingleton<UIManager>
    {
        [Header("Rank")]
        [SerializeField] private RankView _rankView;
        [SerializeField] private int _rankModelCnt = 10;
        private RankPresenter _rankPresenter;

        [Header("RankOSA")]
        [SerializeField] private RankOSAView _rankOSAView;
        [SerializeField] private int _rankOSAModelCnt = 10;
        private RankOSAPresenter _rankOSAPresenter;

        [Header("Timer")]
        [SerializeField] private TimerView _timerView;
        private TimerPresenter _timerPresenter;

        [Header("Inven")]
        [SerializeField] private InvenView _invenView;
        [SerializeField] private int _invenModelCnt = 50;
        private InvenPresenter _invenPresenter;

        private void Awake()
        {
            if (_rankView != null && _rankView.gameObject.activeInHierarchy)
                _rankPresenter = new(new(_rankModelCnt), _rankView);

            if (_rankOSAView != null && _rankOSAView.gameObject.activeInHierarchy)
                _rankOSAPresenter = new(new(_rankOSAModelCnt, new() { 3, 10 }), _rankOSAView);

            if (_timerView != null && _timerView.gameObject.activeInHierarchy)
                _timerPresenter = new(new(), _timerView);

            if (_invenView != null && _invenView.gameObject.activeInHierarchy)
                _invenPresenter = new(new(_invenModelCnt), _invenView);
        }

        private void Start()
        {
            _rankPresenter?.Show();
            _rankOSAPresenter?.Show();
            _timerPresenter?.Show();
            _invenPresenter?.Show();
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
