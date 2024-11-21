using System;
using UnityEngine;

namespace UnityTools.Util
{
    public interface IDynamicScrollItem
    {
        int Index { get; set; }
        void SetPositionY(float y);
    }

    public class DynamicScrollView<TView> : MonoBehaviour
        where TView : Component, IDynamicScrollItem, IPoolable
    {
        [SerializeField] TView _item;
        [SerializeField] int _visibleItemCnt = 3;
        [SerializeField] float _spacing = 0.0f;
        [SerializeField] RectTransform _rtContent;
        [SerializeField] RectTransform _rtItem;

        ObjectPool<TView> _itemPool;
        int _idx;
        int _totalItemCnt;
        readonly Deque<TView> _visibleItems = new();

        public Action<TView> OnItemIndexUpdated;

        public int TotalItemCount => _totalItemCnt;
        public int VisibleItemCount => _visibleItemCnt;
        float ItemHeight => _rtItem.sizeDelta.y + _spacing;

        public void InitView(int totalCnt)
        {
            _itemPool = new ObjectPool<TView>(_rtContent, _item, _visibleItemCnt);
            _idx = 0;
            _totalItemCnt = totalCnt;

            SetContentSize(_totalItemCnt);
            AddVisibleItems(_idx, _visibleItemCnt);
        }

        int ClampIndex(int idx)
        {
            return Mathf.Clamp(idx, 0, Mathf.Max(0, _totalItemCnt - _visibleItemCnt));
        }

        int GetIndex()
        {
            return ClampIndex(Mathf.FloorToInt(_rtContent.anchoredPosition.y / ItemHeight + 0.0001f));
        }

        void SetContentSize(int totalCnt)
        {
            _rtContent.SetSizeHeight(totalCnt * ItemHeight - _spacing);
        }

        TView CreateItem(int idx)
        {
            TView item = _itemPool.Get();
            InitItem(item, idx);
            return item;
        }

        void InitItem(TView item, int idx)
        {
            item.Index = idx;
            item.SetPositionY(CalculateItemPositionY(idx));

            if (idx < _totalItemCnt)
                OnItemIndexUpdated?.Invoke(item);
        }

        public void UpdateItems()
        {
            _visibleItems.ForEach(item => OnItemIndexUpdated?.Invoke(item));
        }

        float CalculateItemPositionY(int idx)
        {
            return _rtContent.sizeDelta.y / 2.0f - _rtItem.sizeDelta.y / 2.0f - idx * ItemHeight;
        }

        public void SetTotalItemCount(int cnt)
        {
            if (cnt == _totalItemCnt || cnt <= 0 || cnt < _visibleItemCnt)
                return;

            _totalItemCnt = cnt;

            SetContentSize(_totalItemCnt);
            _visibleItems.ForEach(item => item.SetPositionY(CalculateItemPositionY(item.Index)));
        }

        public void SetVisibleItemCount(int cnt)
        {
            if (cnt == _visibleItemCnt || cnt <= 0 || cnt > _totalItemCnt)
                return;

            int oldVisibleItemCnt = _visibleItemCnt;
            _visibleItemCnt = cnt;

            if (_visibleItemCnt < oldVisibleItemCnt)
                RemoveVisibleItems(oldVisibleItemCnt - _visibleItemCnt);
            else if (_visibleItemCnt > oldVisibleItemCnt)
                AddVisibleItems(_idx + oldVisibleItemCnt, _visibleItemCnt - oldVisibleItemCnt);
        }

        void AddVisibleItems(int startIdx, int cnt)
        {
            for (int i = startIdx; i < startIdx + cnt; i++)
            {
                if (i >= _totalItemCnt)
                {
                    _idx = i - _visibleItemCnt;
                    _visibleItems.EnqueueFront(CreateItem(_idx));
                }
                else
                {
                    _visibleItems.Enqueue(CreateItem(i));
                }
            }
        }

        void RemoveVisibleItems(int cnt)
        {
            for (int i = 0; i < cnt; i++)
            {
                _itemPool.Return(_visibleItems.DequeueBack());
            }

            _idx = GetIndex();
            int idx = _idx;
            _visibleItems.ForEach(item => { InitItem(item, idx++); });
        }

        public void OnScrollValueChanged(Vector2 value)
        {
            int newIdx = GetIndex();
            if (_idx == newIdx)
                return;

            int idxDiff = Mathf.Abs(newIdx - _idx);
            for (int i = 0; i < idxDiff; i++)
            {
                if (_idx < newIdx)
                {
                    _itemPool.Return(_visibleItems.Dequeue());
                    _visibleItems.Enqueue(CreateItem(i + _idx + _visibleItemCnt));
                }
                else
                {
                    _itemPool.Return(_visibleItems.DequeueBack());
                    _visibleItems.EnqueueFront(CreateItem(newIdx + idxDiff - i - 1));
                }
            }

            _idx = newIdx;
        }
    }
}
