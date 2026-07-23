using UnityEngine;
using UnityTools.Samples.Inven;
using UnityTools.Util.Core;

namespace UnityTools.Samples.Modules
{
    public class InvenSampleModule : SampleModuleBase
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Inventory View")] [SerializeField] private InvenView _invenView;

        [Header("Inventory Data")] [Min(0)] [SerializeField] private int _invenModelCnt = 50;

        //============================================================
        // Fields
        //============================================================
        private InvenPresenter _invenPresenter;

        //============================================================
        // Properties
        //============================================================
        public override string ModuleKey => SampleModuleKeys.Inven;

        //============================================================
        // Logic
        //============================================================
        protected override void OnInitModule()
        {
            _invenPresenter = new InvenPresenter(new InvenModel(_invenModelCnt), _invenView);
            _invenPresenter.Init();
        }

        protected override void OnShowModule()
        {
            _invenPresenter.Show();
        }

        protected override void OnHideModule()
        {
            _invenPresenter.Hide();
        }

        protected override void OnReleaseModule()
        {
            _invenPresenter.Release();
            _invenPresenter = null;
        }
    }
}
