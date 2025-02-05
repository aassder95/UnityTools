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

        public UnityEvent<TView> OnItemUpdated = new();

        void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();

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
            _scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
            _itemCtrl.OnItemUpdated -= HandleItemUpdated;
            _itemCtrl.Clear();
        }

        public void InitView(int totalItemCnt)
        {
            SetTotalItemCount(totalItemCnt);
            _itemCtrl.AddRange(Mathf.Min(MaxVisibleItemCount, _totalItemCnt), 0, true);
        }

        public void UpdateView()
        {
            _itemCtrl.Update();
        }

        int GetLineItemCount(int line)
        {
            return line == _totalLineCnt - 1 ? _totalItemCnt - line * _itemCntPerLine : _itemCntPerLine;
        }

        protected void SetTotalItemCount(int totalItemCnt)
        {
            if (totalItemCnt == _totalItemCnt || totalItemCnt < _itemCtrl.Count)
                return;

            _totalItemCnt = totalItemCnt;
            _totalLineCnt = Mathf.CeilToInt((float)_totalItemCnt / _itemCntPerLine);
            SetContentSize(_totalLineCnt);
        }

        protected void SetVisibleLineCount(int visibleLineCnt)
        {
            if (visibleLineCnt == _visibleLineCnt || visibleLineCnt <= 0 || visibleLineCnt > _totalLineCnt)
                return;

            bool isAdd = visibleLineCnt > _visibleLineCnt;
            int itemCnt = 0;
            for (int i = 0, lineCnt = Mathf.Abs(visibleLineCnt - _visibleLineCnt); i < lineCnt; i++)
            {
                itemCnt += GetLineItemCount((isAdd ? _visibleLineCnt : visibleLineCnt) + _itemCtrl.FirstIndex / _itemCntPerLine + i);
            }

            _visibleLineCnt = visibleLineCnt;

            if (isAdd)
                _itemCtrl.AddRange(itemCnt, _totalItemCnt);
            else
                _itemCtrl.RemoveRange(itemCnt, _totalLineCnt - _visibleLineCnt);
        }

        void SetContentPosition(int itemIdx, float offset = 0.0f)
        {
            _rtContent.anchoredPosition = _context.CalculateContentPosition(itemIdx, offset);
            OnScrollValueChanged(Vector2.zero);
        }

        void SetContentSize(int totalLineCnt)
        {
            _rtContent.sizeDelta = _context.CalculateContentSize(totalLineCnt);
            _itemCtrl.UpdatePosition();
        }

        void OnScrollValueChanged(Vector2 value)
        {
            int line = _itemCtrl.FirstIndex / _itemCntPerLine;
            int visibleLine = _context.CalculateFirstVisibleLine(_totalLineCnt - _visibleLineCnt);
            if (line != visibleLine)
            {
                bool isDown = visibleLine > line;

                for (int i = 0, lineCnt = Mathf.Min(Mathf.Abs(line - visibleLine), _visibleLineCnt); i < lineCnt; i++)
                {
                    int addLine = isDown ? visibleLine + _visibleLineCnt - lineCnt + i : visibleLine + i;
                    int removeLine = isDown ? line + i : line + _visibleLineCnt - lineCnt + i;

                    _itemCtrl.AddRange(GetLineItemCount(addLine), addLine * _itemCntPerLine, isDown);
                    _itemCtrl.RemoveRange(GetLineItemCount(removeLine), !isDown);
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
