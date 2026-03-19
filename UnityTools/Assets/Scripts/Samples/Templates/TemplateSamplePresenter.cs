using UnityTools.Util.Constants;
using UnityTools.Util.Core.Collections;
using UnityTools.Util.Core.Events;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.Core.Persistence;
using UnityTools.Util.Core.Pooling;
using UnityTools.Util.Core.Singleton;
using UnityTools.Util.Core.State;
using UnityTools.Util.Core.Timer.Period;
using UnityTools.Util.Core.Timer.Shared;
using UnityTools.Util.Core.Timer.Task;
using UnityTools.Util.Coroutines;
using UnityTools.Util.Extensions;
using UnityTools.Util.UIFramework;
using UnityTools.Util.Utilities;

namespace UnityTools.Samples.Templates
{
    public class TemplateSamplePresenter : BasePresenter<TemplateSampleModel, TemplateSampleView>
    {
        //============================================================
        //Constructors
        //============================================================
        public TemplateSamplePresenter(TemplateSampleModel model, TemplateSampleView view) : base(model, view) { }

        //============================================================
        //Init/Register
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
