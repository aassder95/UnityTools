using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityTools.Ui
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
        private float[] _itemHeights;
        private float[] _itemOffsets;

        //============================================================
        // Properties
        //============================================================
        public bool HasVariableHeights => _itemHeights != null;
        public int ItemCnt => _itemHeights == null ? 0 : _itemHeights.Length;
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
        public Vector2 GetContentPos(int itemIdx, float offset = 0.0f, EDynamicScrollAlignment alignment = EDynamicScrollAlignment.Start)
        {
            int line = itemIdx / _itemCntPerLine;
            if (_scrollRect.vertical)
            {
                float itemHeight = HasVariableHeights ? _itemHeights[itemIdx] : _rtItem.sizeDelta.y;
                float itemTop = HasVariableHeights ? _itemOffsets[itemIdx] : line * ItemSize.y;
                float alignmentOffset = GetAlignmentOffset(alignment, _rtViewport.rect.height, itemHeight);
                if (HasVariableHeights && itemHeight > _rtViewport.rect.height)
                    alignmentOffset = alignment == EDynamicScrollAlignment.Center ? (_rtViewport.rect.height - itemHeight) / 2.0f : alignment == EDynamicScrollAlignment.End ? _rtViewport.rect.height - itemHeight : 0.0f;

                return new Vector2(_rtContent.anchoredPosition.x, _padding.top + itemTop - alignmentOffset + offset);
            }

            float horizontalAlignmentOffset = GetAlignmentOffset(alignment, _rtViewport.rect.width, _rtItem.sizeDelta.x);
            return new Vector2(-(_padding.left + (line * ItemSize.x)) + horizontalAlignmentOffset - offset, _rtContent.anchoredPosition.y);
        }

        public void SetItemCntPerLine(int itemCntPerLine)
        {
            _itemCntPerLine = itemCntPerLine;
        }

        public Vector2 GetContentSize(int totalLineCnt)
        {
            if (HasVariableHeights)
                return new Vector2(ContentSize.x, _padding.top + _itemOffsets[ItemCnt] + _padding.bottom);

            if (totalLineCnt <= 0)
                return _scrollRect.vertical ? new Vector2(ContentSize.x, _padding.top + _padding.bottom) : new Vector2(_padding.left + _padding.right, ContentSize.y);

            return _scrollRect.vertical ? new Vector2(ContentSize.x, _padding.top + (totalLineCnt * ItemSize.y - _spacing.y) + _padding.bottom) : new Vector2(_padding.left + (totalLineCnt * ItemSize.x - _spacing.x) + _padding.right, ContentSize.y);
        }

        public Vector2 ClampContentPos(Vector2 contentPos, int totalLineCnt)
        {
            if (HasVariableHeights)
                return new Vector2(contentPos.x, Mathf.Clamp(contentPos.y, 0.0f, Mathf.Max(0.0f, GetContentSize(ItemCnt).y - _rtViewport.rect.height)));

            if (_scrollRect.vertical)
            {
                float minY = _padding.top;
                float lastLinePos = _padding.top + (Mathf.Max(0, totalLineCnt - 1) * ItemSize.y);
                float endAlignmentOffset = Mathf.Max(0.0f, _rtViewport.rect.height - _rtItem.sizeDelta.y);
                float maxY = Mathf.Max(minY, lastLinePos - endAlignmentOffset);
                return new Vector2(contentPos.x, Mathf.Clamp(contentPos.y, minY, maxY));
            }

            float horizontalLastLinePos = -(_padding.left + (Mathf.Max(0, totalLineCnt - 1) * ItemSize.x));
            float horizontalEndOffset = Mathf.Max(0.0f, _rtViewport.rect.width - _rtItem.sizeDelta.x);
            float minX = Mathf.Min(-_padding.left, horizontalLastLinePos + horizontalEndOffset);
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
            if (HasVariableHeights)
            {
                int low = 0;
                int high = ItemCnt;
                float top = _rtContent.anchoredPosition.y - _padding.top;
                while (low < high)
                {
                    int mid = low + (high - low) / 2;
                    if (_itemOffsets[mid] + _itemHeights[mid] <= top)
                        low = mid + 1;
                    else
                        high = mid;
                }

                return Mathf.Min(low, Mathf.Max(0, ItemCnt - 1));
            }

            float pos = _scrollRect.vertical ? _rtContent.anchoredPosition.y - _padding.top : -_rtContent.anchoredPosition.x - _padding.left;
            float itemSize = _scrollRect.vertical ? ItemSize.y : ItemSize.x;
            int idx = Mathf.FloorToInt((pos / itemSize) + 0.0001f);
            return Mathf.Clamp(idx, 0, lastLine);
        }

        public Vector2 GetItemPos(int itemIdx)
        {
            if (_scrollRect.vertical)
            {
                int x = itemIdx % _itemCntPerLine;
                int y = itemIdx / _itemCntPerLine;
                float posX = (x - ((_itemCntPerLine - 1) / 2.0f)) * ItemSize.x - CenterOffset.x;
                float height = HasVariableHeights ? _itemHeights[itemIdx] : _rtItem.sizeDelta.y;
                float itemTop = HasVariableHeights ? _itemOffsets[itemIdx] : y * ItemSize.y;
                float posY = (_rtContent.sizeDelta.y - height) * (1.0f - _rtItem.pivot.y) - itemTop;
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
            if (line < 0 || totalItemCnt <= 0)
                return 0;

            int totalLineCnt = Mathf.CeilToInt((float)totalItemCnt / _itemCntPerLine);
            if (line >= totalLineCnt)
                return 0;

            int lastLineItemCnt = totalItemCnt % _itemCntPerLine;
            bool isLastLine = line == totalLineCnt - 1;
            if (isLastLine && lastLineItemCnt > 0)
                return lastLineItemCnt;

            return _itemCntPerLine;
        }

        public bool TrySetItemHeights(IReadOnlyList<float> heights)
        {
            if (heights == null || !_scrollRect.vertical || _spacing.y < 0.0f)
                return false;

            double totalHeight = 0.0d;
            for (int idx = 0; idx < heights.Count; ++idx)
            {
                if (float.IsNaN(heights[idx]) || float.IsInfinity(heights[idx]) || heights[idx] <= 0.0f)
                    return false;

                totalHeight += heights[idx] + (idx > 0 ? (double)_spacing.y : 0.0d);
                if (totalHeight > float.MaxValue - (double)_padding.top - _padding.bottom)
                    return false;
            }

            _itemHeights = new float[heights.Count];
            _itemOffsets = new float[heights.Count + 1];
            double offset = 0.0d;
            for (int idx = 0; idx < heights.Count; ++idx)
            {
                _itemHeights[idx] = heights[idx];
                _itemOffsets[idx] = (float)offset;
                offset += heights[idx] + (idx < heights.Count - 1 ? (double)_spacing.y : 0.0d);
            }

            _itemOffsets[heights.Count] = (float)offset;
            return true;
        }

        public void ClearItemHeights()
        {
            _itemHeights = null;
            _itemOffsets = null;
        }

        public float[] CopyItemHeights()
        {
            return HasVariableHeights ? (float[])_itemHeights.Clone() : Array.Empty<float>();
        }

        public int GetVisibleItemCnt(int firstIdx)
        {
            float bottom = _rtContent.anchoredPosition.y - _padding.top + _rtViewport.rect.height;
            int low = firstIdx;
            int high = ItemCnt;
            while (low < high)
            {
                int mid = low + (high - low) / 2;
                if (_itemOffsets[mid] < bottom)
                    low = mid + 1;
                else
                    high = mid;
            }

            return Mathf.Min(ItemCnt, low + 1) - firstIdx;
        }

        public float GetItemHeight(int itemIdx) => HasVariableHeights ? _itemHeights[itemIdx] : _rtItem.sizeDelta.y;

        //============================================================
        // Utilities
        //============================================================
        private static float GetAlignmentOffset(EDynamicScrollAlignment alignment, float viewportSize, float itemSize)
        {
            float availableSize = Mathf.Max(0.0f, viewportSize - itemSize);
            switch (alignment)
            {
                case EDynamicScrollAlignment.Start:
                    return 0.0f;
                case EDynamicScrollAlignment.Center:
                    return availableSize / 2.0f;
                case EDynamicScrollAlignment.End:
                    return availableSize;
            }

            return 0.0f;
        }
    }
}
