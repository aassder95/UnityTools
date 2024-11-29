using UnityEngine;
using UnityEngine.UI;

namespace UnityTools.Util
{
    public interface IDynamicScrollItem
    {
        int Index { get; set; }
        void SetPosition(Vector2 pos);
    }

    public class DynamicScrollView<TView> : MonoBehaviour where TView : Component, IDynamicScrollItem, IPoolable
    {
        enum EScrollDirection
        {
            Vertical,
            Horizontal,
        }

        #region Inspector
        [SerializeField] TView _item;
        [SerializeField] int _visibleCnt = 3;
        [SerializeField] float _spacing = 0.0f;
        #endregion //Inspector

        #region Fields
        EScrollDirection _scrollDir;
        ObjectPool<TView> _pool;
        int _totalCnt;
        RectTransform _rtContent;
        RectTransform _rtItem;
        ScrollRect _scrollRect;
        readonly Deque<TView> _items = new();
        #endregion //Fields

        #region Properties
        int FirstIndex => _items.Peek()?.Index ?? 0;
        int FirstVisibleIndex => Utils.ClampIndexFromPosition(ContentPos, ItemSize, _totalCnt - _visibleCnt);
        public int TotalCount => _totalCnt;
        public int VisibleCount => _visibleCnt;
        float ContentPos => _scrollDir == EScrollDirection.Vertical ? _rtContent.anchoredPosition.y : -_rtContent.anchoredPosition.x;
        float ContentSize => _scrollDir == EScrollDirection.Vertical ? _rtContent.sizeDelta.y : _rtContent.sizeDelta.x;
        float ItemOriginSize => _scrollDir == EScrollDirection.Vertical ? _rtItem.sizeDelta.y : _rtItem.sizeDelta.x;
        float ItemSize => ItemOriginSize + _spacing;
        float ItemPivot => _scrollDir == EScrollDirection.Vertical ? _rtItem.pivot.y : _rtItem.pivot.x;
        #endregion //Properties

        #region Unity Lifecycle
        void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
            _rtItem = _item.GetComponent<RectTransform>();

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
            _items.ForEach(item => EventDispatcher.Instance.Dispatch(EEventDispatcherType.DynamicScrollViewItemUpdated, this, item));
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
            float size = totalCnt * ItemSize - _spacing;

            if (_scrollDir == EScrollDirection.Vertical)
                _rtContent.SetSizeHeight(size);
            else
                _rtContent.SetSizeWidth(size);

            _items.ForEach(item => item.SetPosition(CalculateItemPosition(item.Index)));
        }
        #endregion //Content

        #region Item
        TView CreateItem(int idx)
        {
            TView item = _pool.Get();
            item.Index = idx;
            item.SetPosition(CalculateItemPosition(idx));
            EventDispatcher.Instance.Dispatch(EEventDispatcherType.DynamicScrollViewItemUpdated, this, item);
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

        Vector2 CalculateItemPosition(int idx)
        {
            float pos = (ContentSize - ItemOriginSize) * (1 - ItemPivot) - idx * ItemSize;

            if (_scrollDir == EScrollDirection.Vertical)
                return new Vector2(_rtItem.anchoredPosition.x, pos);
            else
                return new Vector2(-pos, _rtItem.anchoredPosition.y);
        }
        #endregion //Item

        #region Callback
        public void OnScrollValueChanged(Vector2 value)
        {
            if (FirstIndex == FirstVisibleIndex)
                return;

            int idxDiff = Mathf.Abs(FirstVisibleIndex - FirstIndex);
            bool isDown = FirstVisibleIndex > FirstIndex;

            for (int i = 0; i < idxDiff; i++)
            {
                AddItem(isDown);
                RemoveItem(!isDown);
            }
        }
        #endregion //Callback
    }
}
