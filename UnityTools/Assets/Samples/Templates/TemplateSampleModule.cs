using UnityEngine;
using UnityTools.Samples.Modules;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Samples.Templates
{
    public class TemplateSampleModule : SampleModuleBase
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Module")] [SerializeField] private string _moduleKey = "Template";
        [Header("Template View")] [SerializeField] private TemplateSampleView _view;
        [Header("Sample Data")] [SerializeField] private int _startCnt;

        //============================================================
        // Fields
        //============================================================
        private TemplateSampleModel _model;
        private TemplateSamplePresenter _presenter;

        //============================================================
        // Properties
        //============================================================
        public override string ModuleKey => _moduleKey;

        //============================================================
        // Logic
        //============================================================
        protected override bool OnInitModule()
        {
            if(string.IsNullOrWhiteSpace(_moduleKey))
            {
                DebugLogger.LogError("TemplateSampleModule의 Module Key가 필요합니다.", this);
                return false;
            }

            TemplateSampleModel model = new();
            model.SetCnt(_startCnt);
            TemplateSamplePresenter presenter = new(model, _view);
            if(!presenter.TryInit())
                return false;

            _model = model;
            _presenter = presenter;
            return true;
        }

        protected override bool OnShowModule()
        {
            return _presenter.TryShow();
        }

        protected override bool OnHideModule()
        {
            return _presenter.TryHide();
        }

        protected override bool OnReleaseModule()
        {
            bool isSuccess = _presenter.TryRelease();
            _presenter = null;
            _model = null;
            return isSuccess;
        }
    }
}