using UnityEngine;
using UnityEngine.Events;
using UnityTools.Samples.Util;
using UnityTools.Util.UIFramework;

namespace UnityTools.Samples.Inven
{
    public class InvenView : BaseView<InvenModel>
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private InvenScrollView _scrollView;

        //============================================================
        // Fields
        //============================================================
        private bool _isTestLayoutBuilt;

        //============================================================
        // Events
        //============================================================
        public event UnityAction OnRefreshItems { add => _onRefreshItems += value; remove => _onRefreshItems -= value; }
        public event UnityAction OnShuffleItems { add => _onShuffleItems += value; remove => _onShuffleItems -= value; }
        public event UnityAction OnSortByCnt { add => _onSortByCnt += value; remove => _onSortByCnt -= value; }
        public event UnityAction OnSortByGrade { add => _onSortByGrade += value; remove => _onSortByGrade -= value; }
        private event UnityAction _onRefreshItems;
        private event UnityAction _onShuffleItems;
        private event UnityAction _onSortByCnt;
        private event UnityAction _onSortByGrade;

        //============================================================
        // Properties
        //============================================================
        public InvenScrollView ScrollView => _scrollView;

        //============================================================
        // Init/Register
        //============================================================
        protected override void OnInit()
        {
            BuildTestLayout();
        }

        protected override void OnRelease()
        {
            _scrollView.ReleaseView();
        }

        //============================================================
        // Logic
        //============================================================
        protected override void OnRefresh(InvenModel model)
        {
            _scrollView.RefreshItems();
        }

        //============================================================
        // Callbacks
        //============================================================
        public void OnRefreshItemsInspector()
        {
            _onRefreshItems?.Invoke();
        }

        public void OnShuffleItemsInspector()
        {
            _onShuffleItems?.Invoke();
        }

        public void OnSortByCntInspector()
        {
            _onSortByCnt?.Invoke();
        }

        public void OnSortByGradeInspector()
        {
            _onSortByGrade?.Invoke();
        }

        //============================================================
        // Utilities
        //============================================================
        private void BuildTestLayout()
        {
            if(_isTestLayoutBuilt)
                return;

            SampleTestLayout layout = SampleTestUiBuilder.Build(transform, "Inventory Test Sample", "Shuffle / Sort / Dynamic list refresh", 4);
            RectTransform rtScroll = _scrollView.transform as RectTransform;
            SampleTestUiBuilder.ReparentToContent(rtScroll, layout.RtContentViewport, Vector2.zero, Vector2.zero);
            SampleTestUiBuilder.CreateActionButton(layout.RtControls, "BtnTestRefresh", "Refresh", OnRefreshItemsInspector);
            SampleTestUiBuilder.CreateActionButton(layout.RtControls, "BtnTestShuffle", "Shuffle", OnShuffleItemsInspector);
            SampleTestUiBuilder.CreateActionButton(layout.RtControls, "BtnTestSortCount", "Count Sort", OnSortByCntInspector);
            SampleTestUiBuilder.CreateActionButton(layout.RtControls, "BtnTestSortGrade", "Grade Sort", OnSortByGradeInspector);

            _isTestLayoutBuilt = true;
        }
    }
}
