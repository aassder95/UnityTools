using UnityEngine.Events;
using UnityTools.Util.UIFramework;

namespace UnityTools.Samples.Inven
{
    public class InvenPresenter : BasePresenter<InvenModel, InvenView>
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly UnityAction<InvenItemView> _onItemViewUpdated;
        private readonly UnityAction _onRefreshItems;
        private readonly UnityAction _onShuffleItems;
        private readonly UnityAction _onSortByCount;
        private readonly UnityAction _onSortByGrade;

        //============================================================
        //Constructors
        //============================================================
        public InvenPresenter(InvenModel model, InvenView view) : base(model, view)
        {
            _onItemViewUpdated = OnItemViewUpdatedCallback;
            _onRefreshItems = _model.RandomizeCounts;
            _onShuffleItems = _model.ShuffleItems;
            _onSortByCount = _model.SortByCountDesc;
            _onSortByGrade = _model.SortByGradeDesc;
        }

        //============================================================
        //Init/Register
        //============================================================
        protected override void OnInit()
        {
            if(_view.ScrollView == null)
                return;

            _view.ScrollView.InitView(_model.ItemCount);
        }

        protected override void BindEvents()
        {
            base.BindEvents();
            _view.OnRefreshItems += _onRefreshItems;
            _view.OnShuffleItems += _onShuffleItems;
            _view.OnSortByCount += _onSortByCount;
            _view.OnSortByGrade += _onSortByGrade;

            if(_view.ScrollView == null)
                return;

            _view.ScrollView.OnItemUpdated += _onItemViewUpdated;
            _view.ScrollView.RefreshItems();
        }

        protected override void UnbindEvents()
        {
            _view.OnRefreshItems -= _onRefreshItems;
            _view.OnShuffleItems -= _onShuffleItems;
            _view.OnSortByCount -= _onSortByCount;
            _view.OnSortByGrade -= _onSortByGrade;

            if(_view.ScrollView != null)
                _view.ScrollView.OnItemUpdated -= _onItemViewUpdated;

            base.UnbindEvents();
        }

        //============================================================
        //Callbacks
        //============================================================
        private void OnItemViewUpdatedCallback(InvenItemView itemView)
        {
            if(itemView == null)
                return;

            if(!itemView.IsInit)
                itemView.Init();

            InvenItemModel itemModel = _model.Get(itemView.Index);
            if(itemModel == null)
                return;

            itemView.Refresh(itemModel);
        }
    }
}
