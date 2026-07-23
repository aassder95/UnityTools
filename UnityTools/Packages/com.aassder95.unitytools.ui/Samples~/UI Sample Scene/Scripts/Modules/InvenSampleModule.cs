using UnityEngine;
using UnityTools.Samples.Inven;
using UnityTools.Util.Core;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Samples.Modules
{
    public class InvenSampleModule : SampleModuleBase
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Inventory View")]
        [SerializeField] private InvenView _invenView;

        [Header("Inventory Data")]
        [SerializeField] private int _invenModelCnt = 50;

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
        protected override bool OnInitModule()
        {
            if(_invenModelCnt < 0)
            {
                DebugLogger.LogError("InvenSampleModule의 Model Count는 0 이상이어야 합니다. 값=" + _invenModelCnt, this);
                return false;
            }

            InvenPresenter presenter = new InvenPresenter(new InvenModel(_invenModelCnt), _invenView);
            if(!presenter.TryInit())
                return false;

            _invenPresenter = presenter;
            return true;
        }

        protected override bool OnShowModule()
        {
            return _invenPresenter.TryShow();
        }

        protected override bool OnHideModule()
        {
            return _invenPresenter.TryHide();
        }

        protected override bool OnReleaseModule()
        {
            bool isSuccess = _invenPresenter.TryRelease();
            _invenPresenter = null;
            return isSuccess;
        }
    }
}
