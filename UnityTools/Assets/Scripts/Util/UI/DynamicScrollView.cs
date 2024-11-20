using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Util
{
    public interface IDynamicScrollItem<TModel>
    {
        void SetData(TModel model);
        void SetPositionY(float y);
    }

    public class DynamicScrollView<TModel, TView> : MonoBehaviour
        where TView : Component, IDynamicScrollItem<TModel>, IPoolable
    {
        [SerializeField] TView _item;
        [SerializeField] RectTransform _rtContent;
        [SerializeField] RectTransform _rtItem;

        ObjectPool<TView> _itemPool;
        readonly Deque<TView> _visibleItems = new();

        public void InitView(int size)
        {
            _itemPool = new ObjectPool<TView>(_rtContent, _item, size);

            SetContentSize(size);

            for (int i = 0; i < size; i++)
            {
                TView item = _itemPool.Get();
                item.SetPositionY(CalculateItemPositionY(i));
                _visibleItems.Enqueue(item);
            }
        }

        public void UpdateView(List<TModel> models)
        {
            int idx = 0;
            foreach (TView item in _visibleItems)
            {
                item.SetData(models[idx]);
                idx++;
            }
        }

        float CalculateItemPositionY(int idx)
        {
            return _rtContent.sizeDelta.y / 2.0f - _rtItem.sizeDelta.y / 2.0f - idx * _rtItem.sizeDelta.y;
        }

        void SetContentSize(int totalCnt)
        {
            _rtContent.SetSizeHeight(totalCnt * _rtItem.sizeDelta.y);
        }
    }
}
