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
        [SerializeField] private InvenView _invenView;
        [SerializeField] private int _invenModelCount = 50;

        //============================================================
        // Fields
        //============================================================
        private InvenPresenter _invenPresenter;

        //============================================================
        // Properties
        //============================================================
        public override string ModuleKey => SampleModuleKeys.INVEN;

        //============================================================
        // Logic
        //============================================================
        protected override bool OnInitModule()
        {
            if(!TryResolveView(ref _invenView))
                return false;

            int invenModelCount = Mathf.Max(0, _invenModelCount);
            _invenPresenter = new InvenPresenter(new InvenModel(invenModelCount), _invenView);
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

