using UnityTools.Util.UiFramework;

namespace UnityTools.Samples.Templates
{
    public class TemplateSamplePresenter : BasePresenter<TemplateSampleModel, TemplateSampleView>
    {
        //============================================================
        // Constructors
        //============================================================
        public TemplateSamplePresenter(TemplateSampleModel model, TemplateSampleView view) : base(model, view) { }

        //============================================================
        // Init/Register
        //============================================================
        protected override void BindEvents()
        {
            base.BindEvents();
            _view.OnIncreaseClicked += _model.Increase;
        }

        protected override void UnbindEvents()
        {
            _view.OnIncreaseClicked -= _model.Increase;
            base.UnbindEvents();
        }
    }
}
