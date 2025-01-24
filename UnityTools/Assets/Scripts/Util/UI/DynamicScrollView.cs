using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UnityTools.Util
{
    [RequireComponent(typeof(ScrollRect))]
    public class DynamicScrollView<TView> : MonoBehaviour where TView : Component, IDynamicScrollItem, IPoolable
    {
        #region Inspector
        [SerializeField] TView _item;
        [SerializeField] int _visibleCnt = 3;
        [SerializeField] float _spacing = 0.0f;
        [SerializeField] float _paddingStart = 0.0f;
        [SerializeField] float _paddingEnd = 0.0f;
        #endregion //Inspector

        #region Fields
        EScrollDirection _scrollDir;
        DynamicScrollContext _context;
        DynamicScrollItemController<TView> _itemCtrl;
        int _totalCnt;
        RectTransform _rtContent;
        RectTransform _rtItem;
        ScrollRect _scrollRect;
        ObjectPool<TView> _pool;
        readonly Deque<TView> _items = new();
        #endregion //Fields

        #region Properties
        int FirstVisibleIndex => Utils.ClampIndexFromPosition(_context.ContentPos - _paddingStart, _context.ItemSize, _totalCnt - _visibleCnt);
        public int TotalCount => _totalCnt;
        public int VisibleCount => _visibleCnt;
        #endregion //Properties

        #region Events
        public UnityEvent<TView> OnItemUpdated = new();
        #endregion //Events

        #region Unity Lifecycle
        void Awake()
        {
            _rtItem = _item.GetComponent<RectTransform>();
            _scrollRect = GetComponent<ScrollRect>();

            _rtContent = _scrollRect.content;
            _scrollDir = _scrollRect.vertical ? EScrollDirection.Vertical : EScrollDirection.Horizontal;

            if (_scrollDir == EScrollDirection.Vertical)
            {
                _rtContent.anchorMin = new Vector2(0.0f, 1.0f);
                _rtContent.anchorMax = new Vector2(1.0f, 1.0f);
            }
            else
            {
                _rtContent.anchorMin = new Vector2(0.0f, 0.0f);
                _rtContent.anchorMax = new Vector2(0.0f, 1.0f);
            }

            _pool = new ObjectPool<TView>(_rtContent, _item, _visibleCnt);
            _context = new DynamicScrollContext(_scrollDir, _rtContent, _rtItem, _spacing, _paddingStart, _paddingEnd);
            _itemCtrl = new DynamicScrollItemController<TView>(_context, _pool, _items);

            _itemCtrl.OnItemUpdated += HandleItemUpdated;
        }

        void OnDestroy()
        {
            _pool?.Clear();
        }
        #endregion //Unity Lifecycle

        #region View
        public void InitView(int totalCnt)
        {
            _totalCnt = totalCnt;
            _visibleCnt = Mathf.Min(_visibleCnt, _totalCnt);

            SetContentSize(_totalCnt);

            for (int i = 0; i < _visibleCnt; i++)
            {
                _itemCtrl.AddItem(true);
            }
        }

        public void UpdateView()
        {
            _itemCtrl.UpdateAllItems();
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
                    _itemCtrl.AddItem(_itemCtrl.FirstIndex + _items.Count < _totalCnt);
                else
                    _itemCtrl.RemoveItem(_itemCtrl.FirstIndex >= FirstVisibleIndex);
            }
        }
        #endregion //View

        #region Content
        void SetContentSize(int totalCnt)
        {
            float size = _context.CalculateContentSize(totalCnt);
            if (_scrollDir == EScrollDirection.Vertical)
                _rtContent.SetSizeHeight(size);
            else
                _rtContent.SetSizeWidth(size);

            _itemCtrl.UpdateAllItemsPosition();
        }

        protected void SetContentPosition(float value)
        {
            if (_scrollDir == EScrollDirection.Vertical)
                _scrollRect.verticalNormalizedPosition = value;
            else
                _scrollRect.horizontalNormalizedPosition = value;
        }

        protected void SetContentPosition(int idx, float offset = 0.0f)
        {
            float pos = _context.GetContentPos(idx, offset);
            if (_scrollDir == EScrollDirection.Vertical)
                _rtContent.anchoredPosition = new Vector2(_rtContent.anchoredPosition.x, pos);
            else
                _rtContent.anchoredPosition = new Vector2(pos, _rtContent.anchoredPosition.y);

            OnScrollValueChanged(Vector2.zero);
        }
        #endregion //Content

        #region Callback
        public void OnScrollValueChanged(Vector2 value)
        {
            if (_itemCtrl.FirstIndex != FirstVisibleIndex)
            {
                int idxDiff = Mathf.Abs(FirstVisibleIndex - _itemCtrl.FirstIndex);
                bool isDown = FirstVisibleIndex > _itemCtrl.FirstIndex;

                for (int i = 0; i < idxDiff; i++)
                {
                    _itemCtrl.AddItem(isDown);
                    _itemCtrl.RemoveItem(!isDown);
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
        #endregion //Callback
    }
}
