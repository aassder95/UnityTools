using UnityEngine;
using UnityTools.Samples.Rank;
using UnityTools.Util.Core;

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
        [SerializeField, Min(0)] private int _rankModelCnt = 10;

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
        protected override void OnInitModule()
        {
            _rankPresenter = new RankPresenter(new RankModel(_rankModelCnt), _rankView);
            _rankPresenter.Init();
        }

        protected override void OnShowModule()
        {
            _rankPresenter.Show();
        }

        protected override void OnHideModule()
        {
            _rankPresenter.Hide();
        }

        protected override void OnReleaseModule()
        {
            _rankPresenter.Release();
            _rankPresenter = null;
        }
    }
}
