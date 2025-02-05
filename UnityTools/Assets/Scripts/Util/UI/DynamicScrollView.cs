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
        [SerializeField] int _itemCntPerLine = 1;
        [SerializeField] RectOffset _padding;
        [SerializeField] Vector2 _spacing;

        EScrollDirection _scrollDir;
        DynamicScrollContext _context;
        DynamicScrollItemController<TView> _itemCtrl;
        int _totalItemCnt;
        int _totalLineCnt;
        RectTransform _rtContent;
        RectTransform _rtItem;
        ScrollRect _scrollRect;

        protected int VisibleLineCount => _visibleLineCnt;
        protected int TotalItemCount => _totalItemCnt;
        int MaxVisibleItemCount => _visibleLineCnt * _itemCntPerLine;
        int VisibleItemCount => _itemCtrl.Count;

        public UnityEvent<TView> OnItemUpdated = new();

        void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
            _scrollDir = _scrollRect.vertical ? EScrollDirection.Vertical : EScrollDirection.Horizontal;

            _rtContent = _scrollRect.content;
            _rtContent.anchorMin = new Vector2(0.0f, 1.0f);
            _rtContent.anchorMax = new Vector2(0.0f, 1.0f);

            _rtItem = _item.GetComponent<RectTransform>();

            _context = new DynamicScrollContext(_itemCntPerLine, _spacing, _padding, _rtItem, _scrollRect);
            _itemCtrl = new DynamicScrollItemController<TView>(_context, _item, MaxVisibleItemCount, _rtContent);

            _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            _itemCtrl.OnItemUpdated += HandleItemUpdated;
        }

        void OnDestroy()
        {
            _itemCtrl.Clear();
        }

        public void InitView(int cnt)
        {
            SetTotalItemCount(cnt);

            for (int i = 0, itemCnt = Mathf.Min(MaxVisibleItemCount, _totalItemCnt); i < itemCnt; i++)
            {
                _itemCtrl.Add(true);
            }
        }

        public void UpdateView()
        {
            _itemCtrl.Update();
        }

        int GetLineItemCount(int idx)
        {
            return idx == _totalLineCnt - 1 ? _totalItemCnt - idx * _itemCntPerLine : _itemCntPerLine;
        }

        protected void SetTotalItemCount(int cnt)
        {
            if (cnt == _totalItemCnt || cnt < VisibleItemCount)
                return;

            _totalItemCnt = cnt;
            _totalLineCnt = Mathf.CeilToInt((float)_totalItemCnt / _itemCntPerLine);
            SetContentSize(_totalLineCnt);
        }

        protected void SetVisibleLineCount(int cnt)
        {
            if (cnt == _visibleLineCnt || cnt <= 0 || cnt > _totalLineCnt)
                return;

            bool isAdd = cnt > _visibleLineCnt;
            int itemCnt = 0;

            for (int i = 0, lineCnt = Mathf.Abs(cnt - _visibleLineCnt); i < lineCnt; i++)
            {
                itemCnt += GetLineItemCount((isAdd ? _visibleLineCnt : cnt) + _itemCtrl.FirstIndex / _itemCntPerLine + i);
            }

            _visibleLineCnt = cnt;

            for (int i = 0; i < itemCnt; i++)
            {
                if (isAdd)
                    _itemCtrl.Add(_itemCtrl.FirstIndex + _itemCtrl.Count < _totalItemCnt);
                else
                    _itemCtrl.Remove(_itemCtrl.FirstIndex >= _context.CalculateFirstVisibleLine(_totalLineCnt - _visibleLineCnt) * _itemCntPerLine);
            }
        }

        void SetContentPosition(int idx, float offset = 0.0f)
        {
            float pos = _context.CalculateContentPosition(idx, offset);
            if (_scrollDir == EScrollDirection.Vertical)
                _rtContent.anchoredPosition = new Vector2(_rtContent.anchoredPosition.x, pos);
            else
                _rtContent.anchoredPosition = new Vector2(pos, _rtContent.anchoredPosition.y);

            OnScrollValueChanged(Vector2.zero);
        }

        void SetContentSize(int cnt)
        {
            _rtContent.sizeDelta = _context.CalculateContentSize(cnt);
            _itemCtrl.UpdatePosition();
        }

        void OnScrollValueChanged(Vector2 value)
        {
            int lineIdx = _itemCtrl.FirstIndex / _itemCntPerLine;
            int visibleLineIdx = _context.CalculateFirstVisibleLine(_totalLineCnt - _visibleLineCnt);
            if (lineIdx != visibleLineIdx)
            {
                bool isDown = visibleLineIdx > lineIdx;

                for (int i = 0, lineCnt = Mathf.Min(Mathf.Abs(lineIdx - visibleLineIdx), _visibleLineCnt); i < lineCnt; i++)
                {
                    int addLine = isDown ? visibleLineIdx + _visibleLineCnt - lineCnt + i : visibleLineIdx + i;
                    int removeLine = isDown ? lineIdx + i : lineIdx + _visibleLineCnt - lineCnt + i;

                    for (int j = 0, cnt = GetLineItemCount(addLine); j < cnt; j++)
                    {
                        _itemCtrl.Add(addLine * _itemCntPerLine + j, isDown);
                    }

                    for (int j = 0, cnt = GetLineItemCount(removeLine); j < cnt; j++)
                    {
                        _itemCtrl.Remove(!isDown);
                    }
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
