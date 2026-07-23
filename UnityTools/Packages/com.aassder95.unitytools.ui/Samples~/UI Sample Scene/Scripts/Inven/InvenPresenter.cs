using UnityEngine.Events;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.UIFramework;

namespace UnityTools.Samples.Inven
{
    public class InvenPresenter : BasePresenter<InvenModel, InvenView>
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly UnityAction<InvenItemView> _onItemViewUpdated;
        private readonly UnityAction _onRefreshItems;
        private readonly UnityAction _onShuffleItems;
        private readonly UnityAction _onSortByCnt;
        private readonly UnityAction _onSortByGrade;

        //============================================================
        // Constructors
        //============================================================
        public InvenPresenter(InvenModel model, InvenView view) : base(model, view)
        {
            _onItemViewUpdated = OnItemViewUpdatedCallback;
            _onRefreshItems = _model.RandomizeItemCnts;
            _onShuffleItems = _model.ShuffleItems;
            _onSortByCnt = _model.SortByCntDesc;
            _onSortByGrade = _model.SortByGradeDesc;
        }

        //============================================================
        // Init/Register
        //============================================================
        protected override bool OnInit()
        {
            InvenScrollView scrollView = _view.ScrollView;
            scrollView.OnItemUpdated += _onItemViewUpdated;
            if(scrollView.InitView(_model.ItemCnt))
            {
                scrollView.RefreshItems();
                return true;
            }

            scrollView.OnItemUpdated -= _onItemViewUpdated;
            return false;
        }

        protected override void BindEvents()
        {
            base.BindEvents();
            _view.OnRefreshItems += _onRefreshItems;
            _view.OnShuffleItems += _onShuffleItems;
            _view.OnSortByCnt += _onSortByCnt;
            _view.OnSortByGrade += _onSortByGrade;
        }

        protected override void UnbindEvents()
        {
            _view.OnRefreshItems -= _onRefreshItems;
            _view.OnShuffleItems -= _onShuffleItems;
            _view.OnSortByCnt -= _onSortByCnt;
            _view.OnSortByGrade -= _onSortByGrade;
            _view.ScrollView.OnItemUpdated -= _onItemViewUpdated;
            base.UnbindEvents();
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OnItemViewUpdatedCallback(InvenItemView itemView)
        {
            InvenItemModel itemModel = _model.Get(itemView.Idx);
            if(itemModel == null)
            {
                DebugLogger.LogError("Inven Item 모델을 찾을 수 없습니다. 인덱스=" + itemView.Idx);
                StopAfterFailure();
                return;
            }

            if(!itemView.Refresh(itemModel))
                StopAfterFailure();
        }
    }
}
