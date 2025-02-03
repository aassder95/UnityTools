using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UnityTools.Util
{
    [RequireComponent(typeof(ScrollRect))]
    public class DynamicScrollView<TView> : MonoBehaviour where TView : Component, IDynamicScrollItem, IPoolable
    {
        [SerializeField] TView _item;
        [SerializeField] int _visibleLineCnt = 3;
        [SerializeField] int _lineItemCnt = 1;
        [SerializeField] RectOffset _padding;
        [SerializeField] Vector2 _spacing;

        EScrollDirection _scrollDir;
        DynamicScrollContext _context;
        DynamicScrollItemController<TView> _itemCtrl;
        int _totalItemCnt;
        RectTransform _rtContent;
        RectTransform _rtItem;
        ScrollRect _scrollRect;

        public int TotalItemCount => _totalItemCnt;
        public int VisibleLineCount => _visibleLineCnt;
        public int LineItemCount => _lineItemCnt;

        public UnityEvent<TView> OnItemUpdated = new();

        void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
            _rtContent = _scrollRect.content;
            _rtItem = _item.GetComponent<RectTransform>();
            _scrollDir = _scrollRect.vertical ? EScrollDirection.Vertical : EScrollDirection.Horizontal;

            switch (_scrollDir)
            {
                case EScrollDirection.Vertical:
                    _rtContent.anchorMin = new Vector2(0.0f, 1.0f);
                    _rtContent.anchorMax = new Vector2(1.0f, 1.0f);
                    break;
                case EScrollDirection.Horizontal:
                    _rtContent.anchorMin = new Vector2(0.0f, 0.0f);
                    _rtContent.anchorMax = new Vector2(0.0f, 1.0f);
                    break;
            }

            _context = new DynamicScrollContext(_scrollDir, _lineItemCnt, _rtContent, _rtItem, _padding, _spacing);
            _itemCtrl = new DynamicScrollItemController<TView>(_context, _rtContent, _item, _visibleLineCnt);

            _itemCtrl.OnItemUpdated += HandleItemUpdated;
        }

        void OnDestroy()
        {
            _itemCtrl.Clear();
        }

        public void InitView(int totalCnt)
        {
            _totalItemCnt = totalCnt;
            _visibleLineCnt = Mathf.Min(_visibleLineCnt, _totalItemCnt);

            SetContentSize(_totalItemCnt);

            for (int i = 0; i < _visibleLineCnt; i++)
            {
                _itemCtrl.Add(true);
            }
        }

        public void UpdateView()
        {
            _itemCtrl.Update();
        }

        protected void SetTotalItemCount(int cnt)
        {
            if (cnt == _totalItemCnt || cnt < _visibleLineCnt)
                return;

            _totalItemCnt = cnt;
            SetContentSize(_totalItemCnt);
        }

        protected void SetVisibleLineCount(int cnt)
        {
            if (cnt == _visibleLineCnt || cnt <= 0 || cnt > _totalItemCnt)
                return;

            int cntDiff = Mathf.Abs(cnt - _visibleLineCnt);
            bool isAdd = cnt > _visibleLineCnt;

            _visibleLineCnt = cnt;

            for (int i = 0; i < cntDiff; i++)
            {
                if (isAdd)
                    _itemCtrl.Add(_totalItemCnt);
                else
                    _itemCtrl.Remove(_context.CalculateFirstVisibleItemIndex(_totalItemCnt - _visibleLineCnt));
            }
        }

        protected void SetContentPosition(int idx, float offset = 0.0f)
        {
            float pos = _context.CalculateContentPosition(idx, offset);
            if (_scrollDir == EScrollDirection.Vertical)
                _rtContent.anchoredPosition = new Vector2(_rtContent.anchoredPosition.x, pos);
            else
                _rtContent.anchoredPosition = new Vector2(pos, _rtContent.anchoredPosition.y);

            OnScrollValueChanged(Vector2.zero);
        }

        void SetContentSize(int totalCnt)
        {
            _rtContent.sizeDelta = _context.CalculateContentSize(totalCnt);
            _itemCtrl.UpdatePosition();
        }

        public void OnScrollValueChanged(Vector2 value)
        {
            int firstIdx = _itemCtrl.FirstIndex;
            int firstVisibleIdx = _context.CalculateFirstVisibleItemIndex(_totalItemCnt - _visibleLineCnt);
            if (firstIdx != firstVisibleIdx)
            {
                bool isDown = firstVisibleIdx > firstIdx;
                for (int i = 0, diff = Mathf.Abs(firstVisibleIdx - firstIdx); i < diff; i++)
                {
                    _itemCtrl.Add(isDown);
                    _itemCtrl.Remove(!isDown);
                }
            }

            HandleScrollValueChanged(value);
        }

        protected virtual void HandleScrollValueChanged(Vector2 value)
        {

        }

        void HandleItemUpdated(TView itemView)
        {
            OnItemUpdated?.Invoke(itemView);
        }
    }
}
