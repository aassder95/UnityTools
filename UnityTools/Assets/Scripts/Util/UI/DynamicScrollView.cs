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

            EnsureItemViewCount(0, size);
        }

        public void UpdateView(List<TModel> itemModels)
        {
            AdjustContentSize(itemModels.Count);
            int activeCnt = _activeItemViews.Count;
            int modelCnt = itemModels.Count;

            EnsureItemViewCount(activeCnt, modelCnt);
            UpdateItemViewsData(itemModels);
            RemoveExcessItemViews(activeCnt, modelCnt);
        }

        void AdjustContentSize(int modelCount)
        {
            _rtContent.SetSizeHeight(modelCount * _rtItemView.sizeDelta.y);
        }

        void EnsureItemViewCount(int activeCnt, int modelCnt)
        {
            if (activeCnt < modelCnt)
            {
                AddNewItemViews(activeCnt, modelCnt);
            }
        }

        void AddNewItemViews(int startIndex, int endIndex)
        {
            for (int i = startIndex; i < endIndex; i++)
            {
                TView itemView = _itemViewPool.Get();
                float positionY = CalculateItemPositionY(i);
                itemView.SetPositionY(positionY);
                _activeItemViews.Add(itemView);
            }
        }

        float CalculateItemPositionY(int index)
        {
            return _rtContent.sizeDelta.y / 2.0f - _rtItemView.sizeDelta.y / 2.0f - index * _rtItemView.sizeDelta.y;
        }

        void UpdateItemViewsData(List<TModel> itemModels)
        {
            for (int i = 0; i < itemModels.Count; i++)
            {
                TView itemView = _activeItemViews[i];
                itemView.SetData(itemModels[i]);
            }
        }

        void RemoveExcessItemViews(int activeCnt, int modelCnt)
        {
            if (activeCnt > modelCnt)
            {
                ReturnExcessItemViews(modelCnt, activeCnt);
                _activeItemViews.RemoveRange(modelCnt, activeCnt - modelCnt);
            }
        }

        void ReturnExcessItemViews(int startIndex, int endIndex)
        {
            for (int i = startIndex; i < endIndex; i++)
            {
                TView itemView = _activeItemViews[i];
                _itemViewPool.Return(itemView);
            }
        }
    }
}
