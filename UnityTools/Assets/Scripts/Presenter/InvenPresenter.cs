using UnityTools.UI;

namespace UnityTools.Presenter
{
    public class InvenPresenter : BasePresenter<int, InvenView>
    {
        public InvenPresenter(int modelCnt, InvenView view) : base(modelCnt, view) { }

        protected override void BindEvents()
        {
            _view.ScrollView.OnItemUpdated.AddListener(OnItemViewUpdated);
        }

        protected override void UnbindEvents()
        {
            _view.ScrollView.OnItemUpdated.RemoveListener(OnItemViewUpdated);
        }

        void OnItemViewUpdated(InvenItemView itemView)
        {
            itemView.UpdateView(itemView.Index);
        }
    }
}