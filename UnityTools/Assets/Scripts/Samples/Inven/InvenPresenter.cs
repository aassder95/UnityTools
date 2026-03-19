using UnityTools.Util;
using UnityEngine.Events;

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
