using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UnityTools.Util
{
    [RequireComponent(typeof(ScrollRect))]
    public class DynamicScrollView<TView> : MonoBehaviour where TView : Component, IDynamicScrollItem, IPoolable
    {
        //============================================================
        //Inspector Fields
        //============================================================
        [SerializeField] private TView _item;
        [SerializeField] private int _visibleLineCnt = 10;
        [SerializeField] private int _itemCntPerLine = 1;
        [SerializeField] private Vector2 _spacing;
        [SerializeField] private RectOffset _padding;

        //============================================================
        //Fields
        //============================================================
        private DynamicScrollContext _context;
        private DynamicScrollItemController<TView> _itemCtrl;
        private ObjectPool<TView> _pool;
        private int _totalItemCnt;
        private int _totalLineCnt;
        private RectTransform _rt;
        private RectTransform _rtContent;
        private RectTransform _rtItem;
        private ScrollRect _scrollRect;
        private UnityAction<TView> _onControllerItemUpdated;

        //============================================================
        //Properties
        //============================================================
        protected DynamicScrollContext Context => _context;
        protected DynamicScrollItemController<TView> ItemController => _itemCtrl;
        protected int TotalLineCount => _totalLineCnt;
        protected int VisibleLineCount => _visibleLineCnt;
        protected int TotalItemCount => _totalItemCnt;
        protected RectTransform RectTransform => _rt;
        protected RectTransform RtContent => _rtContent;
        protected ScrollRect ScrollRect => _scrollRect;

        //============================================================
        //Events
        //============================================================
        public event UnityAction<TView> OnItemUpdated { add => _onItemUpdated.AddListener(value); remove => _onItemUpdated.RemoveListener(value); }
        private readonly UnityEvent<TView> _onItemUpdated = new();

        //============================================================
        //Unity Methods
        //============================================================
        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
            _scrollRect = GetComponent<ScrollRect>();
            _rtContent = _scrollRect.content;
            _rtItem = _item.GetComponent<RectTransform>();

            if(_scrollRect.vertical)
            {
                _rtContent.anchorMin = new Vector2(0.0f, 1.0f);
                _rtContent.anchorMax = new Vector2(1.0f, 1.0f);
            }
            else
            {
                _rtContent.anchorMin = new Vector2(0.0f, 0.0f);
                _rtContent.anchorMax = new Vector2(0.0f, 1.0f);
            }

            _context = new DynamicScrollContext(_itemCntPerLine, _spacing, _padding, _rtItem, _scrollRect);
            _pool = new ObjectPool<TView>(_visibleLineCnt * _itemCntPerLine, _item, _rtContent);
            _itemCtrl = new DynamicScrollItemController<TView>(_context, _pool);
            _onControllerItemUpdated = itemView => _onItemUpdated?.Invoke(itemView);

            _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            _itemCtrl.OnItemUpdated += _onControllerItemUpdated;
        }

        private void OnDestroy()
        {
            _scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
            _itemCtrl.OnItemUpdated -= _onControllerItemUpdated;
            _pool?.Clear();
        }

        //============================================================
        //Logic
        //============================================================
        public void InitView(int totalItemCnt)
        {
            SetTotalItemCount(totalItemCnt);
            _itemCtrl.AddRange(Mathf.Min(_visibleLineCnt * _itemCntPerLine, _totalItemCnt), 0, true);
        }

        protected void SetTotalItemCount(int totalItemCnt)
        {
            if(totalItemCnt == _totalItemCnt || totalItemCnt < _itemCtrl.Count)
                return;

            _totalItemCnt = totalItemCnt;
            _totalLineCnt = Mathf.CeilToInt((float)_totalItemCnt / _itemCntPerLine);
            _rtContent.sizeDelta = _context.CalculateContentSize(_totalLineCnt);
            _itemCtrl.UpdatePosition();
        }

        protected void SetVisibleLineCount(int visibleLineCnt)
        {
            if(visibleLineCnt == _visibleLineCnt || visibleLineCnt <= 0 || visibleLineCnt > _totalLineCnt)
                return;

            int itemCnt = _context.GetItemCountForLineRange(visibleLineCnt, _visibleLineCnt, _totalItemCnt, _itemCtrl.FirstIndex);
            if(visibleLineCnt > _visibleLineCnt)
                _itemCtrl.AddRange(itemCnt, _totalItemCnt);
            else
                _itemCtrl.RemoveRange(itemCnt, _totalLineCnt - visibleLineCnt);

            _visibleLineCnt = visibleLineCnt;
        }

        protected void SetContentPosition(int itemIdx, float offset = 0.0f)
        {
            _rtContent.anchoredPosition = _context.CalculateContentPosition(itemIdx, offset);
            OnScrollValueChanged(Vector2.zero);
        }

        private void OnScrollValueChanged(Vector2 value)
        {
            int line = _itemCtrl.FirstIndex / _itemCntPerLine;
            int visibleLine = _context.CalculateFirstVisibleLine(_totalLineCnt - _visibleLineCnt);
            if(line != visibleLine)
            {
                bool isDown = visibleLine > line;

                for (int i = 0, lineCnt = Mathf.Min(Mathf.Abs(line - visibleLine), _visibleLineCnt); i < lineCnt; i++)
                {
                    int addLine = isDown ? visibleLine + _visibleLineCnt - lineCnt + i : visibleLine + lineCnt - i - 1;
                    int removeLine = isDown ? line + i : line + _visibleLineCnt - i - 1;

                    _itemCtrl.AddRange(_context.GetItemCountForLine(addLine, _totalItemCnt), addLine * _itemCntPerLine, isDown);
                    _itemCtrl.RemoveRange(_context.GetItemCountForLine(removeLine, _totalItemCnt), !isDown);
                }
            }

            HandleScrollValueChanged(value);
        }

        //============================================================
        //Callbacks
        //============================================================
        protected virtual void HandleScrollValueChanged(Vector2 value) { }
    }
}
