using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UnityTools.Ui
{
    [RequireComponent(typeof(ScrollRect))]
    public class DynamicScrollView<TView> : DynamicScrollViewBase where TView : Component, IDynamicScrollItem
    {
        //============================================================
        // Constants
        //============================================================
        private const int MIN_VISIBLE_LINE_CNT = 1;
        private const int MIN_FIXED_CELLS_PER_GROUP = 1;
        private const int DEFAULT_EXTRA_VISIBLE_LINE_CNT = 1;
        private const float SCROLL_REFRESH_EPSILON = 0.01f;
        private const float MIN_SCROLL_DURATION_SEC = 0.0001f;

        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Item")]
        [SerializeField] private TView _item;
        [Header("Layout")]
        [SerializeField] private EDynamicScrollAxisType _axisType = EDynamicScrollAxisType.Vertical;
        [SerializeField] private EDynamicScrollLayoutMode _layoutMode = EDynamicScrollLayoutMode.FixedCnt;
        [SerializeField, Min(MIN_FIXED_CELLS_PER_GROUP)] private int _fixedCellsPerGroup = MIN_FIXED_CELLS_PER_GROUP;
        [SerializeField, Min(MIN_VISIBLE_LINE_CNT)] private int _minVisibleLineCnt = MIN_VISIBLE_LINE_CNT;
        [SerializeField] private EDynamicScrollContentAlignment _contentAlignment = EDynamicScrollContentAlignment.TopLeft;
        [SerializeField] private Vector2 _spacing;
        [SerializeField] private RectOffset _padding;
        [Header("Movement")]
        [SerializeField] private EDynamicScrollMovementType _movementType = EDynamicScrollMovementType.Clamped;
        [SerializeField] private bool _isInertia = true;
        [SerializeField, Range(0.001f, 0.3f)] private float _decelerationRate = 0.135f;
        [SerializeField, Range(0.1f, 1.0f)] private float _elasticity = 0.1f;

        //============================================================
        // Fields
        //============================================================
        private DynamicScrollContext _context;
        private DynamicScrollItemController<TView> _itemCtrl;
        private int _totalItemCnt;
        private int _totalLineCnt;
        private int _visibleLineCnt = MIN_VISIBLE_LINE_CNT;
        private int _itemCntPerLine = MIN_FIXED_CELLS_PER_GROUP;
        private RectTransform _rtView;
        private RectTransform _rtContent;
        private RectTransform _rtItem;
        private ScrollRect _scrollRect;
        private bool _isComponentReady;
        private bool _isInitialized;
        private float _lastScrollPos = float.MinValue;
        private Coroutine _coSmoothScroll;

        //============================================================
        // Events
        //============================================================
        private event UnityAction<TView> _onItemUpdated;
        public event UnityAction<TView> OnItemUpdated { add => _onItemUpdated += value; remove => _onItemUpdated -= value; }

        //============================================================
        // Properties
        //============================================================
        protected DynamicScrollContext Context => _context;
        protected DynamicScrollItemController<TView> ItemController => _itemCtrl;
        protected int TotalLineCnt => _totalLineCnt;
        protected int VisibleLineCnt => _visibleLineCnt;
        protected int TotalItemCnt => _totalItemCnt;
        protected int ItemCntPerLine => _itemCntPerLine;
        protected RectTransform RtView => _rtView;
        protected RectTransform RtContent => _rtContent;
        protected ScrollRect ScrollRect => _scrollRect;
        protected bool IsVertical => _axisType == EDynamicScrollAxisType.Vertical;
        public override bool IsInitialized => _isInitialized;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnRectTransformDimensionsChange()
        {
            if (!_isInitialized)
                return;

            int prevLineItemCnt = _itemCntPerLine;
            int prevVisibleLineCnt = _visibleLineCnt;
            UpdateLayoutConfig();
            if (prevLineItemCnt == _itemCntPerLine && prevVisibleLineCnt == _visibleLineCnt)
                return;

            UpdateContentLayout();
            RebuildVisibleItems();
        }

        private void OnDestroy()
        {
            ReleaseView();
        }

        //============================================================
        // Init/Register
        //============================================================
        private void PrepareComponents()
        {
            if (_isComponentReady)
                return;

            _rtView = GetComponent<RectTransform>();
            _scrollRect = GetComponent<ScrollRect>();
            _rtContent = _scrollRect.content;
            _rtItem = _item.transform as RectTransform;

            ApplyScrollRectSettings();
            ApplyContentAlignment();
            _itemCntPerLine = GetItemCntPerLine();
            _totalItemCnt = -1;
            _context = new DynamicScrollContext(_itemCntPerLine, _spacing, _padding, _rtItem, _scrollRect);
            UpdateAutoVisibleLineCnt();

            int initialItemCnt = Mathf.Max(_visibleLineCnt * _itemCntPerLine, _itemCntPerLine);
            _itemCtrl = new DynamicScrollItemController<TView>(_context, initialItemCnt, _item, _rtContent);

            _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            _itemCtrl.OnItemUpdated += OnControllerItemUpdated;
            _isComponentReady = true;
        }

        public void InitView(int totalItemCnt)
        {
            if (totalItemCnt < 0)
            {
                Debug.LogError("동적 스크롤 전체 Item 수는 0 이상이어야 합니다. 값=" + totalItemCnt, this);
                return;
            }

            PrepareComponents();
            _isInitialized = true;
            UpdateLayoutConfig();
            bool isSameItemCnt = totalItemCnt == _totalItemCnt;
            SetTotalItemCnt(totalItemCnt);
            if (isSameItemCnt)
                RebuildVisibleItems();
        }

        public void ReleaseView()
        {
            if (!_isComponentReady)
                return;

            StopSmoothScroll();
            _scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
            _itemCtrl.OnItemUpdated -= OnControllerItemUpdated;
            _itemCtrl.Release();
            _context = null;
            _itemCtrl = null;
            _isComponentReady = false;
            _isInitialized = false;
        }

        //============================================================
        // Logic
        //============================================================
        public void RefreshView()
        {
            RebuildVisibleItems();
        }

        public void RefreshItem(int itemIdx)
        {
            if (itemIdx < 0 || itemIdx >= _totalItemCnt)
            {
                Debug.LogError("갱신할 DynamicScroll Item 인덱스가 유효하지 않습니다. 인덱스=" + itemIdx + ", 전체 개수=" + _totalItemCnt, this);
                return;
            }

            _itemCtrl.UpdateItem(itemIdx);
        }

        public void RefreshRange(int startIdx, int cnt)
        {
            if (startIdx < 0 || cnt < 0 || startIdx > _totalItemCnt - cnt)
            {
                Debug.LogError("갱신할 DynamicScroll Item 범위가 유효하지 않습니다. 시작 인덱스=" + startIdx + ", 개수=" + cnt + ", 전체 개수=" + _totalItemCnt, this);
                return;
            }

            if (cnt == 0)
                return;

            _itemCtrl.UpdateRange(startIdx, cnt);
        }

        public void UpdateItemCnt(int totalItemCnt, bool shouldPreserveScrollPos = true)
        {
            if (!_isInitialized || totalItemCnt < 0)
            {
                Debug.LogError("DynamicScroll Item 개수 갱신 조건이 유효하지 않습니다. 초기화=" + _isInitialized + ", 전체 개수=" + totalItemCnt, this);
                return;
            }

            StopSmoothScroll();
            ApplyTotalItemCnt(totalItemCnt);
            if (!shouldPreserveScrollPos)
                _rtContent.anchoredPosition = _context.GetContentPos(0);

            _rtContent.anchoredPosition = _context.ClampContentPos(_rtContent.anchoredPosition, _totalLineCnt);
            RebuildVisibleItems();
        }

        public void InsertItems(int itemIdx, int itemCnt = 1, bool shouldPreserveAnchor = true)
        {
            if (!_isInitialized || itemIdx < 0 || itemIdx > _totalItemCnt || itemCnt <= 0)
            {
                Debug.LogError("삽입할 DynamicScroll Item 범위가 유효하지 않습니다. 초기화=" + _isInitialized + ", 인덱스=" + itemIdx + ", 개수=" + itemCnt + ", 전체 개수=" + _totalItemCnt, this);
                return;
            }

            StopSmoothScroll();
            int anchorIdx = GetFirstVisibleItemIdx();
            Vector2 contentPos = _rtContent.anchoredPosition;
            ApplyTotalItemCnt(_totalItemCnt + itemCnt);
            if (shouldPreserveAnchor && itemIdx <= anchorIdx)
            {
                Vector2 prevAnchorPos = _context.GetContentPos(anchorIdx);
                Vector2 nextAnchorPos = _context.GetContentPos(anchorIdx + itemCnt);
                contentPos += nextAnchorPos - prevAnchorPos;
            }

            _rtContent.anchoredPosition = _context.ClampContentPos(contentPos, _totalLineCnt);
            RebuildVisibleItems();
        }

        public void RemoveItems(int itemIdx, int itemCnt = 1, bool shouldPreserveAnchor = true)
        {
            if (!_isInitialized || itemIdx < 0 || itemCnt <= 0 || itemIdx > _totalItemCnt - itemCnt)
            {
                Debug.LogError("제거할 DynamicScroll Item 범위가 유효하지 않습니다. 초기화=" + _isInitialized + ", 인덱스=" + itemIdx + ", 개수=" + itemCnt + ", 전체 개수=" + _totalItemCnt, this);
                return;
            }

            StopSmoothScroll();
            int anchorIdx = GetFirstVisibleItemIdx();
            int nextTotalItemCnt = _totalItemCnt - itemCnt;
            Vector2 contentPos = _rtContent.anchoredPosition;
            ApplyTotalItemCnt(nextTotalItemCnt);
            if (shouldPreserveAnchor && itemIdx <= anchorIdx && nextTotalItemCnt > 0)
            {
                int nextAnchorIdx = itemIdx + itemCnt <= anchorIdx ? anchorIdx - itemCnt : Mathf.Min(itemIdx, nextTotalItemCnt - 1);
                Vector2 prevAnchorPos = _context.GetContentPos(anchorIdx);
                Vector2 nextAnchorPos = _context.GetContentPos(nextAnchorIdx);
                contentPos += nextAnchorPos - prevAnchorPos;
            }

            _rtContent.anchoredPosition = _context.ClampContentPos(contentPos, _totalLineCnt);
            RebuildVisibleItems();
        }

        public void ScrollTo(int itemIdx, bool isImmediate = false, float durationSec = 0.3f, EDynamicScrollAlignment alignment = EDynamicScrollAlignment.Start, float offset = 0.0f)
        {
            if (_totalItemCnt <= 0)
                return;

            int targetIdx = Mathf.Clamp(itemIdx, 0, _totalItemCnt - 1);
            Vector2 targetPos = _context.GetContentPos(targetIdx, offset, alignment);
            targetPos = _context.ClampContentPos(targetPos, _totalLineCnt);
            if (isImmediate || durationSec <= MIN_SCROLL_DURATION_SEC)
            {
                StopSmoothScroll();
                _rtContent.anchoredPosition = targetPos;
                ProcessScroll(Vector2.zero);
                return;
            }

            StartSmoothScroll(targetPos, durationSec);
        }

        protected void SetTotalItemCnt(int totalItemCnt)
        {
            if (totalItemCnt == _totalItemCnt)
                return;

            ApplyTotalItemCnt(totalItemCnt);
            if (_isInitialized)
            {
                RebuildVisibleItems();
                return;
            }

            _itemCtrl.UpdatePos();
        }

        protected void SetVisibleLineCnt(int visibleLineCnt)
        {
            if (visibleLineCnt == _visibleLineCnt)
                return;

            _visibleLineCnt = visibleLineCnt;
            if (_isInitialized)
                RebuildVisibleItems();
        }

        private void ProcessScroll(Vector2 value)
        {
            if (!_isComponentReady || _totalItemCnt <= 0 || _itemCtrl.Cnt <= 0)
            {
                HandleScrollValueChanged(value);
                return;
            }

            float curScrollPos = IsVertical ? _rtContent.anchoredPosition.y : -_rtContent.anchoredPosition.x;
            if (Mathf.Abs(curScrollPos - _lastScrollPos) <= SCROLL_REFRESH_EPSILON)
            {
                HandleScrollValueChanged(value);
                return;
            }

            _lastScrollPos = curScrollPos;
            int curLine = _itemCtrl.FirstIdx / _itemCntPerLine;
            int visibleLine = _context.GetFirstVisibleLine(Mathf.Max(0, _totalLineCnt - _visibleLineCnt));
            if (curLine != visibleLine)
            {
                bool isDown = visibleLine > curLine;
                int moveLineCnt = Mathf.Min(Mathf.Abs(curLine - visibleLine), _visibleLineCnt);
                for (int i = 0; i < moveLineCnt; i++)
                {
                    int addLine = isDown ? visibleLine + _visibleLineCnt - moveLineCnt + i : visibleLine + moveLineCnt - i - 1;
                    int removeLine = isDown ? curLine + i : curLine + _visibleLineCnt - i - 1;
                    _itemCtrl.AddRange(_context.GetItemCntForLine(addLine, _totalItemCnt), addLine * _itemCntPerLine, isDown);
                    _itemCtrl.RemoveRange(_context.GetItemCntForLine(removeLine, _totalItemCnt), !isDown);
                }
            }

            HandleScrollValueChanged(value);
        }

        private void ApplyScrollRectSettings()
        {
            _scrollRect.vertical = IsVertical;
            _scrollRect.horizontal = !IsVertical;
            _scrollRect.movementType = ConvertMovementType(_movementType);
            _scrollRect.inertia = _isInertia;
            _scrollRect.decelerationRate = _decelerationRate;
            _scrollRect.elasticity = _elasticity;
        }

        private void ApplyContentAlignment()
        {
            if (IsVertical)
            {
                float crossAlignX = ResolveCrossAlignX(_contentAlignment);
                _rtContent.anchorMin = new Vector2(crossAlignX, 1.0f);
                _rtContent.anchorMax = new Vector2(crossAlignX, 1.0f);
                _rtContent.pivot = new Vector2(crossAlignX, 1.0f);
            }
            else
            {
                float crossAlignY = ResolveCrossAlignY(_contentAlignment);
                _rtContent.anchorMin = new Vector2(0.0f, crossAlignY);
                _rtContent.anchorMax = new Vector2(0.0f, crossAlignY);
                _rtContent.pivot = new Vector2(0.0f, crossAlignY);
            }

            _rtContent.anchoredPosition = Vector2.zero;
        }

        private void UpdateLayoutConfig()
        {
            if (!_isComponentReady)
                return;

            int nextLineItemCnt = GetItemCntPerLine();
            if (nextLineItemCnt != _itemCntPerLine)
            {
                _itemCntPerLine = nextLineItemCnt;
                _context.SetItemCntPerLine(_itemCntPerLine);
            }

            UpdateAutoVisibleLineCnt();
        }

        private void UpdateAutoVisibleLineCnt()
        {
            int autoVisibleLineCnt = _context.GetAutoVisibleLineCnt(DEFAULT_EXTRA_VISIBLE_LINE_CNT);
            _visibleLineCnt = Mathf.Max(_minVisibleLineCnt, autoVisibleLineCnt);
        }

        private int GetItemCntPerLine()
        {
            switch (_layoutMode)
            {
                case EDynamicScrollLayoutMode.Single:
                    return MIN_FIXED_CELLS_PER_GROUP;
                case EDynamicScrollLayoutMode.FixedCnt:
                    return _fixedCellsPerGroup;
                case EDynamicScrollLayoutMode.AutoFit:
                    return GetAutoItemCntPerLine();
            }

            return MIN_FIXED_CELLS_PER_GROUP;
        }

        private int GetAutoItemCntPerLine()
        {
            RectTransform rtViewport = _scrollRect.viewport != null ? _scrollRect.viewport : _rtView;

            float viewportCrossSize = IsVertical ? rtViewport.rect.width : rtViewport.rect.height;
            float crossPaddingSize = IsVertical ? _padding.left + _padding.right : _padding.top + _padding.bottom;
            float crossSpacingSize = IsVertical ? _spacing.x : _spacing.y;
            float cellCrossSize = IsVertical ? _rtItem.sizeDelta.x : _rtItem.sizeDelta.y;

            float availableCrossSize = Mathf.Max(0.0f, viewportCrossSize - crossPaddingSize);
            float cellSpan = cellCrossSize + crossSpacingSize;
            int fitCnt = Mathf.FloorToInt((availableCrossSize + crossSpacingSize) / cellSpan);
            return Mathf.Max(MIN_FIXED_CELLS_PER_GROUP, fitCnt);
        }

        private void UpdateContentLayout()
        {
            _totalLineCnt = Mathf.CeilToInt((float)_totalItemCnt / _itemCntPerLine);
            _rtContent.sizeDelta = _context.GetContentSize(_totalLineCnt);
        }

        private void ApplyTotalItemCnt(int totalItemCnt)
        {
            _totalItemCnt = totalItemCnt;
            UpdateContentLayout();
        }

        private int GetFirstVisibleItemIdx()
        {
            if (_totalItemCnt <= 0)
                return 0;

            int lastLine = Mathf.Max(0, _totalLineCnt - _visibleLineCnt);
            return Mathf.Min(_context.GetFirstVisibleItemIdx(lastLine), _totalItemCnt - 1);
        }

        private void RebuildVisibleItems()
        {
            _lastScrollPos = float.MinValue;
            _itemCtrl.Clear();
            if (_totalItemCnt <= 0)
                return;

            _rtContent.anchoredPosition = _context.ClampContentPos(_rtContent.anchoredPosition, _totalLineCnt);
            int firstLine = _context.GetFirstVisibleLine(Mathf.Max(0, _totalLineCnt - _visibleLineCnt));
            int firstIdx = firstLine * _itemCntPerLine;
            if (firstIdx >= _totalItemCnt)
                return;

            int visibleItemCnt = Mathf.Min(_visibleLineCnt * _itemCntPerLine, _totalItemCnt - firstIdx);
            if (visibleItemCnt > 0)
                _itemCtrl.AddRange(visibleItemCnt, firstIdx, true);
        }

        private void StartSmoothScroll(Vector2 targetPos, float durationSec)
        {
            StopSmoothScroll();
            _coSmoothScroll = StartCoroutine(CoSmoothScroll(targetPos, durationSec));
        }

        private void StopSmoothScroll()
        {
            if (_coSmoothScroll == null)
                return;

            StopCoroutine(_coSmoothScroll);
            _coSmoothScroll = null;
        }

        //============================================================
        // Coroutines
        //============================================================
        private IEnumerator CoSmoothScroll(Vector2 targetPos, float durationSec)
        {
            Vector2 startPos = _rtContent.anchoredPosition;
            float elapsedSec = 0.0f;

            while (elapsedSec < durationSec)
            {
                elapsedSec += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedSec / durationSec);
                float smoothT = t * t * (3.0f - (2.0f * t));
                _rtContent.anchoredPosition = Vector2.Lerp(startPos, targetPos, smoothT);
                OnScrollValueChanged(Vector2.zero);
                yield return null;
            }

            _rtContent.anchoredPosition = targetPos;
            OnScrollValueChanged(Vector2.zero);
            _coSmoothScroll = null;
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OnScrollValueChanged(Vector2 value)
        {
            ProcessScroll(value);
        }

        private void OnControllerItemUpdated(TView itemView)
        {
            _onItemUpdated?.Invoke(itemView);
        }

        protected virtual void HandleScrollValueChanged(Vector2 value) { }

        //============================================================
        // Utilities
        //============================================================
        private static ScrollRect.MovementType ConvertMovementType(EDynamicScrollMovementType movementType)
        {
            switch (movementType)
            {
                case EDynamicScrollMovementType.Unrestricted:
                    return ScrollRect.MovementType.Unrestricted;
                case EDynamicScrollMovementType.Elastic:
                    return ScrollRect.MovementType.Elastic;
                case EDynamicScrollMovementType.Clamped:
                    return ScrollRect.MovementType.Clamped;
            }

            return ScrollRect.MovementType.Clamped;
        }

        private static float ResolveCrossAlignX(EDynamicScrollContentAlignment alignment)
        {
            switch (alignment)
            {
                case EDynamicScrollContentAlignment.TopLeft:
                case EDynamicScrollContentAlignment.MiddleLeft:
                case EDynamicScrollContentAlignment.BottomLeft:
                    return 0.0f;
                case EDynamicScrollContentAlignment.TopCenter:
                case EDynamicScrollContentAlignment.MiddleCenter:
                case EDynamicScrollContentAlignment.BottomCenter:
                    return 0.5f;
                case EDynamicScrollContentAlignment.TopRight:
                case EDynamicScrollContentAlignment.MiddleRight:
                case EDynamicScrollContentAlignment.BottomRight:
                    return 1.0f;
            }

            return 0.0f;
        }

        private static float ResolveCrossAlignY(EDynamicScrollContentAlignment alignment)
        {
            switch (alignment)
            {
                case EDynamicScrollContentAlignment.TopLeft:
                case EDynamicScrollContentAlignment.TopCenter:
                case EDynamicScrollContentAlignment.TopRight:
                    return 1.0f;
                case EDynamicScrollContentAlignment.MiddleLeft:
                case EDynamicScrollContentAlignment.MiddleCenter:
                case EDynamicScrollContentAlignment.MiddleRight:
                    return 0.5f;
                case EDynamicScrollContentAlignment.BottomLeft:
                case EDynamicScrollContentAlignment.BottomCenter:
                case EDynamicScrollContentAlignment.BottomRight:
                    return 0.0f;
            }

            return 1.0f;
        }
    }
}
