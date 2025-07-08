using UnityTools.Model;
using UnityTools.UI;
using UnityTools.Util;

namespace UnityTools.Presenter
{
    public class InvenPresenter : BasePresenter<InvenModel, InvenView>
    {
        public InvenPresenter(InvenModel model, InvenView view) : base(model, view)
        {
            
        }

        public override void Init()
        {
            base.Init();
            _view.ScrollView.InitView(_model.ItemCount);
        }

        protected override void BindEvents()
        {
            base.BindEvents();
            _view.ScrollView.OnItemUpdated.AddListener(OnItemViewUpdated);
        }

        protected override void UnbindEvents()
        {
            _view.ScrollView.OnItemUpdated.RemoveListener(OnItemViewUpdated);
            base.UnbindEvents();
        }

        private void OnItemViewUpdated(InvenItemView itemView)
        {
            itemView.Refresh(_model.Get(itemView.Index));
        }
    }
}