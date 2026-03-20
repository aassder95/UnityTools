using UnityEngine;
using UnityTools.Samples.Rank;
using UnityTools.Util.Core;

namespace UnityTools.Samples.Modules
{
    public class RankSampleModule : SampleModuleBase
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

        //============================================================
        //Properties
        //============================================================
        public override string ModuleKey => SampleModuleKeys.RANK;

        //============================================================
        //Logic
        //============================================================
        protected override bool OnInitModule()
        {
            if(!TryResolveView(ref _rankView))
                return false;

            int rankModelCnt = Mathf.Max(0, _rankModelCnt);
            _rankPresenter = new RankPresenter(new RankModel(rankModelCnt), _rankView);
            _rankPresenter.Init();
            return _rankPresenter.IsInit;
        }

        protected override void OnShowModule()
        {
            _rankPresenter?.Show();
        }

        protected override void OnHideModule()
        {
            _rankPresenter?.Hide();
        }

        protected override void OnReleaseModule()
        {
            _rankPresenter?.Release();
            _rankPresenter = null;
        }
    }
}

