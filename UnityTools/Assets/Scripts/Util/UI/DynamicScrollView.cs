using UnityEngine;
using UnityEngine.Events;

namespace UnityTools.Util
{
    public interface IDynamicScrollItem
    {
        int Index { get; set; }
        void SetPositionY(float y);
    }

    public class DynamicScrollView<TView> : MonoBehaviour where TView : Component, IDynamicScrollItem, IPoolable
    {
        [SerializeField] TView _item;
        [SerializeField] int _visibleCnt = 3;
        [SerializeField] float _spacing = 0.0f;
        [SerializeField] RectTransform _rtContent;
        [SerializeField] RectTransform _rtItem;

        ObjectPool<TView> _pool;
        int _totalCnt;
        readonly Deque<TView> _items = new();

        public event UnityAction<TView> OnItemUpdated;

        int FirstIndex => _items.Peek()?.Index ?? 0;
        int FirstVisibleIndex => Utils.ClampIndexFromPositionY(_rtContent.anchoredPosition.y, ItemHeight, _totalCnt - _visibleCnt);
        public int TotalCount => _totalCnt;
        public int VisibleCount => _visibleCnt;
        float ItemHeight => _rtItem.sizeDelta.y + _spacing;

        void OnDestroy()
        {
            _pool?.Clear();
        }

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
            _rtContent.SetSizeHeight(totalCnt * ItemHeight - _spacing);
            _items.ForEach(item => item.SetPositionY(CalculateItemPositionY(item.Index)));
        }
        #endregion //Content

        #region Item
        TView CreateItem(int idx)
        {
            TView item = _pool.Get();
            item.Index = idx;
            item.SetPositionY(CalculateItemPositionY(idx));
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

        float CalculateItemPositionY(int idx)
        {
            return (_rtContent.sizeDelta.y - _rtItem.sizeDelta.y) * (1 - _rtItem.pivot.y) - idx * ItemHeight;
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
