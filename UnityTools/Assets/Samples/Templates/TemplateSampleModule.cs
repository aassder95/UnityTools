using UnityEngine;
using UnityTools.Samples.Modules;

namespace UnityTools.Samples.Templates
{
    public class TemplateSampleModule : SampleModuleBase
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Module")] [SerializeField] private string _moduleKey = "Template";
        [Header("Template View")] [SerializeField] private TemplateSampleView _view;
        [Header("Sample Data")] [Min(0)] [SerializeField] private int _startCnt;

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
        protected override void OnInitModule()
        {
            _model = new TemplateSampleModel();
            _model.SetCnt(_startCnt);
            _presenter = new TemplateSamplePresenter(_model, _view);
            _presenter.Init();
        }

        protected override void OnShowModule()
        {
            _presenter.Show();
        }

        protected override void OnHideModule()
        {
            _presenter.Hide();
        }

        protected override void OnReleaseModule()
        {
            _presenter.Release();
            _presenter = null;
            _model = null;
        }
    }
}
