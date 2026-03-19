using UnityEngine;
using UnityTools.Samples.Rank;

namespace UnityTools.Samples.Modules
{
    public class RankSampleModule : MonoBehaviour, ISampleModule
    {
        //============================================================
        //Inspector Fields
        //============================================================
        [SerializeField] private RankView _rankView;
        [SerializeField] private int _rankModelCnt = 10;

        //============================================================
        //Fields
        //============================================================
        private RankPresenter _rankPresenter;
        private bool _isInitialized;

        //============================================================
        //Properties
        //============================================================
        public string ModuleKey => SampleModuleKeys.RANK;
        public bool IsInit => _isInitialized;

        //============================================================
        //Init/Register
        //============================================================
        public void Init()
        {
            if (_isInitialized)
                return;

            if (_rankView == null || !_rankView.gameObject.activeInHierarchy)
                return;

            _rankPresenter = new(new(_rankModelCnt), _rankView);
            _rankPresenter.Init();
            _isInitialized = _rankPresenter.IsInit;
        }

        public void Show()
        {
            if (!_isInitialized)
                return;

            _rankPresenter?.Show();
        }

        public void Release()
        {
            if (!_isInitialized)
                return;

            _rankPresenter?.Release();
            _rankPresenter = null;
            _isInitialized = false;
        }
    }
}
