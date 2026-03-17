using UnityTools.Model;
using UnityTools.UI;
using UnityTools.Util;
using UnityEngine.Events;

namespace UnityTools.Presenter
{
    //============================================================
    //Logic
    //============================================================
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

                itemView.Refresh(MODEL.Get(itemView.Index));
            };
        }

        //============================================================
        //Init/Register
        //============================================================
        public override void Init()
        {
            base.Init();
            VIEW.ScrollView.InitView(MODEL.ItemCount);
        }

        protected override void BindEvents()
        {
            base.BindEvents();
            VIEW.ScrollView.OnItemUpdated += _onItemViewUpdated;
        }

        protected override void UnbindEvents()
        {
            VIEW.ScrollView.OnItemUpdated -= _onItemViewUpdated;
            base.UnbindEvents();
        }
    }
}
