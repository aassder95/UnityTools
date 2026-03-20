using UnityEngine;
using UnityTools.Samples.Inven;
using UnityTools.Util.Core;

namespace UnityTools.Samples.Modules
{
    public class InvenSampleModule : SampleModuleBase
    {
        //============================================================
        //Inspector Fields
        //============================================================
        [SerializeField] private InvenView _invenView;
        [SerializeField] private int _invenModelCnt = 50;

        //============================================================
        //Fields
        //============================================================
        private InvenPresenter _invenPresenter;

        //============================================================
        //Properties
        //============================================================
        public override string ModuleKey => SampleModuleKeys.INVEN;

        //============================================================
        //Logic
        //============================================================
        protected override bool OnInitModule()
        {
            if(!TryResolveView(ref _invenView))
                return false;

            int invenModelCnt = Mathf.Max(0, _invenModelCnt);
            _invenPresenter = new InvenPresenter(new InvenModel(invenModelCnt), _invenView);
            _invenPresenter.Init();
            return _invenPresenter.IsInit;
        }

        protected override void OnShowModule()
        {
            _invenPresenter?.Show();
        }

        protected override void OnHideModule()
        {
            _invenPresenter?.Hide();
        }

        protected override void OnReleaseModule()
        {
            _invenPresenter?.Release();
            _invenPresenter = null;
        }
    }
}

