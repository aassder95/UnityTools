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

        Vector2 ContentSize => new Vector2(_padding.left + LineSize.x + _padding.right, _padding.top + LineSize.y + _padding.bottom);
        Vector2 LineSize => new Vector2(ItemSize.x * _itemCntPerLine - _spacing.x, ItemSize.y * _itemCntPerLine - _spacing.y);
        Vector2 ItemSize => new Vector2(_rtItem.sizeDelta.x + _spacing.x, _rtItem.sizeDelta.y + _spacing.y);
        Vector2 CenterOffset => new Vector2((ContentSize.x - LineSize.x) / 2.0f, (ContentSize.y - LineSize.y) / 2.0f);

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
                return _padding.top + (line * ItemSize.y) + offset;
            else
                return -(_padding.left + (line * ItemSize.x) + offset);
        }

        public Vector2 CalculateContentSize(int cnt)
        {
            if (_scrollDir == EScrollDirection.Vertical)
                return new Vector2(ContentSize.x, _padding.top + (cnt * ItemSize.y - _spacing.y) + _padding.bottom);
            else
                return new Vector2(_padding.left + (cnt * ItemSize.x - _spacing.x) + _padding.right, ContentSize.y);
        }

        public int CalculateFirstVisibleLine(int lastLine)
        {
            if (_scrollDir == EScrollDirection.Vertical)
                return Utils.ClampIndexFromPosition(_rtContent.anchoredPosition.y - _padding.top, ItemSize.y, lastLine);
            else
                return Utils.ClampIndexFromPosition(-_rtContent.anchoredPosition.x - _padding.left, ItemSize.x, lastLine);
        }

        public int CalculateItemIndex(Vector2 pos)
        {
            if (_scrollDir == EScrollDirection.Vertical)
            {
                int x = Mathf.RoundToInt((((pos.x - _padding.left) + CenterOffset.x) / ItemSize.x) + ((_itemCntPerLine - 1) / 2.0f));
                int y = Mathf.RoundToInt(((_rtContent.sizeDelta.y - _rtItem.sizeDelta.y) * (1 - _rtItem.pivot.y) - (pos.y + _padding.top)) / ItemSize.y);
                return y * _itemCntPerLine + x;
            }
            else
            {
                int x = Mathf.RoundToInt((pos.x - (_padding.left + (1 - _rtItem.pivot.x) * (_rtItem.sizeDelta.x - _rtContent.sizeDelta.x))) / ItemSize.x);
                int y = Mathf.RoundToInt(((_itemCntPerLine - 1) / 2.0f) + ((CenterOffset.y - (_padding.top + pos.y)) / ItemSize.y));
                return x * _itemCntPerLine + y;
            }
        }

        public Vector2 CalculateItemPosition(int idx)
        {
            if (_scrollDir == EScrollDirection.Vertical)
            {
                int x = idx % _itemCntPerLine;
                int y = idx / _itemCntPerLine;
                float posX = (x - ((_itemCntPerLine - 1) / 2.0f)) * ItemSize.x - CenterOffset.x;
                float posY = (_rtContent.sizeDelta.y - _rtItem.sizeDelta.y) * (1 - _rtItem.pivot.y) - (y * ItemSize.y);
                return new Vector2(_padding.left + posX, posY - _padding.top);
            }
            else
            {
                int y = idx % _itemCntPerLine;
                int x = idx / _itemCntPerLine;
                float posX = (_rtContent.sizeDelta.x - _rtItem.sizeDelta.x) * _rtItem.pivot.x + (x * ItemSize.x);
                float posY = (y - ((_itemCntPerLine - 1) / 2.0f)) * ItemSize.y - CenterOffset.y;
                return new Vector2(_padding.left + posX - _rtContent.sizeDelta.x + _rtItem.sizeDelta.x, -posY - _padding.top);
            }
        }
    }
}