using UnityEngine;
using UnityTools.Samples.Rank;
using UnityTools.Util.Core;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Samples.Modules
{
    public class RankSampleModule : SampleModuleBase
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Rank View")]
        [SerializeField] private RankView _rankView;

        [Header("Rank Data")]
        [SerializeField] private int _rankModelCnt = 10;

        //============================================================
        // Fields
        //============================================================
        private RankPresenter _rankPresenter;

        //============================================================
        // Properties
        //============================================================
        public override string ModuleKey => SampleModuleKeys.Rank;

        //============================================================
        // Logic
        //============================================================
        protected override bool OnInitModule()
        {
            if(_rankModelCnt < 0)
            {
                DebugLogger.LogError("RankSampleModule의 Model Count는 0 이상이어야 합니다. 값=" + _rankModelCnt, this);
                return false;
            }

            RankPresenter presenter = new RankPresenter(new RankModel(_rankModelCnt), _rankView);
            if(!presenter.Init())
                return false;

            _rankPresenter = presenter;
            return true;
        }

        protected override bool OnShowModule()
        {
            return _rankPresenter.Show();
        }

        protected override bool OnHideModule()
        {
            return _rankPresenter.Hide();
        }

        protected override bool OnReleaseModule()
        {
            bool isSuccess = _rankPresenter.Release();
            _rankPresenter = null;
            return isSuccess;
        }
    }
}
