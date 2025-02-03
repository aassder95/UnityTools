using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UnityTools.Util
{
    [RequireComponent(typeof(ScrollRect))]
    public class DynamicScrollView<TView> : MonoBehaviour where TView : Component, IDynamicScrollItem, IPoolable
    {
        [SerializeField] TView _item;
        [SerializeField] int _visibleCnt = 3;
        [SerializeField] RectOffset _padding;
        [SerializeField] float _spacing = 0.0f;
        [SerializeField] float _paddingStart = 0.0f;
        [SerializeField] float _paddingEnd = 0.0f;

        EScrollDirection _scrollDir;
        DynamicScrollContext _context;
        DynamicScrollItemController<TView> _itemCtrl;
        int _totalCnt;
        RectTransform _rtContent;
        RectTransform _rtItem;
        ScrollRect _scrollRect;

        public int TotalCount => _totalCnt;
        public int VisibleCount => _visibleCnt;

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

            _context = new DynamicScrollContext(_scrollDir, _rtContent, _rtItem, _padding, _spacing);
            _itemCtrl = new DynamicScrollItemController<TView>(_context, _rtContent, _item, _visibleCnt);

            _itemCtrl.OnItemUpdated += HandleItemUpdated;
        }

        void OnDestroy()
        {
            _itemCtrl.Clear();
        }

        public void InitView(int totalCnt)
        {
            _totalCnt = totalCnt;
            _visibleCnt = Mathf.Min(_visibleCnt, _totalCnt);

            SetContentSize(_totalCnt);

            for (int i = 0; i < _visibleCnt; i++)
            {
                _itemCtrl.Add(true);
            }
        }

        public void UpdateView()
        {
            _itemCtrl.Update();
        }

        protected void SetTotalCount(int cnt)
        {
            if (cnt == _totalCnt || cnt < _visibleCnt)
                return;

            _totalCnt = cnt;
            SetContentSize(_totalCnt);
        }

        protected void SetVisibleCount(int cnt)
        {
            if (cnt == _visibleCnt || cnt <= 0 || cnt > _totalCnt)
                return;

            int cntDiff = Mathf.Abs(cnt - _visibleCnt);
            bool isAdd = cnt > _visibleCnt;

            _visibleCnt = cnt;

            for (int i = 0; i < cntDiff; i++)
            {
                if (isAdd)
                    _itemCtrl.Add(_totalCnt);
                else
                    _itemCtrl.Remove(_context.CalculateFirstVisibleItemIndex(_totalCnt - _visibleCnt));
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
            int firstVisibleIdx = _context.CalculateFirstVisibleItemIndex(_totalCnt - _visibleCnt);
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
