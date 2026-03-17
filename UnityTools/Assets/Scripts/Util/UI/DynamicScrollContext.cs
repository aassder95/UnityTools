using UnityEngine;
using UnityEngine.UI;

namespace UnityTools.Util
{
    //============================================================
    //Logic
    //============================================================
    public class DynamicScrollContext
    {
        private readonly int _itemCntPerLine;
        private readonly Vector2 _spacing;
        private readonly RectOffset _padding;
        private readonly RectTransform _rtContent;
        private readonly RectTransform _rtItem;
        private readonly ScrollRect _scrollRect;

        private Vector2 ContentSize => new Vector2(_padding.left + LineSize.x + _padding.right, _padding.top + LineSize.y + _padding.bottom);
        private Vector2 LineSize => new Vector2(ItemSize.x * _itemCntPerLine - _spacing.x, ItemSize.y * _itemCntPerLine - _spacing.y);
        private Vector2 ItemSize => new Vector2(_rtItem.sizeDelta.x + _spacing.x, _rtItem.sizeDelta.y + _spacing.y);
        private Vector2 CenterOffset => new Vector2((ContentSize.x - LineSize.x) / 2.0f, (ContentSize.y - LineSize.y) / 2.0f);

        public DynamicScrollContext(int itemCntPerLine, Vector2 spacing, RectOffset padding, RectTransform rtItem, ScrollRect scrollRect)
        {
            _itemCntPerLine = itemCntPerLine;
            _spacing = spacing;
            _padding = padding;
            _rtContent = scrollRect.content;
            _rtItem = rtItem;
            _scrollRect = scrollRect;
        }

        public Vector2 CalculateContentPosition(int itemIdx, float offset = 0.0f)
        {
            int line = itemIdx / _itemCntPerLine;
            return _scrollRect.vertical ?
                new Vector2(_rtContent.anchoredPosition.x, _padding.top + (line * ItemSize.y) + offset) :
                new Vector2(-(_padding.left + (line * ItemSize.x) + offset), _rtContent.anchoredPosition.y);
        }

        public Vector2 CalculateContentSize(int totalLineCnt)
        {
            return _scrollRect.vertical ?
                new Vector2(_rtContent.sizeDelta.x, _padding.top + (totalLineCnt * ItemSize.y - _spacing.y) + _padding.bottom) :
                new Vector2(_padding.left + (totalLineCnt * ItemSize.x - _spacing.x) + _padding.right, _rtContent.sizeDelta.y);
        }

        public int CalculateFirstVisibleItemIndex(int lastLine) => CalculateFirstVisibleLine(lastLine) * _itemCntPerLine;
        public int CalculateFirstVisibleLine(int lastLine)
        {
            return _scrollRect.vertical ?
                IndexUtils.CalculateClampedIndexFromPosition(_rtContent.anchoredPosition.y - _padding.top, ItemSize.y, lastLine) :
                IndexUtils.CalculateClampedIndexFromPosition(-_rtContent.anchoredPosition.x - _padding.left, ItemSize.x, lastLine);
        }

        public int CalculateItemIndex(Vector2 itemPos)
        {
            if (_scrollRect.vertical)
            {
                int x = Mathf.RoundToInt((((itemPos.x - _padding.left) + CenterOffset.x) / ItemSize.x) + ((_itemCntPerLine - 1) / 2.0f));
                int y = Mathf.RoundToInt(((_rtContent.sizeDelta.y - _rtItem.sizeDelta.y) * (1 - _rtItem.pivot.y) - (itemPos.y + _padding.top)) / ItemSize.y);
                return y * _itemCntPerLine + x;
            }
            else
            {
                int x = Mathf.RoundToInt((itemPos.x - (_padding.left + (1 - _rtItem.pivot.x) * (_rtItem.sizeDelta.x - _rtContent.sizeDelta.x))) / ItemSize.x);
                int y = Mathf.RoundToInt(((_itemCntPerLine - 1) / 2.0f) + ((CenterOffset.y - (_padding.top + itemPos.y)) / ItemSize.y));
                return x * _itemCntPerLine + y;
            }
        }

        public Vector2 CalculateItemPosition(int itemIdx)
        {
            if (_scrollRect.vertical)
            {
                int x = itemIdx % _itemCntPerLine;
                int y = itemIdx / _itemCntPerLine;
                float posX = (x - ((_itemCntPerLine - 1) / 2.0f)) * ItemSize.x - CenterOffset.x;
                float posY = (_rtContent.sizeDelta.y - _rtItem.sizeDelta.y) * (1 - _rtItem.pivot.y) - (y * ItemSize.y);
                return new Vector2(_padding.left + posX, posY - _padding.top);
            }
            else
            {
                int y = itemIdx % _itemCntPerLine;
                int x = itemIdx / _itemCntPerLine;
                float posX = (_rtContent.sizeDelta.x - _rtItem.sizeDelta.x) * _rtItem.pivot.x + (x * ItemSize.x);
                float posY = (y - ((_itemCntPerLine - 1) / 2.0f)) * ItemSize.y - CenterOffset.y;
                return new Vector2(_padding.left + posX - _rtContent.sizeDelta.x + _rtItem.sizeDelta.x, -posY - _padding.top);
            }
        }

        public int GetItemCountForLine(int line, int totalItemCnt)
        {
            int lastLineItemCnt = totalItemCnt % _itemCntPerLine;
            return (lastLineItemCnt > 0 && line == totalItemCnt / _itemCntPerLine) ? lastLineItemCnt : _itemCntPerLine;
        }

        public int GetItemCountForLineRange(int newLineCnt, int prevLineCnt, int totalItemCnt, int firstIdx)
        {
            bool isAdd = newLineCnt > prevLineCnt;
            int itemCnt = 0;
            for (int i = 0, lineCnt = Mathf.Abs(newLineCnt - prevLineCnt); i < lineCnt; i++)
            {
                itemCnt += GetItemCountForLine((isAdd ? prevLineCnt : newLineCnt) + firstIdx / _itemCntPerLine + i, totalItemCnt);
            }

            return itemCnt;
        }
    }
}
