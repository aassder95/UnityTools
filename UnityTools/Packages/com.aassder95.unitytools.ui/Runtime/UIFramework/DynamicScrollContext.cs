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
        private readonly Vector2 _spacing;
        private readonly RectOffset _padding;
        private readonly RectTransform _rtContent;
        private readonly RectTransform _rtItem;
        private readonly RectTransform _rtViewport;
        private readonly ScrollRect _scrollRect;

        //============================================================
        // Fields
        //============================================================
        private int _itemCntPerLine;

        //============================================================
        // Properties
        //============================================================
        private Vector2 ContentSize => new(_padding.left + LineSize.x + _padding.right, _padding.top + LineSize.y + _padding.bottom);
        private Vector2 LineSize => new(ItemSize.x * _itemCntPerLine - _spacing.x, ItemSize.y * _itemCntPerLine - _spacing.y);
        private Vector2 ItemSize => new(_rtItem.sizeDelta.x + _spacing.x, _rtItem.sizeDelta.y + _spacing.y);
        private Vector2 CenterOffset => new((ContentSize.x - LineSize.x) / 2.0f, (ContentSize.y - LineSize.y) / 2.0f);

        //============================================================
        // Constructors
        //============================================================
        public DynamicScrollContext(int itemCntPerLine, Vector2 spacing, RectOffset padding, RectTransform rtItem, ScrollRect scrollRect)
        {
            _itemCntPerLine = itemCntPerLine;
            _spacing = spacing;
            _padding = padding;
            _rtContent = scrollRect.content;
            _rtItem = rtItem;
            _rtViewport = scrollRect.viewport != null ? scrollRect.viewport : scrollRect.transform as RectTransform;
            _scrollRect = scrollRect;
        }

        //============================================================
        // Logic
        //============================================================
        public Vector2 GetContentPos(int itemIdx, float offset = 0.0f)
        {
            int line = itemIdx / _itemCntPerLine;
            return _scrollRect.vertical ? new Vector2(_rtContent.anchoredPosition.x, _padding.top + (line * ItemSize.y) + offset) : new Vector2(-(_padding.left + (line * ItemSize.x) + offset), _rtContent.anchoredPosition.y);
        }

        public void SetItemCntPerLine(int itemCntPerLine)
        {
            _itemCntPerLine = itemCntPerLine;
        }

        public Vector2 GetContentSize(int totalLineCnt)
        {
            if(totalLineCnt <= 0)
                return _scrollRect.vertical ? new Vector2(ContentSize.x, _padding.top + _padding.bottom) : new Vector2(_padding.left + _padding.right, ContentSize.y);

            return _scrollRect.vertical ? new Vector2(ContentSize.x, _padding.top + (totalLineCnt * ItemSize.y - _spacing.y) + _padding.bottom) : new Vector2(_padding.left + (totalLineCnt * ItemSize.x - _spacing.x) + _padding.right, ContentSize.y);
        }

        public Vector2 ClampContentPos(Vector2 contentPos, int totalLineCnt, int visibleLineCnt)
        {
            int maxLine = Mathf.Max(0, totalLineCnt - visibleLineCnt);
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

        public int GetAutoVisibleLineCnt(int extraLineCnt)
        {
            float viewportSize = _scrollRect.vertical ? _rtViewport.rect.height : _rtViewport.rect.width;
            float paddingSize = _scrollRect.vertical ? _padding.top + _padding.bottom : _padding.left + _padding.right;
            float availableSize = Mathf.Max(0.0f, viewportSize - paddingSize);
            float itemMainSize = _scrollRect.vertical ? ItemSize.y : ItemSize.x;
            float spacingMain = _scrollRect.vertical ? _spacing.y : _spacing.x;

            int visibleLineCnt = Mathf.Max(1, Mathf.CeilToInt((availableSize + spacingMain) / itemMainSize));
            return visibleLineCnt + Mathf.Max(0, extraLineCnt);
        }

        public int GetFirstVisibleItemIdx(int lastLine) => GetFirstVisibleLine(lastLine) * _itemCntPerLine;

        public int GetFirstVisibleLine(int lastLine)
        {
            return _scrollRect.vertical ? IdxUtils.GetClampedIdx(_rtContent.anchoredPosition.y - _padding.top, ItemSize.y, lastLine) : IdxUtils.GetClampedIdx(-_rtContent.anchoredPosition.x - _padding.left, ItemSize.x, lastLine);
        }

        public Vector2 GetItemPos(int itemIdx)
        {
            if(_scrollRect.vertical)
            {
                int x = itemIdx % _itemCntPerLine;
                int y = itemIdx / _itemCntPerLine;
                float posX = (x - ((_itemCntPerLine - 1) / 2.0f)) * ItemSize.x - CenterOffset.x;
                float posY = (_rtContent.sizeDelta.y - _rtItem.sizeDelta.y) * (1 - _rtItem.pivot.y) - (y * ItemSize.y);
                return new Vector2(_padding.left + posX, posY - _padding.top);
            }

            int col = itemIdx / _itemCntPerLine;
            int row = itemIdx % _itemCntPerLine;
            float xPos = (_rtContent.sizeDelta.x - _rtItem.sizeDelta.x) * _rtItem.pivot.x + (col * ItemSize.x);
            float yPos = (row - ((_itemCntPerLine - 1) / 2.0f)) * ItemSize.y - CenterOffset.y;
            return new Vector2(_padding.left + xPos - _rtContent.sizeDelta.x + _rtItem.sizeDelta.x, -yPos - _padding.top);
        }

        public int GetItemCntForLine(int line, int totalItemCnt)
        {
            if(line < 0 || totalItemCnt <= 0)
                return 0;

            int totalLineCnt = Mathf.CeilToInt((float)totalItemCnt / _itemCntPerLine);
            if(line >= totalLineCnt)
                return 0;

            int lastLineItemCnt = totalItemCnt % _itemCntPerLine;
            bool isLastLine = line == totalLineCnt - 1;
            if(isLastLine && lastLineItemCnt > 0)
                return lastLineItemCnt;

            return _itemCntPerLine;
        }
    }
}
