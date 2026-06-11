using UnityEngine;
using UnityEngine.UI;
using UnityTools.Util.Utilities;

namespace UnityTools.Util.UIFramework
{
    public class DynamicScrollContext
    {
        //============================================================
        // Readonly
        //============================================================
        private int _itemCountPerLine;
        private readonly Vector2 _spacing;
        private readonly RectOffset _padding;
        private readonly RectTransform _rtContent;
        private readonly RectTransform _rtItem;
        private readonly RectTransform _rtViewport;
        private readonly ScrollRect _scrollRect;

        //============================================================
        // Properties
        //============================================================
        private Vector2 ContentSize => new Vector2(_padding.left + LineSize.x + _padding.right, _padding.top + LineSize.y + _padding.bottom);
        private Vector2 LineSize => new Vector2(ItemSize.x * _itemCountPerLine - _spacing.x, ItemSize.y * _itemCountPerLine - _spacing.y);
        private Vector2 ItemSize => new Vector2(_rtItem.sizeDelta.x + _spacing.x, _rtItem.sizeDelta.y + _spacing.y);
        private Vector2 CenterOffset => new Vector2((ContentSize.x - LineSize.x) / 2.0f, (ContentSize.y - LineSize.y) / 2.0f);

        //============================================================
        // Constructors
        //============================================================
        public DynamicScrollContext(int itemCountPerLine, Vector2 spacing, RectOffset padding, RectTransform rtItem, ScrollRect scrollRect)
        {
            _itemCountPerLine = Mathf.Max(1, itemCountPerLine);
            _spacing = spacing;
            _padding = padding;
            _rtContent = scrollRect.content;
            _rtItem = rtItem;
            _rtViewport = scrollRect.viewport != null ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>();
            _scrollRect = scrollRect;
        }

        //============================================================
        // Logic
        //============================================================
        public Vector2 CalculateContentPosition(int itemIdx, float offset = 0.0f)
        {
            int line = itemIdx / _itemCountPerLine;
            return _scrollRect.vertical
                ? new Vector2(_rtContent.anchoredPosition.x, _padding.top + (line * ItemSize.y) + offset)
                : new Vector2(-(_padding.left + (line * ItemSize.x) + offset), _rtContent.anchoredPosition.y);
        }

        public void SetItemCountPerLine(int itemCountPerLine)
        {
            _itemCountPerLine = Mathf.Max(1, itemCountPerLine);
        }

        public Vector2 CalculateContentSize(int totalLineCount)
        {
            if(totalLineCount <= 0)
            {
                return _scrollRect.vertical
                    ? new Vector2(_rtContent.sizeDelta.x, _padding.top + _padding.bottom)
                    : new Vector2(_padding.left + _padding.right, _rtContent.sizeDelta.y);
            }

            return _scrollRect.vertical
                ? new Vector2(_rtContent.sizeDelta.x, _padding.top + (totalLineCount * ItemSize.y - _spacing.y) + _padding.bottom)
                : new Vector2(_padding.left + (totalLineCount * ItemSize.x - _spacing.x) + _padding.right, _rtContent.sizeDelta.y);
        }

        public Vector2 ClampContentPosition(Vector2 contentPos, int totalLineCount, int visibleLineCount)
        {
            int maxLine = Mathf.Max(0, totalLineCount - visibleLineCount);
            if(_scrollRect.vertical)
            {
                float minY = _padding.top;
                float maxY = _padding.top + (maxLine * ItemSize.y);
                return new Vector2(contentPos.x, Mathf.Clamp(contentPos.y, minY, maxY));
            }

            float minX = -(_padding.left + (maxLine * ItemSize.x));
            float maxX = -_padding.left;
            return new Vector2(Mathf.Clamp(contentPos.x, minX, maxX), contentPos.y);
        }

        public int CalculateAutoVisibleLineCount(int extraLineCount)
        {
            float viewportSize = _scrollRect.vertical ? _rtViewport.rect.height : _rtViewport.rect.width;
            float paddingSize = _scrollRect.vertical ? _padding.top + _padding.bottom : _padding.left + _padding.right;
            float availableSize = Mathf.Max(0.0f, viewportSize - paddingSize);
            float itemMainSize = _scrollRect.vertical ? ItemSize.y : ItemSize.x;
            float spacingMain = _scrollRect.vertical ? _spacing.y : _spacing.x;

            if(itemMainSize <= 0.0f)
                return Mathf.Max(1, extraLineCount + 1);

            int visibleLineCount = Mathf.CeilToInt((availableSize + spacingMain) / itemMainSize);
            if(visibleLineCount <= 0)
                visibleLineCount = 1;

            return visibleLineCount + Mathf.Max(0, extraLineCount);
        }

        public int CalculateFirstVisibleItemIndex(int lastLine) => CalculateFirstVisibleLine(lastLine) * _itemCountPerLine;

        public int CalculateFirstVisibleLine(int lastLine)
        {
            return _scrollRect.vertical
                ? IndexUtils.CalculateClampedIndexFromPosition(_rtContent.anchoredPosition.y - _padding.top, ItemSize.y, lastLine)
                : IndexUtils.CalculateClampedIndexFromPosition(-_rtContent.anchoredPosition.x - _padding.left, ItemSize.x, lastLine);
        }

        public Vector2 CalculateItemPosition(int itemIdx)
        {
            if(_scrollRect.vertical)
            {
                int x = itemIdx % _itemCountPerLine;
                int y = itemIdx / _itemCountPerLine;
                float posX = (x - ((_itemCountPerLine - 1) / 2.0f)) * ItemSize.x - CenterOffset.x;
                float posY = (_rtContent.sizeDelta.y - _rtItem.sizeDelta.y) * (1 - _rtItem.pivot.y) - (y * ItemSize.y);
                return new Vector2(_padding.left + posX, posY - _padding.top);
            }

            int col = itemIdx / _itemCountPerLine;
            int row = itemIdx % _itemCountPerLine;
            float xPos = (_rtContent.sizeDelta.x - _rtItem.sizeDelta.x) * _rtItem.pivot.x + (col * ItemSize.x);
            float yPos = (row - ((_itemCountPerLine - 1) / 2.0f)) * ItemSize.y - CenterOffset.y;
            return new Vector2(_padding.left + xPos - _rtContent.sizeDelta.x + _rtItem.sizeDelta.x, -yPos - _padding.top);
        }

        public int GetItemCountForLine(int line, int totalItemCount)
        {
            if(line < 0 || _itemCountPerLine <= 0 || totalItemCount <= 0)
                return 0;

            int totalLineCount = Mathf.CeilToInt((float)totalItemCount / _itemCountPerLine);
            if(line >= totalLineCount)
                return 0;

            int lastLineItemCount = totalItemCount % _itemCountPerLine;
            bool isLastLine = line == totalLineCount - 1;
            if(isLastLine && lastLineItemCount > 0)
                return lastLineItemCount;

            return _itemCountPerLine;
        }
    }
}

