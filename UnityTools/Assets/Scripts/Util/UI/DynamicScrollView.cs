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
        [SerializeField] int _originVisibleItemCnt = 3;
        [SerializeField] float _spacing = 0.0f;
        [SerializeField] RectTransform _rtContent;
        [SerializeField] RectTransform _rtItem;

        ObjectPool<TView> _itemPool;
        int _idx;
        int _totalItemCnt;
        int _visibleItemCnt;
        readonly Deque<TView> _visibleItems = new();

        public Action<TView> OnItemUpdated;

        float ItemHeight => _rtItem.sizeDelta.y + _spacing;

        public void InitView(int totalCnt)
        {
            _itemPool = new ObjectPool<TView>(_rtContent, _item, _originVisibleItemCnt);
            _idx = 0;
            _totalItemCnt = totalCnt;
            _visibleItemCnt = _originVisibleItemCnt;

            SetContentSize(_totalItemCnt);
            AddVisibleItems(_visibleItemCnt);
        }

        public void UpdateView()
        {
            _visibleItems.ForEach(item => OnItemUpdated?.Invoke(item));
        }

        TView GenerateItem(int idx)
        {
            TView item = _itemPool.Get();
            item.Index = idx;
            item.SetPositionY(CalculateItemPositionY(idx));
            OnItemUpdated?.Invoke(item);

            return item;
        }

        void AddVisibleItems(int endIdx) => AddVisibleItems(_idx, endIdx);
        void AddVisibleItems(int startIdx, int endIdx)
        {
            for (int i = startIdx; i < startIdx + endIdx; i++)
            {
                _visibleItems.Enqueue(GenerateItem(i));
            }
        }

        float CalculateItemPositionY(int idx)
        {
            return _rtContent.sizeDelta.y / 2.0f - _rtItem.sizeDelta.y / 2.0f - idx * ItemHeight;
        }

        int ClampIndex(int idx)
        {
            return Mathf.Clamp(idx, 0, Mathf.Max(0, _totalItemCnt - _visibleItemCnt));
        }

        void SetContentSize(int totalCnt)
        {
            _rtContent.SetSizeHeight(totalCnt * ItemHeight - _spacing);
        }

        public void OnScrollValueChanged(Vector2 value)
        {
            int newIdx = ClampIndex(Mathf.FloorToInt(_rtContent.anchoredPosition.y / ItemHeight + 0.0001f));
            if (_idx == newIdx)
                return;

            int idxDiff = Mathf.Abs(newIdx - _idx);

            for (int i = 0; i < idxDiff; i++)
            {
                if (_idx < newIdx)
                {
                    _itemPool.Return(_visibleItems.Dequeue());
                    _visibleItems.Enqueue(GenerateItem(i + _idx + _visibleItemCnt));
                }
                else
                {
                    _itemPool.Return(_visibleItems.DequeueBack());
                    _visibleItems.EnqueueFront(GenerateItem(i + newIdx));
                }
            }

            _idx = newIdx;
        }
    }
}
