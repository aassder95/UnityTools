using UnityTools.Model;
using UnityTools.UI;
using UnityTools.Util;

namespace UnityTools.Presenter
{
    public class RankPresenter
    {
        readonly RankModel _model;
        readonly RankView _view;

        public RankPresenter(RankModel model, RankView view)
        {
            BindEvents();

            _model = model;
            _view = view;
            _view.InitView(_model.ItemModels.Count);
        }

        void BindEvents()
        {
            EventDispatcher.Instance.Subscribe(EEventDispatcherType.RankRandomScore, OnRandomScore);
            EventDispatcher.Instance.Subscribe<RankItemView>(EEventDispatcherType.DynamicScrollViewItemUpdated, OnItemViewUpdated);
        }

        public void OnRandomScore(object sender)
        {
            _model.SetRandomScore();
            _view.UpdateView();
        }

        public void OnItemViewUpdated(object sender, RankItemView itemView)
        {
            itemView.UpdateView(_model.GetItemModel(itemView.Index));
        }
    }
}
