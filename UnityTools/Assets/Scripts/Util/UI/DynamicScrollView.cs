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
        [SerializeField] TView _itemView;
        [SerializeField] RectTransform _rtContent;
        [SerializeField] RectTransform _rtItemView;

        ObjectPool<TView> _itemViewPool;
        readonly List<TView> _activeItemViews = new();

        public void InitView(int size)
        {
            _itemViewPool = new ObjectPool<TView>(_rtContent, _itemView, size);
        }

        public void UpdateView(List<TModel> itemModels)
        {
            foreach (TView itemView in _activeItemViews)
            {
                _itemViewPool.Return(itemView);
            }
            _activeItemViews.Clear();

            _rtContent.SetSizeHeight(itemModels.Count * _rtItemView.sizeDelta.y);

            for (int i = 0; i < itemModels.Count; i++)
            {
                TView itemView = _itemViewPool.Get();
                itemView.SetData(itemModels[i]);
                itemView.SetPositionY(_rtContent.sizeDelta.y / 2.0f - _rtItemView.sizeDelta.y / 2.0f - i * _rtItemView.sizeDelta.y);
                _activeItemViews.Add(itemView);
            }
        }
    }
}
