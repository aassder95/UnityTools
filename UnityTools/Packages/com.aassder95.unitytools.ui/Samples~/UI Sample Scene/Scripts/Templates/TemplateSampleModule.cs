using UnityEngine;
using UnityTools.Samples.Modules;

namespace UnityTools.Samples.Templates
{
    public class TemplateSampleModule : SampleModuleBase
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private string _moduleKey = "Template";
        [SerializeField] private TemplateSampleView _view;
        [SerializeField] private int _startCount;

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
            if(!TryResolveView(ref _view))
                return false;

            _model = new TemplateSampleModel();
            _model.SetCount(_startCount);

            _presenter = new TemplateSamplePresenter(_model, _view);
            _presenter.Init();
            return _presenter.IsInit;
        }

        protected override void OnShowModule()
        {
            _presenter?.Show();
        }

        protected override void OnHideModule()
        {
            _presenter?.Hide();
        }

        protected override void OnReleaseModule()
        {
            _presenter?.Release();
            _presenter = null;
            _model = null;
        }
    }
}
