using System;
using UnityEngine;

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
        [SerializeField] int _originVisibleCnt = 3;
        [SerializeField] float _spacing = 0.0f;
        [SerializeField] RectTransform _rtContent;
        [SerializeField] RectTransform _rtItem;

        ObjectPool<TView> _pool;
        int _firstIdx;
        int _totalCnt;
        int _visibleCnt;
        readonly Deque<TView> _items = new();

        public Action<TView> OnItemUpdated;

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
            if (_visibleCnt > totalCnt)
                _visibleCnt = totalCnt;

            _pool = new ObjectPool<TView>(_rtContent, _item, _visibleCnt);

            SetTotalCount(totalCnt);
            SetVisibleCount(_originVisibleCnt);
        }

        public void UpdateView()
        {
            _items.ForEach(item => OnItemUpdated?.Invoke(item));
        }

        public void SetTotalCount(int cnt)
        {
            if (cnt == _totalCnt || cnt < _visibleCnt)
                return;

            _totalCnt = cnt;
            SetContentSize(_totalCnt);
        }

        public void SetVisibleCount(int cnt)
        {
            if (cnt == _visibleCnt || cnt <= 0 || cnt > _totalCnt)
                return;

            int diff = cnt - _visibleCnt;
            _visibleCnt = cnt;

            if (diff > 0)
                AddItems(diff);
            else
                RemoveItems(-diff);
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
            InitItem(item, idx);
            return item;
        }

        void InitItem(TView item, int idx)
        {
            item.Index = idx;
            item.SetPositionY(CalculateItemPositionY(idx));

            if (idx < _totalCnt)
                OnItemUpdated?.Invoke(item);
        }

        void AddItems(int cnt)
        {
            int startIdx = _firstIdx + _items.Count;
            for (int i = 0; i < cnt; i++)
            {
                if (startIdx + i >= _totalCnt)
                    _items.EnqueueFront(CreateItem(--_firstIdx));
                else
                    _items.Enqueue(CreateItem(startIdx + i));
            }
        }

        void RemoveItems(int cnt)
        {
            int newFirstIdx = GetIndex();
            for (int i = 0; i < cnt; i++)
            {
                if (_firstIdx < newFirstIdx)
                {
                    _pool.Return(_items.Dequeue());
                    _firstIdx++;
                }
                else
                {
                    _pool.Return(_items.DequeueBack());
                }
            }
        }

        float CalculateItemPositionY(int idx)
        {
            return _rtContent.sizeDelta.y / 2.0f - _rtItem.sizeDelta.y / 2.0f - idx * ItemHeight;
        }
        #endregion //Item

        #region Index
        int ClampIndex(int idx)
        {
            return Mathf.Clamp(idx, 0, Mathf.Max(0, _totalCnt - _visibleCnt));
        }

        int GetIndex()
        {
            return ClampIndex(Mathf.FloorToInt(_rtContent.anchoredPosition.y / ItemHeight + 0.0001f));
        }
        #endregion //Index

        #region Callback
        public void OnScrollValueChanged(Vector2 value)
        {
            int newFirstIdx = GetIndex();
            if (_firstIdx == newFirstIdx)
                return;

            int idxDiff = Mathf.Abs(newFirstIdx - _firstIdx);
            for (int i = 0; i < idxDiff; i++)
            {
                if (_firstIdx < newFirstIdx)
                {
                    _pool.Return(_items.Dequeue());
                    _items.Enqueue(CreateItem(i + _firstIdx + _visibleCnt));
                }
                else
                {
                    _pool.Return(_items.DequeueBack());
                    _items.EnqueueFront(CreateItem(newFirstIdx + idxDiff - i - 1));
                }
            }

            _firstIdx = newFirstIdx;
        }
        #endregion //Callback
    }
}
