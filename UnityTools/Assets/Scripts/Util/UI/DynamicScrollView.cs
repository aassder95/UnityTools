using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UnityTools.Util
{
    public interface IDynamicScrollItem
    {
        int Index { get; set; }
        void SetPosition(Vector2 pos);
    }

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
        int _totalCnt;
        RectTransform _rtContent;
        RectTransform _rtItem;
        ScrollRect _scrollRect;
        ObjectPool<TView> _pool;
        readonly Deque<TView> _items = new();
        #endregion //Fields

        #region Properties
        int FirstIndex => _items.Peek()?.Index ?? 0;
        int FirstVisibleIndex => Utils.ClampIndexFromPosition(_context.ContentPos - _paddingStart, _context.ItemSize, _totalCnt - _visibleCnt);
        public int TotalCount => _totalCnt;
        public int VisibleCount => _visibleCnt;
        #endregion //Properties

        #region Events
        public event UnityAction<TView> OnItemUpdated;
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

            _context = new DynamicScrollContext(_scrollDir, _rtContent, _rtItem, _spacing, _paddingStart, _paddingEnd);
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
            _pool = new ObjectPool<TView>(_rtContent, _item, _visibleCnt);

            SetContentSize(_totalCnt);

            for (int i = 0; i < _visibleCnt; i++)
            {
                AddItem(true);
            }
        }

        public void UpdateView()
        {
            _items.ForEach(item => OnItemUpdated?.Invoke(item));
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
                    AddItem(FirstIndex + _items.Count < _totalCnt);
                else
                    RemoveItem(FirstIndex >= FirstVisibleIndex);
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

            _items.ForEach(item => item.SetPosition(_context.CalculateItemPosition(item.Index)));
        }

        protected void SetContentPos(float value)
        {
            if (_scrollDir == EScrollDirection.Vertical)
                _scrollRect.verticalNormalizedPosition = value;
            else
                _scrollRect.horizontalNormalizedPosition = value;
        }

        protected void SetContentPos(int idx, float offset = 0.0f)
        {
            float pos = _context.GetContentPos(idx, offset);
            if (_scrollDir == EScrollDirection.Vertical)
                _rtContent.anchoredPosition = new Vector2(_rtContent.anchoredPosition.x, pos);
            else
                _rtContent.anchoredPosition = new Vector2(pos, _rtContent.anchoredPosition.y);

            OnScrollValueChanged(Vector2.zero);
        }
        #endregion //Content

        #region Item
        TView CreateItem(int idx)
        {
            TView item = _pool.Get();
            item.Index = idx;
            item.SetPosition(_context.CalculateItemPosition(idx));
            OnItemUpdated?.Invoke(item);
            return item;
        }

        void AddItem(bool isBack)
        {
            if (isBack)
                _items.Enqueue(CreateItem(FirstIndex + _items.Count));
            else
                _items.EnqueueFront(CreateItem(FirstIndex - 1));
        }

        void RemoveItem(bool isBack)
        {
            if (isBack)
                _pool.Return(_items.DequeueBack());
            else
                _pool.Return(_items.Dequeue());
        }

        protected TView GetItem(int idx)
        {
            foreach (var item in _items)
            {
                if (item.Index == idx)
                    return item;
            }

            return null;
        }
        #endregion //Item

        #region Callback
        public void OnScrollValueChanged(Vector2 value)
        {
            if (FirstIndex != FirstVisibleIndex)
            {
                int idxDiff = Mathf.Abs(FirstVisibleIndex - FirstIndex);
                bool isDown = FirstVisibleIndex > FirstIndex;

                for (int i = 0; i < idxDiff; i++)
                {
                    AddItem(isDown);
                    RemoveItem(!isDown);
                }
            }

            HandleScrollValueChanged(value);
        }

        protected virtual void HandleScrollValueChanged(Vector2 value)
        {

        }
        #endregion //Callback
    }
}
