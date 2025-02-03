using UnityEngine;

namespace UnityTools.Util
{
    public class DynamicScrollContext
    {
        readonly EScrollDirection _scrollDir;
        readonly RectTransform _rtContent;
        readonly RectTransform _rtItem;
        readonly RectOffset _padding;
        readonly float _spacing;

        public float ContentPos => _scrollDir == EScrollDirection.Vertical ? _rtContent.anchoredPosition.y : -_rtContent.anchoredPosition.x;
        float ContentSize => _scrollDir == EScrollDirection.Vertical ? _rtContent.sizeDelta.y : _rtContent.sizeDelta.x;
        float ItemPivot => _scrollDir == EScrollDirection.Vertical ? _rtItem.pivot.y : _rtItem.pivot.x;
        float ItemOriginSize => _scrollDir == EScrollDirection.Vertical ? _rtItem.sizeDelta.y : _rtItem.sizeDelta.x;
        public float ItemSize => ItemOriginSize + _spacing;

        public DynamicScrollContext(EScrollDirection scrollDir, RectTransform rtContent, RectTransform rtItem, RectOffset padding, float spacing)
        {
            _scrollDir = scrollDir;
            _rtContent = rtContent;
            _rtItem = rtItem;
            _padding = padding;
            _spacing = spacing;
        }

        public float CalculateContentPosition(int idx, float offset = 0.0f)
        {
            return (idx * (_rtItem.sizeDelta.y + _spacing)) + offset;
        }

        public Vector2 CalculateContentSize(int totalCnt)
        {
            if (_scrollDir == EScrollDirection.Vertical)
                return new Vector2(_rtContent.sizeDelta.x, _padding.top + (totalCnt * ItemSize - _spacing) + _padding.bottom);
            else
                return new Vector2(_padding.left + (totalCnt * ItemSize - _spacing) + _padding.right, _rtContent.sizeDelta.y);
        }

        public int CalculateItemIndex(float pos)
        {
            pos = _scrollDir == EScrollDirection.Vertical ? pos + _padding.top : -pos + _padding.left;
            return (int)((((ContentSize - ItemOriginSize) * (1 - ItemPivot)) - pos) / ItemSize);
        }

        public int CalculateFirstVisibleItemIndex(int lastIdx)
        {
            int padding = _scrollDir == EScrollDirection.Vertical ? _padding.top : _padding.left;
            return Utils.ClampIndexFromPosition(ContentPos - padding, ItemSize, lastIdx);
        }

        public Vector2 CalculateItemPosition(int idx)
        {
            float pos = (ContentSize - ItemOriginSize) * (1 - ItemPivot) - (idx * ItemSize);
            if (_scrollDir == EScrollDirection.Vertical)
                return new Vector2(_padding.left + _rtItem.anchoredPosition.x, pos - _padding.top);
            else
                return new Vector2(-(pos - _padding.left), _padding.top + _rtItem.anchoredPosition.y);
        }
    }
}