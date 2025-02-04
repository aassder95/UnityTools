using UnityTools.UI;

namespace UnityTools.Presenter
{
    public class InvenPresenter
    {
        readonly InvenView _view;

        public InvenPresenter(int modelCnt, InvenView view)
        {
            _view = view;
            _view.ScrollView.OnItemUpdated.AddListener(OnItemViewUpdated);
            _view.InitView(modelCnt);
        }

        void OnItemViewUpdated(InvenItemView itemView)
        {
            itemView.UpdateView();
        }
    }
}