using UnityEngine;
using UnityEngine.UI;

namespace UnityTools.Util
{
    public class DynamicScrollContext
    {
        readonly EScrollDirection _scrollDir;
        readonly int _itemCntPerLine;
        readonly Vector2 _spacing;
        readonly RectOffset _padding;
        readonly RectTransform _rtContent;
        readonly RectTransform _rtItem;

        float ContentWidth => _padding.left + LineWidth + _padding.right;
        float ContentHeight => _padding.top + LineHeight + _padding.bottom;
        float LineWidth => ItemWidth * _itemCntPerLine - _spacing.x;
        float LineHeight => ItemHeight * _itemCntPerLine - _spacing.y;
        float ItemWidth => _rtItem.sizeDelta.x + _spacing.x;
        float ItemHeight => _rtItem.sizeDelta.y + _spacing.y;

        public DynamicScrollContext(int itemCntPerLine, Vector2 spacing, RectOffset padding, RectTransform rtItem, ScrollRect scrollRect)
        {
            _scrollDir = scrollRect.vertical ? EScrollDirection.Vertical : EScrollDirection.Horizontal;
            _itemCntPerLine = itemCntPerLine;
            _spacing = spacing;
            _padding = padding;
            _rtContent = scrollRect.content;
            _rtItem = rtItem;
        }

        public float CalculateContentPosition(int idx, float offset = 0.0f)
        {
            int line = idx / _itemCntPerLine;
            if (_scrollDir == EScrollDirection.Vertical)
                return _padding.top + (line * ItemHeight) + offset;
            else
                return -(_padding.left + (line * ItemWidth) + offset);
        }

        public Vector2 CalculateContentSize(int cnt)
        {
            if (_scrollDir == EScrollDirection.Vertical)
                return new Vector2(ContentWidth, _padding.top + (cnt * ItemHeight - _spacing.y) + _padding.bottom);
            else
                return new Vector2(_padding.left + (cnt * ItemWidth - _spacing.x) + _padding.right, ContentHeight);
        }

        public int CalculateFirstVisibleLine(int lastLine)
        {
            if (_scrollDir == EScrollDirection.Vertical)
                return Utils.ClampIndexFromPosition(_rtContent.anchoredPosition.y - _padding.top, ItemHeight, lastLine);
            else
                return Utils.ClampIndexFromPosition(-_rtContent.anchoredPosition.x - _padding.left, ItemWidth, lastLine);
        }

        public int CalculateItemIndex(Vector2 pos)
        {
            if (_scrollDir == EScrollDirection.Vertical)
            {
                int x = Mathf.RoundToInt((((pos.x - _padding.left) + (ContentWidth - LineWidth) / 2.0f) / ItemWidth) + ((_itemCntPerLine - 1) / 2.0f));
                int y = Mathf.RoundToInt(((_rtContent.sizeDelta.y - _rtItem.sizeDelta.y) * (1 - _rtItem.pivot.y) - (pos.y + _padding.top)) / ItemHeight);
                return y * _itemCntPerLine + x;
            }
            else
            {
                int x = Mathf.RoundToInt((pos.x - (_padding.left + (1 - _rtItem.pivot.x) * (_rtItem.sizeDelta.x - _rtContent.sizeDelta.x))) / ItemWidth);
                int y = Mathf.RoundToInt(((_itemCntPerLine - 1) / 2.0f) + (((ContentHeight - LineHeight) / 2.0f - (_padding.top + pos.y)) / ItemHeight));
                return x * _itemCntPerLine + y;
            }
        }

        public Vector2 CalculateItemPosition(int idx)
        {
            if (_scrollDir == EScrollDirection.Vertical)
            {
                int x = idx % _itemCntPerLine;
                int y = idx / _itemCntPerLine;
                float posX = (x - ((_itemCntPerLine - 1) / 2.0f)) * ItemWidth - (ContentWidth - LineWidth) / 2.0f;
                float posY = (_rtContent.sizeDelta.y - _rtItem.sizeDelta.y) * (1 - _rtItem.pivot.y) - (y * ItemHeight);
                return new Vector2(_padding.left + posX, posY - _padding.top);
            }
            else
            {
                int y = idx % _itemCntPerLine;
                int x = idx / _itemCntPerLine;
                float posX = (_rtContent.sizeDelta.x - _rtItem.sizeDelta.x) * _rtItem.pivot.x + (x * ItemWidth);
                float posY = (y - ((_itemCntPerLine - 1) / 2.0f)) * ItemHeight - (ContentHeight - LineHeight) / 2.0f;
                return new Vector2(_padding.left + posX - _rtContent.sizeDelta.x + _rtItem.sizeDelta.x, -posY - _padding.top);
            }
        }
    }
}