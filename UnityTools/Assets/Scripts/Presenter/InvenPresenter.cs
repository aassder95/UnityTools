using UnityTools.Model;
using UnityTools.UI;
using UnityTools.Util;

namespace UnityTools.Presenter
{
    //============================================================
    //Logic
    //============================================================
    public class InvenPresenter : BasePresenter<InvenModel, InvenView>
    {
        public InvenPresenter(InvenModel model, InvenView view) : base(model, view)
        {
            
        }

        public override void Init()
        {
            base.Init();
            VIEW.ScrollView.InitView(MODEL.ItemCount);
        }

        protected override void BindEvents()
        {
            base.BindEvents();
            VIEW.ScrollView.OnItemUpdated.AddListener(OnItemViewUpdated);
        }

        protected override void UnbindEvents()
        {
            VIEW.ScrollView.OnItemUpdated.RemoveListener(OnItemViewUpdated);
            base.UnbindEvents();
        }

        private void OnItemViewUpdated(InvenItemView itemView)
        {
            itemView.Refresh(MODEL.Get(itemView.Index));
        }
    }
}