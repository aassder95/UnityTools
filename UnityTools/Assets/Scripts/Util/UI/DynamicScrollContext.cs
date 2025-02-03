using UnityEngine;

namespace UnityTools.Util
{
    public class DynamicScrollContext
    {
        readonly EScrollDirection _scrollDir;
        readonly int _lineItemCnt;
        readonly RectTransform _rtContent;
        readonly RectTransform _rtItem;
        readonly RectOffset _padding;
        readonly Vector2 _spacing;

        float ItemWidth => _rtItem.sizeDelta.x + _spacing.x;
        float ItemHeight => _rtItem.sizeDelta.y + _spacing.y;

        public DynamicScrollContext(EScrollDirection scrollDir, int lineItemCnt, RectTransform rtContent, RectTransform rtItem, RectOffset padding, Vector2 spacing)
        {
            _scrollDir = scrollDir;
            _lineItemCnt = lineItemCnt;
            _rtContent = rtContent;
            _rtItem = rtItem;
            _padding = padding;
            _spacing = spacing;
        }

        public float CalculateContentPosition(int idx, float offset = 0.0f)
        {
            if (_scrollDir == EScrollDirection.Vertical)
                return (idx * ItemHeight) + offset;
            else
                return (idx * ItemWidth) + offset;
        }

        public Vector2 CalculateContentSize(int totalCnt)
        {
            if (_scrollDir == EScrollDirection.Vertical)
                return new Vector2(_rtContent.sizeDelta.x, _padding.top + (totalCnt * ItemHeight - _spacing.y) + _padding.bottom);
            else
                return new Vector2(_padding.left + (totalCnt * ItemWidth - _spacing.x) + _padding.right, _rtContent.sizeDelta.y);
        }

        public int CalculateItemIndex(float pos)
        {
            if (_scrollDir == EScrollDirection.Vertical)
                return (int)((((_rtContent.sizeDelta.y - _rtItem.sizeDelta.y) * (1 - _rtItem.pivot.y)) - (pos + _padding.top)) / ItemHeight);
            else
                return (int)((((_rtContent.sizeDelta.x - _rtItem.sizeDelta.x) * (1 - _rtItem.pivot.x)) - (-pos + _padding.left)) / ItemWidth);
        }

        public int CalculateFirstVisibleItemIndex(int lastIdx)
        {
            if (_scrollDir == EScrollDirection.Vertical)
                return Utils.ClampIndexFromPosition(_rtContent.anchoredPosition.y - _padding.top, ItemHeight, lastIdx);
            else
                return Utils.ClampIndexFromPosition(-_rtContent.anchoredPosition.x - _padding.left, ItemWidth, lastIdx);
        }

        public Vector2 CalculateItemPosition(int idx)
        {
            if (_scrollDir == EScrollDirection.Vertical)
            {
                float pos = (_rtContent.sizeDelta.y - _rtItem.sizeDelta.y) * (1 - _rtItem.pivot.y) - (idx * ItemHeight);
                return new Vector2(_padding.left + _rtItem.anchoredPosition.x, pos - _padding.top);
            }
            else
            {
                float pos = (_rtContent.sizeDelta.x - _rtItem.sizeDelta.x) * (1 - _rtItem.pivot.x) - (idx * ItemWidth);
                return new Vector2(-(pos - _padding.left), -_padding.top + _rtItem.anchoredPosition.y);
            }
        }
    }
}