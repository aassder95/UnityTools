using UnityTools.Util;
using UnityEngine.Events;
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

namespace UnityTools.Samples.Inven
{
    public class InvenPresenter : BasePresenter<InvenModel, InvenView>
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly UnityAction<InvenItemView> _onItemViewUpdated;

        //============================================================
        //Constructors
        //============================================================
        public InvenPresenter(InvenModel model, InvenView view) : base(model, view)
        {
            _onItemViewUpdated = itemView =>
            {
                if(itemView == null)
                    return;

                itemView.Refresh(_model.Get(itemView.Index));
            };
        }

        //============================================================
        //Init/Register
        //============================================================
        protected override void OnInit()
        {
            _view.ScrollView.InitView(_model.ItemCount);
        }

        protected override void BindEvents()
        {
            base.BindEvents();
            _view.ScrollView.OnItemUpdated += _onItemViewUpdated;
        }

        protected override void UnbindEvents()
        {
            _view.ScrollView.OnItemUpdated -= _onItemViewUpdated;
            base.UnbindEvents();
        }
    }
}
