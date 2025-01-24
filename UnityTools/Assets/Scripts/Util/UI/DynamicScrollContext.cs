using UnityEngine;

namespace UnityTools.Util
{
    public class DynamicScrollContext
    {
        readonly EScrollDirection _scrollDir;
        readonly RectTransform _rtContent;
        readonly RectTransform _rtItem;
        readonly float _spacing;
        readonly float _paddingStart;
        readonly float _paddingEnd;

        public float ContentPos => _scrollDir == EScrollDirection.Vertical ? _rtContent.anchoredPosition.y : -_rtContent.anchoredPosition.x;
        float ContentSize => _scrollDir == EScrollDirection.Vertical ? _rtContent.sizeDelta.y : _rtContent.sizeDelta.x;
        float ItemPivot => _scrollDir == EScrollDirection.Vertical ? _rtItem.pivot.y : _rtItem.pivot.x;
        float ItemOriginSize => _scrollDir == EScrollDirection.Vertical ? _rtItem.sizeDelta.y : _rtItem.sizeDelta.x;
        public float ItemSize => ItemOriginSize + _spacing;

        public DynamicScrollContext(EScrollDirection scrollDir, RectTransform rtContent, RectTransform rtItem, float spacing, float paddingStart, float paddingEnd)
        {
            _scrollDir = scrollDir;
            _rtContent = rtContent;
            _rtItem = rtItem;
            _spacing = spacing;
            _paddingStart = paddingStart;
            _paddingEnd = paddingEnd;
        }

        public float CalculateContentPosition(int idx, float offset = 0.0f)
        {
            return (idx * (_rtItem.sizeDelta.y + _spacing)) + offset;
        }

        public float CalculateContentSize(int totalItemCount)
        {
            return _paddingStart + (totalItemCount * ItemSize - _spacing) + _paddingEnd;
        }

        public int CalculateItemIndex(float pos)
        {
            pos = _scrollDir == EScrollDirection.Vertical ? pos : -pos;
            return (int)((((ContentSize - ItemOriginSize) * (1 - ItemPivot)) - (pos + _paddingStart)) / ItemSize);
        }

        public int CalculateFirstVisibleItemIndex(int totalCnt, int visibleCnt)
        {
            return Utils.ClampIndexFromPosition(ContentPos - _paddingStart, ItemSize, totalCnt - visibleCnt);
        }

        public Vector2 CalculateItemPosition(int idx)
        {
            float pos = (ContentSize - ItemOriginSize) * (1 - ItemPivot) - (idx * ItemSize) - _paddingStart;

            if (_scrollDir == EScrollDirection.Vertical)
                return new Vector2(_rtItem.anchoredPosition.x, pos);
            else
                return new Vector2(-pos, _rtItem.anchoredPosition.y);
        }
    }
}